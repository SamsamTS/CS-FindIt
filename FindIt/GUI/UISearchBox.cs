// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;

using ColossalFramework.DataBinding;
using ColossalFramework.UI;

using System;
using System.Collections.Generic;
using System.Linq;

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
        public UIFilterProp filterProp;

        public UICheckBox workshopFilter;
        public UICheckBox vanillaFilter;
        public UIButton sortButton;

        // true = sort by relevance
        // false = sort by most recently downloaded
        public bool sortButtonTextState = true;

        public string[] filterItemsGrowable = { Translations.Translate("FIF_SE_IA"), "1", "2", "3", "4"};
        public string[] filterItemsRICO = { Translations.Translate("FIF_SE_IA"), "1", "2", "3", "4", "5-8", "9-12", "13+"};

        public ItemClass.Level buildingLevel
        {
            get { return (ItemClass.Level)(levelFilter.selectedIndex - 1); }
        }

        public Vector2 buildingSizeFilterIndex
        {
            get
            {
                if (sizeFilterX.selectedIndex == 0 && sizeFilterY.selectedIndex == 0) return Vector2.zero;
                return new Vector2(sizeFilterX.selectedIndex, sizeFilterY.selectedIndex);
            }
        }

        public override void Start()
        {
            instance = this;

            inputPanel = AddUIComponent<UIPanel>();
            inputPanel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            inputPanel.backgroundSprite = "GenericTab";
            inputPanel.size = new Vector2(300, 40);
            inputPanel.relativePosition = new Vector2(0, -inputPanel.height - 40);

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
            filterPanel.size = new Vector2(155, 35);
            filterPanel.SendToBack();
            filterPanel.relativePosition = new Vector3(inputPanel.width, -filterPanel.height - 40);

            // workshop filter checkbox (custom assets saved in local asset folder are also included)
            workshopFilter = SamsamTS.UIUtils.CreateCheckBox(filterPanel);
            workshopFilter.isChecked = true;
            workshopFilter.width = 80;
            workshopFilter.label.text = Translations.Translate("FIF_SE_WF");
            workshopFilter.label.textScale = 0.8f;
            workshopFilter.relativePosition = new Vector3(10, 10);
            workshopFilter.eventCheckChanged += (c, i) => Search();

            // vanilla filter checkbox
            vanillaFilter = SamsamTS.UIUtils.CreateCheckBox(filterPanel);
            vanillaFilter.isChecked = true;
            vanillaFilter.width = 80;
            vanillaFilter.label.text = Translations.Translate("FIF_SE_VF");
            vanillaFilter.label.textScale = 0.8f;
            vanillaFilter.relativePosition = new Vector3(workshopFilter.relativePosition.x + workshopFilter.width, 10);
            vanillaFilter.eventCheckChanged += (c, i) => Search();

            // asset type filter
            typeFilter = SamsamTS.UIUtils.CreateDropDown(filterPanel);
            typeFilter.size = new Vector2(100, 25);
            typeFilter.relativePosition = new Vector3(vanillaFilter.relativePosition.x + vanillaFilter.width, 5);

            if (FindIt.isRicoEnabled)
            {
                string[] items = {
                    Translations.Translate("FIF_SE_IA"),
                    Translations.Translate("FIF_SE_IN"),
                    Translations.Translate("FIF_SE_IP"),
                    Translations.Translate("FIF_SE_IG"),
                    Translations.Translate("FIF_SE_IR"),
                    Translations.Translate("FIF_SE_IPR"),
                    Translations.Translate("FIF_SE_ID"),
                    Translations.Translate("FIF_SE_IT")
                };
                typeFilter.items = items;

            }
            else
            {
                string[] items = {
                    Translations.Translate("FIF_SE_IA"),
                    Translations.Translate("FIF_SE_IN"),
                    Translations.Translate("FIF_SE_IP"),
                    Translations.Translate("FIF_SE_IG"),
                    Translations.Translate("FIF_SE_IPR"),
                    Translations.Translate("FIF_SE_ID"),
                    Translations.Translate("FIF_SE_IT")
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
            levelLabel.text = Translations.Translate("FIF_SE_LV");
            levelLabel.relativePosition = new Vector3(10, 5);

            levelFilter = SamsamTS.UIUtils.CreateDropDown(buildingFilters);
            levelFilter.size = new Vector2(55, 25);
            levelFilter.AddItem(Translations.Translate("FIF_SE_IA"));
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
            sizeLabel.text = Translations.Translate("FIF_SE_SZ");
            sizeLabel.relativePosition = new Vector3(levelFilter.relativePosition.x + levelFilter.width + 10, 5);

            sizeFilterX = SamsamTS.UIUtils.CreateDropDown(buildingFilters);
            sizeFilterX.size = new Vector2(55, 25);
            sizeFilterX.items = filterItemsGrowable;
            sizeFilterX.selectedIndex = 0;
            sizeFilterX.relativePosition = new Vector3(sizeLabel.relativePosition.x + sizeLabel.width + 5, 5);

            /*
            UILabel XLabel = buildingFilters.AddUIComponent<UILabel>();
            XLabel.textScale = 0.8f;
            XLabel.padding = new RectOffset(0, 0, 8, 0);
            //XLabel.text = "X";
            XLabel.text = " ";
            XLabel.isVisible = false;
            XLabel.relativePosition = new Vector3(sizeFilterX.relativePosition.x + sizeFilterX.width - 5, 5);
            */

            sizeFilterY = SamsamTS.UIUtils.CreateDropDown(buildingFilters);
            sizeFilterY.size = new Vector2(55, 25);
            sizeFilterY.items = filterItemsGrowable;
            sizeFilterY.selectedIndex = 0;
            //sizeFilterY.isVisible = false;
            sizeFilterY.relativePosition = new Vector3(sizeFilterX.relativePosition.x + sizeFilterX.width + 10, 5);

            sizeFilterX.eventSelectedIndexChanged += (c, i) => Search();
            sizeFilterY.eventSelectedIndexChanged += (c, i) => Search();

            UIPanel panel = AddUIComponent<UIPanel>();
            panel.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            panel.backgroundSprite = "GenericTabHovered";
            panel.size = new Vector2(parent.width, 45);
            panel.relativePosition = new Vector3(0, -panel.height + 5);

            // sort button
            sortButton = SamsamTS.UIUtils.CreateButton(panel);
            sortButton.size = new Vector2(100, 35);
            sortButton.text = Translations.Translate("FIF_SO_RE");
            sortButton.tooltip = Translations.Translate("FIF_SO_RETP");
            sortButton.relativePosition = new Vector3(5, 5);

            sortButton.eventClick += (c, p) =>
            {
                if (sortButtonTextState)
                {
                    sortButton.text = Translations.Translate("FIF_SO_NE");
                    sortButtonTextState = false;
                    sortButton.tooltip = Translations.Translate("FIF_SO_NETP");
                }
                else
                {
                    sortButton.text = Translations.Translate("FIF_SO_RE");
                    sortButtonTextState = true;
                    sortButton.tooltip = Translations.Translate("FIF_SO_RETP");
                }
                Search();
            };

            // ploppable filter tabs
            filterPloppable = panel.AddUIComponent<UIFilterPloppable>();
            filterPloppable.isVisible = false;
            filterPloppable.relativePosition = new Vector3(sortButton.relativePosition.x + sortButton.width, 0);
            //filterPloppable.relativePosition = new Vector3(0, 0);

            filterPloppable.eventFilteringChanged += (c,p) => Search();

            // growable filter tabs
            filterGrowable = panel.AddUIComponent<UIFilterGrowable>();
            filterGrowable.isVisible = false;
            filterGrowable.relativePosition = new Vector3(sortButton.relativePosition.x + sortButton.width, 0);
            //filterGrowable.relativePosition = new Vector3(0, 0);

            filterGrowable.eventFilteringChanged += (c, p) => Search();

            // prop filter tabs
            filterProp = panel.AddUIComponent<UIFilterProp>();
            filterProp.isVisible = false;
            filterProp.relativePosition = new Vector3(sortButton.relativePosition.x + sortButton.width, 0);

            filterProp.eventFilteringChanged += (c, p) => Search();

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
            try
            {
                if (buildingFilters.isVisible)
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
            catch(Exception e)
            {
                Debugging.Message("UpdateBuildingFilters exception");
                Debugging.LogException(e);
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
                        HideFilterPanel(filterProp);
                        ShowFilterPanel(filterPloppable);
                        HideBuildingFilters();
                        break;
                    case Asset.AssetType.Rico:
                        sizeFilterX.items = filterItemsRICO;
                        sizeFilterY.items = filterItemsRICO;
                        HideFilterPanel(filterPloppable);
                        HideFilterPanel(filterProp);
                        ShowFilterPanel(filterGrowable);
                        ShowBuildingFilters();
                        break;
                    case Asset.AssetType.Growable:
                        // if switch back from rico with size > 4, default size = all
                        if (sizeFilterX.selectedIndex > filterItemsGrowable.Length - 1) sizeFilterX.selectedIndex = 0;
                        if (sizeFilterY.selectedIndex > filterItemsGrowable.Length - 1) sizeFilterY.selectedIndex = 0;
                        sizeFilterX.items = filterItemsGrowable;
                        sizeFilterY.items = filterItemsGrowable;
                        HideFilterPanel(filterPloppable);
                        HideFilterPanel(filterProp);
                        ShowFilterPanel(filterGrowable);
                        ShowBuildingFilters();
                        break;
                    case Asset.AssetType.Prop:
                        HideFilterPanel(filterGrowable);
                        HideFilterPanel(filterPloppable);
                        ShowFilterPanel(filterProp);
                        HideBuildingFilters();
                        break;
                    default:
                        HideFilterPanel(filterPloppable);
                        HideFilterPanel(filterGrowable);
                        HideFilterPanel(filterProp);
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

            // sort again by most recently downloaded
            // I tried to do this directly in Find so the list isn't being sorted twice
            // however I got some UI glitches and was not able to solve it
            if (sortButtonTextState == false)
            {
                matches = matches.OrderByDescending(s => s.downloadTime).ToList();
            }

            scrollPanel.Clear();
            foreach (Asset asset in matches)
            {
                if (asset.prefab != null)
                {
                    // filter custom/vanilla assets based on user choice
                    // I tried to do this directly in Find
                    // however I got some UI glitches and was not able to solve it
                    if (workshopFilter != null && vanillaFilter != null)
                    {
                        // filter out custom asset
                        if (asset.prefab.m_isCustomContent && !workshopFilter.isChecked) continue;

                        // filter out vanilla asset. will not filter out content creater pack assets
                        if (!asset.prefab.m_isCustomContent && !vanillaFilter.isChecked && !asset.isCCPBuilding) continue;
                    }

                    UIScrollPanelItem.ItemData data = new UIScrollPanelItem.ItemData();
                    data.name = asset.title;// + "___" + asset.prefab.editorCategory;
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
