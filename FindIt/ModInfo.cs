// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ICities;
using System;
using ColossalFramework.UI;
using CitiesHarmony.API;
using System.IO;
using ColossalFramework.IO;

namespace FindIt
{
    public class ModInfo : IUserMod
    {
        public const string version = "2.7.5-beta2";
        public const bool isBeta = true;
        public const bool debug = false;
        public const double updateNoticeDate = 20211215;
        public const string updateNotice =

            "- Better error handling for problematic assets\n\n" +

            "- Fix DLC icon scaling bug on vanilla panels when toolbar mods are used\n";

        public string Name
        {
            get { return "Find It! " + (isBeta ? "[BETA] " : "") + version; }
        }

        public string Description
        {
            get { return Translations.Translate("FIF_DESC"); }
        }

        /// <summary>
        /// Called by the game when mod is enabled.
        /// </summary>
        public void OnEnabled()
        {
            // Apply Harmony patches via Cities Harmony.
            // Called here instead of OnCreated to allow the auto-downloader to do its work prior to launch.
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
            Debugging.Message("Harmony patches applied");
            // Load settings here.
            XMLUtils.LoadSettings();
            Debugging.Message("XML Settings loaded");
        }

        /// <summary>
        /// Called by the game when the mod is disabled.
        /// </summary>
        public void OnDisabled()
        {
            // Unapply Harmony patches via Cities Harmony.
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }
        }

        /// <summary>
        /// Called by the game when the mod options panel is setup.
        /// </summary>
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

                // Center the main toolbar
                UICheckBox centerToolbar = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_CMT"), Settings.centerToolbar, (b) =>
                {
                    Settings.centerToolbar = b;
                    XMLUtils.SaveSettings();

                    if (FindIt.instance != null)
                    {
                        FindIt.instance.UpdateMainToolbar();
                    }
                });
                centerToolbar.tooltip = Translations.Translate("FIF_SET_CMTTP");
                group.AddSpace(5);

                // Unlock all
                UICheckBox unlockAll = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_UL"), Settings.unlockAll, (b) =>
                {
                    Settings.unlockAll = b;
                    XMLUtils.SaveSettings();
                });
                unlockAll.tooltip = Translations.Translate("FIF_SET_ULTP");
                group.AddSpace(5);

                /*
                // Fix bad props next loaded save
                // Implemented by samsamTS. Only needed for pre-2018 savefiles 
                UICheckBox fixProps = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_BP"), false, (b) =>
                {
                    Settings.fixBadProps = b;
                    XMLUtils.SaveSettings();
                });
                fixProps.tooltip = Translations.Translate("FIF_SET_BPTP");
                group.AddSpace(10);
                */

                // Use system default browser instead of steam overlay
                UICheckBox useDefaultBrowser = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_DB"), Settings.useDefaultBrowser, (b) =>
                {
                    Settings.useDefaultBrowser = b;
                    XMLUtils.SaveSettings();
                });
                useDefaultBrowser.tooltip = Translations.Translate("FIF_SET_DBTP");
                group.AddSpace(5);

                // Do not show extra Find It 2 UI on vanilla panels
                UICheckBox hideExtraUIonVP = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_UIVP"), Settings.hideExtraUIonVP, (b) =>
                {
                    Settings.hideExtraUIonVP = b;
                    XMLUtils.SaveSettings();
                });
                hideExtraUIonVP.tooltip = Translations.Translate("FIF_SET_UIVPTP");
                group.AddSpace(5);

                // Disable instant search
                UICheckBox disableInstantSearch = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_DIS"), Settings.disableInstantSearch, (b) =>
                {
                    Settings.disableInstantSearch = b;
                    XMLUtils.SaveSettings();
                });
                disableInstantSearch.tooltip = Translations.Translate("FIF_SET_DISTP");
                group.AddSpace(5);

                // Disable update notice
                UICheckBox disableUpdateNotice = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_DUN"), Settings.disableUpdateNotice, (b) =>
                {
                    Settings.disableUpdateNotice = b;
                    XMLUtils.SaveSettings();
                });
                group.AddSpace(5);

                // Disable secondary keyboard shortcuts
                UICheckBox disableSecondaryKeyboardShortcuts = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_DSK"), Settings.disableSecondaryKeyboardShortcuts, (b) =>
                {
                    Settings.disableSecondaryKeyboardShortcuts = b;
                    XMLUtils.SaveSettings();
                });
                disableSecondaryKeyboardShortcuts.tooltip = Translations.Translate("FIF_SET_DSKTP");
                group.AddSpace(5);

                // Recent DLs sorting
                string[] RencentDLsSortingList =
                {
                    Translations.Translate("FIF_SET_RECPC"),
                    Translations.Translate("FIF_SET_RECPM"),
                    Translations.Translate("FIF_SET_RECFC"),
                    Translations.Translate("FIF_SET_RECFM")
                };
                UIDropDown rencentDLsDropDown = (UIDropDown)group.AddDropdown(Translations.Translate("FIF_SET_REC"), RencentDLsSortingList, Settings.recentDLSorting, (value) =>
                {
                    Settings.recentDLSorting = value;
                    XMLUtils.SaveSettings();
                });
                rencentDLsDropDown.width = 400;
                group.AddSpace(5);

                // languate settings
                UIDropDown languageDropDown = (UIDropDown)group.AddDropdown(Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index, (value) =>
                {
                    Translations.Index = value;
                    XMLUtils.SaveSettings();
                });
                languageDropDown.width = 300;
                group.AddSpace(5);

                // show path to FindItCustomTags.xml
                string customTagsFilePathStr = Path.Combine(DataLocation.localApplicationData, "FindItCustomTags.xml");
                UITextField customTagsFilePath = (UITextField)group.AddTextfield(Translations.Translate("FIF_SET_CTFL"), customTagsFilePathStr, _ => { }, _ => { });
                customTagsFilePath.width = panel.width - 30;
                group.AddButton(Translations.Translate("FIF_SET_CTFOP"), () => System.Diagnostics.Process.Start(DataLocation.localApplicationData));
                group.AddSpace(5);

                // show path to FindIt.xml
                string configFilePathStr = Path.Combine(DataLocation.executableDirectory, "FindIt.xml");
                UITextField configFilePath = (UITextField)group.AddTextfield(Translations.Translate("FIF_SET_CFFL"), configFilePathStr, _ => { }, _ => { });
                configFilePath.width = panel.width - 30;
                group.AddButton(Translations.Translate("FIF_SET_CTFOP"), () => System.Diagnostics.Process.Start(DataLocation.executableDirectory));

                // shortcut keys
                panel.gameObject.AddComponent<MainButtonKeyMapping>();
                panel.gameObject.AddComponent<AllKeyMapping>();
                panel.gameObject.AddComponent<NetworkKeyMapping>();
                panel.gameObject.AddComponent<PloppableKeyMapping>();
                panel.gameObject.AddComponent<GrowableKeyMapping>();
                panel.gameObject.AddComponent<RicoKeyMapping>();
                panel.gameObject.AddComponent<GrwbRicoKeyMapping>();
                panel.gameObject.AddComponent<PropKeyMapping>();
                panel.gameObject.AddComponent<DecalKeyMapping>();
                panel.gameObject.AddComponent<TreeKeyMapping>();
                panel.gameObject.AddComponent<RandomSelectionKeyMapping>();
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
