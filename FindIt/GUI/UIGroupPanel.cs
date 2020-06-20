// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

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
