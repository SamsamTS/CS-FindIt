using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using FindIt.Redirection;
using FindIt.GUI;

namespace FindIt.Detours
{
    public class UIComponentDetour : UIComponent
    {
        private static MethodInfo from = typeof(UIComponent).GetMethod("OnResolutionChanged", BindingFlags.NonPublic | BindingFlags.Instance);
        private static MethodInfo to = typeof(UIComponentDetour).GetMethod("OnResolutionChanged", BindingFlags.NonPublic | BindingFlags.Instance);

        private static RedirectCallsState m_state;
        private static bool m_deployed = false;

        public static void Deploy()
        {
            if (!m_deployed)
            {
                m_state = RedirectionHelper.RedirectCalls(from, to);
                m_deployed = true;
            }
        }
        
        public static void Revert()
        {
            if (m_deployed)
            {
                RedirectionHelper.RevertRedirect(from, m_state);
                m_deployed = false;
            }
        }

        protected override void OnResolutionChanged(Vector2 previousResolution, Vector2 currentResolution)
        {
            if (m_Layout == null)
            {
                UIAnchorStyle anchor = this.anchor;
            }

            RedirectionHelper.RevertRedirect(from, m_state);
            base.OnResolutionChanged(previousResolution, currentResolution);
            m_state = RedirectionHelper.RedirectCalls(from, to);
        }
    }

    [TargetType(typeof(GeneratedScrollPanel))]
    public class GeneratedScrollPanelDetour : GeneratedScrollPanel
    {
        public static List<UIFakeButton> fakeButtons;
        public static object lockObject = new object();

        [RedirectMethod]
        new protected UIButton CreateButton(string name, string tooltip, string baseIconName, int index, UITextureAtlas atlas, UIComponent tooltipBox, bool enabled)
        {
            UIScrollPanel scrollPanel = GetComponentInChildren<UIScrollPanel>();

            if (scrollPanel == null)
            {
                UIScrollablePanel oldPanel = GetComponentInChildren<UIScrollablePanel>();

                scrollPanel = UIScrollPanel.Create(oldPanel, buttonsAlignment);

                FieldInfo m_ScrollablePanel = typeof(GeneratedScrollPanel).GetField("m_ScrollablePanel", BindingFlags.Instance | BindingFlags.NonPublic);
                m_ScrollablePanel.SetValue(this, scrollPanel as UIScrollablePanel);
            }

            if (atlas == null)
            {
                atlas = UIView.GetAView().defaultAtlas;
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

            if (data == null)
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
            data.panel = this;

            scrollPanel.DisplayAt(0);

            UIFakeButton fakeButton = new UIFakeButton();
            fakeButton.data = data;
            fakeButton.atlas = data.atlas;

            lock (lockObject)
            {
                if (fakeButtons == null)
                {
                    fakeButtons = new List<UIFakeButton>();
                    SimulationManager.instance.AddAction(() =>
                    {
                        lock (lockObject)
                        {
                            foreach (UIFakeButton button in fakeButtons)
                            {
                                button.data.objectUserData = button.objectUserData;
                                button.data.atlas = button.atlas;

                                if (button.objectUserData is PrefabInfo)
                                {
                                    string key = Asset.GetName(button.objectUserData as PrefabInfo);
                                    if (AssetTagList.instance.assets.ContainsKey(key))
                                    {
                                        if (AssetTagList.instance.assets[key].onButtonClicked == null)
                                        {
                                            MethodInfo onButtonClicked = button.data.panel.GetType().GetMethod("OnButtonClicked", BindingFlags.NonPublic | BindingFlags.Instance);
                                            AssetTagList.instance.assets[key].onButtonClicked = Delegate.CreateDelegate(typeof(Asset.OnButtonClicked), button.data.panel, onButtonClicked, false) as Asset.OnButtonClicked;
                                        }

                                        button.data.asset = AssetTagList.instance.assets[key];
                                    }
                                }
                            }
                            fakeButtons = null;
                        }
                    });
                }
                fakeButtons.Add(fakeButton);
            }

            return fakeButton;
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
