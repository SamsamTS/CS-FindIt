using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.PlatformServices;
using ColossalFramework.Packaging;

namespace FindIt
{
    public class Asset
    {
        public string name;
        public PrefabInfo prefab;
        public ulong steamID;
        public string author;
        public string service;
        public float score;

        public HashSet<string> tagsTitle = new HashSet<string>();
        public HashSet<string> tagsDesc = new HashSet<string>();
        public HashSet<string> tagsHash = new HashSet<string>();

        public static string GetLocalizedTitle(PrefabInfo prefab)
        {
            string result;

            if (prefab is BuildingInfo)
            {
                if (Locale.GetUnchecked("BUILDING_TITLE", prefab.name, out result))
                {
                    return result;
                }
            }
            else if (prefab is PropInfo)
            {
                if (Locale.GetUnchecked("PROPS_TITLE", prefab.name, out result))
                {
                    return result;
                }
            }
            else if (prefab is TreeInfo)
            {
                if (Locale.GetUnchecked("TREE_TITLE", prefab.name, out result))
                {
                    return result;
                }
            }

            string name = prefab.name;

            if (name.Contains("."))
            {
                name = prefab.name.Substring(prefab.name.IndexOf('.') + 1);
            }

            if (name.EndsWith("_Data"))
            {
                name = name.Substring(0, name.LastIndexOf("_Data"));
            }

            return Regex.Replace(name, "([A-Z][a-z]+)", " $1");
        }

        public static string GetLocalizedDescription(PrefabInfo prefab)
        {
            string result;

            if (prefab is BuildingInfo)
            {
                if (Locale.GetUnchecked("BUILDING_DESC", prefab.name, out result))
                {
                    return result;
                }
            }
            else if (prefab is PropInfo)
            {
                if (Locale.GetUnchecked("PROPS_DESC", prefab.name, out result))
                {
                    return result;
                }
            }
            else if (prefab is TreeInfo)
            {
                if (Locale.GetUnchecked("TREE_DESC", prefab.name, out result))
                {
                    return result;
                }
            }

            return "";
        }

        public static string GetLocalizedTooltip(PrefabInfo prefab)
        {
            MilestoneInfo unlockMilestone = prefab.GetUnlockMilestone();

            string text = TooltipHelper.Format(new string[]
	        {
		        LocaleFormatter.Title,
		        Asset.GetLocalizedTitle(prefab),
		        LocaleFormatter.Sprite,
		        (!string.IsNullOrEmpty(prefab.m_InfoTooltipThumbnail)) ? prefab.m_InfoTooltipThumbnail : prefab.name,
		        LocaleFormatter.Text,
		        Asset.GetLocalizedDescription(prefab),
		        LocaleFormatter.Locked,
		        (!ToolsModifierControl.IsUnlocked(unlockMilestone)).ToString()
	        });

            string unlockDesc, currentValue, targetValue, progress, locked;
            ToolsModifierControl.GetUnlockingInfo(unlockMilestone, out unlockDesc, out currentValue, out targetValue, out progress, out locked);

            string addTooltip = TooltipHelper.Format(new string[]
	        {
		        LocaleFormatter.LockedInfo,
		        locked,
		        LocaleFormatter.UnlockDesc,
		        unlockDesc,
		        LocaleFormatter.UnlockPopulationProgressText,
		        progress,
		        LocaleFormatter.UnlockPopulationTarget,
		        targetValue,
		        LocaleFormatter.UnlockPopulationCurrent,
		        currentValue
	        });

            text = TooltipHelper.Append(text, addTooltip);
            PrefabAI aI = prefab.GetAI();
            if (aI != null)
            {
                text = TooltipHelper.Append(text, aI.GetLocalizedTooltip());
            }

            if (prefab is PropInfo || prefab is TreeInfo)
            {
                text = TooltipHelper.Append(text, TooltipHelper.Format(new string[]
	            {
		            LocaleFormatter.Cost,
		            LocaleFormatter.FormatCost(prefab.GetConstructionCost(), false)
	            }));
            }

            return text;
        }
    }

    public class AssetTagList
    {
        private bool initialized = false;

        public static AssetTagList instance;

        public Dictionary<string, int> tagsTitle = new Dictionary<string, int>();
        public Dictionary<string, int> tagsDesc = new Dictionary<string, int>();
        public Dictionary<string, Asset> assets = new Dictionary<string, Asset>();

        public List<Asset> matches = new List<Asset>();

        public List<Asset> Find(string text)
        {
            if(!initialized)
            {
                Init();
            }

            matches.Clear();

            text = text.ToLower().Trim();

            if (!text.IsNullOrWhiteSpace())
            {
                string[] keywords = Regex.Split(text, @"([^\w]|\s)+", RegexOptions.IgnoreCase);

                foreach (Asset asset in assets.Values)
                {
                    if (asset.prefab != null)
                    {
                        foreach (string keyword in keywords)
                        {
                            if (!keyword.IsNullOrWhiteSpace())
                            {
                                float score = 0;

                                if (!asset.author.IsNullOrWhiteSpace())
                                {
                                    score += 10 * GetScore(keyword, asset.author, null);
                                }

                                if (!asset.service.IsNullOrWhiteSpace())
                                {
                                    score += 10 * GetScore(keyword, asset.service, null);
                                }

                                foreach (string tag in asset.tagsTitle)
                                {
                                    score += 5 * GetScore(keyword, tag, tagsTitle);
                                }

                                foreach (string tag in asset.tagsDesc)
                                {
                                    score += GetScore(keyword, tag, tagsDesc);
                                }

                                if (score > 0)
                                {
                                    asset.score += score;
                                }
                                else
                                {
                                    asset.score = 0;
                                    break;
                                }
                            }
                        }
                    }

                    if (asset.score > 0)
                    {
                        matches.Add(asset);
                    }
                }
                matches = matches.OrderByDescending(s => s.score).ToList();
            }
            else
            {
                foreach (Asset asset in assets.Values)
                {
                    if (asset.prefab != null)
                    {
                        matches.Add(asset);
                    }
                }
                matches = matches.OrderBy(s => s.name).ToList();
            }

            return matches;
        }

        private float GetScore(string keyword, string tag, Dictionary<string, int> dico)
        {
            int index = tag.IndexOf(keyword);
            float scoreMultiplier = 1f;

            if (index >= 0)
            {
                if (index == 0)
                { 
                    scoreMultiplier = 10f;
                }
                if (dico != null && dico.ContainsKey(tag))
                {
                    return scoreMultiplier / dico[tag] * ((tag.Length - index) / (float)tag.Length) * (keyword.Length / (float)tag.Length);
                }
                else
                {
                    if (dico != null) DebugUtils.Log("Tag not found in dico: " + tag);
                    return scoreMultiplier * ((tag.Length - index) / (float)tag.Length) * (keyword.Length / (float)tag.Length);
                }
            }

            return 0;
        }

        public AssetTagList()
        {
            foreach (Package.Asset current in PackageManager.FilterAssets(new Package.AssetType[] { UserAssetType.CustomAssetMetaData }))
            {
                PublishedFileId id = current.package.GetPublishedFileID();
                string author = null;

                if (!current.package.packageAuthor.IsNullOrWhiteSpace())
                {
                    ulong authorID;
                    if (UInt64.TryParse(current.package.packageAuthor.Substring("steamid:".Length), out authorID))
                    {
                        author = new Friend(new UserID(authorID)).personaName;
                        author = Regex.Replace(author.ToLower().Trim(), @"([^\w]|\s)+", "_");
                    }
                }

                if (!assets.ContainsKey(current.fullName))
                {
                    assets[current.fullName] = new Asset()
                    {
                        name = current.fullName,
                        steamID = id.AsUInt64,
                        author = author
                    };
                }
            }
        }

        public void Init()
        {
            foreach (Asset asset in assets.Values)
            {
                asset.prefab = null;
            }

            tagsTitle.Clear();
            tagsDesc.Clear();
            
            GetPrefabs<BuildingInfo>();
            //GetPrefab<NetInfo>();
            GetPrefabs<PropInfo>();
            GetPrefabs<TreeInfo>();

            foreach (Asset asset in assets.Values)
            {
                if (asset.prefab != null)
                {
                    asset.tagsTitle = AddAssetTags(asset, tagsTitle, Asset.GetLocalizedTitle(asset.prefab));
                    asset.tagsDesc = AddAssetTags(asset, tagsDesc, Asset.GetLocalizedDescription(asset.prefab));

                    string tag = null;
                    if (asset.prefab is BuildingInfo)
                    {
                        tag = "building";
                    }
                    else if (asset.prefab is PropInfo)
                    {
                        tag = "prop";
                    }
                    else if (asset.prefab is TreeInfo)
                    {
                        tag = "tree";
                    }

                    if(!tag.IsNullOrWhiteSpace())
                    {
                        asset.tagsTitle.Add(tag);
                        if (!tagsTitle.ContainsKey(tag))
                        {
                            tagsTitle.Add(tag, 0);
                        }
                        tagsTitle[tag]++;
                    }

                    if (asset.prefab.GetService() != ItemClass.Service.None)
                    {
                        asset.service = asset.prefab.GetService().ToString().ToLower();
                    }
                }
            }

            CleanDictionarys();

            initialized = true;
        }

        private void GetPrefabs<T>() where T : PrefabInfo
        {
            for (uint i = 0; i < PrefabCollection<T>.PrefabCount(); i++)
            {
                T prefab = PrefabCollection<T>.GetPrefab(i);

                if (prefab == null) continue;

                string name = prefab.name;
                if(name.EndsWith("_Data"))
                {
                    name = name.Substring(0, name.LastIndexOf("_Data"));
                }

                if (assets.ContainsKey(name))
                {
                    assets[name].prefab = prefab;
                }
                else
                {
                    assets[name] = new Asset()
                    {
                        name = name,
                        prefab = prefab,
                        steamID = GetSteamID(prefab)
                    };
                }
            }
        }

        private HashSet<string> AddAssetTags(Asset asset, Dictionary<string, int> dico, string text)
        {
            //text = Regex.Replace(text, "([A-Z][a-z]+)", " $1");

            string[] tagsArr = Regex.Split(text, @"([^\w]|\s)+", RegexOptions.IgnoreCase);

            HashSet<string> tags = new HashSet<string>();

            foreach (string t in tagsArr)
            {
                string tag = t.ToLower().Trim();

                if (tag.Length > 1 && !tag.Contains("_"))
                {
                    if (!dico.ContainsKey(tag))
                    {
                        dico.Add(tag, 0);
                    }
                    dico[tag]++;
                    tags.Add(tag);
                }
            }

            return tags;
        }

        private void CleanDictionarys()
        {
            foreach (Asset asset in assets.Values)
            {
                List<string> keys = asset.tagsTitle.ToList();
                foreach (string key in keys)
                {
                    if (key.EndsWith("s"))
                    {
                        string tag = key.Substring(0, key.Length - 1);
                        if (tagsTitle.ContainsKey(tag))
                        {
                            if (tagsTitle.ContainsKey(key))
                            {
                                tagsTitle[tag] += tagsTitle[key];
                                tagsTitle.Remove(key);
                            }
                            asset.tagsTitle.Remove(key);
                            asset.tagsTitle.Add(tag);
                        }
                    }
                }

                keys = asset.tagsDesc.ToList();
                foreach (string key in keys)
                {
                    if (key.EndsWith("s"))
                    {
                        string tag = key.Substring(0, key.Length - 1);
                        if (tagsDesc.ContainsKey(tag))
                        {
                            if (tagsDesc.ContainsKey(key))
                            {
                                tagsDesc[tag] += tagsDesc[key];
                                tagsDesc.Remove(key);
                            }
                            asset.tagsDesc.Remove(key);
                            asset.tagsDesc.Add(tag);
                        }
                    }
                }
            }
        }

        private static ulong GetSteamID(PrefabInfo prefab)
        {
            ulong id = 0;

            if (prefab.name.Contains("."))
            {
                string steamID = prefab.name.Substring(0, prefab.name.IndexOf("."));
                UInt64.TryParse(steamID, out id);
            }

            return id;
        }
    }
}
