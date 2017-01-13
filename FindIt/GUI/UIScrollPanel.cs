using UnityEngine;

using ColossalFramework.UI;

namespace FindIt.GUI
{
    public class UIScrollPanel : UIHorizontalFastList<UIScrollPanelItem.ItemData, UIScrollPanelItem, UIButton>
    {
        public UIVerticalAlignment buttonsAlignment;

        public UIPanel blocker;

        public override void Start()
        {
            base.Start();

            blocker = AddUIComponent<UIPanel>();
            blocker.size = size;
            blocker.relativePosition = Vector3.zero;
            blocker.SendToBack();

            Refresh();
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            if (blocker != null)
            {
                blocker.size = size;
            }

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
            if (data.atlas != null)
            {
                item.atlas = data.atlas;
            }
            m_baseIconName = data.baseIconName;
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

            if (data.enabled)
            {
                item.BringToFront();
            }
            else
            {
                item.SendToBack();
            }

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
    }
}
