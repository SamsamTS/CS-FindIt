// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt
// extra filters panel

using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace FindIt.GUI
{
    public class UIFilterExtraPanel : UIPanel
    {
        public static UIFilterExtraPanel instance;

        public UICheckBox optionDropDownCheckBox;
        public UIDropDown optionDropDownMenu;

        // asset creator
        public UITextField assetCreatorInput;
        private UISprite assetCreatorSearchIcon;
        public UIDropDown assetCreatorDropDownMenu;
        private List<KeyValuePair<string, int>> assetCreatorList;
        private string[] assetCreatorListStrArray;

        // building height
        private UILabel buildingHeightMinLabel;
        private UILabel buildingHeightMaxLabel;
        public UITextField buildingHeightMinInput;
        public UITextField buildingHeightMaxInput;
        public UIDropDown builingHeightUnit;
        public float minBuildingHeight = float.MinValue;
        public float maxBuildingHeight = float.MaxValue;

        // building level
        private UILabel buildingLevelMinLabel;
        private UILabel buildingLevelMaxLabel;
        public UIDropDown buildingLevelMinDropDownMenu;
        public UIDropDown buildingLevelMaxDropDownMenu;

        // unused assets
        private UIButton exportAllUnusedButton;
        private UIButton exportSearchedUnusedButton;

        // unused assets
        private UIButton exportAllUsedButton;
        private UIButton exportSearchedUsedButton;

        // DLC & CCP
        public UIDropDown DLCDropDownMenu;

        public enum DropDownOptions
        {
            AssetCreator = 0,
            BuildingHeight,
            BuildingLevel,
            DLC,
            SubBuildings,
            UsedAssets,
            UnusedAssets,
            LocalCustom,
            WorkshopCustom,
            WithCustomTag,
            WithoutCustomTag,
            TerrainConforming,
            NonTerrainConforming
        }

        string[] options = {
                    Translations.Translate("FIF_EF_AC"), // Asset Creator
                    Translations.Translate("FIF_EF_BH"), // Building Height
                    Translations.Translate("FIF_SE_LV"), // Building Level
                    Translations.Translate("FIF_EF_DLC"), // Require DLC/CCP
                    Translations.Translate("FIF_EF_SB"), // Sub-building
                    Translations.Translate("FIF_EF_US"), // Used Asset
                    Translations.Translate("FIF_EF_UN"), // Unused Asset
                    Translations.Translate("FIF_EF_LC"), // Local Custom
                    Translations.Translate("FIF_EF_WC"), // Workshop Subscription
                    Translations.Translate("FIF_EF_CT"), // With Custom Tag
                    Translations.Translate("FIF_EF_NCT"), // Without Custom Tag
                    Translations.Translate("FIF_PROP_TC"), // Terrain conforming
                    Translations.Translate("FIF_PROP_NTC") // Non-Terrain conforming
                };

        public enum DLCDropDownOptions
        {
            BaseGame = 0,
            AfterDark,
            Airports,
            Campus,
            GreenCities,
            Industries,
            MassTransit,
            NaturalDisasters,
            Parklife,
            SnowFall,
            SunsetHarbor,
            ArtDeco,
            HighTechBuildings,
            EuropeanSuburbias,
            UniverisityCity,
            ModernCityCenter,
            ModernJapan,
            TrainStations,
            BridgesPiers,
            VehiclesoftheWorld,
            Concerts,
            DeluxeUpgrade,
            MatchDay,
            PearlsFromTheEast,
            Stadiums
        }

        public override void Start()
        {
            instance = this;

            // extra filter checkbox
            optionDropDownCheckBox = SamsamTS.UIUtils.CreateCheckBox(this);
            optionDropDownCheckBox.isChecked = false;
            optionDropDownCheckBox.width = 20;
            optionDropDownCheckBox.relativePosition = new Vector3(10, 10);
            optionDropDownCheckBox.eventCheckChanged += (c, i) =>
            {
                if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.SubBuildings)
                {
                    if (optionDropDownCheckBox.isChecked)
                        UISearchBox.instance.typeFilter.selectedIndex = (int)UISearchBox.DropDownOptions.All;
                }

                ((UISearchBox)parent).Search();
            };

            // extra filter dropdown
            optionDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            optionDropDownMenu.size = new Vector2(230, 25);
            optionDropDownMenu.listHeight = 300;
            optionDropDownMenu.itemHeight = 30;
            optionDropDownMenu.items = options;
            optionDropDownMenu.selectedIndex = 0;
            optionDropDownMenu.relativePosition = new Vector3(optionDropDownCheckBox.relativePosition.x + optionDropDownCheckBox.width + 5, 5);
            SamsamTS.UIUtils.CreateDropDownScrollBar(UIFilterExtraPanel.instance.optionDropDownMenu);

            optionDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                HideAll();

                if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.AssetCreator)
                {
                    UpdateAssetCreatorOptionVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.BuildingHeight)
                {
                    UpdateBuildingHeightOptionVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.BuildingLevel)
                {
                    UpdateBuildingLevelOptionVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.UnusedAssets)
                {
                    UpdateUnusedAssetsVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.UsedAssets)
                {
                    UpdateUsedAssetsVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.DLC)
                {
                    UpdateDLCVisibility(true);
                }
                else if (optionDropDownMenu.selectedIndex == (int)DropDownOptions.SubBuildings)
                {
                    if (optionDropDownCheckBox.isChecked)
                        UISearchBox.instance.typeFilter.selectedIndex = (int)UISearchBox.DropDownOptions.All;
                }

                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // asset creator
            assetCreatorInput = SamsamTS.UIUtils.CreateTextField(this);
            assetCreatorInput.size = new Vector2(110, 25);
            assetCreatorInput.padding.top = 5;
            assetCreatorInput.isVisible = true;
            assetCreatorInput.text = "";
            assetCreatorInput.textScale = 0.9f;
            assetCreatorInput.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 20, 5);
            assetCreatorInput.eventTextChanged += (c, p) =>
            {
                if (assetCreatorInput.text == "") return;

                for (int i = 0; i < assetCreatorDropDownMenu.items.Length; ++i)
                {
                    if (assetCreatorDropDownMenu.items[i].ToLower().StartsWith(assetCreatorInput.text.ToLower()))
                    {
                        assetCreatorDropDownMenu.selectedIndex = i;
                        return;
                    }
                }
                for (int i = 0; i < assetCreatorDropDownMenu.items.Length; ++i)
                {
                    if (assetCreatorDropDownMenu.items[i].ToLower().Contains(assetCreatorInput.text.ToLower()))
                    {
                        assetCreatorDropDownMenu.selectedIndex = i;
                        return;
                    }
                }
            };

            // search icon
            assetCreatorSearchIcon = AddUIComponent<UISprite>();
            assetCreatorSearchIcon.size = new Vector2(25, 30);
            assetCreatorSearchIcon.atlas = FindIt.atlas;
            assetCreatorSearchIcon.spriteName = "FindItDisabled";
            assetCreatorSearchIcon.isVisible = true;
            assetCreatorSearchIcon.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 20, 3);

            assetCreatorDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            assetCreatorDropDownMenu.size = new Vector2(270, 25);
            assetCreatorDropDownMenu.tooltip = Translations.Translate("FIF_POP_SCR");
            assetCreatorDropDownMenu.listHeight = 300;
            assetCreatorDropDownMenu.itemHeight = 30;
            UpdateAssetCreatorList();
            assetCreatorDropDownMenu.isVisible = true;
            assetCreatorDropDownMenu.relativePosition = new Vector3(assetCreatorInput.relativePosition.x + assetCreatorInput.width + 10, 5);
            SamsamTS.UIUtils.CreateDropDownScrollBar(UIFilterExtraPanel.instance.assetCreatorDropDownMenu);

            assetCreatorDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // building height min label
            buildingHeightMinLabel = this.AddUIComponent<UILabel>();
            buildingHeightMinLabel.textScale = 0.8f;
            buildingHeightMinLabel.padding = new RectOffset(0, 0, 8, 0);
            buildingHeightMinLabel.text = "Min:";
            buildingHeightMinLabel.isVisible = false;
            buildingHeightMinLabel.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 50, 5);

            // building height min input box
            buildingHeightMinInput = SamsamTS.UIUtils.CreateTextField(this);
            buildingHeightMinInput.size = new Vector2(60, 25);
            buildingHeightMinInput.padding.top = 5;
            buildingHeightMinInput.isVisible = false;
            buildingHeightMinInput.text = "";
            buildingHeightMinInput.relativePosition = new Vector3(buildingHeightMinLabel.relativePosition.x + buildingHeightMinLabel.width + 10, 5);
            buildingHeightMinInput.eventTextChanged += (c, p) =>
            {
                if (float.TryParse(buildingHeightMinInput.text, out minBuildingHeight))
                {
                    if (builingHeightUnit.selectedIndex == 1)
                    {
                        minBuildingHeight *= 0.3048f;
                    }
                    ((UISearchBox)parent).Search();

                }
                if (buildingHeightMinInput.text == "")
                {
                    minBuildingHeight = float.MinValue;
                    ((UISearchBox)parent).Search();
                }
            };

            // building height max label
            buildingHeightMaxLabel = this.AddUIComponent<UILabel>();
            buildingHeightMaxLabel.textScale = 0.8f;
            buildingHeightMaxLabel.padding = new RectOffset(0, 0, 8, 0);
            buildingHeightMaxLabel.text = "Max:";
            buildingHeightMaxLabel.isVisible = false;
            buildingHeightMaxLabel.relativePosition = new Vector3(buildingHeightMinInput.relativePosition.x + buildingHeightMinInput.width + 20, 5);

            // building height max input box
            buildingHeightMaxInput = SamsamTS.UIUtils.CreateTextField(this);
            buildingHeightMaxInput.size = new Vector2(60, 25);
            buildingHeightMaxInput.padding.top = 5;
            buildingHeightMaxInput.isVisible = false;
            buildingHeightMaxInput.text = "";
            buildingHeightMaxInput.relativePosition = new Vector3(buildingHeightMaxLabel.relativePosition.x + buildingHeightMaxLabel.width + 10, 5);
            buildingHeightMaxInput.eventTextChanged += (c, p) =>
            {

                if (float.TryParse(buildingHeightMaxInput.text, out maxBuildingHeight))
                {
                    if (builingHeightUnit.selectedIndex == 1)
                    {
                        maxBuildingHeight *= 0.3048f;
                    }
                    ((UISearchBox)parent).Search();
                }

                if (buildingHeightMaxInput.text == "")
                {
                    maxBuildingHeight = float.MaxValue;
                    ((UISearchBox)parent).Search();
                }
            };

            // building height unit
            builingHeightUnit = SamsamTS.UIUtils.CreateDropDown(this);
            builingHeightUnit.size = new Vector2(80, 25);
            builingHeightUnit.listHeight = 210;
            builingHeightUnit.itemHeight = 30;
            builingHeightUnit.AddItem(Translations.Translate("FIF_EF_MET"));
            builingHeightUnit.AddItem(Translations.Translate("FIF_EF_FEE"));
            builingHeightUnit.selectedIndex = 0;
            builingHeightUnit.isVisible = false;
            builingHeightUnit.relativePosition = new Vector3(buildingHeightMaxInput.relativePosition.x + buildingHeightMaxInput.width + 30, 5);
            builingHeightUnit.eventSelectedIndexChanged += (c, p) =>
            {
                if (float.TryParse(buildingHeightMinInput.text, out minBuildingHeight))
                {
                    if (builingHeightUnit.selectedIndex == 1) minBuildingHeight *= 0.3048f;
                }

                if (float.TryParse(buildingHeightMaxInput.text, out maxBuildingHeight))
                {
                    if (builingHeightUnit.selectedIndex == 1) maxBuildingHeight *= 0.3048f;
                }
                if (buildingHeightMinInput.text == "") minBuildingHeight = float.MinValue;
                if (buildingHeightMaxInput.text == "") maxBuildingHeight = float.MaxValue;
                ((UISearchBox)parent).Search();
            };

            // building level min label
            buildingLevelMinLabel = this.AddUIComponent<UILabel>();
            buildingLevelMinLabel.textScale = 0.8f;
            buildingLevelMinLabel.padding = new RectOffset(0, 0, 8, 0);
            buildingLevelMinLabel.text = "Min:";
            buildingLevelMinLabel.isVisible = false;
            buildingLevelMinLabel.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 50, 5);

            // building level min dropdown
            buildingLevelMinDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            buildingLevelMinDropDownMenu.size = new Vector2(60, 25);
            buildingLevelMinDropDownMenu.listHeight = 300;
            buildingLevelMinDropDownMenu.itemHeight = 30;
            buildingLevelMinDropDownMenu.AddItem("1");
            buildingLevelMinDropDownMenu.AddItem("2");
            buildingLevelMinDropDownMenu.AddItem("3");
            buildingLevelMinDropDownMenu.AddItem("4");
            buildingLevelMinDropDownMenu.AddItem("5");
            buildingLevelMinDropDownMenu.isVisible = false;
            buildingLevelMinDropDownMenu.selectedIndex = 0;
            buildingLevelMinDropDownMenu.relativePosition = new Vector3(buildingLevelMinLabel.relativePosition.x + buildingLevelMinLabel.width + 10, 5);
            buildingLevelMinDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // building level max label
            buildingLevelMaxLabel = this.AddUIComponent<UILabel>();
            buildingLevelMaxLabel.textScale = 0.8f;
            buildingLevelMaxLabel.padding = new RectOffset(0, 0, 8, 0);
            buildingLevelMaxLabel.text = "Max:";
            buildingLevelMaxLabel.isVisible = false;
            buildingLevelMaxLabel.relativePosition = new Vector3(buildingLevelMinDropDownMenu.relativePosition.x + buildingLevelMinDropDownMenu.width + 20, 5);

            // building level max dropdown
            buildingLevelMaxDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            buildingLevelMaxDropDownMenu.size = new Vector2(60, 25);
            buildingLevelMaxDropDownMenu.listHeight = 300;
            buildingLevelMaxDropDownMenu.itemHeight = 30;
            buildingLevelMaxDropDownMenu.AddItem("1");
            buildingLevelMaxDropDownMenu.AddItem("2");
            buildingLevelMaxDropDownMenu.AddItem("3");
            buildingLevelMaxDropDownMenu.AddItem("4");
            buildingLevelMaxDropDownMenu.AddItem("5");
            buildingLevelMaxDropDownMenu.isVisible = false;
            buildingLevelMaxDropDownMenu.selectedIndex = 0;
            buildingLevelMaxDropDownMenu.relativePosition = new Vector3(buildingLevelMaxLabel.relativePosition.x + buildingLevelMaxLabel.width + 10, 5);
            buildingLevelMaxDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // export all unused asset list
            exportAllUnusedButton = SamsamTS.UIUtils.CreateButton(this);
            exportAllUnusedButton.size = new Vector2(80, 25);
            exportAllUnusedButton.text = Translations.Translate("FIF_EF_UNEXP");
            exportAllUnusedButton.textScale = 0.8f;
            exportAllUnusedButton.textPadding = new RectOffset(0, 0, 5, 0);
            exportAllUnusedButton.tooltip = Translations.Translate("FIF_EF_UNEXPTP");
            exportAllUnusedButton.isVisible = false;
            exportAllUnusedButton.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 15, 5);
            exportAllUnusedButton.eventClick += (c, p) =>
            {
                ExportUnusedTool.ExportUnused(true);
            };

            // export searched unused asset list
            exportSearchedUnusedButton = SamsamTS.UIUtils.CreateButton(this);
            exportSearchedUnusedButton.size = new Vector2(130, 25);
            exportSearchedUnusedButton.text = Translations.Translate("FIF_EF_UNEXPSE");
            exportSearchedUnusedButton.textScale = 0.8f;
            exportSearchedUnusedButton.textPadding = new RectOffset(0, 0, 5, 0);
            exportSearchedUnusedButton.tooltip = Translations.Translate("FIF_EF_UNEXPTPSE");
            exportSearchedUnusedButton.isVisible = false;
            exportSearchedUnusedButton.relativePosition = new Vector3(exportAllUnusedButton.relativePosition.x + exportAllUnusedButton.width + 5, 5);
            exportSearchedUnusedButton.eventClick += (c, p) =>
            {
                ExportUnusedTool.ExportUnused(false);
            };

            // export all used asset list
            exportAllUsedButton = SamsamTS.UIUtils.CreateButton(this);
            exportAllUsedButton.size = new Vector2(80, 25);
            exportAllUsedButton.text = Translations.Translate("FIF_EF_UNEXP");
            exportAllUsedButton.textScale = 0.8f;
            exportAllUsedButton.textPadding = new RectOffset(0, 0, 5, 0);
            exportAllUsedButton.tooltip = Translations.Translate("FIF_EF_USEXPTP");
            exportAllUsedButton.isVisible = false;
            exportAllUsedButton.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 15, 5);
            exportAllUsedButton.eventClick += (c, p) =>
            {
                ExportUsedTool.ExportUsed(true);
            };

            // export searched used asset list
            exportSearchedUsedButton = SamsamTS.UIUtils.CreateButton(this);
            exportSearchedUsedButton.size = new Vector2(130, 25);
            exportSearchedUsedButton.text = Translations.Translate("FIF_EF_UNEXPSE");
            exportSearchedUsedButton.textScale = 0.8f;
            exportSearchedUsedButton.textPadding = new RectOffset(0, 0, 5, 0);
            exportSearchedUsedButton.tooltip = Translations.Translate("FIF_EF_USEXPTPSE");
            exportSearchedUsedButton.isVisible = false;
            exportSearchedUsedButton.relativePosition = new Vector3(exportAllUsedButton.relativePosition.x + exportAllUsedButton.width + 5, 5);
            exportSearchedUsedButton.eventClick += (c, p) =>
            {
                ExportUsedTool.ExportUsed(false);
            };

            // DLC & CCP
            DLCDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            DLCDropDownMenu.size = new Vector2(300, 25);
            DLCDropDownMenu.listHeight = 300;
            DLCDropDownMenu.itemHeight = 30;
            DLCDropDownMenu.AddItem("Base Game");
            DLCDropDownMenu.AddItem("After Dark DLC");
            DLCDropDownMenu.AddItem("Airports DLC");
            DLCDropDownMenu.AddItem("Campus DLC");
            DLCDropDownMenu.AddItem("Green Cities DLC");
            DLCDropDownMenu.AddItem("Industries DLC");
            DLCDropDownMenu.AddItem("Mass Transit DLC");
            DLCDropDownMenu.AddItem("Natural Disasters DLC");
            DLCDropDownMenu.AddItem("Parklife DLC");
            DLCDropDownMenu.AddItem("Snow Fall DLC");
            DLCDropDownMenu.AddItem("Sunset Harbor DLC");
            DLCDropDownMenu.AddItem("Art Deco CCP");
            DLCDropDownMenu.AddItem("High-Tech Buildings CCP");
            DLCDropDownMenu.AddItem("European Suburbias CCP");
            DLCDropDownMenu.AddItem("University City CCP");
            DLCDropDownMenu.AddItem("Modern City Center CCP");
            DLCDropDownMenu.AddItem("Modern Japan CCP");
            DLCDropDownMenu.AddItem("Train Stations CCP");
            DLCDropDownMenu.AddItem("Bridges & Piers CCP");
            DLCDropDownMenu.AddItem("Vehicles of the World CCP");
            DLCDropDownMenu.AddItem("Concerts DLC");
            DLCDropDownMenu.AddItem("Deluxe Upgrade Pack");
            DLCDropDownMenu.AddItem("Match Day DLC");
            DLCDropDownMenu.AddItem("Pearls from the East DLC");
            DLCDropDownMenu.AddItem("Stadiums: European Club Pack DLC");
            DLCDropDownMenu.isVisible = false;
            DLCDropDownMenu.selectedIndex = 0;
            DLCDropDownMenu.relativePosition = new Vector3(optionDropDownMenu.relativePosition.x + optionDropDownMenu.width + 50, 5);
            SamsamTS.UIUtils.CreateDropDownScrollBar(UIFilterExtraPanel.instance.DLCDropDownMenu);

            DLCDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if (optionDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

        }
        public void Close()
        {
            if (instance != null)
            {
                instance.isVisible = false;
                Destroy(instance.gameObject);
                instance = null;
            }
        }

        // Update asset creator list 
        public void UpdateAssetCreatorList()
        {
            assetCreatorList = AssetTagList.instance.GetAssetCreatorList();

            List<string> list = new List<string>();

            foreach (KeyValuePair<string, int> entry in assetCreatorList)
            {
                list.Add(entry.Key.ToString() + " (" + entry.Value.ToString() + ")");
            }

            assetCreatorListStrArray = list.ToArray();
            assetCreatorDropDownMenu.items = assetCreatorListStrArray;
            assetCreatorDropDownMenu.selectedIndex = 0;
        }

        public string GetAssetCreatorDropDownListKey()
        {
            return assetCreatorList[assetCreatorDropDownMenu.selectedIndex].Key;
        }

        private void UpdateAssetCreatorOptionVisibility(bool visibility)
        {
            assetCreatorDropDownMenu.isVisible = visibility;
            assetCreatorInput.isVisible = visibility;
            assetCreatorSearchIcon.isVisible = visibility;
        }

        private void UpdateBuildingHeightOptionVisibility(bool visibility)
        {
            buildingHeightMinLabel.isVisible = visibility;
            buildingHeightMinInput.isVisible = visibility;
            buildingHeightMaxLabel.isVisible = visibility;
            buildingHeightMaxInput.isVisible = visibility;
            builingHeightUnit.isVisible = visibility;
        }

        private void UpdateBuildingLevelOptionVisibility(bool visibility)
        {
            buildingLevelMinDropDownMenu.isVisible = visibility;
            buildingLevelMaxDropDownMenu.isVisible = visibility;
            buildingLevelMinLabel.isVisible = visibility;
            buildingLevelMaxLabel.isVisible = visibility;
        }

        private void UpdateUnusedAssetsVisibility(bool visibility)
        {
            exportAllUnusedButton.isVisible = visibility;
            exportSearchedUnusedButton.isVisible = visibility;
        }
        private void UpdateUsedAssetsVisibility(bool visibility)
        {
            exportAllUsedButton.isVisible = visibility;
            exportSearchedUsedButton.isVisible = visibility;
        }
        private void UpdateDLCVisibility(bool visibility)
        {
            DLCDropDownMenu.isVisible = visibility;
        }

        private void HideAll()
        {
            UpdateAssetCreatorOptionVisibility(false);
            UpdateBuildingHeightOptionVisibility(false);
            UpdateBuildingLevelOptionVisibility(false);
            UpdateUnusedAssetsVisibility(false);
            UpdateUsedAssetsVisibility(false);
            UpdateDLCVisibility(false);
        }
    }
}
