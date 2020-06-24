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
        private static Translator _translator;


        /// <summary>
        /// Static interface to instance's translate method.
        /// </summary>
        /// <param name="text">Key to translate</param>
        /// <returns>Translation (or key if translation failed)</returns>
        public static string Translate(string key) => Instance.Translate(key);

        public static string Language
        {
            get
            {
                return Instance.Language;
            }
            set
            {
                Instance.SetLanguage(value);
            }
        }


        /// <summary>
        /// Static interface to instance's language list property.
        /// Returns an alphabetically-sorted (by unique name) string array of language display names, with an additional "system settings" item as the first item.
        /// Useful for automatically populating drop-down language selection menus; works in conjunction with Index.
        /// </summary>
        public static string[] LanguageList => Instance.LanguageList;


        /// <summary>
        /// The current language index number (equals the index number of the language names list provied bye LanguageList).
        /// Useful for easy automatic drop-down language selection menus, working in conjunction with LanguageList:
        /// Set to set the language to the equivalent LanguageList index.
        /// Get to return the LanguageList index of the current languge.
        /// </summary>
        public static int Index
        {
            get
            {
                return Instance.Index;
            }
            set
            {
                Instance.SetLanguage(value);
            }
        }


        /// <summary>
        /// On-demand initialisation of translator.
        /// </summary>
        /// <returns>Translator instance</returns>
        private static Translator Instance
        {
            get
            {
                if (_translator == null)
                {
                    _translator = new Translator();
                }

                return _translator;
            }
        }
    }


    /// <summary>
    /// Handles translations.  Framework by algernon, based off BloodyPenguin's framework.
    /// </summary>
    public class Translator
    {
        private Language currentLanguage;
        private SortedList<string, Language> languages;
        private string defaultLanguage = "en";
        private int currentIndex = 0;
        public int Index => currentIndex;


        /// <summary>
        /// Returns the current language code if one has specifically been set; otherwise, return "default".
        /// </summary>
        public string Language => currentIndex <= 0 ? "default" : currentLanguage.uniqueName;


        /// <summary>
        /// Returns an alphabetically-sorted (by code) array of language display names, with an additional "system settings" item as the first item.
        /// </summary>
        /// <returns>Readable language names in alphabetical order by unique name (language code) as string array</returns>
        public string[] LanguageList
        {
            get
            {
                // Get list of readable language names.
                List<string> readableNames = languages.Values.Select((language) => language.readableName).ToList();

                // Insert system settings item at the start.
                readableNames.Insert(0, Translate("TRN_SYS"));

                // Return out list as a string array.
                return readableNames.ToArray();
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        public Translator()
        {
            // Initialise languages list.
            languages = new SortedList<string, Language>();

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
        /// Sets the current language; if there's a previously set valid index number, it'll use that.
        /// Otherwise, it will fall back to system settings, falling back further to the default language if unsucessful.
        /// </summary>
        public void SetLanguage()
        {
            SetLanguage(currentIndex);
        }


        /// <summary>
        /// Sets the current language to the provided language code.
        /// If the key isn't in the list of loaded translations, then the system default is attempted instead.
        /// </summary>
        /// <param name="uniqueName">Language unique name (code)</param>
        public void SetLanguage(string uniqueName) => SetLanguage(languages.IndexOfKey(uniqueName) + 1);


        /// <summary>
        /// Sets the current language based on the supplied index number.
        /// If index number is invalid (negative or out-of-bounds) then the system language setting is tried instead.
        /// If even that fails, the default language is used.
        /// </summary>
        /// <param name="index">1-based language index number (zero or negative values will use system language settings instead)</param>
        public void SetLanguage(int index)
        {
            // Don't do anything if no languages have been loaded.
            if (languages != null && languages.Count > 0)
            {
                // If we have a valid index number (greater than zero but within bounds), use that to get the language.
                // Remember that we've effectively added an additional 'system' index at 0, so less-than-or-equals is needed.
                if (index > 0 && index <= languages.Count)
                {
                    // The index is one greater than the 'real' index in our languages list, as index 0 is for the 'system settings' option.
                    currentLanguage = languages.Values[index - 1];

                    // Since we've been given a valid index number, we'll store it for future reference (prevent override of settings by system locale changes).
                    currentIndex = index;
                }
                else
                {
                    // Try to set current language.
                    try
                    {
                        currentLanguage = languages[LocaleManager.instance?.language];
                    }
                    catch
                    {
                        // Don't care.
                        Debugging.Message("couldn't set current system language");
                    }

                    // If we didn't get a valid language, try to fall back.
                    if (currentLanguage == null)
                    {
                        currentLanguage = FallbackLanguage();
                    }


                    // We weren't given a valid index, so remove any stored index.
                    currentIndex = 0;
                }

                Debugging.Message("setting language to " + currentLanguage?.uniqueName ?? "none");
            }
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

                try
                {
                    fallbackLanguage = languages[newName];
                }
                catch
                {
                    // Don't care.
                }
            }

            // If we picked up a fallback language, return that; otherwise, return the default language.
            return fallbackLanguage ?? languages[defaultLanguage];
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
                                languages.Add(translation.uniqueName, translation);
                            }
                            else
                            {
                                Debugging.Message("couldn't deserialize translation file '" + translationFile);
                            }
                        }
                    }

                    // Sort language list by language key alphabetical order.
                    //languages.OrderBy(language => language.uniqueName);
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
            throw new FileNotFoundException("Find It Fix: assembly path not found!");
        }
    }
}