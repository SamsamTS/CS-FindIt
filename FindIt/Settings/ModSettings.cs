using ColossalFramework;
using System.Xml.Serialization;


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
        internal static InputKey searchKey;
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