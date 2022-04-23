using ColossalFramework;
using FindIt.GUI;
using System;
using System.Xml.Serialization;
using UnityEngine;

namespace FindIt {
    /// <summary>
    /// Class to hold global mod settings.
    /// </summary>
    internal static class Settings {
        internal static bool unlockAll = false;

        internal static bool centerToolbar = true;

        internal static bool useDefaultBrowser = false;

        internal static bool hideExtraUIonVP = false;

        internal static bool disableInstantSearch = false;

        internal static bool resetPanelWhenClosed = false;

        /// <summary>
        /// 0 = sort by parent folder creation time,
        /// 1 = soty by parent folder modification time,
        /// 2 = sort by crp file creation time,
        /// 3 = sory by crp file modification time
        /// </summary>
        internal static int recentDLSorting = 0;

        // false = sort by number of assets
        // true = sort alphabetically
        internal static bool customTagListSort = false;
        internal static bool assetCreatorListSort = false;

        internal static bool showInstancesCounter = false;

        internal static int instanceCounterSort = 0;

        internal static bool includePOinstances = false;

        internal static bool useLightBackground = false;

        internal static bool showSearchTabs = false;

        internal static bool disableUpdateNotice = false;

        internal static bool disableSecondaryKeyboardShortcuts = true;

        internal static double lastUpdateNotice = 0.0;

        internal static bool useRelevanceSort = true;

        internal static bool useWorkshopFilter = true;

        internal static bool useVanillaFilter = true;

        internal static bool showAssetTypePanel = true;

        internal static float assetTypePanelX = -80.0f;

        internal static float assetTypePanelY = -75.0f;

        internal static UnsavedInputKey searchKey = new KeyBinding{ keyCode = (int)KeyCode.F, control = true, shift = false, alt = false }.ToUnSavedInputKey("FIF_SET_KS");

        internal static UnsavedInputKey allKey = new KeyBinding { keyCode = (int)KeyCode.Alpha1, control = false, shift = false, alt = true }.ToUnSavedInputKey("FIF_SE_IA");

        internal static UnsavedInputKey networkKey = new KeyBinding { keyCode = (int)KeyCode.Alpha2, control = false, shift = false, alt = true }.ToUnSavedInputKey("FIF_SE_IN");

        internal static UnsavedInputKey ploppableKey = new KeyBinding { keyCode = (int)KeyCode.Alpha3, control = false, shift = false, alt = true }.ToUnSavedInputKey("FIF_SE_IP");

        internal static UnsavedInputKey growableKey = new KeyBinding { keyCode = (int)KeyCode.Alpha4, control = false, shift = false, alt = true }.ToUnSavedInputKey("FIF_SE_IG");

        internal static UnsavedInputKey ricoKey = new KeyBinding { keyCode = (int)KeyCode.Alpha5, control = false, shift = false, alt = true }.ToUnSavedInputKey("FIF_SE_IR");

        internal static UnsavedInputKey grwbRicoKey = new KeyBinding { keyCode = (int)KeyCode.Alpha6, control = false, shift = false, alt = true }.ToUnSavedInputKey("FIF_SE_IGR");

        internal static UnsavedInputKey propKey = new KeyBinding { keyCode = (int)KeyCode.Alpha7, control = false, shift = false, alt = true }.ToUnSavedInputKey("FIF_SE_IPR");

        internal static UnsavedInputKey decalKey = new KeyBinding { keyCode = (int)KeyCode.Alpha8, control = false, shift = false, alt = true }.ToUnSavedInputKey("FIF_SE_ID");

        internal static UnsavedInputKey treeKey = new KeyBinding { keyCode = (int)KeyCode.Alpha9, control = false, shift = false, alt = true }.ToUnSavedInputKey("FIF_SE_IT");

        internal static UnsavedInputKey randomSelectionKey = new KeyBinding { keyCode = (int)KeyCode.V, control = false, shift = false, alt = true }.ToUnSavedInputKey("FIF_GR_RAN");

        internal static void RegisterHotkeys() {
            UnifiedUI.Helpers.UUIHelpers.RegisterHotkeys(
                onToggle: delegate () { OpenFindIt(-1); },
                activationKey: searchKey);
            UnifiedUI.Helpers.UUIHelpers.RegisterHotkeys(
                onToggle: delegate () { OpenFindIt(0); },
                activationKey: allKey);
            UnifiedUI.Helpers.UUIHelpers.RegisterHotkeys(
                onToggle: delegate () { OpenFindIt(1); },
                activationKey: networkKey);
            UnifiedUI.Helpers.UUIHelpers.RegisterHotkeys(
                onToggle: delegate () { OpenFindIt(2); },
                activationKey: ploppableKey);
            UnifiedUI.Helpers.UUIHelpers.RegisterHotkeys(
                onToggle: delegate () { OpenFindIt(3); },
                activationKey: grwbRicoKey);
            UnifiedUI.Helpers.UUIHelpers.RegisterHotkeys(
                onToggle: delegate () { OpenFindIt(4); },
                activationKey: ricoKey);
            UnifiedUI.Helpers.UUIHelpers.RegisterHotkeys(
                onToggle: delegate () { OpenFindIt(5); },
                activationKey: grwbRicoKey);
            UnifiedUI.Helpers.UUIHelpers.RegisterHotkeys(
                onToggle: delegate () { OpenFindIt(6); },
                activationKey: propKey);
            UnifiedUI.Helpers.UUIHelpers.RegisterHotkeys(
                onToggle: delegate () { OpenFindIt(7); },
                activationKey: decalKey);
            UnifiedUI.Helpers.UUIHelpers.RegisterHotkeys(
                onToggle: delegate () { OpenFindIt(8); },
                activationKey: treeKey);
            UnifiedUI.Helpers.UUIHelpers.RegisterHotkeys(
                onToggle: delegate () { OpenFindIt(-2); },
                activationKey: randomSelectionKey);
        }

        public static void OpenFindIt(int index)
        {
            try
            {
                // secondary keyboard shortcuts
                // if users choose to disable secondary hotkeys when Find it is invisible, don't do anything
                if (index != -1 && Settings.disableSecondaryKeyboardShortcuts && !FindIt.instance.searchBox.isVisible)
                {
                    return;
                }

                if (index > -1)
                {
                    if (index > 5 && !FindIt.isRicoEnabled)
                    {
                        index -= 2;
                    }
                    FindIt.instance.searchBox.typeFilter.selectedIndex = index;
                }

                // If the searchbox isn't visible, simulate a click on the main button.
                if (!FindIt.instance.searchBox.isVisible)
                {
                    FindIt.instance.mainButton.SimulateClick();
                }

                if (index == -2)
                {
                    UISearchBox.instance.PickRandom();
                }
                else
                {
                    // From Brot:
                    // Simulate a search
                    // Select search box text only if FindIt was opened via a category-specific hotkey or the "all"
                    // hotkey. This is intended to make overall behaviour more intuitive now that we're storing search
                    // queries separately for each asset category. This way, when you open FindIt directly to a specific
                    // category, you'll be all set up for starting a new search, but when you open it using the general
                    // hotkey, you'll start out ready for placement of whatever asset was last selected, without having
                    // to press Return first to get rid of the text selection.
                    if (index > -1)
                    {
                        FindIt.instance.searchBox.input.Focus();
                        FindIt.instance.searchBox.input.SelectAll();
                    }
                    else
                    {
                        FindIt.instance.searchBox.input.Focus(); // To-do. without the focus() the camera will move when F is pressed. need to avoid this.
                        FindIt.instance.searchBox.input.SelectAll(); // To-do
                        //FindIt.instance.searchBox.Search(); To-do
                    }
                }
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }
        }
    }

    /// <summary>
    /// Defines the XML settings file.
    /// </summary>
    [XmlRoot(ElementName = "FindIt", Namespace = "", IsNullable = false)]
    public class XMLSettingsFile {
        [XmlElement("UnlockAll")]
        public bool UnlockAll { get => Settings.unlockAll; set => Settings.unlockAll = value; }

        [XmlElement("CenterToolbar")]
        public bool CenterToolbar { get => Settings.centerToolbar; set => Settings.centerToolbar = value; }

        [XmlElement("UseDefaultBrowser")]
        public bool UseDefaultBrowser { get => Settings.useDefaultBrowser; set => Settings.useDefaultBrowser = value; }

        [XmlElement("HideExtraUIonVP")]
        public bool HideExtraUIonVP { get => Settings.hideExtraUIonVP; set => Settings.hideExtraUIonVP = value; }

        [XmlElement("DisableInstantSearch")]
        public bool DisableInstantSearch { get => Settings.disableInstantSearch; set => Settings.disableInstantSearch = value; }

        [XmlElement("ResetPanelWhenClosed")]
        public bool ResetPanelWhenClosed { get => Settings.resetPanelWhenClosed; set => Settings.resetPanelWhenClosed = value; }

        [XmlElement("RecentDLSorting")]
        public int RecentDLSorting { get => Settings.recentDLSorting; set => Settings.recentDLSorting = value; }

        [XmlElement("CustomTagListSort")]
        public bool CustomTagListSort { get => Settings.customTagListSort; set => Settings.customTagListSort = value; }

        [XmlElement("AssetCreatorListSort")]
        public bool AssetCreatorListSort { get => Settings.assetCreatorListSort; set => Settings.assetCreatorListSort = value; }

        [XmlElement("ShowInstancesCounter")]
        public bool ShowInstancesCounter { get => Settings.showInstancesCounter; set => Settings.showInstancesCounter = value; }

        [XmlElement("InstanceCounterSort")]
        public int InstanceCounterSort { get => Settings.instanceCounterSort; set => Settings.instanceCounterSort = value; }

        [XmlElement("IncludePOinstances")]
        public bool IncludePOinstances { get => Settings.includePOinstances; set => Settings.includePOinstances = value; }

        [XmlElement("UseLightBackground")]
        public bool UseLightBackground { get => Settings.useLightBackground; set => Settings.useLightBackground = value; }

        [XmlElement("ShowSearchTabs")]
        public bool ShowSearchTabs { get => Settings.showSearchTabs; set => Settings.showSearchTabs = value; }

        [XmlElement("DisableUpdateNotice")]
        public bool DisableUpdateNotice { get => Settings.disableUpdateNotice; set => Settings.disableUpdateNotice = value; }

        [XmlElement("DisableSecondaryKeyboardShortcuts")]
        public bool DisableSecondaryKeyboardShortcuts { get => Settings.disableSecondaryKeyboardShortcuts; set => Settings.disableSecondaryKeyboardShortcuts = value; }

        [XmlElement("UseRelevanceSort")]
        public bool UseRelevanceSort { get => Settings.useRelevanceSort; set => Settings.useRelevanceSort = value; }

        [XmlElement("UseWorkshopFilter")]
        public bool UseWorkshopFilter { get => Settings.useWorkshopFilter; set => Settings.useWorkshopFilter = value; }

        [XmlElement("UseVanillaFilter")]
        public bool UseVanillaFilter { get => Settings.useVanillaFilter; set => Settings.useVanillaFilter = value; }

        [XmlElement("LastUpdateNotice")]
        public double LastUpdateNotice { get => Settings.lastUpdateNotice; set => Settings.lastUpdateNotice = value; }

        [XmlElement("ShowAssetTypePanel")]
        public bool ShowAssetTypePanel { get => Settings.showAssetTypePanel; set => Settings.showAssetTypePanel = value; }

        [XmlElement("AssetTypePanelX")]
        public float AssetTypePanelX { get => Settings.assetTypePanelX; set => Settings.assetTypePanelX = value; }

        [XmlElement("AssetTypePanelY")]
        public float AssetTypePanelY { get => Settings.assetTypePanelY; set => Settings.assetTypePanelY = value; }

        [XmlElement("SearchKey")]
        public KeyBinding SearchKey { get => Settings.searchKey.KeyBinding; set => Settings.searchKey.KeyBinding = value; }

        [XmlElement("AllKey")]
        public KeyBinding AllKey { get => Settings.allKey.KeyBinding; set => Settings.allKey.KeyBinding = value; }

        [XmlElement("NetworkKey")]
        public KeyBinding NetworkKey { get => Settings.networkKey.KeyBinding; set => Settings.networkKey.KeyBinding = value; }

        [XmlElement("PloppableKey")]
        public KeyBinding PloppableKey { get => Settings.ploppableKey.KeyBinding; set => Settings.ploppableKey.KeyBinding = value; }

        [XmlElement("GrowableKey")]
        public KeyBinding GrowableKey { get => Settings.growableKey.KeyBinding; set => Settings.growableKey.KeyBinding = value; }

        [XmlElement("RicoKey")]
        public KeyBinding RicoKey { get => Settings.ricoKey.KeyBinding; set => Settings.ricoKey.KeyBinding = value; }

        [XmlElement("GrwbRicoKey")]
        public KeyBinding GrwbRicoKey { get => Settings.grwbRicoKey.KeyBinding; set => Settings.grwbRicoKey.KeyBinding = value; }

        [XmlElement("PropKey")]
        public KeyBinding PropKey { get => Settings.propKey.KeyBinding; set => Settings.propKey.KeyBinding = value; }

        [XmlElement("DecalKey")]
        public KeyBinding DecalKey { get => Settings.decalKey.KeyBinding; set => Settings.decalKey.KeyBinding = value; }

        [XmlElement("TreeKey")]
        public KeyBinding TreeKey { get => Settings.treeKey.KeyBinding; set => Settings.treeKey.KeyBinding = value; }

        [XmlElement("RandomSelectionKey")]
        public KeyBinding RandomSelectionKey { get => Settings.randomSelectionKey.KeyBinding; set => Settings.randomSelectionKey.KeyBinding = value; }

        [XmlElement("Language")]
        public string Language {
            get {
                return Translations.CurrentLanguage;
            }
            set {
                Translations.CurrentLanguage = value;
            }
        }
    }


    /// <summary>
    /// Basic keybinding class - code and modifiers.
    /// </summary>
    public struct KeyBinding {
        [XmlAttribute("KeyCode")]
        public int keyCode;

        [XmlAttribute("Control")]
        public bool control;

        [XmlAttribute("Shift")]
        public bool shift;

        [XmlAttribute("Alt")]
        public bool alt;

        internal InputKey Encode() => SavedInputKey.Encode((KeyCode)keyCode, control, shift, alt);

        internal UnsavedInputKey ToUnSavedInputKey(string translationkey) => new UnsavedInputKey(translationkey, Encode());
    }

    ///<summary> since we want to save in XML file we don't save in CS settings file.</summary>
    public class UnsavedInputKey : UnifiedUI.Helpers.UnsavedInputKey {
        public UnsavedInputKey(string name, InputKey key) : base(name, "FindIt2", key) { 
            this.m_Synced = true; // no need to sync with file.
        }

        public override void OnConflictResolved() => XMLUtils.SaveSettings();

        public KeyBinding KeyBinding {
            get => new KeyBinding { keyCode = (int)Key, control = Control, shift = Shift, alt = Alt};
            set => this.value = value.Encode();
        }
    }
}