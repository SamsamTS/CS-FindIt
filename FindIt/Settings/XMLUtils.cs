using System;
using System.IO;
using System.Xml.Serialization;


namespace FindIt
{
    /// <summary>
    /// XML serialization/deserialization utilities class.
    /// </summary>
    internal static class XMLUtils
    {
        internal static readonly string SettingsFileName = "FindIt.xml";


        /// <summary>
        /// Load settings from XML file.
        /// </summary>
        internal static void LoadSettings()
        {
            try
            {
                // Check to see if configuration file exists.
                if (File.Exists(SettingsFileName))
                {
                    // Read it.
                    using (StreamReader reader = new StreamReader(SettingsFileName))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLSettingsFile));
                        if (!(xmlSerializer.Deserialize(reader) is XMLSettingsFile xmlSettingsFile))
                        {
                            Debugging.Message("couldn't deserialize settings file");
                        }
                    }
                }
                else
                {
                    Debugging.Message("no settings file found");
                }
            }
            catch (Exception e)
            {
                Debugging.Message("exception reading XML settings file");
                Debugging.LogException(e);
            }
        }


        /// <summary>
        /// Save settings to XML file.
        /// </summary>
        internal static void SaveSettings()
        {
            try
            {
                // Pretty straightforward.  Serialisation is within GBRSettingsFile class.
                using (StreamWriter writer = new StreamWriter(SettingsFileName))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(XMLSettingsFile));
                    xmlSerializer.Serialize(writer, new XMLSettingsFile());
                }
            }
            catch (Exception e)
            {
                Debugging.Message("exception saving XML settings file");
                Debugging.LogException(e);
            }
        }
    }
}