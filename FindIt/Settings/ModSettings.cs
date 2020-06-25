using System.Xml.Serialization;
using UnityEngine;
using ColossalFramework;


namespace FindIt
{
    /// <summary>
    /// Class to hold global mod settings.
    /// </summary>
    internal static class Settings
    {
        internal static bool hideDebugMessages = true;
        internal static bool unlockAll =false;
        internal static bool centerToolbar = true;
        internal static bool fixBadProps = false;
        internal static InputKey searchKey = SavedInputKey.Encode(KeyCode.F, true, false, false);


        /// <summary>
        /// Checks to see if the search hotkey has been pressed.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        public static bool IsSearchPressed(Event e)
        {
            // Keycode is lower 7 nibbles of CO InputKey.
            KeyCode keyCode = (KeyCode)(searchKey & 0xFFFFFFF);

            // Don't do anything if a keycode hasn't been set, or no key has been pressed.
            if (keyCode == KeyCode.None || !Input.GetKey(keyCode))
            {
                return false;
            }

            // Check for control (CO InputKey mask 0x4 of high nibble).
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) != ((searchKey & 0x40000000) != 0))
            {
                return false;
            }

            // Check for shift (CO InputKey mask 0x2 of high nibble).
            if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) != ((searchKey & 0x20000000) != 0))
            {
                return false;
            }

            // Check for alt (CO InputKey mask 0x1 of high nibble).
            if ((Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr)) != ((searchKey & 0x10000000) != 0))
            {
                return false;
            }

            // If we got here, all checks have passed - search has been pressed.
            return true;
        }
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

        [XmlElement("SearchKey")]
        public int SearchKey { get => Settings.searchKey; set => Settings.searchKey = value; }

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
}