using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using UnityEngine;

using FindIt.Redirection;
using FindIt.GUI;

namespace FindIt.Detours
{
    /*[TargetType(typeof(UIView))]
    public class UIViewDetour : UIView
    {
        [RedirectMethod]
        private void OnResolutionChanged(Vector2 oldSize, Vector2 currentSize)
        {
            //this.m_CachedScreenResolution = currentSize;
            typeof(UIView).GetField("m_CachedScreenResolution", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, currentSize);

            UIUtils.NeedHalfPixelOffset(this.m_PreserveSizes, true);
            float aspect = this.m_UICamera.aspect;
            float x = oldSize.y * aspect;
            float x2 = currentSize.y * aspect;
            Vector2 previousResolution = new Vector2(x, oldSize.y);
            Vector2 currentResolution = new Vector2(x2, currentSize.y);
            UIComponent[] componentsInChildren = base.GetComponentsInChildren<UIComponent>();
            Array.Sort<UIComponent>(componentsInChildren, new Comparison<UIComponent>(this.RenderSortFunc));
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                if (this.m_PreserveSizes && componentsInChildren[i].parent == null)
                {
                    componentsInChildren[i].MakePixelPerfect();
                }
                try
                {
                    componentsInChildren[i].GetType().GetMethod("OnResolutionChanged", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(componentsInChildren[i], new object[] { previousResolution, currentResolution });
                    //componentsInChildren[i].OnResolutionChanged(previousResolution, currentResolution);
                }
                catch(Exception e)
                {
                    DebugUtils.Log(componentsInChildren[i].name + " " + componentsInChildren[i].GetType());
                }
            }
            for (int j = 0; j < componentsInChildren.Length; j++)
            {
                try
                {
                    componentsInChildren[j].PerformLayout();
                }
                catch (Exception e)
                {
                    DebugUtils.Log(componentsInChildren[j].name + " " + componentsInChildren[j].GetType());
                }
            }
        }

        private int RenderSortFunc(UIComponent lhs, UIComponent rhs)
        {
            return lhs.renderOrder.CompareTo(rhs.renderOrder);
        }
    }*/

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
                    bool m_UIFilterInitialized = (bool)typeof(GeneratedScrollPanel).GetField("m_UIFilterInitialized", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
                    if (!m_UIFilterInitialized)
                    {
                        UIPanel m_UIFilterPanel = (UIPanel)typeof(GeneratedScrollPanel).GetField("m_UIFilterPanel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
                        if (m_UIFilterPanel != null)
                        {
                            InitializeUIAssetFilters();
                        }
                        typeof(GeneratedScrollPanel).GetField("m_UIFilterInitialized", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, true);
                    }

                    this.OnShow();

                    UIScrollPanel scrollPanel = GetComponentInChildren<UIScrollPanel>();
                    if (scrollPanel != null)
                    {
                        // If nothing selected then select the first item
                        if(scrollPanel.selectedItem == null && scrollPanel.itemsData.m_size > 0 && (scrollPanel.itemsData.m_buffer[0].enabled || FindIt.unlockAll.value))
                        {
                            scrollPanel.selectedItem = scrollPanel.itemsData.m_buffer[0];
                        }

                        // Simulate item click
                        if(scrollPanel.selectedItem != null)
                        {
                            UIScrollPanelItem.ItemData item = scrollPanel.selectedItem;

                            UIScrollPanelItem panelItem = scrollPanel.GetItem(0);
                            panelItem.Display(scrollPanel.selectedItem, 0);
                            panelItem.component.SimulateClick();

                            scrollPanel.selectedItem = item;
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
        
        private void InitializeUIAssetFilters()
        {
            UIScrollPanel scrollPanel = GetComponentInChildren<UIScrollPanel>();

            if (scrollPanel == null)
            {
                DebugUtils.Warning("Couldn't find scrollPanel");
                return;
            }
            scrollPanel.savedItems = scrollPanel.itemsData;
            scrollPanel.itemsData = new FastList<UIScrollPanelItem.ItemData>();

            IList m_AssetsWithoutFilter = (IList)typeof(GeneratedScrollPanel).GetField("m_AssetsWithoutFilter", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            IList m_UIFilterTypes = (IList)typeof(GeneratedScrollPanel).GetField("m_UIFilterTypes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

            MethodInfo GetFilterMask = typeof(GeneratedScrollPanel).GetMethod("GetFilterMask", BindingFlags.Instance | BindingFlags.NonPublic);

            m_AssetsWithoutFilter.Clear();
            int[] array = new int[m_UIFilterTypes.Count];
            for (int i = 0; i < scrollPanel.savedItems.m_size; i++)
            {
                UIScrollPanelItem.ItemData item = scrollPanel.savedItems.m_buffer[i];
                if (item.objectUserData != null)
                {
                    bool[] filterMask = (bool[])GetFilterMask.Invoke(this, new object[] { item.objectUserData });
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (filterMask[j])
                        {
                            array[j]++;
                        }
                    }
                }
            }
            int num = 0;
            bool[] array2 = new bool[m_UIFilterTypes.Count];
            for (int j = 0; j < array.Length; j++)
            {
                if (array[j] > 0 && array[j] < scrollPanel.savedItems.m_size - 1)
                {
                    array2[j] = true;
                    num++;
                }
                else
                {
                    array2[j] = false;
                }
            }
            for (int i = 0; i < scrollPanel.savedItems.m_size; i++)
            {
                UIScrollPanelItem.ItemData item = scrollPanel.savedItems.m_buffer[i];
                if (item.objectUserData != null)
                {
                    bool flag = false;
                    for (int k = 0; k < array.Length; k++)
                    {
                        MethodInfo FilterApplies = m_UIFilterTypes[k].GetType().GetMethod("FilterApplies", BindingFlags.Instance | BindingFlags.Public);
                        if (array2[k] && (bool)FilterApplies.Invoke(m_UIFilterTypes[k], new object[] { item.objectUserData }))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        m_AssetsWithoutFilter.Add(item.objectUserData);
                    }
                }
            }
            array2[array2.Length - 1] = (m_AssetsWithoutFilter.Count > 0);
            if (m_AssetsWithoutFilter.Count > 0)
            {
                num++;
            }
            if (num < 2)
            {
                for (int l = 0; l < array.Length; l++)
                {
                    array2[l] = false;
                }
            }
            int m = 0;
            int num2 = 0;
            UIPanel m_UIFilterPanel = (UIPanel)typeof(GeneratedScrollPanel).GetField("m_UIFilterPanel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            while (m < m_UIFilterPanel.components.Count)
            {
                UIMultiStateButton uIMultiStateButton = m_UIFilterPanel.components[m] as UIMultiStateButton;
                if (uIMultiStateButton != null)
                {
                    uIMultiStateButton.isVisible = array2[num2];
                    if (m < m_UIFilterPanel.components.Count - 1)
                    {
                        UIPanel uIPanel = m_UIFilterPanel.components[m + 1] as UIPanel;
                        if (uIPanel != null)
                        {
                            bool isVisible = false;
                            string name = uIMultiStateButton.name;
                            for (int n = m; n >= 0; n--)
                            {
                                UIMultiStateButton uIMultiStateButton2 = m_UIFilterPanel.components[n] as UIMultiStateButton;
                                if (uIMultiStateButton2 != null)
                                {
                                    if (uIMultiStateButton2.name != name)
                                    {
                                        break;
                                    }
                                    if (uIMultiStateButton2.name == name && uIMultiStateButton2.isVisible)
                                    {
                                        isVisible = true;
                                        break;
                                    }
                                }
                            }
                            uIPanel.isVisible = isVisible;
                        }
                    }
                    num2++;
                }
                m++;
            }
            this.ApplyUIAssetFilter();
        }

        [RedirectMethod]
        private void ApplyUIAssetFilter()
        {
            UIScrollPanel scrollPanel = GetComponentInChildren<UIScrollPanel>();

            if (scrollPanel == null || scrollPanel.savedItems == null)
            {
                DebugUtils.Warning("Couldn't find scrollPanel");
                return;
            }

            UIScrollPanelItem.ItemData item = scrollPanel.selectedItem;
            scrollPanel.Clear();
            scrollPanel.selectedItem = item;

            MethodInfo ShouldAssetBeVisible = typeof(GeneratedScrollPanel).GetMethod("ShouldAssetBeVisible", BindingFlags.Instance | BindingFlags.NonPublic);

            for (int i = 0; i < scrollPanel.savedItems.m_size; i++)
            {
                UIScrollPanelItem.ItemData data = scrollPanel.savedItems.m_buffer[i];
                if (data != null && data.objectUserData != null)
                {
                    if ((bool)ShouldAssetBeVisible.Invoke(this, new object[] { data.objectUserData }))
                    {
                        scrollPanel.itemsData.Add(data);
                    }
                }
            }

            scrollPanel.DisplayAt(0);
        }

        public override ItemClass.Service service
        {
            get { throw new NotImplementedException(); }
        }
    }
}
