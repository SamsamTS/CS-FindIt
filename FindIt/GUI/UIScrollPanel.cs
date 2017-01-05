using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

namespace FindIt.GUI
{
    public class UIScrollPanel : UIHorizontalFastList<UIScrollPanelItem.ItemData, UIScrollPanelItem, UIButton>
    {
        public UIVerticalAlignment buttonsAlignment;

        public static UIScrollPanel Create(UIScrollablePanel oldPanel, UIVerticalAlignment buttonsAlignment)
        {
            UIScrollPanel scrollPanel = oldPanel.parent.AddUIComponent<UIScrollPanel>();
            scrollPanel.buttonsAlignment = buttonsAlignment;
            scrollPanel.template = "PlaceableItemTemplate";
            scrollPanel.itemWidth = 109f;
            scrollPanel.canSelect = true;
            scrollPanel.size = new Vector2(763, 100);
            scrollPanel.relativePosition = new Vector3(48, 5);
            scrollPanel.atlas = oldPanel.atlas;

            DestroyImmediate(oldPanel);

            UIButton button = scrollPanel.parent.AddUIComponent<UIButton>();
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
            scrollPanel.LeftArrow = button;

            button = scrollPanel.parent.AddUIComponent<UIButton>();
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
            scrollPanel.RightArrow = button;

            return scrollPanel;
        }
    }

    public class UIFakeButton : UIButton
    {
        public UIScrollPanelItem.ItemData data;
    }

    public class UIScrollPanelItem : IUIFastListItem<UIScrollPanelItem.ItemData, UIButton>
    {
        private string m_baseIconName;

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

            UIComponent uIComponent = (item.childCount <= 0) ? null : item.components[0];
            if (uIComponent != null)
            {
                uIComponent.isVisible = false;
            }
        }

        public void Display(ItemData data, int index)
        {
            if (item == null) return;

            item.isVisible = false;

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

            item.isVisible = true;
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
    }
}
