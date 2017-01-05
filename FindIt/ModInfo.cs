using ICities;

using System;
using System.Linq;

using ColossalFramework;
using ColossalFramework.UI;

using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;

namespace FindIt
{
    public class ModInfo : IUserMod
    {
        public ModInfo()
        {
            try
            {
                // Creating setting file
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FindIt.settingsFileName } });
            }
            catch (Exception e)
            {
                DebugUtils.Log("Could load/create the setting file.");
                DebugUtils.LogException(e);
            }
        }

        public string Name
        {
            get { return "Find It! " + version; }
        }

        public string Description
        {
            get { return "Find and organize your assets"; }
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                AssetTagList.instance = new AssetTagList();

                UIHelper group = helper.AddGroup(Name) as UIHelper;
                UIPanel panel = group.self as UIPanel;

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnSettingsUI failed");
                DebugUtils.LogException(e);
            }
        }

        public const string version = "0.1.0";
    }
}
