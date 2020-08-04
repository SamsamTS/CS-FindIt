// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using System.IO;

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

        public enum PropType
        {
            Invalid = -1,
            PropsIndustrial,
            PropsParks,
            PropsCommon,
            PropsResidential,
            PropsBillboards,
            PropsSpecialBillboards,
            PropsLights,
            Natural,
            Unsorted,
            Hidden
        }

        public enum TreeType
        {
            Invalid = -1,
            SmallTree,
            MediumTree,
            LargeTree
        }

        public string name;
        public string title;

        public PrefabInfo prefab
        {
            get { return m_prefab; }
            set
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
                            assetType = AssetType.Ploppable;
                        }

                        // check if a building is from a content creator pack. Only works for Modern Japan CCP
                        if (buildingPrefab.editorCategory.EndsWith("ModderPack") && buildingPrefab.name.StartsWith("PDX"))
                        {
                            isCCPBuilding = true;
                        }

                        size = new Vector2(buildingPrefab.m_cellWidth, buildingPrefab.m_cellLength);
                        if (buildingPrefab.m_generatedInfo != null && buildingPrefab.m_generatedInfo.m_heights != null)
                        {
                            if (buildingPrefab.m_generatedInfo.m_heights.Length != 0)
                            {
                                buildingHeight = buildingPrefab.m_generatedInfo.m_heights.Max();
                            }
                        }

                        return;
                    }

                    PropInfo propPrefab = m_prefab as PropInfo;
                    if (propPrefab != null)
                    {
                        assetType = AssetType.Prop;
                        propType = SetPropType(prefab.editorCategory);

                        if (propPrefab.m_material != null)
                        {
                            if (propPrefab.m_material.shader == shaderBlend || propPrefab.m_material.shader == shaderSolid)
                            {
                                assetType = AssetType.Decal;
                            }
                        }

                        return;
                    }
                    else if (m_prefab is NetInfo)
                    {
                        assetType = AssetType.Road;
                    }
                    else if (m_prefab is TreeInfo)
                    {
                        assetType = AssetType.Tree;
                        TreeInfo info = m_prefab as TreeInfo;
                        treeType = SetTreeType(info);
                    }
                }
            }
        }

        public void RefreshRico()
        {
            if (FindIt.isRicoEnabled && m_prefab != null && assetType == AssetType.Ploppable)
            {
                service = m_prefab.GetService();
                subService = m_prefab.GetSubService();

                if (service == ItemClass.Service.Residential ||
                   service == ItemClass.Service.Industrial ||
                   service == ItemClass.Service.Commercial ||
                   service == ItemClass.Service.Office)
                {
                    assetType = AssetType.Rico;
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
        public ulong downloadTime;
        public bool isCCPBuilding = false;
        public float buildingHeight = 0;
        public PropType propType = PropType.Invalid;
        public TreeType treeType = TreeType.Invalid;

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

        // check the type of a prop based on editor category
        private Asset.PropType SetPropType(string propEditorCategory)
        {
            if (propEditorCategory.StartsWith("PropsIndustrial")) return Asset.PropType.PropsIndustrial;
            else if (propEditorCategory.StartsWith("PropsParks")) return Asset.PropType.PropsParks;

            else if (propEditorCategory.StartsWith("PropsCommonLights") || propEditorCategory.StartsWith("PropsCommonStreets"))
                return Asset.PropType.PropsLights;

            else if (propEditorCategory.StartsWith("PropsCommon")) return Asset.PropType.PropsCommon;
            else if (propEditorCategory.StartsWith("PropsResidential")) return Asset.PropType.PropsResidential;
            else if (propEditorCategory.StartsWith("PropsBillboards")) return Asset.PropType.PropsBillboards;
            else if (propEditorCategory.StartsWith("PropsSpecialBillboards")) return Asset.PropType.PropsSpecialBillboards;
            else if (propEditorCategory.StartsWith("PropsRocks")) return Asset.PropType.Natural;
            else if (propEditorCategory.StartsWith("Beautification")) return Asset.PropType.Natural;
            else if (propEditorCategory.StartsWith("PropsMarker")) return Asset.PropType.Hidden;

            return Asset.PropType.Unsorted;
        }

        // check the size of a tree and decide its type. Same catergorization as the vanilla game
        private Asset.TreeType SetTreeType(TreeInfo info)
        {
            if (info == null) return Asset.TreeType.Invalid;

            float size = info.m_generatedInfo.m_size.y * (info.m_minScale + info.m_maxScale);

            if (size <= 16f) return Asset.TreeType.SmallTree;
            if (size > 16f && size <= 30f) return Asset.TreeType.MediumTree;
            //if (size > 30f) 
            return Asset.TreeType.LargeTree;
        }
    }

    public class AssetTagList
    {
        public static AssetTagList instance;

        public Dictionary<string, int> tagsTitleDictionary = new Dictionary<string, int>();
        public Dictionary<string, int> tagsDescDictionary = new Dictionary<string, int>();
        public Dictionary<string, int> tagsCustomDictionary = new Dictionary<string, int>();

        public Dictionary<string, Asset> assets = new Dictionary<string, Asset>();

        /// <summary>
        /// key = asset steam id, value = author name
        /// </summary>
        public Dictionary<ulong, string> authors = new Dictionary<ulong, string>();

        /// <summary>
        /// key = asset steam ID, value = asset download timestamp
        /// </summary>
        public Dictionary<ulong, ulong> downloadTimes = new Dictionary<ulong, ulong>();

        /// <summary>
        /// key = asset creator name, value = number of assets made by this creator
        /// </summary>
        public Dictionary<string, int> assetCreatorDictionary = new Dictionary<string, int>();

        public List<Asset> matches = new List<Asset>();

        public List<Asset> Find(string text, UISearchBox.DropDownOptions filter)
        {
            matches.Clear();

            text = text.ToLower().Trim();

            // if there is something in the search input box
            if (!text.IsNullOrWhiteSpace())
            {
                string[] keywords = Regex.Split(text, @"([^\w]|[_-]|\s)+", RegexOptions.IgnoreCase);

                foreach (Asset asset in assets.Values)
                {
                    asset.RefreshRico();
                    if (asset.prefab != null)
                    {
                        if (!CheckAssetFilter(asset, filter)) continue;

                        // Calculate relevance score. Algorithm decided by Sam. Unchanged.
                        foreach (string keyword in keywords)
                        {
                            if (!keyword.IsNullOrWhiteSpace())
                            {
                                float score = 0;

                                if (!asset.author.IsNullOrWhiteSpace())
                                {
                                    score += 10 * GetScore(keyword, asset.author.ToLower(), null);
                                }

                                if (filter == UISearchBox.DropDownOptions.All && asset.assetType != Asset.AssetType.Invalid)
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

                        if (asset.score > 0) matches.Add(asset);
                    }
                }
            }

            // if there isn't anything in the search input box
            else
            {
                foreach (Asset asset in assets.Values)
                {
                    asset.RefreshRico();
                    if (asset.prefab != null)
                    {
                        if (!CheckAssetFilter(asset, filter)) continue;
                        matches.Add(asset);
                    }
                }
            }

            return matches;
        }

        // return true if the asset type matches UISearchbox filter dropdown options
        private bool CheckAssetFilter(Asset asset, UISearchBox.DropDownOptions filter)
        {
            if (asset.assetType != Asset.AssetType.Road && filter == UISearchBox.DropDownOptions.Network) return false;
            if (asset.assetType != Asset.AssetType.Prop && filter == UISearchBox.DropDownOptions.Prop) return false;
            if (asset.assetType != Asset.AssetType.Rico && filter == UISearchBox.DropDownOptions.Rico) return false;
            if (asset.assetType != Asset.AssetType.Ploppable && filter == UISearchBox.DropDownOptions.Ploppable) return false;
            if (asset.assetType != Asset.AssetType.Growable && filter == UISearchBox.DropDownOptions.Growable) return false;
            if (asset.assetType != Asset.AssetType.Tree && filter == UISearchBox.DropDownOptions.Tree) return false;
            if (asset.assetType != Asset.AssetType.Decal && filter == UISearchBox.DropDownOptions.Decal) return false;
            if ((asset.assetType != Asset.AssetType.Rico && asset.assetType != Asset.AssetType.Growable) && filter == UISearchBox.DropDownOptions.GrwbRico) return false;

            if (filter == UISearchBox.DropDownOptions.Growable || filter == UISearchBox.DropDownOptions.Rico || filter == UISearchBox.DropDownOptions.GrwbRico)
            {
                BuildingInfo buildingInfo = asset.prefab as BuildingInfo;

                // Distinguish growable and rico
                if ((filter == UISearchBox.DropDownOptions.Growable) && (asset.assetType == Asset.AssetType.Rico)) return false;
                if ((filter == UISearchBox.DropDownOptions.Rico) && (asset.assetType == Asset.AssetType.Growable)) return false;

                // filter by Level
                ItemClass.Level level = UISearchBox.instance.buildingLevel;
                if (level != ItemClass.Level.None && buildingInfo.m_class.m_level != level) return false;

                // filter by size
                if (!CheckBuildingSize(asset.size, UISearchBox.instance.buildingSizeFilterIndex)) return false;

                // filter by growable type
                if (!UIFilterGrowable.instance.IsAllSelected())
                {
                    UIFilterGrowable.Category category = UIFilterGrowable.GetCategory(buildingInfo.m_class);
                    if (category == UIFilterGrowable.Category.None || !UIFilterGrowable.instance.IsSelected(category)) return false;
                }
            }
            else if (filter == UISearchBox.DropDownOptions.Ploppable)
            {
                BuildingInfo buildingInfo = asset.prefab as BuildingInfo;

                // filter by ploppable type
                if (!UIFilterPloppable.instance.IsAllSelected())
                {
                    UIFilterPloppable.Category category = UIFilterPloppable.GetCategory(buildingInfo.m_class);
                    if (category == UIFilterPloppable.Category.None || !UIFilterPloppable.instance.IsSelected(category)) return false;
                }
            }
            else if (filter == UISearchBox.DropDownOptions.Prop)
            {
                // filter by prop type
                if (!UIFilterProp.instance.IsAllSelected())
                {
                    UIFilterProp.Category category = UIFilterProp.GetCategory(asset.propType);
                    if (category == UIFilterProp.Category.None || !UIFilterProp.instance.IsSelected(category)) return false;
                }
            }
            else if (filter == UISearchBox.DropDownOptions.Tree)
            {
                // filter by tree type
                if (!UIFilterTree.instance.IsAllSelected())
                {
                    UIFilterTree.Category category = UIFilterTree.GetCategory(asset.treeType);
                    if (category == UIFilterTree.Category.None || !UIFilterTree.instance.IsSelected(category)) return false;
                }
            }

            try
            {
                if (UISearchBox.instance?.workshopFilter != null && UISearchBox.instance?.vanillaFilter != null && UISearchBox.instance?.tagPanel != null && UISearchBox.instance?.extraFiltersPanel != null)
                {
                    // filter out custom asset
                    if (asset.prefab.m_isCustomContent && !UISearchBox.instance.workshopFilter.isChecked) return false;

                    // filter out vanilla asset. will not filter out content creater pack assets
                    if (!asset.prefab.m_isCustomContent && !UISearchBox.instance.vanillaFilter.isChecked && !asset.isCCPBuilding) return false;

                    // filter out assets without matching custom tag
                    if (UISearchBox.instance.tagPanel.tagDropDownCheckBox.isChecked && UISearchBox.instance.tagPanel.customTagListStrArray.Length > 0)
                    {
                        if (!asset.tagsCustom.Contains(UISearchBox.instance.tagPanel.GetDropDownListKey())) return false;
                    }

                    // extra filters check
                    if (UISearchBox.instance.extraFiltersPanel.optionDropDownCheckBox.isChecked)
                    {
                        // filter asset by asset creator
                        if (UISearchBox.instance.extraFiltersPanel.optionDropDownMenu.selectedIndex == 0)
                        {
                            if (asset.author != UISearchBox.instance.extraFiltersPanel.GetAssetCreatorDropDownListKey()) return false;
                        }
                        // filter asset by building height
                        else
                        {
                            if (asset.assetType == Asset.AssetType.Ploppable || asset.assetType == Asset.AssetType.Rico || asset.assetType == Asset.AssetType.Growable)
                            {
                                if (asset.buildingHeight > UISearchBox.instance.extraFiltersPanel.maxBuildingHeight) return false;
                                if (asset.buildingHeight < UISearchBox.instance.extraFiltersPanel.minBuildingHeight) return false;
                            }
                            else return false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }

            return true;
        }

        // return true if the asset size matches the building size filter options
        // index 0 = all
        // index 1 - 4 = corresponding sizes
        // index 5 = size 5 - 8
        // index 6 = size 9 - 12
        // index 7 = size 13+
        private bool CheckBuildingSize(Vector2 assetSize, Vector2 buildingSizeFilterIndex)
        {
            if (buildingSizeFilterIndex.x > 0.0f && buildingSizeFilterIndex.y > 0.0f)
            {
                if (!CheckBuildingSizeXY(assetSize.x, buildingSizeFilterIndex.x) || !CheckBuildingSizeXY(assetSize.y, buildingSizeFilterIndex.y)) return false;
            }
            // if filter = (All, not All)
            if (buildingSizeFilterIndex.x == 0.0f && buildingSizeFilterIndex.y != 0.0f)
            {
                if (!CheckBuildingSizeXY(assetSize.y, buildingSizeFilterIndex.y)) return false;
            }
            // if filter = (not All, All)
            if (buildingSizeFilterIndex.x != 0.0f && buildingSizeFilterIndex.y == 0.0f)
            {
                if (!CheckBuildingSizeXY(assetSize.x, buildingSizeFilterIndex.x)) return false;
            }
            return true;
        }

        private bool CheckBuildingSizeXY(float assetSizeXY, float buildingSizeFilterIndex)
        {
            if (buildingSizeFilterIndex == 0.0f) return true; // all
            if (buildingSizeFilterIndex < 5.0f) // size of 1 - 4
            {
                if (assetSizeXY == buildingSizeFilterIndex) return true;
                else return false;
            }
            if (buildingSizeFilterIndex == 5.0f) // size 5 - 8
            {
                if (assetSizeXY <= 8.0f && assetSizeXY >= 5.0f) return true;
                else return false;
            }
            if (buildingSizeFilterIndex == 6.0f) // size 9 - 12
            {
                if (assetSizeXY <= 12.0f && assetSizeXY >= 9.0f) return true;
                else return false;
            }
            if (buildingSizeFilterIndex == 7.0f) // size 13+
            {
                if (assetSizeXY >= 13.0f) return true;
                else return false;
            }
            return true;
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
                    if (dico != null) Debugging.Message("Tag not found in dico: " + tag);
                    return scoreMultiplier * ((tag.Length - index) / (float)tag.Length) * (keyword.Length / (float)tag.Length);
                }
            }

            return 0;
        }

        public AssetTagList()
        {

            foreach (Package.Asset current in PackageManager.FilterAssets(new Package.AssetType[] { UserAssetType.CustomAssetMetaData }))
            {
                //PublishedFileId id = current.package.GetPublishedFileID();

                if (current?.package?.packagePath != null)
                {
                    if (UInt64.TryParse(current.package.packageName, out ulong steamid))
                    {
                        if (!authors.ContainsKey(steamid) && !current.package.packageAuthor.IsNullOrWhiteSpace())
                        {
                            if (UInt64.TryParse(current.package.packageAuthor.Substring("steamid:".Length), out ulong authorID))
                            {
                                string author = new Friend(new UserID(authorID)).personaName;
                                authors.Add(steamid, author);

                                // Get the downloaded time of an asset by checking the creation time of its parent folder
                                // store this info and use it for sorting
                                string path = current.package.packagePath;
                                string parentPath = Directory.GetParent(path).FullName;
                                DateTime dt = Directory.GetCreationTimeUtc(parentPath);
                                ulong time = (ulong)dt.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
                                downloadTimes.Add(steamid, time);
                            }
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
            assetCreatorDictionary.Clear();

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

                        // if steamID == 0, non-workshop, download time = 0
                        asset.downloadTime = 0;

                        // make Kaminogi's Modern Japan builldings included in the asset creator filter
                        if (asset.isCCPBuilding)
                        {
                            asset.author = "Ryuichi Kaminogi";
                            if (!assetCreatorDictionary.ContainsKey("Ryuichi Kaminogi"))
                            {
                                assetCreatorDictionary.Add("Ryuichi Kaminogi", 1);
                            }
                            else
                            {
                                assetCreatorDictionary["Ryuichi Kaminogi"] += 1;
                            }
                        }
                    }
                    else
                    {
                        if (downloadTimes.ContainsKey(asset.steamID))
                        {
                            asset.downloadTime = downloadTimes[asset.steamID];

                            if (authors.ContainsKey(asset.steamID))
                            {
                                if (!assetCreatorDictionary.ContainsKey(authors[asset.steamID]))
                                {
                                    assetCreatorDictionary.Add(authors[asset.steamID], 1);
                                }
                                else
                                {
                                    assetCreatorDictionary[authors[asset.steamID]] += 1;
                                }
                            }
                        }
                        else
                        {
                            asset.downloadTime = 0;
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

            if (tagsCustomDictionary.ContainsKey(tag))
            {
                tagsCustomDictionary[tag]--;
                if (tagsCustomDictionary[tag] == 0)
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

            if (!filtered.IsNullOrWhiteSpace())
            {
                filtered = filtered.Remove(filtered.Length - 2);
                Debugging.Message("Filtered " + typeof(T) + ": " + filtered);
            }
        }

        private HashSet<string> AddAssetTags(Asset asset, Dictionary<string, int> dico, string text)
        {
            // break input text into multiple tags
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

        // if SteamID == 0, not a workshop asset
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

        public List<KeyValuePair<string, int>> GetCustomTagList()
        {
            List<KeyValuePair<string, int>> list = tagsCustomDictionary.ToList();

            // sort list by number of assets in each tag
            if (!Settings.customTagListSort)
            {
                list = list.OrderByDescending(s => s.Value).ToList();
            }
            // sort list alphabetically
            else
            {
                list = list.OrderBy(s => s.Key).ToList();
            }

            return list;
        }

        public List<KeyValuePair<string, int>> GetAssetCreatorList()
        {
            List<KeyValuePair<string, int>> list = assetCreatorDictionary.ToList();

            // sort list by number of assets by each asset creator
            if (!Settings.assetCreatorListSort)
            {
                list = list.OrderByDescending(s => s.Value).ToList();
            }
            // sort list alphabetically
            else
            {
                list = list.OrderBy(s => s.Key).ToList();
            }

            return list;
        }
    }
}
