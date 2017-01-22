using UnityEngine;
using ColossalFramework.UI;

namespace FindIt.GUI
{
    public class UIFilterGrowable : UIPanel
    {
        public static UIFilterGrowable instance;

        public enum Category
        {
            None = -1,
            ResidentialLow = 0,
            ResidentialHigh,
            CommercialLow,
            CommercialHigh,
            Industrial,
            Office,
            Farming,
            Forestry,
            Oil,
            Ore,
            CommercialLeisure,
            CommercialTourism,
            All
        }

        public class CategoryIcons
        {
            public static readonly string[] atlases =
            {
                "Thumbnails",
                "Thumbnails",
                "Thumbnails",
                "Thumbnails",
                "Thumbnails",
                "Thumbnails",
                "Ingame",
                "Ingame",
                "Ingame",
                "Ingame",
                "FindItAtlas",
                "FindItAtlas"
            };
            
            public static readonly string[] spriteNames =
            {
                "ZoningResidentialLow",
                "ZoningResidentialHigh",
                "ZoningCommercialLow",
                "ZoningCommercialHigh",
                "ZoningOffice",
                "ZoningIndustrial",
                "IconPolicyFarming",
                "IconPolicyForest",
                "IconPolicyOil",
                "IconPolicyOre",
                "IconPolicyLeisure",
                "IconPolicyTourist"
            };

            public static readonly string[] tooltips =
            {
                "Low density residential",
                "High density residential",
                "Low density commercial",
                "High density commercial",
                "Office",
                "Generic Industry",
                "Farming Industry",
                "Forest Industry",
                "Oil Industry",
                "Ore Industry",
                "Leisure commercial",
                "Tourism commercial"
            };
        }

        public UICheckBox[] toggles;
        public UIButton all;
        public UIButton none;

        public static Category GetCategory(ItemClass itemClass)
        {
            if (itemClass.m_subService == ItemClass.SubService.ResidentialLow) return Category.ResidentialLow;
            if (itemClass.m_subService == ItemClass.SubService.ResidentialHigh) return Category.ResidentialHigh;
            if (itemClass.m_subService == ItemClass.SubService.CommercialLow) return Category.CommercialLow;
            if (itemClass.m_subService == ItemClass.SubService.CommercialHigh) return Category.CommercialHigh;
            if (itemClass.m_subService == ItemClass.SubService.CommercialLeisure) return Category.CommercialLeisure;
            if (itemClass.m_subService == ItemClass.SubService.CommercialTourist) return Category.CommercialTourism;
            if (itemClass.m_subService == ItemClass.SubService.IndustrialGeneric) return Category.Industrial;
            if (itemClass.m_subService == ItemClass.SubService.IndustrialFarming) return Category.Farming;
            if (itemClass.m_subService == ItemClass.SubService.IndustrialForestry) return Category.Forestry;
            if (itemClass.m_subService == ItemClass.SubService.IndustrialOil) return Category.Oil;
            if (itemClass.m_subService == ItemClass.SubService.IndustrialOre) return Category.Ore;
            if (itemClass.m_service == ItemClass.Service.Office) return Category.Office;

            return Category.None;
        }

        public bool IsSelected(Category category)
        {
            return toggles[(int)category].isChecked;
        }

        public bool IsAllSelected()
        {
            return toggles[(int)Category.ResidentialLow].isChecked &&
                toggles[(int)Category.ResidentialHigh].isChecked &&
                toggles[(int)Category.CommercialLow].isChecked &&
                toggles[(int)Category.CommercialHigh].isChecked &&
                toggles[(int)Category.CommercialLeisure].isChecked &&
                toggles[(int)Category.CommercialTourism].isChecked &&
                toggles[(int)Category.Industrial].isChecked &&
                toggles[(int)Category.Farming].isChecked &&
                toggles[(int)Category.Forestry].isChecked &&
                toggles[(int)Category.Oil].isChecked &&
                toggles[(int)Category.Ore].isChecked &&
                toggles[(int)Category.Office].isChecked;
        }


        public event PropertyChangedEventHandler<int> eventFilteringChanged;

        public override void Start()
        {
            instance = this;

            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericTabHovered";
            size = new Vector2(605, 45);

            // Zoning
            toggles = new UICheckBox[(int)Category.All];
            for (int i = 0; i < (int)Category.All; i++)
            {
                toggles[i] = SamsamTS.UIUtils.CreateIconToggle(this, CategoryIcons.atlases[i], CategoryIcons.spriteNames[i], CategoryIcons.spriteNames[i] + "Disabled");
                toggles[i].tooltip = CategoryIcons.tooltips[i];
                toggles[i].relativePosition = new Vector3(5 + 40 * i, 5);
                toggles[i].isChecked = true;
                toggles[i].readOnly = true;
                toggles[i].checkedBoxObject.isInteractive = false; // Don't eat my double click event please

                toggles[i].eventClick += (c, p) =>
                {
                    ((UICheckBox)c).isChecked = !((UICheckBox)c).isChecked;
                    eventFilteringChanged(this, 0);
                };

                toggles[i].eventDoubleClick += (c, p) =>
                {
                    for (int j = 0; j < (int)Category.All; j++)
                        toggles[j].isChecked = false;
                    ((UICheckBox)c).isChecked = true;

                    eventFilteringChanged(this, 0);
                };
            }

            UICheckBox last = toggles[toggles.Length - 1];

            all = SamsamTS.UIUtils.CreateButton(this);
            all.size = new Vector2(55, 35);
            all.text = "All";
            all.relativePosition = new Vector3(last.relativePosition.x + last.width + 5, 5);

            all.eventClick += (c, p) =>
            {
                for (int i = 0; i < (int)Category.All; i++)
                {
                    toggles[i].isChecked = true;
                }
                eventFilteringChanged(this, 0);
            };

            none = SamsamTS.UIUtils.CreateButton(this);
            none.size = new Vector2(55, 35);
            none.text = "None";
            none.relativePosition = new Vector3(all.relativePosition.x + all.width + 5, 5);

            none.eventClick += (c, p) =>
            {
                for (int i = 0; i < (int)Category.All; i++)
                {
                    toggles[i].isChecked = false;
                }
                eventFilteringChanged(this, 0);
            };

            width = parent.parent.width - 10;
        }
    }
}
