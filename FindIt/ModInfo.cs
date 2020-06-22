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
        public const string version = "1.6.3";

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

                UICheckBox checkBox = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_DM"), Debugging.hideDebugMessages.value, (b) =>
                {
                   Debugging.hideDebugMessages.value = b;
                });
                checkBox.tooltip = Translations.Translate("FIF_SET_DMTP");

                group.AddSpace(10);


                checkBox = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_CMT"), FindIt.centerToolbar.value, (b) =>
                {
                    FindIt.centerToolbar.value = b;

                    if(FindIt.instance != null)
                    {
                        FindIt.instance.UpdateMainToolbar();
                    }
                });
                checkBox.tooltip = Translations.Translate("FIF_SET_CMTTP");

                checkBox = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_UL"), FindIt.unlockAll.value, (b) =>
                {
                    FindIt.unlockAll.value = b;
                });
                checkBox.tooltip = Translations.Translate("FIF_SET_ULTP");

                group.AddSpace(10);

                UICheckBox fixProps = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_BP"), false, (b) =>
                {
                    FindIt.fixBadProps = b;
                });
                fixProps.tooltip = Translations.Translate("FIF_SET_BPTP");

                group.AddSpace(10);

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);

                UIDropDown languageDropDown = (UIDropDown)group.AddDropdown(Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index, (value) => { Translations.Index = value; });

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
