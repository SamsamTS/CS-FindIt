﻿using ICities;

using System;

using ColossalFramework;
using ColossalFramework.UI;

using FindIt.Redirection;

namespace FindIt
{
    public class ModInfo : IUserMod
    {
        public ModInfo()
        {
            try
            {
                // Creating setting file
                if (GameSettings.FindSettingsFileByName(FindIt.settingsFileName) == null)
                {
                    GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FindIt.settingsFileName } });
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("Couldn't load/create the setting file.");
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
                if (FindIt.instance == null)
                {
                    AssetTagList.instance = new AssetTagList();
                }

                UIHelper group = helper.AddGroup(Name) as UIHelper;
                UIPanel panel = group.self as UIPanel;

                UICheckBox checkBox = (UICheckBox)group.AddCheckbox("Disable debug messages logging", DebugUtils.hideDebugMessages.value, (b) =>
                {
                    DebugUtils.hideDebugMessages.value = b;
                });
                checkBox.tooltip = "If checked, debug messages won't be logged.";

                group.AddSpace(10);


                checkBox = (UICheckBox)group.AddCheckbox("Center the main toolbar", FindIt.centerToolbar.value, (b) =>
                {
                    FindIt.centerToolbar.value = b;

                    if(FindIt.instance != null)
                    {
                        FindIt.instance.UpdateMainToolbar();
                    }
                });
                checkBox.tooltip = "The main toolbar will be centered if it exceed a certain length.";

                checkBox = (UICheckBox)group.AddCheckbox("Unlock all", FindIt.unlockAll.value, (b) =>
                {
                    FindIt.unlockAll.value = b;
                });
                checkBox.tooltip = "Let you select and place items even when locked.";

                group.AddSpace(10);

                UICheckBox fixProps = (UICheckBox)group.AddCheckbox("Fix bad props next loaded save", false, (b) =>
                {
                    FindIt.fixBadProps = b;
                });
                fixProps.tooltip = "Remove all props causing issue\nCheck the option and load your save";

                group.AddSpace(10);

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnSettingsUI failed");
                DebugUtils.LogException(e);
            }
        }

        public const string version = "1.6";
    }
}
