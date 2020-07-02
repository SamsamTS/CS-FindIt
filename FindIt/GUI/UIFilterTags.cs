// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;
using ColossalFramework.UI;
using System.Linq;
using HarmonyLib;

namespace FindIt.GUI
{
    public class UIFilterTags : UIPanel
    {
        public static UIFilterTags instance;

        public UIDropDown dropDownList;
        public UIButton refresh;
        public UIButton delete;

        public override void Start()
        {
            instance = this;

            /*atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericTabHovered";*/
            size = new Vector2(605, 45);

            dropDownList = SamsamTS.UIUtils.CreateDropDown(instance);
            dropDownList.size = new Vector2(200, 30);
            dropDownList.relativePosition = new Vector3(5, 5);
            dropDownList.itemHeight = 30;
            dropDownList.listHeight = 750;

            string[] items = {
                    "custom tag test 1",
                    "custom tag test 2",
                    "custom tag test 3",
                    "custom tag test 4",
                    "custom tag test 5",
                    "custom tag test 6",
                    "custom tag test 7",
                    "custom tag test 8",
                    "custom tag test 9",
                    "custom tag test 10",
                    "custom tag test 11",
                    "custom tag test 12",
                    "custom tag test 13",
                    "custom tag test 14",
                    "custom tag test 15",
                    "custom tag test 16",
                    "custom tag test 17",
                    "custom tag test 18",
                    "custom tag test 19",
                    "custom tag test 20",
                    "custom tag test 21",
                    "custom tag test 22",
                    "custom tag test 23",
                    "custom tag test 24",
                    "custom tag test 25"
                };
            dropDownList.items = items;

            refresh = SamsamTS.UIUtils.CreateButton(this);
            refresh.size = new Vector2(75, 35);
            refresh.text = "Refresh";
            refresh.relativePosition = new Vector3(dropDownList.relativePosition.x + dropDownList.width + 5, 5);
            refresh.eventClick += (c, p) =>
            {
                
            };

            delete = SamsamTS.UIUtils.CreateButton(this);
            delete.size = new Vector2(70, 35);
            delete.text = "Delete";
            delete.relativePosition = new Vector3(refresh.relativePosition.x + refresh.width + 5, 5);
            delete.eventClick += (c, p) =>
            {

            };

            width = parent.width;
        }
    }
}
