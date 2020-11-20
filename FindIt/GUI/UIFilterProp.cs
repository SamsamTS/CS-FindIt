﻿// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt
// Filter tabs panel for prop

using UnityEngine;
using ColossalFramework.UI;

namespace FindIt.GUI
{
    public class UIFilterProp : UIPanel
    {
        public static UIFilterProp instance;

        public enum Category
        {
            None = -1,
            PropsMoterVehicle = 0, // Vehicles (from Tree & Vehicle Props mod)
            PropsRailwayVehicle,
            PropsAircraft,
            PropsWaterCraft,
            PropsUnsortedVehicle,
            PropsTree, // Tree (from Tree & Vehicle Props mod)
            PropsIndustrial,
            PropsParks,
            PropsCommon,
            PropsResidential,
            PropsBillboards,
            PropsSpecialBillboards,
            PropsLights,
            Natural,
            Unsorted,
            Marker,
            All
        }

        public UICheckBox[] toggles;
        public UIButton all;
        private UICheckBox randomIcon;

        public static Category GetCategory(Asset.PropType propType)
        {
            if (propType == Asset.PropType.PropsIndustrial) return Category.PropsIndustrial;
            if (propType == Asset.PropType.PropsParks) return Category.PropsParks;
            if (propType == Asset.PropType.PropsCommon) return Category.PropsCommon;
            if (propType == Asset.PropType.PropsResidential) return Category.PropsResidential;
            if (propType == Asset.PropType.PropsBillboards) return Category.PropsBillboards;
            if (propType == Asset.PropType.PropsSpecialBillboards) return Category.PropsSpecialBillboards;
            if (propType == Asset.PropType.PropsLights) return Category.PropsLights;
            if (propType == Asset.PropType.Natural) return Category.Natural;
            if (propType == Asset.PropType.Unsorted) return Category.Unsorted;
            if (propType == Asset.PropType.PropsMarker) return Category.Marker;
            if (propType == Asset.PropType.PropsMoterVehicle) return Category.PropsMoterVehicle;
            if (propType == Asset.PropType.PropsRailwayVehicle) return Category.PropsRailwayVehicle;
            if (propType == Asset.PropType.PropsAircraft) return Category.PropsAircraft;
            if (propType == Asset.PropType.PropsWaterCraft) return Category.PropsWaterCraft;
            if (propType == Asset.PropType.PropsUnsortedVehicle) return Category.PropsUnsortedVehicle;
            if (propType == Asset.PropType.PropsTree) return Category.PropsTree;

            return Category.None;
        }

        public class CategoryIcons
        {
            public static readonly string[] atlases =
            {
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "Thumbnails",
                "Ingame",
                "Ingame",
                "Thumbnails",
                "FindItAtlas",
                "FindItAtlas",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame"
            };

            public static readonly string[] spriteNames =
            {
                "ToolbarIconPublicTransport",
                "SubBarPublicTransportTrain",
                "SubBarPublicTransportPlane",
                "SubBarPublicTransportShip",
                "IconPolicyEncourageBiking",
                "IconPolicyForest",
                "ZoningIndustrial",
                "ToolbarIconBeautification",
                "ToolbarIconProps",
                "ZoningResidentialLow",
                "ToolbarIconPropsBillboards",
                "ToolbarIconPropsSpecialBillboards",
                "SubBarPropsCommonLights",
                "InfoIconResources",
                "ToolbarIconHelp",
                "RoadOptionUpgrade"
            };

            public static readonly string[] tooltips =
            {
                Translations.Translate("FIF_PROP_MOT"), // Motor Vehicle (from Tree & Vehicle Props mod)
                Translations.Translate("FIF_PROP_RAI"), // Railway Vehicle (from Tree & Vehicle Props mod)
                Translations.Translate("FIF_PROP_AIR"), // Aircraft (from Tree & Vehicle Props mod)
                Translations.Translate("FIF_PROP_WAT"), // Watercraft (from Tree & Vehicle Props mod)
                Translations.Translate("FIF_PROP_VEH"), // Unsorted Vehicle (from Tree & Vehicle Props mod)
                Translations.Translate("FIF_PROP_TRE"), // Tree (from Tree & Vehicle Props mod)
                Translations.Translate("FIF_PROP_IND"), // Industrial
                Translations.Translate("FIF_PROP_PAR"), // Parks
                Translations.Translate("FIF_PROP_COM"), // Common
                Translations.Translate("FIF_PROP_RES"), // Residential
                Translations.Translate("FIF_PROP_BIL"), // Billboards
                Translations.Translate("FIF_PROP_SPE"), // Special Billboards
                Translations.Translate("FIF_PROP_LIG"), // Lights
                Translations.Translate("FIF_PROP_NAT"), // Natural
                Translations.Translate("FIF_PROP_UNS"), // Unsorted
                Translations.Translate("FIF_PROP_MAR") // Marker
            };
        }

        public bool IsSelected(Category category)
        {
            return toggles[(int)category].isChecked;
        }

        public bool IsAllSelected()
        {
            for (int i = 0; i < (int)Category.All; i++)
            {
                if (!toggles[i].isChecked)
                {
                    return false;
                }
            }
            return true;
        }

        public void SelectAll()
        {
            for (int i = 0; i < (int)Category.All; i++)
            {
                toggles[i].isChecked = true;
            }
        }

        public event PropertyChangedEventHandler<int> eventFilteringChanged;

        public override void Start()
        {
            instance = this;

            /*atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericTabHovered";*/
            size = new Vector2(605, 45);

            // generate filter tabs UI
            toggles = new UICheckBox[(int)Category.All];
            for (int i = 0; i < (int)Category.All; i++)
            {
                toggles[i] = SamsamTS.UIUtils.CreateIconToggle(this, CategoryIcons.atlases[i], CategoryIcons.spriteNames[i], CategoryIcons.spriteNames[i], 0.4f);
                toggles[i].tooltip = CategoryIcons.tooltips[i] + "\n" + Translations.Translate("FIF_SE_SC");
                toggles[i].relativePosition = new Vector3(5 + 40 * i, 5);
                toggles[i].isChecked = true;
                toggles[i].readOnly = true;
                toggles[i].checkedBoxObject.isInteractive = false; // Don't eat my double click event please

                toggles[i].eventClick += (c, p) =>
                {
                    Event e = Event.current;

                    if (e.shift || e.control)
                    {
                        ((UICheckBox)c).isChecked = !((UICheckBox)c).isChecked;
                        eventFilteringChanged(this, 0);
                    }
                    else
                    {
                        // when all tabs are checked, toggle a tab will uncheck all the other tabs
                        bool check = true;
                        for (int j = 0; j < (int)Category.All; j++)
                        {
                            check = check && toggles[j].isChecked;
                        }
                        if (check)
                        {
                            for (int j = 0; j < (int)Category.All; j++)
                            {
                                toggles[j].isChecked = false;
                            }
                            ((UICheckBox)c).isChecked = true;
                            eventFilteringChanged(this, 0);
                            return;
                        }

                        // when a tab is unchecked, toggle it will uncheck all the other tabs
                        if (((UICheckBox)c).isChecked == false)
                        {
                            for (int j = 0; j < (int)Category.All; j++)
                                toggles[j].isChecked = false;
                            ((UICheckBox)c).isChecked = true;
                            eventFilteringChanged(this, 0);
                            return;
                        }

                        // when a tab is already checked, toggle it will move back to select all
                        if (((UICheckBox)c).isChecked == true)
                        {
                            for (int j = 0; j < (int)Category.All; j++)
                                toggles[j].isChecked = true;
                            eventFilteringChanged(this, 0);
                            return;
                        }
                    }
                };
            }

            // Hide vehicle and tree tabs if the Tree & Vehicle Props Patch mod is not enabled
            if (!FindIt.isTVPPatchEnabled)
            {
                for (int i = 0; i < 6; i++)
                {
                    toggles[i].isVisible = false;
                }
                for (int i = 6; i < (int)Category.All; i++)
                {
                    toggles[i].relativePosition = new Vector3(5 + 40 * (i - 6), 5);
                }
            }

            UICheckBox last = toggles[toggles.Length - 1];

            randomIcon = SamsamTS.UIUtils.CreateIconToggle(this, "FindItAtlas", "Dice", "Dice");
            randomIcon.relativePosition = new Vector3(last.relativePosition.x + last.width + 5, 5);
            randomIcon.tooltip = Translations.Translate("FIF_GR_RAN");
            randomIcon.isChecked = true;
            randomIcon.readOnly = true;
            randomIcon.checkedBoxObject.isInteractive = false;
            randomIcon.eventClicked += (c, p) =>
            {
                UISearchBox.instance.PickRandom();
            };

            all = SamsamTS.UIUtils.CreateButton(this);
            all.size = new Vector2(55, 35);
            all.text = Translations.Translate("FIF_SE_IA");
            all.relativePosition = new Vector3(randomIcon.relativePosition.x + randomIcon.width + 5, 5);

            all.eventClick += (c, p) =>
            {
                for (int i = 0; i < (int)Category.All; i++)
                {
                    toggles[i].isChecked = true;
                }
                eventFilteringChanged(this, 0);
            };

            UpdateMarkerToggleVisibility();

            width = parent.width;
        }

        /// <summary>
        /// Hide marder toggle checkbox if not in asset editor
        /// </summary>
        public void UpdateMarkerToggleVisibility()
        {
            UICheckBox last = toggles[toggles.Length - 1];

            if (FindIt.inEditor || Settings.showPropMarker)
            {
                last.isVisible = true;
                randomIcon.relativePosition = new Vector3(last.relativePosition.x + last.width + 5, 5);
            }
            else
            {
                last.isVisible = false;
                UICheckBox secondLast = toggles[toggles.Length - 2];
                randomIcon.relativePosition = new Vector3(secondLast.relativePosition.x + secondLast.width + 5, 5);
            }
            all.relativePosition = new Vector3(randomIcon.relativePosition.x + randomIcon.width + 5, 5);
        }
    }
}
