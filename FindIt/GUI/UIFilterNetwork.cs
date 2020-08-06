// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;
using ColossalFramework.UI;
using System.Linq;

namespace FindIt.GUI
{
    public class UIFilterNetwork : UIPanel
    {
        public static UIFilterNetwork instance;

        public enum Category
        {
            None = -1,
            SmallRoads = 0,
            MediumRoads,
            LargeRoads,
            Highway,
            Path,
            Fence,
            WaterStructures,
            Unsorted,
            All
        }

        public UICheckBox[] toggles;
        public UIButton all;
        private UICheckBox randomIcon;

        public static Category GetCategory(Asset.NetworkType networkType)
        {
            if (networkType == Asset.NetworkType.SmallRoads) return Category.SmallRoads;
            if (networkType == Asset.NetworkType.MediumRoads) return Category.MediumRoads;
            if (networkType == Asset.NetworkType.LargeRoads) return Category.LargeRoads;
            if (networkType == Asset.NetworkType.Highway) return Category.Highway;
            if (networkType == Asset.NetworkType.Path) return Category.Path;
            if (networkType == Asset.NetworkType.Fence) return Category.Fence;
            if (networkType == Asset.NetworkType.WaterStructures) return Category.WaterStructures;
            if (networkType == Asset.NetworkType.Unsorted) return Category.Unsorted;

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
                "Ingame",
                "Ingame"
            };

            public static readonly string[] spriteNames =
            {
                "SubBarRoadsSmall",
                "SubBarRoadsMedium",
                "SubBarRoadsLarge",
                "SubBarRoadsHighway",
                "SubBarLandscapingPaths",
                "SubBarLandscapingFences",
                "SubBarLandscapingWaterStructures",
                "ToolbarIconProps"
            };

            public static readonly string[] tooltips =
            {
                Translations.Translate("FIF_NET_SMR"), // Small Roads
                Translations.Translate("FIF_NET_MDR"), // Medium Roads
                Translations.Translate("FIF_NET_LGR"), // Large Roads
                Translations.Translate("FIF_NET_HGHW"), // Highway
                Translations.Translate("FIF_NET_PATH"), // Path
                Translations.Translate("FIF_NET_WALL"), // Fence & Wall
                Translations.Translate("FIF_NET_WAT"), // Water Structures
                Translations.Translate("FIF_NET_UNS") // Unsorted
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
                toggles[i] = SamsamTS.UIUtils.CreateIconToggle(this, CategoryIcons.atlases[i], CategoryIcons.spriteNames[i], CategoryIcons.spriteNames[i] + "Disabled");
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

            UICheckBox last = toggles[toggles.Length - 1];

            randomIcon = SamsamTS.UIUtils.CreateIconToggle(this, "FindItAtlas", "Dice", "Dice");
            randomIcon.relativePosition = new Vector3(last.relativePosition.x + last.width + 5, 5);
            randomIcon.tooltip = Translations.Translate("FIF_GR_RAN");
            randomIcon.isChecked = true;
            randomIcon.readOnly = true;
            randomIcon.checkedBoxObject.isInteractive = false;
            Random.InitState(System.Environment.TickCount);
            randomIcon.eventClicked += (c, p) =>
            {
                PickRandom();
            };

            all = SamsamTS.UIUtils.CreateButton(this);
            all.size = new Vector2(55, 35);
            all.text = Translations.Translate("FIF_SE_IA");
            all.relativePosition = new Vector3(last.relativePosition.x + last.width + 5, 5);

            all.eventClick += (c, p) =>
            {
                for (int i = 0; i < (int)Category.All; i++)
                {
                    toggles[i].isChecked = true;
                }
                eventFilteringChanged(this, 0);
            };

            width = parent.width;
        }

        /// <summary>
        /// Pick a random growable or RICO building from the search result
        /// </summary>
        private void PickRandom()
        {
            int index = Random.Range(0, UISearchBox.instance.searchResultList.Count);
            string name = UISearchBox.instance.searchResultList.ElementAt(index);
            FindIt.instance.scrollPanel.DisplayAt(index);
            foreach (UIButton button in FindIt.instance.scrollPanel.GetComponentsInChildren<UIButton>())
            {
                if (button.name == name)
                {
                    button.SimulateClick();
                    break;
                }
            }
        }
    }
}
