// Adapted from Ploppable RICO Revisited.  Inspired by work by SamsamTS (Boogieman Sam); all blame for everything wrong goes to algernon.

using System;
using System.Linq;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using System.Collections.Generic;


namespace FindIt.GUI
{
    public static class ThumbnailManager
    {
        // Instances.
        private static GameObject gameObject;
        private static ThumbnailQueue _queue;
        private static PreviewRenderer _renderer;
        internal static PreviewRenderer Renderer => _renderer;


        /// <summary>
        /// Queues a prefab for rendering.
        /// </summary>
        /// <param name="prefab">Prefab to queue</param>
        /// <param name="prefab">Button for thumbnail</param>
        public static void QueueThumbnail(PrefabInfo prefab, UIButton button)
        {
            // Create the render if there isn't one already.
            if (gameObject == null)
            {
                Create();
            }

            _queue.QueueThumbnail(prefab, button);
        }


        /// <summary>
        /// Creates our renderer GameObject.
        /// </summary>
        internal static void Create()
        {
            try
            {
                // If no instance already set, create one.
                if (gameObject == null)
                {
                    // Give it a unique name for easy finding with ModTools.
                    gameObject = new GameObject("FindItThumbnailRenderer");
                    gameObject.transform.parent = UIView.GetAView().transform;

                    // Add our queue manager and renderer directly to the gameobject.
                    _renderer = gameObject.AddComponent<PreviewRenderer>();
                    _queue = gameObject.AddComponent<ThumbnailQueue>();

                    Debugging.Message("thumbnail renderer created");
                }
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }
        }


        /// <summary>
        /// Cleans up when finished.
        /// </summary>
        internal static void Close()
        {
            GameObject.Destroy(_queue);
            GameObject.Destroy(_renderer);
            GameObject.Destroy(gameObject);

            Debugging.Message("thumbnail renderer destroyed");
        }
    }


    /// <summary>
    /// Manages a queue for rendering thumbnail images.
    /// Inspired by Boogieman Sam's FindIt! UI.
    /// </summary>
    public class ThumbnailQueue : UIComponent
    {
        // Renderer for thumbnail images.
        private PreviewRenderer renderer;

        // Render queue.
        private Dictionary<PrefabInfo, UIButton> renderQueue;


        /// <summary>
        /// Update method - we render a new thumbnail every time this is called.
        /// Called by Unity every frame.
        /// </summary>
        public override void Update()
        {
            base.Update();

            try
            {
                // TODO: Refactor and merge in RenderThumbnailAtlas
                if (renderQueue != null && renderQueue.Count > 0)
                {
                    List<PrefabInfo> prefabs;
                    lock (renderQueue)
                    {
                        prefabs = new List<PrefabInfo>(renderQueue.Keys);
                    }

                    int count = 0;
                    foreach (PrefabInfo prefab in prefabs)
                    {
                        string name = Asset.GetName(prefab);
                        string baseIconName = prefab.m_Thumbnail;
                        if (!ImageUtils.CreateThumbnailAtlas(name, prefab) && !baseIconName.IsNullOrWhiteSpace())
                        {
                            prefab.m_Thumbnail = baseIconName;
                        }
                        UIButton button = renderQueue[prefab];
                        if (button != null)
                        {
                            button.atlas = prefab.m_Atlas;

                            button.normalFgSprite = prefab.m_Thumbnail;
                            button.hoveredFgSprite = prefab.m_Thumbnail + "Hovered";
                            button.pressedFgSprite = prefab.m_Thumbnail + "Pressed";
                            button.disabledFgSprite = prefab.m_Thumbnail + "Disabled";
                            button.focusedFgSprite = null;
                        }

                        lock (renderQueue)
                        {
                            renderQueue.Remove(prefab);
                        }
                        count++;

                        // Generate 1 thumbnail max
                        if (count > 1) break;
                    }

                    FindIt.instance.scrollPanel.Refresh();
                }
            }
            catch (Exception e)
            {
                Debugging.Message("thumbnail failed");
                Debugging.LogException(e);
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public ThumbnailQueue()
        {
            Debugging.Message("creating thumbnail queue");

            // Get local reference from parent.
            renderer = ThumbnailManager.Renderer;

            // Size and setting for thumbnail images: 109 x 100, doubled for anti-aliasing.
            renderer.Size = new Vector2(109, 100) * 2f;
            renderer.CameraRotation = 325f;
        }


        /// <summary>
        /// Adds a building to the render queue; assumes building button already created.
        /// </summary>
        /// <param name="building">Prefab data</param>
        /// <param name="button">Button</param>
        internal void QueueThumbnail(PrefabInfo prefab, UIButton button)
        {
            if (renderQueue == null)
            {
                // Initialise queue.
                renderQueue = new Dictionary<PrefabInfo, UIButton>();
            }
            
            // See if this prefab is already queued; if it is, update the button reference, otherwise add a new queue entry.
            if (renderQueue.ContainsKey(prefab))
            {
                renderQueue[prefab] = button;
            }
            else
            {
                renderQueue.Add(prefab, button);
            }
        }


        /// <summary>
        /// Generates  thumbnail images (normal, focused, hovered, and pressed) for the given prefab.
        /// </summary>
        /// <param name="prefab">The prefab to generate thumbnails for</param>
        internal void CreateThumbnail(PrefabInfo prefab, UIButton button)
        {
            // Don't do anything with null prefabs or prefabs without buttons.
            if (prefab == null || button == null)
            {
                return;
            }

            // Name setups.
            string thumbName = Asset.GetName(prefab);
            string baseIconName = prefab.m_Thumbnail;

            // Attempt to generate icon.
            if (!ImageUtils.CreateThumbnailAtlas(thumbName, prefab) && !baseIconName.IsNullOrWhiteSpace())
            {
                // If it failed, use the default thumbnail icon.
                prefab.m_Thumbnail = baseIconName;
            }

            // Set button thumbnail images.
            button.atlas = prefab.m_Atlas;
            button.normalFgSprite = prefab.m_Thumbnail;
            button.hoveredFgSprite = prefab.m_Thumbnail + "Hovered";
            button.pressedFgSprite = prefab.m_Thumbnail + "Pressed";
            button.disabledFgSprite = prefab.m_Thumbnail + "Disabled";
            button.focusedFgSprite = null;
        }


        /// <summary>
        /// Generates building thumbnail variants - focused, hovered, pressed, and disabled., 
        /// </summary>
        /// <param name="baseTexture">Base texture of the thumbnail</param>
        /// <returns>2d variant icon textures</returns>
        private Texture2D[] GenerateThumbnailVariants(Texture2D baseTexture)
        {
            var variantPixels = new Color32[baseTexture.width * baseTexture.height];
            var basePixels = baseTexture.GetPixels32();


            // Focused.
            ColorFilter(basePixels, variantPixels, 32, 64, 128, 2);
            Texture2D focusedTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false, false);
            focusedTexture.SetPixels32(variantPixels);
            focusedTexture.Apply(false);
            focusedTexture.name = baseTexture.name + "Focused";

            // Hovered.
            ColorFilter(basePixels, variantPixels, 128, 128, 128, 1);
            Texture2D hoveredTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false, false);
            hoveredTexture.SetPixels32(variantPixels);
            hoveredTexture.Apply(false);
            hoveredTexture.name = baseTexture.name + "Hovered";

            // Pressed.
            ColorFilter(basePixels, variantPixels, 128, 128, 128, 2);
            Texture2D pressedTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false, false);
            pressedTexture.SetPixels32(variantPixels);
            pressedTexture.Apply(false);
            pressedTexture.name = baseTexture.name + "Pressed";

            // Don't bother with 'disabled' texture since we don't use it, and we save memory by not adding it.

            return new Texture2D[]
            {
                baseTexture,
                focusedTexture,
                hoveredTexture,
                pressedTexture
            };
        }


        /// <summary>
        /// Applies an RGB filter to a source colour, optionally reducing the intensity of the source colour before filtering (alpha is left unchanged).
        /// </summary>
        /// <param name="sourceColor">Source colour to filter</param>
        /// <param name="resultColor">Result of filtering</param>
        /// <param name="filterR">Red component of filter</param>
        /// <param name="filterG">Green component of filter</param>
        /// <param name="filterB">Blue component of filter</param>
        /// <param name="filterStrength">Each channel (RGB) of the original colour is bitshifted right this number before filtering (to reduce its intensity)</param>
        private void ColorFilter(Color32[] sourceColor, Color32[] resultColor, byte filterR, byte filterG, byte filterB, byte filterStrength)
        {
            for (int i = 0; i < sourceColor.Length; i++)
            {
                // Rightshift the source channel by the required amount before adding the relevant filter channel.
                resultColor[i].r = (byte)((sourceColor[i].r >> filterStrength) + filterR);
                resultColor[i].g = (byte)((sourceColor[i].g >> filterStrength) + filterG);
                resultColor[i].b = (byte)((sourceColor[i].b >> filterStrength) + filterB);
                resultColor[i].a = sourceColor[i].a;
            }
        }


        /// <summary>
        /// Adds a collection of textures to an atlas.
        /// </summary>
        /// <param name="atlas">Atlas to add to</param>
        /// <param name="newTextures">Textures to add</param>
        private void AddTexturesToAtlas(UITextureAtlas atlas, Texture2D[] newTextures)
        {
            Texture2D[] textures = new Texture2D[atlas.count + newTextures.Length];


            // Populate textures with sprites from the atlas.
            for (int i = 0; i < atlas.count; i++)
            {
                textures[i] = atlas.sprites[i].texture;
                textures[i].name = atlas.sprites[i].name;
            }

            // Append new textures to our list.
            for (int i = 0; i < newTextures.Length; i++)
            {
                textures[atlas.count + i] = newTextures[i];
            }

            // Repack atlas with our new additions (regions are individual texture areas within the atlas).
            Rect[] regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            // Clear original atlas sprites.
            atlas.sprites.Clear();

            // Iterate through our list, adding each sprite into the atlas.
            for (int i = 0; i < textures.Length; i++)
            {
                UITextureAtlas.SpriteInfo spriteInfo = atlas[textures[i].name];
                atlas.sprites.Add(new UITextureAtlas.SpriteInfo
                {
                    texture = textures[i],
                    name = textures[i].name,
                    border = (spriteInfo != null) ? spriteInfo.border : new RectOffset(),
                    region = regions[i]
                });
            }

            // Rebuild atlas indexes.
            atlas.RebuildIndexes();
        }
    }
}