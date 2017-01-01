using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

namespace FindIt.GUI
{
    public class UIScrollPanel : UIHorizontalFastList<UIScrollPanelItem.ItemData, UIScrollPanelItem, UIButton>{ }

    public class UIFakeButton : UIButton
    {
        public UIScrollPanelItem.ItemData data;

        public override void Update()
        {
            base.Update();
            data.objectUserData = objectUserData;
            Destroy(gameObject);
        }
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

            item.isEnabled = data.enabled;
            item.tooltip = data.tooltip;
            item.tooltipBox = data.tooltipBox;
            item.objectUserData = data.objectUserData;
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
