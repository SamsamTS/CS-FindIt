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
            Invalid = -1,
            All,
            Road,
            Ploppable,
            Growable,
            Rico,
            Prop,
            Decal,
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
                        else if(m_prefab is NetInfo)
                        {
                            assetType = AssetType.Road;
                        }
                        else if (m_prefab is TreeInfo)
                        {
                            assetType = AssetType.Tree;
                        }
                    }
                }
            }
        }
        public AssetType assetType = AssetType.Invalid;
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
        public HashSet<string> tagsCustom = new HashSet<string>();

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
                else
                {
                    name = name.Replace(".", "");
                }
            }
            else if (prefab is PropInfo)
            {
                if (!Locale.GetUnchecked("PROPS_TITLE", prefab.name, out name))
                {
                    name = prefab.name;
                }
                else
                {
                    name = name.Replace(".", "");
                }
            }
            else if (prefab is TreeInfo)
            {
                if (!Locale.GetUnchecked("TREE_TITLE", prefab.name, out name))
                {
                    name = prefab.name;
                }
                else
                {
                    name = name.Replace(".", "");
                }
            }
            else if (prefab is NetInfo)
            {
                if (!Locale.GetUnchecked("NET_TITLE", prefab.name, out name))
                {
                    name = prefab.name;
                }
                else
                {
                    name = name.Replace(".", "");
                }
            }

            int index = name.IndexOf('.');
            if (index >= 0)
            {
                name = name.Substring(index + 1);
            }

            if (name.IsNullOrWhiteSpace())
            {
                name = prefab.name;
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
            else if (prefab is NetInfo)
            {
                if (Locale.GetUnchecked("NET_DESC", prefab.name, out result))
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

            ToolsModifierControl.GetUnlockingInfo(unlockMilestone, out string unlockDesc, out string currentValue, out string targetValue, out string progress, out string locked);

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
        public static AssetTagList instance;

        public Dictionary<string, int> tagsTitleDictionary = new Dictionary<string, int>();
        public Dictionary<string, int> tagsDescDictionary = new Dictionary<string, int>();
        public Dictionary<string, int> tagsCustomDictionary = new Dictionary<string, int>();

        public Dictionary<string, Asset> assets = new Dictionary<string, Asset>();

        public Dictionary<ulong, string> authors = new Dictionary<ulong, string>();

        public List<Asset> matches = new List<Asset>();

        public List<Asset> Find(string text, Asset.AssetType filter)
        {
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

                                if (filter == Asset.AssetType.All && asset.assetType != Asset.AssetType.Invalid)
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

                                foreach (string tag in asset.tagsCustom)
                                {
                                    score += 20 * GetScore(keyword, tag, tagsCustomDictionary);
                                }

                                foreach (string tag in asset.tagsTitle)
                                {
                                    score += 5 * GetScore(keyword, tag, tagsTitleDictionary);
                                }

                                foreach (string tag in asset.tagsDesc)
                                {
                                    score += GetScore(keyword, tag, tagsDescDictionary);
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
                matches = matches.OrderBy(s => s.title).ToList();
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

                if (UInt64.TryParse(current.package.packageName, out ulong steamid))
                {
                    if (!authors.ContainsKey(steamid) && !current.package.packageAuthor.IsNullOrWhiteSpace())
                    {
                        if (UInt64.TryParse(current.package.packageAuthor.Substring("steamid:".Length), out ulong authorID))
                        {
                            string author = new Friend(new UserID(authorID)).personaName;
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

            tagsTitleDictionary.Clear();
            tagsDescDictionary.Clear();
            tagsCustomDictionary.Clear();

            GetPrefabs<BuildingInfo>();
            GetPrefabs<NetInfo>();
            GetPrefabs<PropInfo>();
            GetPrefabs<TreeInfo>();

            if (CustomTagsLibrary.assetTags.Count == 0)
            {
                CustomTagsLibrary.Deserialize();
            }

            foreach (Asset asset in assets.Values)
            {
                if (asset.prefab != null)
                {
                    asset.title = Asset.GetLocalizedTitle(asset.prefab);
                    asset.tagsTitle = AddAssetTags(asset, tagsTitleDictionary, asset.title);

                    if (asset.steamID == 0)
                    {
                        int index = asset.prefab.name.IndexOf(".");
                        if (index >= 0)
                        {
                            asset.tagsTitle.UnionWith(AddAssetTags(asset, tagsTitleDictionary, asset.prefab.name.Substring(0, index)));
                        }
                    }

                    asset.tagsDesc = AddAssetTags(asset, tagsDescDictionary, Asset.GetLocalizedDescription(asset.prefab));

                    string name = Asset.GetName(asset.prefab);
                    if (CustomTagsLibrary.assetTags.ContainsKey(name))
                    {
                        asset.tagsCustom = AddAssetTags(asset, tagsCustomDictionary, CustomTagsLibrary.assetTags[name]);
                    }
                }
            }

            CleanDictionarys();
        }

        public void AddCustomTags(Asset asset, string text)
        {
            if (asset == null || asset.prefab == null || text.IsNullOrWhiteSpace()) return;

            string name = Asset.GetName(asset.prefab);

            asset.tagsCustom.UnionWith(AddAssetTags(asset, tagsCustomDictionary, text));

            if (asset.tagsCustom.Count > 0)
            {
                CustomTagsLibrary.assetTags[name] = string.Join(" ", asset.tagsCustom.OrderBy(s => s).ToArray<string>());
                CustomTagsLibrary.Serialize();
            }
        }

        public void RemoveCustomTag(Asset asset, string tag)
        {
            if (asset == null || asset.prefab == null || tag.IsNullOrWhiteSpace()) return;

            if (!asset.tagsCustom.Remove(tag)) return;

            string name = Asset.GetName(asset.prefab);

            if(tagsCustomDictionary.ContainsKey(tag))
            {
                tagsCustomDictionary[tag]--;
                if(tagsCustomDictionary[tag] == 0)
                {
                    tagsCustomDictionary.Remove(tag);
                }
            }

            if (asset.tagsCustom.Count == 0)
            {
                CustomTagsLibrary.assetTags.Remove(name);
            }
            else
            {
                CustomTagsLibrary.assetTags[name] = string.Join(" ", asset.tagsCustom.OrderBy(s => s).ToArray<string>());
            }

            CustomTagsLibrary.Serialize();
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

                NetInfo netPrefab = prefab as NetInfo;
                if (netPrefab != null)
                {
                    if (netPrefab.category == PrefabInfo.kDefaultCategory || netPrefab.m_Thumbnail.IsNullOrWhiteSpace() ||
                        (netPrefab.name != "Pedestrian Pavement" && netPrefab.m_Thumbnail == "ThumbnailBuildingBeautificationPedestrianPavement"))
                    {
                        continue;
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
            string[] tagsArr = Regex.Split(text, @"([^\w]|[_-]|\s)+", RegexOptions.IgnoreCase);

            HashSet<string> tags = new HashSet<string>();

            foreach (string t in tagsArr)
            {
                string tag = CleanTag(t);

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

        private string CleanTag(string tag)
        {
            tag = tag.ToLower().Trim();
            return tag;
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
                        if (tagsTitleDictionary.ContainsKey(tag))
                        {
                            if (tagsTitleDictionary.ContainsKey(key))
                            {
                                tagsTitleDictionary[tag] += tagsTitleDictionary[key];
                                tagsTitleDictionary.Remove(key);
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
                        if (tagsDescDictionary.ContainsKey(tag))
                        {
                            if (tagsDescDictionary.ContainsKey(key))
                            {
                                tagsDescDictionary[tag] += tagsDescDictionary[key];
                                tagsDescDictionary.Remove(key);
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
