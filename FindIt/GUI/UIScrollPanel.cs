// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using System;
using System.Collections.Generic;
using UnityEngine;

using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.UI;

namespace FindIt.GUI
{
    public class UIScrollPanel : UIFastList<UIScrollPanelItem.ItemData, UIScrollPanelItem, UIButton>
    {
        public FastList<UIScrollPanelItem.ItemData> savedItems;

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

        public static UIScrollPanel Create(UIScrollablePanel oldPanel)
        {
            UIScrollPanel scrollPanel = oldPanel.parent.AddUIComponent<UIScrollPanel>();
            scrollPanel.name = oldPanel.name;
            scrollPanel.autoLayout = false;
            scrollPanel.autoReset = false;
            scrollPanel.autoSize = false;
            scrollPanel.template = "PlaceableItemTemplate";
            scrollPanel.itemWidth = 109f;
            scrollPanel.itemHeight = 100f;
            scrollPanel.canSelect = true;
            scrollPanel.size = new Vector2(763, 100);
            scrollPanel.relativePosition = new Vector3(48, 5);
            scrollPanel.atlas = oldPanel.atlas;

            scrollPanel.parent.parent.eventSizeChanged += (c, p) =>
            {
                if (scrollPanel.isVisible)
                {
                    scrollPanel.size = new Vector2((int)((p.x - 40f) / scrollPanel.itemWidth) * scrollPanel.itemWidth, (int)(p.y / scrollPanel.itemHeight) * scrollPanel.itemHeight);
                    scrollPanel.relativePosition = new Vector3(scrollPanel.relativePosition.x, Mathf.Floor((p.y - scrollPanel.height) / 2));

                    if(scrollPanel.rightArrow != null)
                    {
                        scrollPanel.rightArrow.relativePosition = new Vector3(scrollPanel.relativePosition.x + scrollPanel.width, 0);
                    }
                }
            };

            int zOrder = oldPanel.zOrder;

            DestroyImmediate(oldPanel.gameObject);
            DestroyScrollbars(scrollPanel.parent);

            scrollPanel.zOrder = zOrder;

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
                DestroyImmediate(scrollbar.gameObject);
            }
        }
    }

    public class UIScrollPanelItem : IUIFastListItem<UIScrollPanelItem.ItemData, UIButton>
    {
        private ItemData currentData;
        private UISprite m_tagSprite;
        private UISprite m_steamSprite;

        private UICheckBox m_batchCheckBox;

        private static UIComponent m_tooltipBox;

        public static HashSet<PrefabInfo> fixedFocusedTexture = new HashSet<PrefabInfo>();

        public UIButton component
        {
            get;
            set;
        }

        public class ItemData
        {
            public string name;
            public string tooltip;
            public UIComponent tooltipBox;
            public GeneratedScrollPanel panel;
            public Asset asset;
        }

        public void Init()
        {
            component.text = string.Empty;
            component.tooltipAnchor = UITooltipAnchor.Anchored;
            component.tabStrip = true;
            component.horizontalAlignment = UIHorizontalAlignment.Center;
            component.verticalAlignment = UIVerticalAlignment.Middle;
            component.pivot = UIPivotPoint.TopCenter;
            component.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            component.group = component.parent;

            component.eventTooltipShow += (c, p) =>
            {
                if (m_tooltipBox != null && m_tooltipBox.isVisible && m_tooltipBox != p.tooltip)
                {
                    m_tooltipBox.Hide();
                }
                m_tooltipBox = p.tooltip;
            };

            UIComponent uIComponent = (component.childCount <= 0) ? null : component.components[0];
            if (uIComponent != null)
            {
                uIComponent.isVisible = false;
            }

            m_tagSprite = component.AddUIComponent<UISprite>();
            m_tagSprite.size = new Vector2(20, 16);
            m_tagSprite.atlas = FindIt.atlas;
            m_tagSprite.spriteName = "Tag";
            m_tagSprite.opacity = 0.5f;
            m_tagSprite.tooltipBox = UIView.GetAView().defaultTooltipBox;
            m_tagSprite.relativePosition = new Vector3(component.width - m_tagSprite.width - 5, 5);
            m_tagSprite.isVisible = false;

            m_tagSprite.eventMouseEnter += (c, p) =>
            {
                m_tagSprite.opacity = 1f;
            };

            m_tagSprite.eventMouseLeave += (c, p) =>
            {
                m_tagSprite.opacity = 0.5f;
            };

            m_tagSprite.eventClick += (c, p) =>
            {
                p.Use();

                UITagsWindow.ShowAt(currentData.asset, m_tagSprite);
            };

            

            // batch action check box
            m_batchCheckBox = SamsamTS.UIUtils.CreateCheckBox(component);
            m_batchCheckBox.isChecked = false;
            m_batchCheckBox.isVisible = false;
            m_batchCheckBox.width = 20;
            m_batchCheckBox.relativePosition = new Vector3(5, component.height - m_batchCheckBox.height - 5);
            m_batchCheckBox.eventClicked += (c, i) =>
            {
                if (currentData != null && currentData.asset != null && m_batchCheckBox)
                {
                    if (m_batchCheckBox.isChecked)
                    {
                        UIFilterTag.instance.batchAssetSet.Add(currentData.asset);
                        Debugging.Message("Batch - Add to batch set: " + currentData.asset.name);
                    }
                    else
                    {
                        UIFilterTag.instance.batchAssetSet.Remove(currentData.asset);
                        Debugging.Message("Batch - Remove from batch set: " + currentData.asset.name);
                    }
                }
                
            };

            component.eventMouseEnter += (c, p) =>
            {
                if (currentData != null && currentData.asset != null &&
                    AssetTagList.instance.assets.ContainsValue(currentData.asset))
                {
                    m_tagSprite.isVisible = true;
                }
            };

            component.eventMouseLeave += (c, p) =>
            {
                if (m_tagSprite.isVisible && currentData != null && currentData.asset != null && currentData.asset.tagsCustom.Count == 0)
                {
                    m_tagSprite.isVisible = false;
                }
            };

            m_steamSprite = component.AddUIComponent<UISprite>();
            m_steamSprite.size = new Vector2(26, 16);
            m_steamSprite.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            m_steamSprite.spriteName = "SteamWorkshop";
            m_steamSprite.opacity = 0.05f;
            m_steamSprite.tooltipBox = UIView.GetAView().defaultTooltipBox;
            m_steamSprite.relativePosition = new Vector3(component.width - m_steamSprite.width - 5, component.height - m_steamSprite.height - 5);
            m_steamSprite.isVisible = false;

            if(PlatformService.IsOverlayEnabled())
            {
                m_steamSprite.eventMouseUp += OnTooltipClicked;
            }
        }

        public void Display(ItemData data, int index)
        {
            try
            {
                if (data == null)
                {
                    Debugging.Message("Data null");
                }

                if (component == null || data == null) return;

                currentData = data;

                component.Unfocus();
                component.name = data.name;
                component.gameObject.GetComponent<TutorialUITag>().tutorialTag = data.name;

                PrefabInfo prefab = data.asset.prefab;
                if (prefab == null)
                {
                    Debugging.Message("Couldn't display item. Prefab is null");
                    return;
                }

                ImageUtils.FixThumbnails(prefab, null);

                component.atlas = prefab.m_Atlas;
                component.verticalAlignment = UIVerticalAlignment.Middle;

                component.normalFgSprite = prefab.m_Thumbnail;
                component.hoveredFgSprite = prefab.m_Thumbnail + "Hovered";
                component.pressedFgSprite = prefab.m_Thumbnail + "Pressed";
                component.disabledFgSprite = prefab.m_Thumbnail + "Disabled";
                component.focusedFgSprite = null;

                bool rico = false;

                if (FindIt.isRicoEnabled)
                {
                    string name = Asset.GetName(prefab);
                    if (AssetTagList.instance.assets.ContainsKey(name))
                    {
                        rico = AssetTagList.instance.assets[name].assetType == Asset.AssetType.Rico;
                    }
                }

                component.isEnabled = rico || Settings.unlockAll || ToolsModifierControl.IsUnlocked(prefab.GetUnlockMilestone());
                component.tooltip = data.tooltip;
                component.tooltipBox = data.tooltipBox;
                component.objectUserData = data.asset.prefab;
                component.forceZOrder = index;

                if (component.containsMouse)
                {
                    component.RefreshTooltip();

                    if (m_tooltipBox != null && m_tooltipBox.isVisible && m_tooltipBox != data.tooltipBox)
                    {
                        m_tooltipBox.Hide();
                        data.tooltipBox.Show(true);
                        data.tooltipBox.opacity = 1f;
                        data.tooltipBox.relativePosition = m_tooltipBox.relativePosition + new Vector3(0, m_tooltipBox.height - data.tooltipBox.height);
                    }

                    m_tooltipBox = data.tooltipBox;

                    RefreshTooltipAltas(component);
                }

                if(m_tagSprite != null)
                {
                    m_tagSprite.atlas = FindIt.atlas;

                    m_tagSprite.isVisible = currentData.asset != null && AssetTagList.instance.assets.ContainsValue(currentData.asset) && (component.containsMouse || currentData.asset.tagsCustom.Count > 0);
                }

                // batch action check box
                if (m_batchCheckBox != null)
                {
                    if (UIFilterTag.instance.batchAssetSet.Contains(data.asset))
                    {
                        m_batchCheckBox.isChecked = true;
                    }
                    else
                    {
                        m_batchCheckBox.isChecked = false;
                    }

                    m_batchCheckBox.isVisible = UISearchBox.instance.tagPanel.isBatchActionsEnabled;
                }

                if (m_steamSprite != null)
                {
                    m_steamSprite.tooltip = null;

                    if (data.asset != null)
                    {
                        m_steamSprite.isVisible = data.asset.steamID != 0;
                        if (!data.asset.author.IsNullOrWhiteSpace())
                        {
                            m_steamSprite.tooltip = "By " + data.asset.author + "\n" + Translations.Translate("FIF_UIS_WS");
                        }
                    }

                    if (m_steamSprite.containsMouse)
                    {
                        m_steamSprite.tooltipBox.isVisible = m_steamSprite.tooltip != null;
                    }

                    m_steamSprite.isVisible = m_steamSprite.tooltip != null;

                    if (m_steamSprite.containsMouse)
                    {
                        m_steamSprite.RefreshTooltip();
                        m_steamSprite.tooltipBox.isVisible = m_steamSprite.tooltip != null;
                    }
                }
            }
            catch (Exception e)
            {
                if (data != null)
                {
                    Debugging.Message("Display failed : " + data.name);
                }
                else
                {
                    Debugging.Message("Display failed");
                }
                Debugging.LogException(e);
            }
        }

        public void Select(int index)
        {
            try
            {
                if (currentData != null && currentData.asset != null && currentData.asset.prefab != null && !fixedFocusedTexture.Contains(currentData.asset.prefab))
                {
                    if (ImageUtils.FixFocusedTexture(currentData.asset.prefab))
                    {
                        Debugging.Message("Fixed focused texture: " + currentData.asset.prefab.name);
                    }
                    fixedFocusedTexture.Add(currentData.asset.prefab);
                }

                component.normalFgSprite = currentData.asset.prefab.m_Thumbnail + "Focused";
                component.hoveredFgSprite = currentData.asset.prefab.m_Thumbnail + "Focused";
            }
            catch (Exception e)
            {
                if (currentData != null)
                {
                    Debugging.Message("Select failed : " + currentData.name);
                }
                else
                {
                    Debugging.Message("Select failed");
                }
                Debugging.LogException(e);
            }
        }

        public void Deselect(int index)
        {
            try
            {
                component.normalFgSprite = currentData.asset.prefab.m_Thumbnail;
                component.hoveredFgSprite = currentData.asset.prefab.m_Thumbnail + "Hovered";
            }
            catch (Exception e)
            {
                if (currentData != null)
                {
                    Debugging.Message("Deselect failed : " + currentData.name);
                }
                else
                {
                    Debugging.Message("Deselect failed");
                }
                Debugging.LogException(e);
            }
        }

        private void OnTooltipClicked(UIComponent c, UIMouseEventParameter p)
        {
            if (!p.used && p.buttons == UIMouseButton.Right && currentData != null && currentData.asset != null)
            {
                PublishedFileId publishedFileId = new PublishedFileId(currentData.asset.steamID);

                if (publishedFileId != PublishedFileId.invalid)
                {
                    PlatformService.ActivateGameOverlayToWorkshopItem(publishedFileId);
                    p.Use();
                }
            }
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
    }
}
