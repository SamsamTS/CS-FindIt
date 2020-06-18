﻿using System;
using System.Diagnostics;

using UnityEngine;

using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.UI;

using System.Collections.Generic;

namespace FindIt.GUI
{
    public class UISearchBox : UIPanel
    {
        public static UISearchBox instance;

        public UIPanel inputPanel;
        public UITextField input;
        public UIScrollPanel scrollPanel;
        public UIButton searchButton;
        public UIPanel filterPanel;
        public UIDropDown typeFilter;
        public UIPanel buildingFilters;
        public UIDropDown levelFilter;
        public UIDropDown sizeFilterX;
        public UIDropDown sizeFilterY;
        public UIFilterGrowable filterGrowable;
        public UIFilterPloppable filterPloppable;

        public ItemClass.Level buildingLevel
        {
            get { return (ItemClass.Level)(levelFilter.selectedIndex - 1); }
        }

        public Vector2 buildingSize
        {
            get
            {
                if (sizeFilterX.selectedIndex == 0) return Vector2.zero;
                return new Vector2(sizeFilterX.selectedIndex, sizeFilterY.selectedIndex + 1);
            }
        }

        public override void Start()
        {
            instance = this;

            inputPanel = AddUIComponent<UIPanel>();
            inputPanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            inputPanel.backgroundSprite = "GenericTab";
            inputPanel.size = new Vector2(300, 40);
            inputPanel.relativePosition = new Vector2(5, -inputPanel.height - 40);

            input = SamsamTS.UIUtils.CreateTextField(inputPanel);
            input.size = new Vector2(inputPanel.width - 45, 30);
            input.padding.top = 7;
            input.relativePosition = new Vector3(5, 5);

            string search = null;
            input.eventTextChanged += (c, p) =>
            {
                search = p;
                Search();
            };

            input.eventTextCancelled += (c, p) =>
            {
                input.text = search;
            };

            searchButton = inputPanel.AddUIComponent<UIButton>();
            searchButton.size = new Vector2(43, 49);
            searchButton.atlas = FindIt.instance.mainButton.atlas;
            searchButton.playAudioEvents = true;
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
            filterPanel.relativePosition = new Vector3(inputPanel.width, -filterPanel.height - 40);

            typeFilter = SamsamTS.UIUtils.CreateDropDown(filterPanel);
            typeFilter.size = new Vector2(130, 25);
            typeFilter.relativePosition = new Vector3(10, 5);

            if (FindIt.isRicoEnabled)
            {
                string[] items = {
                    "All",
                    "Net Structures",
                    "Ploppable",
                    "Growable",
                    "Rico",
                    "Prop",
                    "Decal",
                    "Tree"
                };
                typeFilter.items = items;

            }
            else
            {
                string[] items = {
                    "All",
                    "Net Structures",
                    "Ploppable",
                    "Growable",
                    "Prop",
                    "Decal",
                    "Tree"
                };
                typeFilter.items = items;
            }
            typeFilter.selectedIndex = 0;

            typeFilter.eventSelectedIndexChanged += (c, p) =>
            {
                UpdateFilterPanels();
                Search();
            };
            
            buildingFilters = filterPanel.AddUIComponent<UIPanel>();
            buildingFilters.size = new Vector2(90, 35);
            buildingFilters.relativePosition = new Vector3(typeFilter.relativePosition.x + typeFilter.width, 0);

            // Level
            UILabel levelLabel = buildingFilters.AddUIComponent<UILabel>();
            levelLabel.textScale = 0.8f;
            levelLabel.padding = new RectOffset(0, 0, 8, 0);
            levelLabel.text = "Level: ";
            levelLabel.relativePosition = new Vector3(10, 5);

            levelFilter = SamsamTS.UIUtils.CreateDropDown(buildingFilters);
            levelFilter.size = new Vector2(55, 25);
            levelFilter.AddItem("All");
            levelFilter.AddItem("1");
            levelFilter.AddItem("2");
            levelFilter.AddItem("3");
            levelFilter.AddItem("4");
            levelFilter.AddItem("5");
            levelFilter.selectedIndex = 0;
            levelFilter.relativePosition = new Vector3(levelLabel.relativePosition.x + levelLabel.width + 5, 5);

            levelFilter.eventSelectedIndexChanged += (c, i) => Search();

            // Size
            UILabel sizeLabel = buildingFilters.AddUIComponent<UILabel>();
            sizeLabel.textScale = 0.8f;
            sizeLabel.padding = new RectOffset(0, 0, 8, 0);
            sizeLabel.text = "Size: ";
            sizeLabel.relativePosition = new Vector3(levelFilter.relativePosition.x + levelFilter.width + 10, 5);

            sizeFilterX = SamsamTS.UIUtils.CreateDropDown(buildingFilters);
            sizeFilterX.size = new Vector2(55, 25);
            sizeFilterX.AddItem("All");
            sizeFilterX.AddItem("1");
            sizeFilterX.AddItem("2");
            sizeFilterX.AddItem("3");
            sizeFilterX.AddItem("4");
            sizeFilterX.selectedIndex = 0;
            sizeFilterX.relativePosition = new Vector3(sizeLabel.relativePosition.x + sizeLabel.width + 5, 5);

            UILabel XLabel = buildingFilters.AddUIComponent<UILabel>();
            XLabel.textScale = 0.8f;
            XLabel.padding = new RectOffset(0, 0, 8, 0);
            XLabel.text = "X";
            XLabel.isVisible = false;
            XLabel.relativePosition = new Vector3(sizeFilterX.relativePosition.x + sizeFilterX.width - 5, 5);

            sizeFilterY = SamsamTS.UIUtils.CreateDropDown(buildingFilters);
            sizeFilterY.size = new Vector2(45, 25);
            sizeFilterY.AddItem("1");
            sizeFilterY.AddItem("2");
            sizeFilterY.AddItem("3");
            sizeFilterY.AddItem("4");
            sizeFilterY.selectedIndex = 0;
            sizeFilterY.isVisible = false;
            sizeFilterY.relativePosition = new Vector3(XLabel.relativePosition.x + XLabel.width + 5, 5);

            sizeFilterX.eventSelectedIndexChanged += (c, i) =>
            {
                if (i == 0)
                {
                    sizeFilterX.width = 55;
                    XLabel.isVisible = false;
                    sizeFilterY.isVisible = false;
                }
                else
                {
                    sizeFilterX.width = 45;
                    XLabel.isVisible = true;
                    sizeFilterY.isVisible = true;
                }

                UpdateBuildingFilters();

                Search();
            };

            sizeFilterY.eventSelectedIndexChanged += (c, i) => Search();

            UIPanel panel = AddUIComponent<UIPanel>();
            panel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            panel.backgroundSprite = "GenericTabHovered";
            panel.size = new Vector2(parent.width, 45);
            panel.relativePosition = new Vector3(0, -panel.height + 5);

            filterPloppable = panel.AddUIComponent<UIFilterPloppable>();
            filterPloppable.isVisible = false;
            filterPloppable.relativePosition = new Vector3(0, 0);

            filterPloppable.eventFilteringChanged += (c,p) => Search();

            filterGrowable = panel.AddUIComponent<UIFilterGrowable>();
            filterGrowable.isVisible = false;
            filterGrowable.relativePosition = new Vector3(0, 0);

            filterGrowable.eventFilteringChanged += (c, p) => Search();

            UpdateFilterPanels();

            size = Vector2.zero;
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (input != null && !isVisible)
            {
                input.Unfocus();
            }
        }

        public void UpdateBuildingFilters()
        {
            if(buildingFilters.isVisible)
            {
                if (sizeFilterY.isVisible)
                {
                    buildingFilters.width = sizeFilterY.relativePosition.x + sizeFilterY.width;
                }
                else
                {
                    buildingFilters.width = sizeFilterX.relativePosition.x + sizeFilterX.width;
                }

                filterPanel.width = buildingFilters.relativePosition.x + buildingFilters.width + 5;
            }
            else
            {
                filterPanel.width = typeFilter.relativePosition.x + typeFilter.width + 5;
            }
        }

        public void UpdateFilterPanels()
        {
            SimulationManager.instance.AddAction(() =>
            {
                int index = typeFilter.selectedIndex;
                if (!FindIt.isRicoEnabled && index >= (int)Asset.AssetType.Rico)
                {
                    index++;
                }

                switch ((Asset.AssetType)index)
                {
                    case Asset.AssetType.Ploppable:
                        HideFilterPanel(filterGrowable);
                        ShowFilterPanel(filterPloppable);
                        HideBuildingFilters();
                        break;
                    case Asset.AssetType.Rico:
                    case Asset.AssetType.Growable:
                        HideFilterPanel(filterPloppable);
                        ShowFilterPanel(filterGrowable);
                        ShowBuildingFilters();
                        break;
                    default:
                        HideFilterPanel(filterPloppable);
                        HideFilterPanel(filterGrowable);
                        HideBuildingFilters();
                        break;
                }
            });
        }

        public void ShowFilterPanel(UIPanel panel)
        {
            //inputPanel.relativePosition = new Vector2(inputPanel.relativePosition.x, -inputPanel.height - 45);
            //filterPanel.relativePosition = new Vector2(filterPanel.relativePosition.x, -filterPanel.height - 45);

            panel.isVisible = true;
        }

        public void HideFilterPanel(UIPanel panel)
        {
            //inputPanel.relativePosition = new Vector2(inputPanel.relativePosition.x, -inputPanel.height);
            //filterPanel.relativePosition = new Vector2(filterPanel.relativePosition.x, -filterPanel.height);

            panel.isVisible = false;
        }

        public void ShowBuildingFilters()
        {
            buildingFilters.isVisible = true;
            UpdateBuildingFilters();
        }

        public void HideBuildingFilters()
        {
            buildingFilters.isVisible = false;
            UpdateBuildingFilters();
        }

        public void Search()
        {
            PrefabInfo current = null;
            UIScrollPanelItem.ItemData selected = null;
            if (scrollPanel.selectedItem != null)
            {
                current = scrollPanel.selectedItem.asset.prefab;
            }

            string text = "";
            Asset.AssetType type = Asset.AssetType.All;

            if (input != null)
            {
                text = input.text;
                type = (Asset.AssetType)typeFilter.selectedIndex;

                if (!FindIt.isRicoEnabled && type >= Asset.AssetType.Rico)
                {
                    type++;
                }
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

                    data.tooltipBox = GeneratedPanel.GetTooltipBox(TooltipHelper.GetHashCode(data.tooltip));
                    data.asset = asset;

                    scrollPanel.itemsData.Add(data);

                    if (asset.prefab == current)
                    {
                        selected = data;
                    }
                }
            }

            scrollPanel.DisplayAt(0);
            scrollPanel.selectedItem = selected;

            if (scrollPanel.selectedItem != null)
            {

                FindIt.SelectPrefab(scrollPanel.selectedItem.asset.prefab);
            }
            else
            {
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }
    }
}
