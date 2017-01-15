using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

namespace FindIt.GUI
{
    public class UIScrollPanel : UIHorizontalFastList<UIScrollPanelItem.ItemData, UIScrollPanelItem, UIButton>
    {
        public UIVerticalAlignment buttonsAlignment;

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (height > itemHeight && scrollbar == null)
            {
                DestroyScrollbars(parent);

                // Scrollbar
                UIScrollbar scroll = parent.AddUIComponent<UIScrollbar>();
                scroll.width = 20f;
                scroll.height = parent.parent.height;
                scroll.orientation = UIOrientation.Vertical;
                scroll.pivot = UIPivotPoint.BottomLeft;
                scroll.thumbPadding = new RectOffset(0, 0, 5, 5);
                scroll.AlignTo(scroll.parent, UIAlignAnchor.TopRight);
                scroll.minValue = 0;
                scroll.value = 0;
                scroll.incrementAmount = 50;

                UISlicedSprite tracSprite = scroll.AddUIComponent<UISlicedSprite>();
                tracSprite.relativePosition = Vector2.zero;
                tracSprite.autoSize = true;
                tracSprite.size = tracSprite.parent.size;
                tracSprite.fillDirection = UIFillDirection.Vertical;
                tracSprite.spriteName = "ScrollbarTrack";

                scroll.trackObject = tracSprite;

                UISlicedSprite thumbSprite = tracSprite.AddUIComponent<UISlicedSprite>();
                thumbSprite.relativePosition = Vector2.zero;
                thumbSprite.fillDirection = UIFillDirection.Vertical;
                thumbSprite.autoSize = true;
                thumbSprite.width = thumbSprite.parent.width - 8;
                thumbSprite.spriteName = "ScrollbarThumb";

                scroll.thumbObject = thumbSprite;

                scrollbar = scroll;
            }
            else if (height <= itemHeight && scrollbar != null)
            {
                DestroyScrollbars(parent);
            }
        }

        public static UIScrollPanel Create(UIScrollablePanel oldPanel, UIVerticalAlignment buttonsAlignment)
        {
            UIScrollPanel scrollPanel = oldPanel.parent.AddUIComponent<UIScrollPanel>();
            scrollPanel.autoLayout = false;
            scrollPanel.autoReset = false;
            scrollPanel.autoSize = false;
            scrollPanel.buttonsAlignment = buttonsAlignment;
            scrollPanel.template = "PlaceableItemTemplate";
            scrollPanel.itemWidth = 109f;
            scrollPanel.itemHeight = 100f;
            scrollPanel.canSelect = true;
            scrollPanel.size = new Vector2(763, 100);
            scrollPanel.relativePosition = new Vector3(48, 5);
            scrollPanel.atlas = oldPanel.atlas;

            scrollPanel.parent.parent.eventSizeChanged += (c, p) =>
            {
                if(scrollPanel.isVisible)
                {
                    scrollPanel.size = new Vector2((int)((p.x - 40f) / scrollPanel.itemWidth) * scrollPanel.itemWidth, (int)(p.y / scrollPanel.itemHeight) * scrollPanel.itemHeight);
                }
            };

            DestroyImmediate(oldPanel);
            DestroyScrollbars(scrollPanel.parent);

            // Left / Right buttons
            UIButton button = scrollPanel.parent.AddUIComponent<UIButton>();
            button.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            button.name = "ArrowLeft";
            button.size = new Vector2(32, 109);
            button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.normalFgSprite = "ArrowLeft";
            button.focusedFgSprite = "ArrowLeftFocused";
            button.hoveredFgSprite = "ArrowLeftHovered";
            button.pressedFgSprite = "ArrowLeftPressed";
            button.disabledFgSprite = "ArrowLeftDisabled";
            button.isEnabled = false;
            button.relativePosition = new Vector3(16, 0);
            scrollPanel.leftArrow = button;

            button = scrollPanel.parent.AddUIComponent<UIButton>();
            button.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            button.name = "ArrowRight";
            button.size = new Vector2(32, 109);
            button.foregroundSpriteMode = UIForegroundSpriteMode.Scale;
            button.horizontalAlignment = UIHorizontalAlignment.Center;
            button.verticalAlignment = UIVerticalAlignment.Middle;
            button.normalFgSprite = "ArrowRight";
            button.focusedFgSprite = "ArrowRightFocused";
            button.hoveredFgSprite = "ArrowRightHovered";
            button.pressedFgSprite = "ArrowRightPressed";
            button.disabledFgSprite = "ArrowRightDisabled";
            button.isEnabled = false;
            button.relativePosition = new Vector3(811, 0);
            scrollPanel.rightArrow = button;

            return scrollPanel;
        }

        private static void DestroyScrollbars(UIComponent parent)
        {
            UIScrollbar[] scrollbars = parent.GetComponentsInChildren<UIScrollbar>();
            foreach (UIScrollbar scrollbar in scrollbars)
            {
                DestroyImmediate(scrollbar);
            }
        }
    }

    public class UIFakeButton : UIButton
    {
        public UIScrollPanelItem.ItemData data;

        public override void Invalidate() { }
    }

    public class UIScrollPanelItem : IUIFastListItem<UIScrollPanelItem.ItemData, UIButton>
    {
        private string m_baseIconName;
        private ItemData oldData;

        private static UIComponent m_tooltipBox;
        private static PreviewRenderer m_previewRenderer;
        private static Texture2D focusedFilterTexture;

        public UIButton item
        {
            get;
            set;
        }

        public class ItemData
        {
            public string name;
            public string tooltip;
            public string baseIconName;
            public UITextureAtlas atlas;
            public UIComponent tooltipBox;
            public bool enabled;
            public UIVerticalAlignment verticalAlignment;
            public object objectUserData;
            public GeneratedScrollPanel panel;
        }

        public void Init()
        {
            item.text = string.Empty;
            item.tooltipAnchor = UITooltipAnchor.Anchored;
            item.tabStrip = true;
            item.horizontalAlignment = UIHorizontalAlignment.Center;
            item.verticalAlignment = UIVerticalAlignment.Middle;
            item.pivot = UIPivotPoint.TopCenter;
            item.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            item.group = item.parent;

            item.eventTooltipShow += (c, p) =>
            {
                if (m_tooltipBox != null && m_tooltipBox.isVisible && m_tooltipBox != p.tooltip)
                {
                    m_tooltipBox.Hide();
                }
                m_tooltipBox = p.tooltip;
            };

            UIComponent uIComponent = (item.childCount <= 0) ? null : item.components[0];
            if (uIComponent != null)
            {
                uIComponent.isVisible = false;
            }
        }

        public void Display(ItemData data, int index)
        {
            if(data == null)
            {
                DebugUtils.Log("Data null");
            }

            if (item == null || data == null) return;

            if(oldData != null)
            {
                oldData.atlas = item.atlas;
            }

            item.name = data.name;
            item.gameObject.GetComponent<TutorialUITag>().tutorialTag = data.name;

            PrefabInfo prefab = data.objectUserData as PrefabInfo;
            if (prefab != null)
            {
                if (prefab.m_Atlas == null || prefab.m_Thumbnail.IsNullOrWhiteSpace())
                {
                    string name = Asset.GetName(prefab);
                    CreateThumbnailAtlas(name, prefab);
                }

                data.baseIconName = prefab.m_Thumbnail;
                if (prefab.m_Atlas != null)
                {
                    data.atlas = prefab.m_Atlas;
                }
            }

            m_baseIconName = data.baseIconName;
            if (data.atlas != null)
            {
                item.atlas = data.atlas;
            }

            item.verticalAlignment = data.verticalAlignment;

            item.normalFgSprite = m_baseIconName;
            item.hoveredFgSprite = m_baseIconName + "Hovered";
            item.pressedFgSprite = m_baseIconName + "Pressed";
            item.disabledFgSprite = m_baseIconName + "Disabled";
            item.focusedFgSprite = null;

            item.isEnabled = data.enabled;
            item.tooltip = data.tooltip;
            item.tooltipBox = data.tooltipBox;
            item.objectUserData = data.objectUserData;
            item.forceZOrder = index;

            if (item.containsMouse)
            {
                item.RefreshTooltip();

                if (m_tooltipBox != null && m_tooltipBox.isVisible && m_tooltipBox != data.tooltipBox)
                {
                    m_tooltipBox.Hide();
                    data.tooltipBox.Show(true);
                    data.tooltipBox.opacity = 1f;
                    data.tooltipBox.relativePosition = m_tooltipBox.relativePosition + new Vector3(0, m_tooltipBox.height - data.tooltipBox.height);
                }

                m_tooltipBox = data.tooltipBox;

                RefreshTooltipAltas(item);
            }

            /*item.Invoke("OnClick", new object[]
		    {
			    p
		    });

            new UIMouseEventParameter(this, UIMouseButton.Left, 1, default(Ray), Vector2.zero, Vector2.zero, 0f*/

            /*if (oldData != null)
            {
                if (oldData.tooltipBox != data.tooltipBox)
                {
                    oldData.tooltipBox.Hide();
                    data.tooltipBox.Show();
                }
            }

            item.RefreshTooltip();*/

            oldData = data;
        }

        public void Select(int index)
        {
            item.normalFgSprite = m_baseIconName + "Focused";
            item.hoveredFgSprite = m_baseIconName + "Focused";
        }

        public void Deselect(int index)
        {
            item.normalFgSprite = m_baseIconName;
            item.hoveredFgSprite = m_baseIconName + "Hovered";
        }

        public static void RefreshTooltipAltas(UIComponent item)
        {
            PrefabInfo prefab = item.objectUserData as PrefabInfo;
            if (prefab != null)
            {
                UISprite uISprite = item.tooltipBox.Find<UISprite>("Sprite");
                if (uISprite != null)
                {
                    if (prefab.m_InfoTooltipAtlas != null)
                    {
                        uISprite.atlas = prefab.m_InfoTooltipAtlas;
                    }
                    if (!string.IsNullOrEmpty(prefab.m_InfoTooltipThumbnail) && uISprite.atlas[prefab.m_InfoTooltipThumbnail] != null)
                    {
                        uISprite.spriteName = prefab.m_InfoTooltipThumbnail;
                    }
                    else
                    {
                        uISprite.spriteName = "ThumbnailBuildingDefault";
                    }
                }
            }
        }

        public static UITextureAtlas CreateThumbnailAtlas(string name, PrefabInfo prefab)
        {
            if (name.IsNullOrWhiteSpace() || prefab == null) return null;

            if (prefab.m_Thumbnail == name) return prefab.m_Atlas;

            //DebugUtils.Log("CreateThumbnail(" + name + ")");

            if(m_previewRenderer == null)
            {
                m_previewRenderer = new GameObject("FindItPreviewRenderer").AddComponent<PreviewRenderer>();
                m_previewRenderer.size = new Vector2(109, 100) * 2f;
            }

            m_previewRenderer.cameraRotation = 210f;
            m_previewRenderer.zoom = 4f;

            bool rendered = false;

            BuildingInfo buildingPrefab = prefab as BuildingInfo;
            if (buildingPrefab != null)
            {
                m_previewRenderer.mesh = buildingPrefab.m_mesh;
                m_previewRenderer.material = buildingPrefab.m_material;

                if (m_previewRenderer.mesh != null)
                {
                    if (buildingPrefab.m_useColorVariations && m_previewRenderer.material != null)
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
                m_previewRenderer.mesh = propPrefab.m_mesh;
                m_previewRenderer.material = propPrefab.m_material;

                if (m_previewRenderer.mesh != null)
                {
                    if (propPrefab.m_useColorVariations && m_previewRenderer.material != null)
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
            
            TreeInfo treePrefab = prefab as TreeInfo;
            if (treePrefab != null)
            {
                m_previewRenderer.mesh = treePrefab.m_mesh;
                m_previewRenderer.material = treePrefab.m_material;

                if (m_previewRenderer.mesh != null)
                {
                    m_previewRenderer.Render();
                    rendered = true;
                }
            }

            if (rendered)
            {
                Texture2D texture = ResourceLoader.ConvertRenderTexture(m_previewRenderer.texture);
                texture.name = name;

                prefab.m_Thumbnail = name;
                prefab.m_Atlas = ResourceLoader.CreateTextureAtlas("FindItThumbnails_" + name, new string[] { }, null);

                ResourceLoader.ResizeTexture(texture, 109, 100);
                ResourceLoader.AddTexturesInAtlas(prefab.m_Atlas, GenerateMissingThumbnailVariants(texture));
            }
            else
            {
                prefab.m_Thumbnail = "ThumbnailBuildingDefault";
            }
            
            return prefab.m_Atlas;
        }

        // Colorize the focused icon blue using the LUT texture
        // Use a border of 8 (256/32) to ensure we don't pick up neighboring patches
        private static Color32 ColorizeFocused(Color32 c)
        {
            if(focusedFilterTexture == null)
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

            ApplyFilter(pixels, newPixels, c => new Color32(0, 0, 0, c.a));
            Texture2D disabledTexture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, false, false);
            disabledTexture.SetPixels32(newPixels);
            disabledTexture.Apply(false);
            disabledTexture.name = baseTexture.name + "Disabled";

            return new Texture2D[]
            {
                baseTexture,
                focusedTexture,
                hoveredTexture,
                pressedTexture,
                disabledTexture
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
