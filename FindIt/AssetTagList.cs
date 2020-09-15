﻿// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.Packaging;
using FindIt.GUI;

namespace FindIt
{
    public class AssetTagList
    {
        public static AssetTagList instance;

        /// <summary>
        /// Tiny Roads is a network category created by Next2. Doesn't exist in vanilla game
        /// Some asset creators used modtools to self-assign their roads as tiny roads
        /// </summary>
        public bool tinyRoadsExist = false;

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

        /// <summary>
        /// key = asset, value = numbers of active instances of each asset
        /// </summary>
        public Dictionary<PrefabInfo, uint> prefabInstanceCountDictionary = new Dictionary<PrefabInfo, uint>();

        public List<Asset> matches = new List<Asset>();

        public List<Asset> Find(string text, UISearchBox.DropDownOptions filter)
        {
            matches.Clear();
            text = text.ToLower().Trim();

            // if showing instance counts, refresh
            try
            {
                if ((Settings.showInstancesCounter) || (UISearchBox.instance?.extraFiltersPanel != null && UISearchBox.instance.extraFiltersPanel.optionDropDownMenu.selectedIndex == 2))
                {
                    UpdatePrefabInstanceCount();

                    if (FindIt.isPOEnabled && Settings.includePOinstances)
                    {
                        FindIt.instance.POTool.UpdatePOInfoList();
                    }

                    if (Settings.instanceCounterSort != 0) UpdateAssetInstanceCount();
                }
            }
            catch (Exception ex)
            {
                Debugging.LogException(ex);
            }

            // if there is something in the search input box
            if (!text.IsNullOrWhiteSpace())
            {
                string[] keywords = Regex.Split(text, @"([^\w!#+]|[_-]|\s)+", RegexOptions.IgnoreCase);

                bool matched = true;
                float score = 0;
                bool orSearch = false;
                float orScore = 0;

                foreach (Asset asset in assets.Values)
                {
                    asset.RefreshRico();
                    if (asset.prefab != null)
                    {
                        if (!CheckAssetFilter(asset, filter)) continue;
                        matched = true;
                        asset.score = 0;
                        score = 0;
                        orSearch = false;
                        orScore = 0;
                        foreach (string keyword in keywords)
                        {
                            if (!keyword.IsNullOrWhiteSpace())
                            {
                                if (keyword == "!" || keyword == "#" || keyword == "+") continue;
                                if (keyword.StartsWith("!") && keyword.Length > 1)
                                {
                                    score = GetOverallScore(asset, keyword.Substring(1), filter);
                                    if (score > 0)
                                    {
                                        matched = false;
                                        break;
                                    }
                                }
                                else if (keyword.StartsWith("#") && keyword.Length > 1)
                                {
                                    foreach (string tag in asset.tagsCustom)
                                    {
                                        score = GetScore(keyword.Substring(1), tag, tagsCustomDictionary);
                                    }
                                    if (score <= 0)
                                    {
                                        matched = false;
                                        break;
                                    }
                                }
                                else if (keyword.StartsWith("+") && keyword.Length > 1)
                                {
                                    orSearch = true;
                                    score = GetOverallScore(asset, keyword.Substring(1), filter);
                                    orScore += score;
                                    asset.score += score;
                                }
                                else
                                {
                                    // Calculate relevance score. Algorithm decided by Sam. Unchanged.
                                    score = GetOverallScore(asset, keyword, filter);
                                    if (score <= 0)
                                    {
                                        matched = false;
                                        break;
                                    }
                                    else asset.score += score;
                                }
                            }
                        }
                        if (orSearch && orScore <= 0) continue;
                        if (matched) matches.Add(asset);
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

        /// <summary>
        /// return true if the asset type matches UISearchbox filter dropdown options
        /// </summary>
        private bool CheckAssetFilter(Asset asset, UISearchBox.DropDownOptions filter)
        {
            if (asset.assetType != Asset.AssetType.Network && filter == UISearchBox.DropDownOptions.Network) return false;
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

                // filter by size
                if (!CheckBuildingSize(asset.size, UISearchBox.instance.buildingSizeFilterIndex)) return false;

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
            else if (filter == UISearchBox.DropDownOptions.Network)
            {
                // filter by network type
                if (!UIFilterNetwork.instance.IsAllSelected())
                {
                    UIFilterNetwork.Category category = UIFilterNetwork.GetCategory(asset.networkType);
                    NetInfo info = asset.prefab as NetInfo;
                    if (info == null) return false;

                    // not mutually exclusive with other categories. Handle them differently.
                    if (UIFilterNetwork.instance.IsOnlySelected(UIFilterNetwork.Category.OneWay))
                    {
                        if (!UIFilterNetwork.IsNormalRoads(asset.networkType)) return false;
                        if (!UIFilterNetwork.IsOneWay(info)) return false;
                    }
                    else if (UIFilterNetwork.instance.IsOnlySelected(UIFilterNetwork.Category.Parking))
                    {
                        if (!UIFilterNetwork.IsNormalRoads(asset.networkType)) return false;
                        if (!UIFilterNetwork.HasParking(info)) return false;
                    }
                    else if (UIFilterNetwork.instance.IsOnlySelected(UIFilterNetwork.Category.NoParking))
                    {
                        if (!UIFilterNetwork.IsNormalRoads(asset.networkType)) return false;
                        if (UIFilterNetwork.HasParking(info)) return false;
                    }
                    else
                    {
                        if (category == UIFilterNetwork.Category.None || !UIFilterNetwork.instance.IsSelected(category)) return false;
                    }
                }
            }

            // filter out marker prop if not in editor mode
            if ((!FindIt.inEditor && !Settings.showPropMarker) && (asset.propType == Asset.PropType.PropsMarker)) return false;

            try
            {
                if (UISearchBox.instance?.workshopFilter != null && UISearchBox.instance?.vanillaFilter != null)
                {
                    // filter out custom asset
                    if (asset.prefab.m_isCustomContent && !UISearchBox.instance.workshopFilter.isChecked) return false;

                    // filter out vanilla asset. will not filter out content creater pack assets
                    if (!asset.prefab.m_isCustomContent && !UISearchBox.instance.vanillaFilter.isChecked && !asset.isCCP) return false;

                    // filter out assets without matching custom tag
                    if (UISearchBox.instance?.tagPanel != null)
                    {
                        if (UISearchBox.instance.tagPanel.tagDropDownCheckBox.isChecked && UISearchBox.instance.tagPanel.customTagListStrArray.Length > 0)
                        {
                            if (!asset.tagsCustom.Contains(UISearchBox.instance.tagPanel.GetDropDownListKey())) return false;
                        }
                    }

                    // extra filters check
                    if (UISearchBox.instance?.extraFiltersPanel != null)
                    {
                        if (UISearchBox.instance.extraFiltersPanel.optionDropDownCheckBox.isChecked)
                        {
                            // filter asset by asset creator
                            if (UISearchBox.instance.extraFiltersPanel.optionDropDownMenu.selectedIndex == (int)UIFilterExtra.DropDownOptions.AssetCreator)
                            {
                                if (asset.author != UISearchBox.instance.extraFiltersPanel.GetAssetCreatorDropDownListKey()) return false;
                            }
                            // filter asset by building height
                            else if (UISearchBox.instance.extraFiltersPanel.optionDropDownMenu.selectedIndex == (int)UIFilterExtra.DropDownOptions.BuildingHeight)
                            {
                                if (asset.assetType == Asset.AssetType.Ploppable || asset.assetType == Asset.AssetType.Rico || asset.assetType == Asset.AssetType.Growable)
                                {
                                    if (asset.buildingHeight > UISearchBox.instance.extraFiltersPanel.maxBuildingHeight) return false;
                                    if (asset.buildingHeight < UISearchBox.instance.extraFiltersPanel.minBuildingHeight) return false;
                                }
                                else return false;
                            }

                            // filter asset by building level
                            else if (UISearchBox.instance.extraFiltersPanel.optionDropDownMenu.selectedIndex == (int)UIFilterExtra.DropDownOptions.BuildingLevel)
                            {
                                if (!(asset.prefab is BuildingInfo)) return false;
                                BuildingInfo info = asset.prefab as BuildingInfo;
                                ItemClass.Level level = (ItemClass.Level)UISearchBox.instance.extraFiltersPanel.buildingLevelDropDownMenu.selectedIndex;
                                if (info.m_class.m_level != level) return false;
                            }

                            // only show sub-buildings
                            else if (UISearchBox.instance.extraFiltersPanel.optionDropDownMenu.selectedIndex == (int)UIFilterExtra.DropDownOptions.SubBuildings)
                            {
                                if (!asset.isSubBuilding) return false;
                                if (asset.assetType != Asset.AssetType.Invalid) return false;
                            }

                            // only show unused assets
                            else if (UISearchBox.instance.extraFiltersPanel.optionDropDownMenu.selectedIndex == (int)UIFilterExtra.DropDownOptions.UnusedAssets)
                            {
                                if (prefabInstanceCountDictionary.ContainsKey(asset.prefab))
                                {
                                    if (prefabInstanceCountDictionary[asset.prefab] > 0) return false;
                                }
                                if (FindIt.isPOEnabled && Settings.includePOinstances)
                                {
                                    if (FindIt.instance.POTool.GetPrefabInstanceCount(asset.prefab) > 0) return false;
                                }
                            }
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

        public bool CheckBuildingSizeXY(float assetSizeXY, float buildingSizeFilterIndex)
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

        /// <summary>
        /// Get relevance score. Metrics decided by SamsamTS. Unchanged in Find It 2
        /// </summary>
        private float GetOverallScore(Asset asset, string keyword, UISearchBox.DropDownOptions filter)
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

            return score;
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
                    if (dico != null)
                    {
                        if (ModInfo.showExtraDebuggingMessage)
                            Debugging.Message("Tag not found in dico: " + tag);
                    }
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

                                // Get the downloaded time of an asset by checking the creation time of its package folder
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

                        if (asset.isCCP)
                        {
                            if (asset.prefab.m_dlcRequired == SteamHelper.DLC_BitMask.ModderPack1) asset.author = "Shroomblaze";
                            if (asset.prefab.m_dlcRequired == SteamHelper.DLC_BitMask.ModderPack2) asset.author = "GCVos";
                            if (asset.prefab.m_dlcRequired == SteamHelper.DLC_BitMask.ModderPack3) asset.author = "Avanya";
                            if (asset.prefab.m_dlcRequired == SteamHelper.DLC_BitMask.ModderPack4) asset.author = "KingLeno";
                            if (asset.prefab.m_dlcRequired == SteamHelper.DLC_BitMask.ModderPack5) asset.author = "AmiPolizeiFunk";
                            if (asset.prefab.m_dlcRequired == SteamHelper.DLC_BitMask.ModderPack6) asset.author = "Ryuichi Kaminogi";

                            if (!assetCreatorDictionary.ContainsKey(asset.author)) assetCreatorDictionary.Add(asset.author, 1);
                            else assetCreatorDictionary[asset.author] += 1;
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
                            //filtered += prefab.name + ", ";
                            //continue;
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

        public void UpdatePrefabInstanceCount()
        {
            prefabInstanceCountDictionary.Clear();

            if (BuildingManager.exists)
            {
                foreach (Building building in BuildingManager.instance.m_buildings.m_buffer)
                {
                    if (building.m_flags != Building.Flags.None && building.m_flags != Building.Flags.Deleted)
                    {
                        if (prefabInstanceCountDictionary.ContainsKey(building.Info))
                        {
                            prefabInstanceCountDictionary[building.Info] += 1;
                        }
                        else
                        {
                            prefabInstanceCountDictionary.Add(building.Info, 1);
                        }
                    }
                }
            }

            if (PropManager.exists)
            {
                foreach (PropInstance prop in PropManager.instance.m_props.m_buffer)
                {
                    if ((PropInstance.Flags)prop.m_flags != PropInstance.Flags.None && (PropInstance.Flags)prop.m_flags != PropInstance.Flags.Deleted)
                    {
                        if (prefabInstanceCountDictionary.ContainsKey(prop.Info))
                        {
                            prefabInstanceCountDictionary[prop.Info] += 1;
                        }
                        else
                        {
                            prefabInstanceCountDictionary.Add(prop.Info, 1);
                        }
                    }
                }
            }

            if (TreeManager.exists)
            {
                foreach (TreeInstance tree in TreeManager.instance.m_trees.m_buffer)
                {
                    if ((TreeInstance.Flags)tree.m_flags != TreeInstance.Flags.None && (TreeInstance.Flags)tree.m_flags != TreeInstance.Flags.Deleted)
                    {
                        if (prefabInstanceCountDictionary.ContainsKey(tree.Info))
                        {
                            prefabInstanceCountDictionary[tree.Info] += 1;
                        }
                        else
                        {
                            prefabInstanceCountDictionary.Add(tree.Info, 1);
                        }
                    }
                }
            }

            if (NetManager.exists)
            {
                foreach (NetSegment segment in NetManager.instance.m_segments.m_buffer)
                {
                    if (segment.m_flags != NetSegment.Flags.None && segment.m_flags != NetSegment.Flags.Deleted)
                    {
                        if (prefabInstanceCountDictionary.ContainsKey(segment.Info))
                        {
                            prefabInstanceCountDictionary[segment.Info] += 1;
                        }
                        else
                        {
                            prefabInstanceCountDictionary.Add(segment.Info, 1);
                        }
                    }
                }
            }
        }

        private void UpdateAssetInstanceCount()
        {
            foreach (Asset asset in assets.Values)
            {
                if (!prefabInstanceCountDictionary.ContainsKey(asset.prefab)) asset.instanceCount = 0;
                else asset.instanceCount = prefabInstanceCountDictionary[asset.prefab];

                if (Settings.includePOinstances && FindIt.isPOEnabled)
                {
                    asset.poInstanceCount = FindIt.instance.POTool.GetPrefabInstanceCount(asset.prefab);
                }
            }
        }
    }
}
