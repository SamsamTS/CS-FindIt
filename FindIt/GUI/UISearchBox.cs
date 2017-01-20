using System;

using UnityEngine;

using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.UI;

using System.Collections.Generic;

namespace FindIt.GUI
{
    public class UISearchBox : UIPanel
    {
        public UIPanel inputPanel;
        public UITextField input;
        public UIScrollPanel scrollPanel;
        public UIButton searchButton;
        public UIPanel filterPanel;
        public UIDropDown typeFilter;

        public override void Start()
        {
            inputPanel = AddUIComponent<UIPanel>();
            inputPanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            inputPanel.backgroundSprite = "GenericTab";
            inputPanel.size = new Vector2(300, 40);
            inputPanel.relativePosition = new Vector2(0, 0);

            input = SamsamTS.UIUtils.CreateTextField(inputPanel);
            input.size = new Vector2(inputPanel.width - 45, 30);
            input.padding.top = 7;
            input.relativePosition = new Vector3(5, 5);

            input.eventTextChanged += (c, p) => Search();

            searchButton = inputPanel.AddUIComponent<UIButton>();
            searchButton.size = new Vector2(43, 49);
            searchButton.atlas = FindIt.instance.m_mainButton.atlas;
            searchButton.normalFgSprite = "FindIt";
            searchButton.hoveredFgSprite = "FindItFocused";
            searchButton.pressedFgSprite = "FindItPressed";
            searchButton.relativePosition = new Vector3(inputPanel.width - 41, -3);

            searchButton.eventClick += (c, p) =>
            {
                input.Focus();
                input.SelectAll();
            };

            filterPanel = AddUIComponent<UIPanel>();
            filterPanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            filterPanel.backgroundSprite = "GenericTab";
            filterPanel.color = new Color32(196, 200, 206, 255);
            filterPanel.size = new Vector2(105, 35);
            filterPanel.SendToBack();
            filterPanel.relativePosition = inputPanel.relativePosition + new Vector3(inputPanel.width - 5, 5);

            typeFilter = SamsamTS.UIUtils.CreateDropDown(filterPanel);
            typeFilter.size = new Vector2(90, 25);
            typeFilter.relativePosition = new Vector3(10, 5);
            typeFilter.listPosition = UIDropDown.PopupListPosition.Above;

            typeFilter.items = Enum.GetNames(typeof(Asset.AssetType));
            typeFilter.selectedIndex = 0;

            typeFilter.eventSelectedIndexChanged += (c, p) => Search();

            size = Vector2.zero;
            autoSize = true;
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (input != null && !isVisible)
            {
                input.Unfocus();
            }

        }

        public void Search()
        {
            PrefabInfo current = null;
            int selected = -1;
            if (scrollPanel.selectedItem != null)
            {
                current = scrollPanel.selectedItem.objectUserData as PrefabInfo;
            }

            string text ="";
            Asset.AssetType type = Asset.AssetType.All;

            if (input != null)
            {
                text = input.text;
                type = (Asset.AssetType)typeFilter.selectedIndex;
            }

            List<Asset> matches = AssetTagList.instance.Find(text, type);
            scrollPanel.Clear();
            foreach (Asset asset in matches)
            {
                if (asset.prefab != null)
                {
                    UIScrollPanelItem.ItemData data = new UIScrollPanelItem.ItemData();
                    data.name = asset.title;
                    data.tooltip = Asset.GetLocalizedTooltip(asset.prefab, data.name);

                    data.atlas = asset.prefab.m_Atlas;
                    if (data.atlas == null)
                    {
                        data.atlas = scrollPanel.atlas;
                    }

                    data.baseIconName = asset.prefab.m_Thumbnail;

                    data.tooltipBox = GeneratedPanel.GetTooltipBox(TooltipHelper.GetHashCode(data.tooltip));
                    data.enabled = ToolsModifierControl.IsUnlocked(asset.prefab.GetUnlockMilestone()) || asset.assetType == Asset.AssetType.Rico;
                    data.verticalAlignment = scrollPanel.buttonsAlignment;
                    data.objectUserData = asset.prefab;
                    data.asset = asset;

                    scrollPanel.itemsData.Add(data);

                    if (asset.prefab == current)
                    {
                        selected = scrollPanel.itemsData.m_size - 1;
                    }
                }
            }

            scrollPanel.DisplayAt(0);
            scrollPanel.selectedIndex = selected;

            if (scrollPanel.selectedItem != null)
            {
                FindIt.SelectPrefab(scrollPanel.selectedItem.objectUserData as PrefabInfo);
            }
            else
            {
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }
    }
}
