// From algernon's Advanced Building Level Control mod
// https://github.com/algernon-A/AdvancedBuildingLevelControl

using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Serialization;
using ICities;
using ColossalFramework;
using ColossalFramework.Plugins;
using ColossalFramework.Globalization;

namespace FindIt
{
    /// <summary>
    /// Static class to provide translation interface.
    /// </summary>
    public static class Translations
    {
        // Instance reference.
        private static Translator _instance;


        /// <summary>
        /// Static interface to instance's translate method.
        /// </summary>
        /// <param name="text">Key to translate</param>
        /// <returns>Translation (or key if translation failed)</returns>
        public static string Translate(string key)
        {
            // Initialise translator if we haven't already.
            if (_instance == null)
            {
                _instance = new Translator();
            }

            return _instance.Translate(key);
        }
    }


    /// <summary>
    /// Handles translations.  Based off BloodyPenguin's framework.
    /// </summary>
    public class Translator
    {
        private Language currentLanguage;
        private List<Language> languages;
        private string defaultLanguage = "en";

        /// <summary>
        /// Constructor.
        /// </summary>
        public Translator()
        {
            // Initialise languages list.
            languages = new List<Language>();

            // Load translation files and set the current language.
            LoadLanguages();
            SetLanguage();

            // Event handler to update the current language when system locale changes.
            LocaleManager.eventLocaleChanged += SetLanguage;
        }


        /// <summary>
        /// Returns the translation for the given key in the current language.
        /// </summary>
        /// <param name="key">Translation key to transate</param>
        /// <returns>Translation </returns>
        public string Translate(string key)
        {
            // Check that a valid current language is set.
            if (currentLanguage != null)
            {
                // Check that the current key is included in the translation.
                if (currentLanguage.translationDictionary.ContainsKey(key))
                {
                    // All good!  Return translation.
                    return currentLanguage.translationDictionary[key];
                }
                else
                {
                    Debugging.Message("no translation for language " + currentLanguage.uniqueName + " found for key " + key);

                    // Attempt fallack language; if even that fails, just return the key.
                    return FallbackLanguage().translationDictionary.ContainsKey(key) ? FallbackLanguage().translationDictionary[key] ?? key : key;
                }
            }
            else
            {
                Debugging.Message("no current language set when translating key " + key);
            }

            // If we've made it this far, something went wrong; just return the key.
            return key;
        }


        /// <summary>
        /// Sets the current language based on system settings.
        /// </summary>
        private void SetLanguage()
        {
            // Don't do anything if no languages have been loaded, or the LocaleManager isn't available.
            if (languages != null && languages.Count > 0 && LocaleManager.exists)
            {
                // Try to set current language, using fallback if null.
                currentLanguage = languages.Find(language => language.uniqueName == LocaleManager.instance.language) ?? FallbackLanguage();
            }

            // algernon: This is where any calls needed to insert for 'live' updating of languages should go instead.
            // Update 'control levels' checkbox text.
            //BuildingPanelManager.SetText();
        }


        /// <summary>
        /// Returns a fallback language reference in case the primary one fails (for whatever reason).
        /// </summary>
        /// <returns>Fallback language reference</returns>
        private Language FallbackLanguage()
        {
            Language fallbackLanguage = null;

            // First, check to see if there is a shortened version of this language id (e.g. zh-tw -> zh).
            if (LocaleManager.instance.language.Length > 2)
            {
                string newName = LocaleManager.instance.language.Substring(0, 2);
                Debugging.Message("language " + LocaleManager.instance.language + " failed; trying " + newName);

                fallbackLanguage = languages.Find(language => language.uniqueName == newName);
            }

            // If we picked up a fallback language, return that; otherwise, return the default language.
            return fallbackLanguage ?? languages.Find(language => language.uniqueName == defaultLanguage);
        }


        /// <summary>
        /// Loads languages from XML files.
        /// </summary>
        private void LoadLanguages()
        {
            // Clear existing dictionary.
            languages.Clear();

            // Get the current assembly path and append our locale directory name.
            string assemblyPath = GetAssemblyPath();
            if (!assemblyPath.IsNullOrWhiteSpace())
            {
                string localePath = Path.Combine(assemblyPath, "Translations");

                // Ensure that the directory exists before proceeding.
                if (Directory.Exists(localePath))
                {
                    // Load each file in directory and attempt to deserialise as a translation file.
                    string[] translationFiles = Directory.GetFiles(localePath);
                    foreach (string translationFile in translationFiles)
                    {
                        using (StreamReader reader = new StreamReader(translationFile))
                        {
                            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Language));
                            if (xmlSerializer.Deserialize(reader) is Language translation)
                            {
                                // Got one!  add it to the list.
                                languages.Add(translation);
                            }
                            else
                            {
                                Debugging.Message("couldn't deserialize translation file '" + translationFile);
                            }
                        }
                    }
                }
                else
                {
                    Debugging.Message("translations directory not found");
                }
            }
            else
            {
                Debugging.Message("assembly path was empty");
            }
        }


        /// <summary>
        /// Returns the filepath of the mod assembly.
        /// </summary>
        /// <returns>Mod assembly filepath</returns>
        private string GetAssemblyPath()
        {
            // Get list of currently active plugins.
            IEnumerable<PluginManager.PluginInfo> plugins = PluginManager.instance.GetPluginsInfo();

            // Iterate through list.
            foreach (PluginManager.PluginInfo plugin in plugins)
            {
                try
                {
                    // Get all (if any) mod instances from this plugin.
                    IUserMod[] mods = plugin.GetInstances<IUserMod>();

                    // Check to see if the primary instance is this mod.
                    if (mods.FirstOrDefault() is ModInfo)
                    {
                        // Found it! Return path.
                        return plugin.modPath;
                    }
                }
                catch
                {
                    // Don't care.
                }
            }

            // If we got here, then we didn't find the assembly.
            Debugging.Message("assembly path not found");
            throw new FileNotFoundException("Find It! Fix: assembly path not found!");
        }
    }
}