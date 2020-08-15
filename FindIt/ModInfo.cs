// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ICities;
using System;
using ColossalFramework.UI;
using CitiesHarmony.API;
using System.IO;
using ColossalFramework.IO;
using UnityEngine;
using FindIt.GUI;

namespace FindIt
{
    public class ModInfo : IUserMod
    {
        public const string version = "2.0.2-beta5";
        public const bool isBeta = true;

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
                Debugging.Message("Harmony patches reverted");
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

                /*

                // Sort custom tag list alphabetically. Default = sort by number of assets in each tag
                UICheckBox customTagListSort = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_CTLS"), Settings.customTagListSort, (b) =>
                {
                    Settings.customTagListSort = b;
                    XMLUtils.SaveSettings();
                    if (FindIt.instance != null && UISearchBox.instance != null && UIFilterTag.instance != null)
                    {
                        UIFilterTag.instance.UpdateCustomTagList();
                        UISearchBox.instance.Search();
                    }
                });
                group.AddSpace(10);

                // Sort asset creator list alphabetically. Default = sort by number of assets in each tag
                UICheckBox assetCreatorListSort = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_ACLS"), Settings.assetCreatorListSort, (b) =>
                {
                    Settings.assetCreatorListSort = b;
                    XMLUtils.SaveSettings();
                    if (FindIt.instance != null && UISearchBox.instance != null && UIFilterExtra.instance != null)
                    {
                        UIFilterExtra.instance.UpdateAssetCreatorList();
                        UISearchBox.instance.Search();
                    }
                });
                group.AddSpace(10);

                // Show prop markers in 'game' mode
                UICheckBox showPropMarker = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_PM"), Settings.showPropMarker, (b) =>
                {
                    Settings.showPropMarker = b;
                    XMLUtils.SaveSettings();

                    if (FindIt.instance?.searchBox != null && UIFilterProp.instance != null)
                    {
                        UIFilterProp.instance.UpdateMarkerToggleVisibility();
                    }
                });
                showPropMarker.tooltip = Translations.Translate("FIF_SET_PMTP");
                group.AddSpace(10);

                // Show the number of existing instances of each asset
                UICheckBox showInstancesCounter = (UICheckBox)group.AddCheckbox(Translations.Translate("FIF_SET_IC"), Settings.showInstancesCounter, (b) =>
                {
                    Settings.showInstancesCounter = b;
                    XMLUtils.SaveSettings();
                    if (FindIt.instance?.scrollPanel != null)
                    {
                        if (Settings.showInstancesCounter && AssetTagList.instance?.prefabInstanceCountDictionary != null)
                        {
                            AssetTagList.instance.UpdatePrefabInstanceCount();
                        }
                        FindIt.instance.scrollPanel.Refresh();
                    }
                });
                group.AddSpace(10);

                */

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
                group.AddSpace(10);

                // shortcut key
                panel.gameObject.AddComponent<OptionsKeymapping>();
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
