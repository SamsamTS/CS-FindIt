using System.Xml.Serialization;
using UnityEngine;


namespace FindIt
{
    /// <summary>
    /// Class to hold global mod settings.
    /// </summary>
    [XmlRoot(ElementName = "FindIt2", Namespace = "", IsNullable = false)]
    internal static class Settings
    {
        internal static bool hideDebugMessages = true;

        internal static bool unlockAll = false;

        internal static bool centerToolbar = true;

        internal static bool fixBadProps = false;

        // false = sort by number of assets
        // true = sort alphabetically
        internal static bool customTagListSort = false;
        internal static bool assetCreatorListSort = false;

        internal static bool showPropMarker = false;

        internal static bool showInstancesCounter = false;

        internal static int instanceCounterSort = 0;

        internal static bool includePOinstances = false;

        internal static KeyBinding searchKey = new KeyBinding { keyCode = (int)KeyCode.F, control = true, shift = false, alt = false };

        internal static KeyBinding allKey = new KeyBinding { keyCode = (int)KeyCode.Alpha1, control = false, shift = false, alt = true };

        internal static KeyBinding networkKey = new KeyBinding { keyCode = (int)KeyCode.Alpha2, control = false, shift = false, alt = true };

        internal static KeyBinding ploppableKey = new KeyBinding { keyCode = (int)KeyCode.Alpha3, control = false, shift = false, alt = true };

        internal static KeyBinding growableKey = new KeyBinding { keyCode = (int)KeyCode.Alpha4, control = false, shift = false, alt = true };

        internal static KeyBinding ricoKey = new KeyBinding { keyCode = (int)KeyCode.Alpha5, control = false, shift = false, alt = true };

        internal static KeyBinding grwbRicoKey = new KeyBinding { keyCode = (int)KeyCode.Alpha6, control = false, shift = false, alt = true };

        internal static KeyBinding propKey = new KeyBinding { keyCode = (int)KeyCode.Alpha7, control = false, shift = false, alt = true };

        internal static KeyBinding decalKey = new KeyBinding { keyCode = (int)KeyCode.Alpha8, control = false, shift = false, alt = true };

        internal static KeyBinding treeKey = new KeyBinding { keyCode = (int)KeyCode.Alpha9, control = false, shift = false, alt = true };

        internal static KeyBinding randomSelectionKey = new KeyBinding { keyCode = (int)KeyCode.R, control = false, shift = false, alt = true };

        /*
        /// <summary>
        /// Checks to see if the search hotkey has been pressed.
        /// </summary>
        /// <returns>True if pressed, false otherwise</returns>
        public static bool IsSearchPressed()
        {
            // Don't do anything if a keycode hasn't been set, or if the key isn't pressed.
            if (searchKey.keyCode == (int)KeyCode.None || !Input.GetKey((KeyCode)searchKey.keyCode))
            {
                return false;
            }

            // get the states of the modifier keys
            bool inputControlState = (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
            bool inputShiftState = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
            bool inputAltState = (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr));

            // check if the states match
            if ((searchKey.control == inputControlState) && (searchKey.shift == inputShiftState) && (searchKey.alt == inputAltState))
            {
                return true;
            }
            else return false;
        }
        */
    }


    /// <summary>
    /// Defines the XML settings file.
    /// </summary>
    [XmlRoot(ElementName = "FindIt", Namespace = "", IsNullable = false)]
    public class XMLSettingsFile
    {
        [XmlElement("HideDebugMessages")]
        public bool HideDebugMessages { get => Settings.hideDebugMessages; set => Settings.hideDebugMessages = value; }

        [XmlElement("UnlockAll")]
        public bool UnlockAll { get => Settings.unlockAll; set => Settings.unlockAll = value; }

        [XmlElement("CenterToolbar")]
        public bool CenterToolbar { get => Settings.centerToolbar; set => Settings.centerToolbar = value; }

        [XmlElement("FixBadProps")]
        public bool FixBadProps { get => Settings.fixBadProps; set => Settings.fixBadProps = value; }

        [XmlElement("customTagListSort")]
        public bool CustomTagListSort { get => Settings.customTagListSort; set => Settings.customTagListSort = value; }

        [XmlElement("assetCreatorListSort")]
        public bool AssetCreatorListSort { get => Settings.assetCreatorListSort; set => Settings.assetCreatorListSort = value; }

        [XmlElement("showPropMarker")]
        public bool ShowPropMarker { get => Settings.showPropMarker; set => Settings.showPropMarker = value; }

        [XmlElement("showInstancesCounter")]
        public bool ShowInstancesCounter { get => Settings.showInstancesCounter; set => Settings.showInstancesCounter = value; }

        [XmlElement("instanceCounterSort")]
        public int InstanceCounterSort { get => Settings.instanceCounterSort; set => Settings.instanceCounterSort = value; }

        [XmlElement("includePOinstances")]
        public bool IncludePOinstances { get => Settings.includePOinstances; set => Settings.includePOinstances = value; }

        [XmlElement("SearchKey")]
        public KeyBinding SearchKey { get => Settings.searchKey; set => Settings.searchKey = value; }

        [XmlElement("AllKey")]
        public KeyBinding AllKey { get => Settings.allKey; set => Settings.allKey = value; }

        [XmlElement("NetworkKey")]
        public KeyBinding NetworkKey { get => Settings.networkKey; set => Settings.networkKey = value; }

        [XmlElement("PloppableKey")]
        public KeyBinding PloppableKey { get => Settings.ploppableKey; set => Settings.ploppableKey = value; }

        [XmlElement("GrowableKey")]
        public KeyBinding GrowableKey { get => Settings.growableKey; set => Settings.growableKey = value; }

        [XmlElement("RicoKey")]
        public KeyBinding RicoKey { get => Settings.ricoKey; set => Settings.ricoKey = value; }

        [XmlElement("GrwbRicoKey")]
        public KeyBinding GrwbRicoKey { get => Settings.grwbRicoKey; set => Settings.grwbRicoKey = value; }

        [XmlElement("PropKey")]
        public KeyBinding PropKey { get => Settings.propKey; set => Settings.propKey = value; }

        [XmlElement("DecalKey")]
        public KeyBinding DecalKey { get => Settings.decalKey; set => Settings.decalKey = value; }

        [XmlElement("TreeKey")]
        public KeyBinding TreeKey { get => Settings.treeKey; set => Settings.treeKey = value; }

        [XmlElement("RandomSelectionKey")]
        public KeyBinding RandomSelectionKey { get => Settings.randomSelectionKey; set => Settings.randomSelectionKey = value; }

        [XmlElement("Language")]
        public string Language
        {
            get
            {
                return Translations.Language;
            }
            set
            {
                Translations.Language = value;
            }
        }
    }


    /// <summary>
    /// Basic keybinding class - code and modifiers.
    /// </summary>
    public struct KeyBinding
    {
        [XmlAttribute("KeyCode")]
        public int keyCode;

        [XmlAttribute("Control")]
        public bool control;

        [XmlAttribute("Shift")]
        public bool shift;

        [XmlAttribute("Alt")]
        public bool alt;
    }
}