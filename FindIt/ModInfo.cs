// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ICities;
using System;
using ColossalFramework.UI;
using CitiesHarmony.API;
using System.IO;
using ColossalFramework.IO;
using UnityEngine;

namespace FindIt
{
    public class ModInfo : IUserMod
    {
        public const string version = "2.3.1";
        public const bool isBeta = true;
        public const double updateNoticeDate = 20210214;
        public const string updateNotice =

            "- Update to CitiesHarmony.API 2.0.0\n\n" +

            "From 2.3:\n\n" + 
            "- Add \"Locate Next Instance\" tool:\n" +
            "   Click an asset thumbnail, then click the new tool icon. Find It 2 will\n" +
            "   find a placed instance of the asset and move the camera to the placed\n" +
            "   instance. Click the icon again to find the next instance.Hold SHIFT and\n" +
            "   click the tool icon to locate next Procedural Objects instance\n\n" +

            "- If an asset has terrible default thumbnail, add the custom tag\n" +
            "   \"bad_thumbnail\" to the asset and Find It 2 will attempt to generate\n" +
            "   a custom thumbnail\n" +
            "   Not applicable to every asset. Not applicable to networks\n" +
            "   Remove the custom tag and RESTART the game to change back to\n" +
            "   default thumbnail\n\n" +

            "- Add \"Refresh Display\" tool:\n" +
            "   You can use it to refresh the asset instance counter\n\n" +

            "- Add \"Used Assets\" filter\n\n" +

            "- When the \"Sub-buildings\" filter is selected, the asset type dropdown\n" +
            "   menu will be set to \"All\" automatically\n\n";

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
                group.AddSpace(10);

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

                // Use system default browser instead of steam overlay
                UICheckBox useDefaultBrowser = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_DB"), Settings.useDefaultBrowser, (b) =>
                {
                    Settings.useDefaultBrowser = b;
                    XMLUtils.SaveSettings();
                });
                useDefaultBrowser.tooltip = Translations.Translate("FIF_SET_DBTP");
                group.AddSpace(10);

                // Disable update notice
                UICheckBox disableUpdateNotice = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_DUN"), Settings.disableUpdateNotice, (b) =>
                {
                    Settings.disableUpdateNotice = b;
                    XMLUtils.SaveSettings();
                });
                useDefaultBrowser.tooltip = Translations.Translate("FIF_SET_DBTP");
                group.AddSpace(10);

                // Disable update notice
                UICheckBox separateSearchKeyword = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_SSK"), Settings.separateSearchKeyword, (b) =>
                {
                    Settings.separateSearchKeyword = b;
                    XMLUtils.SaveSettings();
                });
                separateSearchKeyword.tooltip = Translations.Translate("FIF_SET_SSKTP");
                group.AddSpace(10);

                // languate settings
                UIDropDown languageDropDown = (UIDropDown)group.AddDropdown(Translations.Translate("TRN_CHOICE"), Translations.LanguageList, Translations.Index, (value) =>
                {
                    Translations.Index = value;
                    XMLUtils.SaveSettings();
                });

                languageDropDown.width = 300;
                group.AddSpace(10);

                // show path to FindItCustomTags.xml
                string path = Path.Combine(DataLocation.localApplicationData, "FindItCustomTags.xml");
                UITextField customTagsFilePath = (UITextField)group.AddTextfield(Translations.Translate("FIF_SET_CTFL"), path, _ => { }, _ => { });
                customTagsFilePath.width = panel.width - 30;

                // from aubergine10's AutoRepair
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    group.AddButton(Translations.Translate("FIF_SET_CTFOP"), () => System.Diagnostics.Process.Start("explorer.exe", "/select," + path));
                }

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
