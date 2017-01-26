using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;

using UnityEngine;

using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;
using ColossalFramework.PlatformServices;
using ColossalFramework.Packaging;

using FindIt.GUI;

namespace FindIt
{
    public class Asset
    {
        private PrefabInfo m_prefab;

        public static Shader shaderDefault = Shader.Find("Custom/Props/Decal/Default");
        public static Shader shaderBlend = Shader.Find("Custom/Props/Decal/Blend");
        public static Shader shaderSolid = Shader.Find("Custom/Props/Decal/Solid");
        public static Shader shaderFence = Shader.Find("Custom/Props/Decal/Fence");

        public enum AssetType
        {
            All,
            Ploppable,
            Growable,
            Rico,
            Prop,
            Decal,
            //Fence,
            Tree
        }

        public string name;
        public string title;
        public PrefabInfo prefab
        {
            get { return m_prefab; }
            set
            {
                if (m_prefab != value)
                {
                    m_prefab = value;

                    if (m_prefab != null)
                    {
                        service = m_prefab.GetService();
                        subService = m_prefab.GetSubService();

                        BuildingInfo buildingPrefab = m_prefab as BuildingInfo;
                        if (buildingPrefab != null)
                        {
                            if (buildingPrefab.m_placementStyle != ItemClass.Placement.Manual)
                            {
                                assetType = AssetType.Growable;
                            }
                            else
                            {
                                if (service == ItemClass.Service.Residential ||
                                    service == ItemClass.Service.Industrial ||
                                    service == ItemClass.Service.Commercial ||
                                    service == ItemClass.Service.Office)
                                {
                                    assetType = AssetType.Rico;
                                }
                                else
                                {
                                    assetType = AssetType.Ploppable;
                                }
                            }
                                
                            size = new Vector2(buildingPrefab.m_cellWidth, buildingPrefab.m_cellLength);

                            return;
                        }

                        PropInfo propPrefab = m_prefab as PropInfo;
                        if (propPrefab != null)
                        {
                            assetType = AssetType.Prop;

                            if (propPrefab.m_material != null)
                            {
                                if (propPrefab.m_material.shader == shaderBlend || propPrefab.m_material.shader == shaderSolid)
                                {
                                    assetType = AssetType.Decal;
                                }
                                /*else if (propPrefab.m_material.shader == shaderFence)
                                {
                                    assetType = AssetType.Fence;
                                }*/
                            }

                            return;
                        }

                        else if (m_prefab is TreeInfo)
                        {
                            assetType = AssetType.Tree;
                        }
                    }
                }
            }
        }
        public AssetType assetType = (AssetType)(-1);
        public ItemClass.Service service = ItemClass.Service.None;
        public ItemClass.SubService subService = ItemClass.SubService.None;
        public Vector2 size;
        public ulong steamID;
        public string author;
        public float score;

        public delegate void OnButtonClicked(UIComponent comp);
        public OnButtonClicked onButtonClicked;

        public HashSet<string> tagsTitle = new HashSet<string>();
        public HashSet<string> tagsDesc = new HashSet<string>();
        public HashSet<string> tagsHash = new HashSet<string>();

        public static string GetName(PrefabInfo prefab)
        {
            string name = prefab.name;
            if (name.EndsWith("_Data"))
            {
                name = name.Substring(0, name.LastIndexOf("_Data"));
            }
            return name;
        }

        public static string GetLocalizedTitle(PrefabInfo prefab)
        {
            string name = prefab.name;

            if (prefab is BuildingInfo)
            {
                if (!Locale.GetUnchecked("BUILDING_TITLE", prefab.name, out name))
                {
                    name = prefab.name;
                }
            }
            else if (prefab is PropInfo)
            {
                if (!Locale.GetUnchecked("PROPS_TITLE", prefab.name, out name))
                {
                    name = prefab.name;
                }
            }
            else if (prefab is TreeInfo)
            {
                if (!Locale.GetUnchecked("TREE_TITLE", prefab.name, out name))
                {
                    name = prefab.name;
                }
            }

            int index = name.IndexOf('.');
            if (index >= 0)
            {
                name = name.Substring(index + 1);
            }

            index = name.LastIndexOf("_Data");
            if (index >= 0)
            {
                name = name.Substring(0, index);
            }

            name = Regex.Replace(name, @"[_-]+", " ");
            name = Regex.Replace(name, @"([A-Z][a-z]+)", " $1");
            name = Regex.Replace(name, @"([^\d])(\d+)", "$1 $2");
            name = Regex.Replace(name, @"\b(.) (\d+)", " $1$2 ");
            name = Regex.Replace(name, @"(\d+) ?x (\d+)", " $1x$2 ");
            name = Regex.Replace(name, @"\s+", " ");

            name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name).Trim();

            return name;
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

        public static string GetLocalizedTooltip(PrefabInfo prefab, string title)
        {
            MilestoneInfo unlockMilestone = prefab.GetUnlockMilestone();

            string text = TooltipHelper.Format(new string[]
	        {
		        LocaleFormatter.Title,
		        title,
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

        public Dictionary<ulong, string> authors = new Dictionary<ulong, string>();

        public List<Asset> matches = new List<Asset>();

        public List<Asset> Find(string text, Asset.AssetType filter)
        {
            if (!initialized)
            {
                Init();
            }

            matches.Clear();

            text = text.ToLower().Trim();

            if (!text.IsNullOrWhiteSpace())
            {
                string[] keywords = Regex.Split(text, @"([^\w]|[_-]|\s)+", RegexOptions.IgnoreCase);

                foreach (Asset asset in assets.Values)
                {
                    if (asset.prefab != null && (filter == Asset.AssetType.All || asset.assetType == filter))
                    {
                        if (filter == Asset.AssetType.Growable || filter == Asset.AssetType.Rico)
                        {
                            BuildingInfo buildingInfo = asset.prefab as BuildingInfo;

                            // Level
                            ItemClass.Level level = UISearchBox.instance.buildingLevel;
                            if (level != ItemClass.Level.None && buildingInfo.m_class.m_level != level) continue;

                            // size
                            Vector2 buildingSize = UISearchBox.instance.buildingSize;
                            if (buildingSize != Vector2.zero && asset.size != buildingSize) continue;

                            // zone
                            if (!UIFilterGrowable.instance.IsAllSelected())
                            {
                                UIFilterGrowable.Category category = UIFilterGrowable.GetCategory(buildingInfo.m_class);
                                if (category == UIFilterGrowable.Category.None || !UIFilterGrowable.instance.IsSelected(category)) continue;
                            }
                        }
                        else if (filter == Asset.AssetType.Ploppable)
                        {
                            BuildingInfo buildingInfo = asset.prefab as BuildingInfo;

                            if (!UIFilterPloppable.instance.IsAllSelected())
                            {
                                UIFilterPloppable.Category category = UIFilterPloppable.GetCategory(buildingInfo.m_class);
                                if (category == UIFilterPloppable.Category.None || !UIFilterPloppable.instance.IsSelected(category)) continue;
                            }
                        }

                        foreach (string keyword in keywords)
                        {
                            if (!keyword.IsNullOrWhiteSpace())
                            {
                                float score = 0;

                                if (!asset.author.IsNullOrWhiteSpace())
                                {
                                    score += 10 * GetScore(keyword, asset.author.ToLower(), null);
                                }

                                if (filter == Asset.AssetType.All && asset.assetType != (Asset.AssetType)(-1))
                                {
                                    score += 10 * GetScore(keyword, asset.assetType.ToString().ToLower(), null);
                                }

                                if (asset.service != ItemClass.Service.None)
                                {
                                    score += 10 * GetScore(keyword, asset.service.ToString().ToLower(), null);
                                }

                                if (asset.subService != ItemClass.SubService.None)
                                {
                                    score += 10 * GetScore(keyword, asset.subService.ToString().ToLower(), null);
                                }

                                if (asset.size != Vector2.zero)
                                {
                                    score += 10 * GetScore(keyword, asset.size.x + "x" + asset.size.y, null);
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

                        if (asset.score > 0)
                        {
                            matches.Add(asset);
                        }
                    }
                }
                matches = matches.OrderByDescending(s => s.score).ToList();
            }
            else
            {
                foreach (Asset asset in assets.Values)
                {
                    if (asset.prefab != null && (filter == Asset.AssetType.All || asset.assetType == filter))
                    {
                        if (filter == Asset.AssetType.Growable || filter == Asset.AssetType.Rico)
                        {
                            BuildingInfo buildingInfo = asset.prefab as BuildingInfo;

                            // Level
                            ItemClass.Level level = UISearchBox.instance.buildingLevel;
                            if (level != ItemClass.Level.None && buildingInfo.m_class.m_level != level) continue;

                            // size
                            Vector2 buildingSize = UISearchBox.instance.buildingSize;
                            if (buildingSize != Vector2.zero && asset.size != buildingSize) continue;

                            // zone
                            if (!UIFilterGrowable.instance.IsAllSelected())
                            {
                                UIFilterGrowable.Category category = UIFilterGrowable.GetCategory(buildingInfo.m_class);
                                if (category == UIFilterGrowable.Category.None || !UIFilterGrowable.instance.IsSelected(category)) continue;
                            }
                        }
                        else if (filter == Asset.AssetType.Ploppable)
                        {
                            BuildingInfo buildingInfo = asset.prefab as BuildingInfo;

                            if (!UIFilterPloppable.instance.IsAllSelected())
                            {
                                UIFilterPloppable.Category category = UIFilterPloppable.GetCategory(buildingInfo.m_class);
                                if (category == UIFilterPloppable.Category.None || !UIFilterPloppable.instance.IsSelected(category)) continue;
                            }
                        }

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

                ulong steamid;
                if (UInt64.TryParse(current.package.packageName, out steamid))
                {
                    if (!authors.ContainsKey(steamid) && !current.package.packageAuthor.IsNullOrWhiteSpace())
                    {
                        ulong authorID;
                        if (UInt64.TryParse(current.package.packageAuthor.Substring("steamid:".Length), out authorID))
                        {
                            string author = new Friend(new UserID(authorID)).personaName;
                            //author = Regex.Replace(author.ToLower().Trim(), @"([^\w]|\s)+", "_");
                            authors.Add(steamid, author);
                        }
                    }
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
                    asset.title = Asset.GetLocalizedTitle(asset.prefab);
                    asset.tagsTitle = AddAssetTags(asset, tagsTitle, asset.title);

                    if (asset.steamID == 0)
                    {
                        int index = asset.prefab.name.IndexOf(".");
                        if (index >= 0)
                        {
                            asset.tagsTitle.UnionWith(AddAssetTags(asset, tagsTitle, asset.prefab.name.Substring(0, index)));
                        }
                    }

                    asset.tagsDesc = AddAssetTags(asset, tagsDesc, Asset.GetLocalizedDescription(asset.prefab));
                }
            }

            CleanDictionarys();

            initialized = true;
        }

        private void GetPrefabs<T>() where T : PrefabInfo
        {
            string filtered = "";
            for (uint i = 0; i < PrefabCollection<T>.PrefabCount(); i++)
            {
                T prefab = PrefabCollection<T>.GetPrefab(i);

                if (prefab == null) continue;

                if (!FindIt.inEditor)
                {
                    BuildingInfo buildingPrefab = prefab as BuildingInfo;
                    if (buildingPrefab != null)
                    {
                        if (buildingPrefab.m_placementStyle == ItemClass.Placement.Procedural && buildingPrefab.m_buildingAI.GetType() != typeof(BuildingAI))
                        {
                            filtered += prefab.name + ", ";
                            continue;
                        }
                    }

                    PropInfo propPrefab = prefab as PropInfo;
                    if (propPrefab != null)
                    {
                        if (propPrefab.m_requireWaterMap && propPrefab.m_lodWaterHeightMap == null)
                        {
                            filtered += prefab.name + ", ";
                            continue;
                        }
                    }
                }

                string name = Asset.GetName(prefab);

                if (assets.ContainsKey(name))
                {
                    assets[name].prefab = prefab;
                }
                else
                {
                    ulong steamID = GetSteamID(prefab);
                    assets[name] = new Asset()
                    {
                        name = name,
                        prefab = prefab,
                        steamID = steamID
                    };

                    if (steamID != 0 && authors.ContainsKey(steamID))
                        assets[name].author = authors[steamID];
                }
            }

            if(!filtered.IsNullOrWhiteSpace())
            {
                filtered = filtered.Remove(filtered.Length - 2);
                DebugUtils.Log("Filtered " + typeof(T) + ": " + filtered);
            }
        }

        private HashSet<string> AddAssetTags(Asset asset, Dictionary<string, int> dico, string text)
        {
            //text = Regex.Replace(text, "([A-Z][a-z]+)", " $1");

            string[] tagsArr = Regex.Split(text, @"([^\w]|[_-]|\s)+", RegexOptions.IgnoreCase);

            HashSet<string> tags = new HashSet<string>();

            foreach (string t in tagsArr)
            {
                string tag = t.ToLower().Trim();

                if (tag.Length > 1)
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
