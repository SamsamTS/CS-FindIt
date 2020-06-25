// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ICities;
using System;
using ColossalFramework;
using ColossalFramework.UI;

namespace FindIt
{
    public class ModInfo : IUserMod
    {
        public const string version = "1.6.5";

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
                Debugging.Message("Couldn't load/create the setting file.");
                Debugging.LogException(e);
            }
        }

        public string Name
        {
            get { return "Find It! Fix [Test] " + version; }
        }

        public string Description
        {
            get { return Translations.Translate("FIF_DESC");  }
        }

        public void OnEnabled()
        {
            XMLUtils.LoadSettings();
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

                // Disable debug messages logging
                 UICheckBox checkBox = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_DM"), Settings.hideDebugMessages, (b) =>
                {
                    Settings.hideDebugMessages = b;
                    XMLUtils.SaveSettings();
                });
                checkBox.tooltip = Translations.Translate("FIF_SET_DMTP");

                group.AddSpace(10);

                // Center the main toolbar
                checkBox = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_CMT"), Settings.centerToolbar, (b) =>
                {
                    Settings.centerToolbar = b;
                    XMLUtils.SaveSettings();

                    if (FindIt.instance != null)
                    {
                        FindIt.instance.UpdateMainToolbar();
                    }
                });
                checkBox.tooltip = Translations.Translate("FIF_SET_CMTTP");

                // Unlock all
                checkBox = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_UL"), Settings.unlockAll, (b) =>
                {
                    Settings.unlockAll = b;
                    XMLUtils.SaveSettings();
                });
                checkBox.tooltip = Translations.Translate("FIF_SET_ULTP");

                group.AddSpace(10);

                // Fix bad props next loaded save
                // Implemented by samsamTS. Need to figure out why this is needed. 
                UICheckBox fixProps = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_BP"), false, (b) =>
                {
                    Settings.fixBadProps = b;
                    XMLUtils.SaveSettings();
                });
                fixProps.tooltip = Translations.Translate("FIF_SET_BPTP");

                group.AddSpace(10);

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);

                // languate settings
                UIDropDown languageDropDown = (UIDropDown)group.AddDropdown(Translations.Translate("TRN_CHOICE"), Translations.LanguageList, /*Translations.Index,*/ Translations.Index,  (value) =>
                { 
                    Translations.Index = value;
                    XMLUtils.SaveSettings();
                });
                
                languageDropDown.width = 300;
                group.AddSpace(10);
            }
            catch (Exception e)
            {
                Debugging.Message("OnSettingsUI failed");
                Debugging.LogException(e);
            }
        }

    }
}
