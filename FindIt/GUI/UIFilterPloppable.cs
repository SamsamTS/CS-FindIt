using UnityEngine;

using ColossalFramework.UI;
using ColossalFramework.Globalization;

namespace FindIt.GUI
{
    public class UIFilterPloppable : UIPanel
    {
        public static UIFilterPloppable instance;

        public enum Category
        {
            None = -1,
            Electricity = 0,
            WaterAndSewage,
            Garbage,
            Healthcare,
            FireDepartment,
            Police,
            Education,
            PublicTransport,
            Beautification,
            Monuments,
            //Wonders,
            All
        }

        public UICheckBox[] toggles;
        public UIButton all;
        public UIButton none;

        public static Category GetCategory(ItemClass itemClass)
        {
            if (itemClass.m_service == ItemClass.Service.Electricity) return Category.Electricity;
            if (itemClass.m_service == ItemClass.Service.Water) return Category.WaterAndSewage;
            if (itemClass.m_service == ItemClass.Service.Garbage) return Category.Garbage;
            if (itemClass.m_service == ItemClass.Service.HealthCare) return Category.Healthcare;
            if (itemClass.m_service == ItemClass.Service.FireDepartment) return Category.FireDepartment;
            if (itemClass.m_service == ItemClass.Service.PoliceDepartment) return Category.Police;
            if (itemClass.m_service == ItemClass.Service.Education) return Category.Education;
            if (itemClass.m_service == ItemClass.Service.PublicTransport) return Category.PublicTransport;
            if (itemClass.m_service == ItemClass.Service.Beautification) return Category.Beautification;
            if (itemClass.m_service == ItemClass.Service.Monument) return Category.Monuments;
            //if (itemClass.m_service == ItemClass.Service.Wonders) return Category.Wonders; ???????

            return Category.None;
        }

        public bool IsSelected(Category category)
        {
            return toggles[(int)category].isChecked;
        }

        public bool IsAllSelected()
        {
            for (int i = 0; i < (int)Category.All; i++)
            {
                if(!toggles[i].isChecked)
                {
                    return false;
                }
            }
            return true;
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
                toggles[i] = SamsamTS.UIUtils.CreateIconToggle(this, "Ingame", "ToolbarIcon" + ((Category)i), "ToolbarIcon" + ((Category)i) + "Disabled");
                toggles[i].tooltip = Locale.Get("MAIN_TOOL", ((Category)i).ToString());
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
