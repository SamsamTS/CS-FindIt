using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;

namespace FindIt.GUI
{
    public class UIGroupPanel : GeneratedGroupPanel
    {
        public override ItemClass.Service service
        {
            get
            {
                return ItemClass.Service.None;
            }
        }

        public override string serviceName
        {
            get
            {
                return "FindIt";
            }
        }

        protected override bool IsServiceValid(PrefabInfo info)
        {
            return true;
        }
    }
}
