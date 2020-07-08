// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

namespace FindIt.GUI
{
    public class ImageUtils
    {
        private static PreviewRenderer m_previewRenderer;
        private static Texture2D focusedFilterTexture;

        public static bool CreateThumbnailAtlas(string name, PrefabInfo prefab)
        {
            if (name.IsNullOrWhiteSpace() || prefab == null) return false;
            if (prefab.m_Thumbnail == name) return true;

            if (m_previewRenderer == null)
            {
                m_previewRenderer = new GameObject("FindItPreviewRenderer").AddComponent<PreviewRenderer>();
                m_previewRenderer.Size = new Vector2(109, 100) * 2f;
            }

            m_previewRenderer.CameraRotation = 210f;
            m_previewRenderer.Zoom = 4f;

            bool rendered = false;

            BuildingInfo buildingPrefab = prefab as BuildingInfo;
            if (buildingPrefab != null)
            {
                m_previewRenderer.Mesh = buildingPrefab.m_mesh;
                m_previewRenderer.Material = buildingPrefab.m_material;

                if (m_previewRenderer.Mesh != null)
                {
                    if (buildingPrefab.m_useColorVariations && buildingPrefab.m_material != null)
                    {
                        Color materialColor = buildingPrefab.m_material.color;
                        buildingPrefab.m_material.color = buildingPrefab.m_color0;
                        m_previewRenderer.Render();
                        buildingPrefab.m_material.color = materialColor;
                    }
                    else
                    {
                        m_previewRenderer.Render();
                    }

                    rendered = true;
                }
            }

            PropInfo propPrefab = prefab as PropInfo;
            if (propPrefab != null)
            {
                if (propPrefab.m_material != null && (propPrefab.m_material.shader == Asset.shaderBlend || propPrefab.m_material.shader == Asset.shaderSolid))
                {
                    //RenderTexture active = RenderTexture.active;

                    Texture2D mainTexture = propPrefab.m_material.GetTexture("_MainTex") as Texture2D;
                    Texture2D aci = propPrefab.m_material.GetTexture("_ACIMap") as Texture2D;

                    Texture2D texture = new Texture2D(mainTexture.width, mainTexture.height);
                    ResourceLoader.CopyTexture(mainTexture, texture);
                    Color32[] colors = texture.GetPixels32();

                    if (aci != null)
                    {
                        ResourceLoader.CopyTexture(aci, texture);
                        Color32[] aciColors = texture.GetPixels32();

                        for (int i = 0; i < colors.Length; i++)
                        {
                            colors[i].a -= aciColors[i].r;
                        }

                        texture.SetPixels32(0, 0, texture.width, texture.height, colors);
                        texture.Apply();
                    }
                    
                    ScaleTexture2(texture, 109 - 10, 100 - 10);
                    texture.name = name;

                    prefab.m_Thumbnail = name;
                    prefab.m_Atlas = ResourceLoader.CreateTextureAtlas("FindItThumbnails_" + name, new string[] { }, null);
                    ResourceLoader.AddTexturesInAtlas(prefab.m_Atlas, GenerateMissingThumbnailVariants(texture));

                    Debugging.Message("Generated thumbnails for: " + name);

                    return true;
                }
                else
                {
                    m_previewRenderer.Mesh = propPrefab.m_mesh;
                    m_previewRenderer.Material = propPrefab.m_material;

                    if (m_previewRenderer.Mesh != null)
                    {
                        if (propPrefab.m_useColorVariations && propPrefab.m_material != null)
                        {
                            Color materialColor = propPrefab.m_material.color;
                            propPrefab.m_material.color = propPrefab.m_color0;
                            m_previewRenderer.Render();
                            propPrefab.m_material.color = materialColor;
                        }
                        else
                        {
                            m_previewRenderer.Render();
                        }

                        rendered = true;
                    }
                }
            }

            TreeInfo treePrefab = prefab as TreeInfo;
            if (treePrefab != null)
            {
                m_previewRenderer.Mesh = treePrefab.m_mesh;
                m_previewRenderer.Material = treePrefab.m_material;

                if (m_previewRenderer.Mesh != null)
                {
                    m_previewRenderer.Render();
                    rendered = true;
                }
            }

            if (rendered)
            {
                Texture2D texture = ResourceLoader.ConvertRenderTexture(m_previewRenderer.Texture);
                texture.name = name;

                prefab.m_Thumbnail = name;
                prefab.m_Atlas = ResourceLoader.CreateTextureAtlas("FindItThumbnails_" + name, new string[] { }, null);

                ResourceLoader.ResizeTexture(texture, 109, 100);
                ResourceLoader.AddTexturesInAtlas(prefab.m_Atlas, GenerateMissingThumbnailVariants(texture));

                Debugging.Message("Generated thumbnails for: " + name);
            }
            else
            {
                prefab.m_Thumbnail = "ThumbnailBuildingDefault";
            }

            return rendered;
        }

        public static void AddThumbnailVariantsInAtlas(PrefabInfo prefab)
        {
            Texture2D texture = prefab.m_Atlas[prefab.m_Thumbnail].texture;
            prefab.m_Atlas = ResourceLoader.CreateTextureAtlas("FindItThumbnails_" + prefab.m_Thumbnail, new string[] { }, null);

            ResourceLoader.AddTexturesInAtlas(prefab.m_Atlas, GenerateMissingThumbnailVariants(texture));

            Debugging.Message("Generated thumbnails variants for: " + prefab.name);
        }

        public static void FixThumbnails(PrefabInfo prefab, UIButton button)
        {
            // Fixing thumbnails
            if (prefab.m_Atlas == null || prefab.m_Thumbnail.IsNullOrWhiteSpace() ||
                // used for more than one prefab
                prefab.m_Thumbnail == "Thumboldasphalt" ||
                prefab.m_Thumbnail == "Thumbbirdbathresidential" ||
                prefab.m_Thumbnail == "Thumbcrate" ||
                prefab.m_Thumbnail == "Thumbhedge" ||
                prefab.m_Thumbnail == "Thumbhedge2" ||
                // empty thumbnails
                prefab.m_Thumbnail == "thumb_Ferry Info Sign" ||
                prefab.m_Thumbnail == "thumb_Paddle Car 01" ||
                prefab.m_Thumbnail == "thumb_Paddle Car 02" ||
                prefab.m_Thumbnail == "thumb_Pier Rope Pole" ||
                prefab.m_Thumbnail == "thumb_RailwayPowerline Singular" ||
                prefab.m_Thumbnail == "thumb_Rubber Tire Row" ||
                prefab.m_Thumbnail == "thumb_Dam" ||
                prefab.m_Thumbnail == "thumb_Power Line" ||
                // terrible thumbnails
                prefab.m_Thumbnail == "thumb_Railway Crossing Long" ||
                prefab.m_Thumbnail == "thumb_Railway Crossing Medium" ||
                prefab.m_Thumbnail == "thumb_Railway Crossing Short" ||
                prefab.m_Thumbnail == "thumb_Railway Crossing Very Long"
                )
            {
                
                /*if(!ThumbnailManager.thumbnailsToGenerate.ContainsKey(prefab))
                {
                    ThumbnailManager.thumbnailsToGenerate.Add(prefab, button);
                }
                else if(button != null)
                {
                    ThumbnailManager.thumbnailsToGenerate[prefab] = button;
                }*/
                ThumbnailManager.QueueThumbnail(prefab, button);

                return;
            }

            if (prefab.m_Atlas != null && (
                // Missing variations
                prefab.m_Atlas.name == "AssetThumbs" ||
                prefab.m_Atlas.name == "Monorailthumbs" ||
                prefab.m_Atlas.name == "Netpropthumbs" ||
                prefab.m_Atlas.name == "Animalthumbs" ||
                prefab.m_Atlas.name == "PublictransportProps" ||
                prefab.m_Thumbnail == "thumb_Path Rock 01" ||
                prefab.m_Thumbnail == "thumb_Path Rock 02" ||
                prefab.m_Thumbnail == "thumb_Path Rock 03" ||
                prefab.m_Thumbnail == "thumb_Path Rock 04" ||
                prefab.m_Thumbnail == "thumb_Path Rock Small 01" ||
                prefab.m_Thumbnail == "thumb_Path Rock Small 02" ||
                prefab.m_Thumbnail == "thumb_Path Rock Small 03" ||
                prefab.m_Thumbnail == "thumb_Path Rock Small 04"
                ))
            {
                AddThumbnailVariantsInAtlas(prefab);

                if (button != null)
                {
                    button.atlas = prefab.m_Atlas;

                    button.normalFgSprite = prefab.m_Thumbnail;
                    button.hoveredFgSprite = prefab.m_Thumbnail + "Hovered";
                    button.pressedFgSprite = prefab.m_Thumbnail + "Pressed";
                    button.disabledFgSprite = prefab.m_Thumbnail + "Disabled";
                    button.focusedFgSprite = null;
                }
            }
        }

        public static void ScaleTexture(Texture2D tex, int width, int height)
        {
            tex.filterMode = FilterMode.Trilinear;
            var newPixels = new Color[width * height];
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    newPixels[y * width + x] = tex.GetPixelBilinear(((float)x) / width, ((float)y) / height);
                }
            }
            tex.Resize(width, height);
            tex.SetPixels(newPixels);
            tex.Apply();
        }

        public static void ScaleTexture2(Texture2D tex, int width, int height)
        {
            var newPixels = new Color[width * height];

            float ratio = ((float)width) / tex.width;
            if (tex.height * ratio > height)
            {
                ratio = ((float)height) / tex.height;
            }

            if (ratio > 1f) ratio = 1f;

            int newW = Mathf.RoundToInt(tex.width * ratio);
            int newH = Mathf.RoundToInt(tex.height * ratio);

            ScaleTexture(tex, newW, newH);
        }

        public static void CropTexture(Texture2D tex, int x, int y, int width, int height)
        {
            var newPixels = tex.GetPixels(x, y, width, height);
            tex.Resize(width, height);
            tex.SetPixels(newPixels);
            tex.Apply();
        }

        // Colorize the focused icon blue using the LUT texture
        // Use a border of 8 (256/32) to ensure we don't pick up neighboring patches
        private static Color32 ColorizeFocused(Color32 c)
        {
            if (focusedFilterTexture == null)
            {
                focusedFilterTexture = ResourceLoader.loadTextureFromAssembly("FindIt.Icons.SelectFilter.png");
            }

            int b = c.b * 31 / 255;
            float u = ((8f + (float)c.r) / 271) / 32 + ((float)b / 32);
            float v = 1f - ((8f + (float)c.g) / 271);
            Color32 result = focusedFilterTexture.GetPixelBilinear(u, v);
            result.a = c.a;
            return result;
        }

        public static bool FixFocusedTexture(PrefabInfo prefab)
        {
            if (prefab == null || prefab.m_Atlas == null || prefab.m_Thumbnail.IsNullOrWhiteSpace()) return false;

            UITextureAtlas.SpriteInfo sprite = prefab.m_Atlas[prefab.m_Thumbnail + "Focused"];
            if (sprite != null)
            {
                Color32[] pixels = sprite.texture.GetPixels32();

                int count = 0;

                foreach (Color32 pixel in pixels)
                {
                    if (pixel.a > 127 && (pixel.r + pixel.g + pixel.b) > 0 )
                    {
                        Color.RGBToHSV(pixel, out float h, out float s, out float v);

                        if (h < 0.66f || h > 0.68f || s < 0.98f)
                        {
                            return false;
                        }

                        if(++count > 32)
                        {
                            break;
                        }
                    }
                }

                if (count > 0)
                {
                    ImageUtils.FixFocusedTexture(prefab.m_Atlas[prefab.m_Thumbnail].texture, sprite.texture);
                    Color32[] colors = sprite.texture.GetPixels32();

                    prefab.m_Atlas.texture.SetPixels32((int)(sprite.region.x * prefab.m_Atlas.texture.width), (int)(sprite.region.y * prefab.m_Atlas.texture.height), sprite.texture.width, sprite.texture.height, colors);
                    prefab.m_Atlas.texture.Apply();

                    return true;
                }
            }

            return false;
        }

        public static void RefreshAtlas(UITextureAtlas atlas)
        {
            Texture2D[] textures = new Texture2D[atlas.sprites.Count];

            int i = 0;
            foreach (UITextureAtlas.SpriteInfo sprite in atlas.sprites)
            {
                textures[i++] = sprite.texture;
            }
            atlas.AddTextures(textures);
        }

        public static void FixFocusedTexture(Texture2D baseTexture, Texture2D focusedTexture)
        {
            var newPixels = new Color32[baseTexture.width * baseTexture.height];
            var pixels = baseTexture.GetPixels32();

            ApplyFilter(pixels, newPixels, ColorizeFocused);
            focusedTexture.SetPixels32(newPixels);
            focusedTexture.Apply(false);
        }

        // Our own version of this as the one in AssetImporterThumbnails has hardcoded dimensions
        // and generates ugly dark blue focused thumbnails.
        public static Texture2D[] GenerateMissingThumbnailVariants(Texture2D baseTexture)
        {
            var newPixels = new Color32[baseTexture.width * baseTexture.height];
            var pixels = baseTexture.GetPixels32();

            ApplyFilter(pixels, newPixels, ColorizeFocused);
            Texture2D focusedTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false, false);
            focusedTexture.SetPixels32(newPixels);
            focusedTexture.Apply(false);
            focusedTexture.name = baseTexture.name + "Focused";

            ApplyFilter(pixels, newPixels, c => new Color32((byte)(128 + c.r / 2), (byte)(128 + c.g / 2), (byte)(128 + c.b / 2), c.a));
            Texture2D hoveredTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false, false);
            hoveredTexture.SetPixels32(newPixels);
            hoveredTexture.Apply(false);
            hoveredTexture.name = baseTexture.name + "Hovered";

            ApplyFilter(pixels, newPixels, c => new Color32((byte)(192 + c.r / 4), (byte)(192 + c.g / 4), (byte)(192 + c.b / 4), c.a));
            Texture2D pressedTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false, false);
            pressedTexture.SetPixels32(newPixels);
            pressedTexture.Apply(false);
            pressedTexture.name = baseTexture.name + "Pressed";

            // Don't need to generate disabled thumbnails as they are never used in Find It.
            /*
            ApplyFilter(pixels, newPixels, c => new Color32(0, 0, 0, c.a));
            Texture2D disabledTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false, false);
            disabledTexture.SetPixels32(newPixels);
            disabledTexture.Apply(false);
            disabledTexture.name = baseTexture.name + "Disabled";
            */

            return new Texture2D[]
            {
                baseTexture,
                focusedTexture,
                hoveredTexture,
                pressedTexture//,
                //disabledTexture
            };
        }

        delegate Color32 Filter(Color32 c);

        private static void ApplyFilter(Color32[] src, Color32[] dst, Filter filter)
        {
            for (int i = 0; i < src.Length; i++)
            {
                dst[i] = filter(src[i]);
            }
        }
    }
}
