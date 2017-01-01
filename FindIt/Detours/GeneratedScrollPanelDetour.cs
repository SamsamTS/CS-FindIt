using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using FindIt.Redirection;
using FindIt.GUI;

namespace FindIt.Detours
{
    [TargetType(typeof(GeneratedScrollPanel))]
    public class GeneratedScrollPanelDetour : GeneratedScrollPanel
    {
        [RedirectMethod]
        new protected UIButton CreateButton(string name, string tooltip, string baseIconName, int index, UITextureAtlas atlas, UIComponent tooltipBox, bool enabled)
        {
            UIScrollPanel scrollPanel = GetComponentInChildren<UIScrollPanel>();

            if (scrollPanel == null)
            {
                UIScrollablePanel oldPanel = GetComponentInChildren<UIScrollablePanel>();

                scrollPanel = oldPanel.parent.AddUIComponent<UIScrollPanel>();
                scrollPanel.template = "PlaceableItemTemplate";
                scrollPanel.itemWidth = 109f;
                scrollPanel.canSelect = true;
                scrollPanel.size = new Vector2(763, 100);
                scrollPanel.relativePosition = new Vector3(48, 5);
                scrollPanel.atlas = oldPanel.atlas;

                FieldInfo m_ScrollablePanel = typeof(GeneratedScrollPanel).GetField("m_ScrollablePanel", BindingFlags.Instance | BindingFlags.NonPublic);
                m_ScrollablePanel.SetValue(this, scrollPanel as UIScrollablePanel);

                Destroy(oldPanel);

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
            }

            if (atlas == null)
            {
                atlas = scrollPanel.atlas;
            }

            UIScrollPanelItem.ItemData data = null;

            if (index >= 0 && index < scrollPanel.itemsData.m_size)
            {
                data = scrollPanel.itemsData[index];
            }
            else
            {
                for (int i = 0; i < scrollPanel.itemsData.m_size; i++)
                {
                    if (scrollPanel.itemsData[i].name == name)
                    {
                        data = scrollPanel.itemsData[i];
                    }
                }
            }

            if(data == null)
            {
                data = new UIScrollPanelItem.ItemData();
                scrollPanel.itemsData.Add(data);
            }

            data.name = name;
            data.tooltip = tooltip;
            data.baseIconName = baseIconName;
            data.atlas = atlas;
            data.tooltipBox = tooltipBox;
            data.enabled = enabled;
            data.verticalAlignment = this.buttonsAlignment;
            scrollPanel.DisplayAt(0);

            UIFakeButton uiButton = component.AddUIComponent<UIFakeButton>();
            uiButton.data = data;

            return uiButton;
        }

        [RedirectMethod]
        new protected void OnVisibilityChanged(UIComponent comp, bool isVisible)
        {
            if (base.component.isVisibleSelf)
            {
                if (isVisible)
                {
                    this.OnShow();

                    UIScrollPanel scrollPanel = GetComponentInChildren<UIScrollPanel>();
                    if (scrollPanel != null)
                    {
                        if (scrollPanel.selectedIndex < 0) scrollPanel.selectedIndex = 0;

                        UIScrollPanelItem panelItem = scrollPanel.GetItem(0);
                        panelItem.Display(scrollPanel.selectedItem, scrollPanel.selectedIndex);

                        if (!panelItem.item.isEnabled)
                        {
                            scrollPanel.selectedIndex = -1;

                            BuildingTool buildingTool = ToolsModifierControl.GetTool<BuildingTool>();
                            if (buildingTool != null)
                            {
                                buildingTool.m_prefab = null;
                            }

                            NetTool netTool = ToolsModifierControl.GetTool<NetTool>();
                            if (buildingTool != null)
                            {
                                netTool.m_prefab = null;
                            }
                        }
                        else
                        {
                            int index = scrollPanel.selectedIndex;
                            panelItem.item.SimulateClick();
                            scrollPanel.selectedIndex = index;
                        }

                        scrollPanel.Refresh();
                    }
                }
                else
                {
                    this.OnHide();
                    if (!GeneratedPanel.m_IsRefreshing && ToolsModifierControl.toolController != null && ToolsModifierControl.toolController.CurrentTool != ToolsModifierControl.GetTool<BulldozeTool>())
                    {
                        ToolsModifierControl.SetTool<DefaultTool>();
                    }
                }
            }
        }

        [RedirectMethod]
        private void ShowSelectedIndex()
        { }

        [RedirectMethod]
        private void SelectByIndex(int value)
        { }

        public override ItemClass.Service service
        {
            get { throw new NotImplementedException(); }
        } 
    }
}
