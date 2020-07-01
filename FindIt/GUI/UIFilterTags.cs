// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;
using ColossalFramework.UI;

namespace FindIt.GUI
{
    public class UIFilterTags : UIPanel
    {
        public static UIFilterTags instance;

        public UIButton refresh;
        public UIButton delete;

        public override void Start()
        {
            instance = this;

            /*atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericTabHovered";*/
            size = new Vector2(605, 45);

            refresh = SamsamTS.UIUtils.CreateButton(this);
            refresh.size = new Vector2(75, 35);
            refresh.text = "Refresh";
            refresh.relativePosition = new Vector3(5, 5);
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
