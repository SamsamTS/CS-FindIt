﻿// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt
// main backend class

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
using System.Reflection;
using ColossalFramework.Globalization;
using ColossalFramework.IO;

namespace FindIt
{
    public partial class AssetTagList
    {
        public static AssetTagList instance;

        public static Shader shaderBlend = Shader.Find("Custom/Props/Decal/Blend");
        public static Shader shaderSolid = Shader.Find("Custom/Props/Decal/Solid");
        public static Shader shaderPropFence = Shader.Find("Custom/Props/Prop/Fence");
        public static Shader shaderBuildingFence = Shader.Find("Custom/Building/Fence");
        public static Shader shaderNetworkFence = Shader.Find("Custom/Net/Fence");

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
        /// assets with steam workshop ID but are local assets, not from workshop subscription
        /// </summary>
        public HashSet<ulong> localWorkshopIDs = new HashSet<ulong>();

        /// <summary>
        /// key = asset creator name, value = number of assets made by this creator
        /// </summary>
        public Dictionary<string, int> assetCreatorDictionary = new Dictionary<string, int>();

        /// <summary>
        /// key = asset, value = numbers of active instances of each asset
        /// </summary>
        public Dictionary<PrefabInfo, uint> prefabInstanceCountDictionary = new Dictionary<PrefabInfo, uint>();

        /// <summary>
        /// Read the set of vehicle & tree props generated by tvp mod and update asset's prop type.
        /// Data is provided by TV Props Patch mod.
        /// </summary>
        public bool isTVPPatchModProcessed = false;

        /// <summary>
        /// Tiny Roads is a network category created by Next2. Doesn't exist in vanilla game
        /// Some asset creators used modtools to self-assign their roads as tiny roads
        /// </summary>
        public bool tinyRoadsExist = false;

        public HashSet<Asset> next2Assets = new HashSet<Asset>();
        public HashSet<Asset> etstAssets = new HashSet<Asset>();
        public HashSet<Asset> owttAssets = new HashSet<Asset>();

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

                                // check local custom assets(not from steam workshop subscription)
                                if (current.package.packagePath.StartsWith(DataLocation.addonsPath))
                                {
                                    localWorkshopIDs.Add(steamid);
                                }
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

            // Inherited from Find It 1. I don't know why this is needed. It only removes the 's' in a name tag
            // CleanDictionarys();
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
                        // sub-buildings. They were filtered out in Find It 1 but are accessible in Find It 2 thru the extral filters panel
                        if (buildingPrefab.m_placementStyle == ItemClass.Placement.Procedural && buildingPrefab.m_buildingAI.GetType() != typeof(BuildingAI))
                        {
                            //filtered += prefab.name + ", ";
                            //continue;
                        }
                    }

                    PropInfo propPrefab = prefab as PropInfo;
                    if (propPrefab != null)
                    {
                        // filter out floating props
                        if (propPrefab.m_requireWaterMap && propPrefab.m_lodWaterHeightMap == null)
                        {
                            filtered += prefab.name + ", ";
                            continue;
                        }

                        // filter out vortex chirpy
                        if (propPrefab.name.StartsWith("Vortex Chirpy"))
                        {
                            filtered += prefab.name + ", ";
                            continue;
                        }
                    }
                }

                NetInfo netPrefab = prefab as NetInfo;
                if (netPrefab != null)
                {
                    if ((!prefab.m_isCustomContent) && (netPrefab.name == "Airplane Runway" || netPrefab.name == "Airplane Taxiway" || netPrefab.name == "Aviation Club Runway"))
                    {
                        SetAirplaneRoads(netPrefab);
                    }

                    // filter out network assets that are not supposed to be used by players directly
                    else if (netPrefab.category == PrefabInfo.kDefaultCategory || netPrefab.m_Thumbnail.IsNullOrWhiteSpace() ||
                        (netPrefab.name != "Pedestrian Pavement" && netPrefab.m_Thumbnail == "ThumbnailBuildingBeautificationPedestrianPavement"))
                    {
                        // filtered += prefab.name + ", ";
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
            string[] tagsArr = Regex.Split(text, @"([^\w]|[-]|\s)+", RegexOptions.IgnoreCase);

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

        public void UpdatePrefabInstanceCount(UISearchBox.DropDownOptions filter)
        {
            prefabInstanceCountDictionary.Clear();

            if (BuildingManager.exists &&
                ((filter == UISearchBox.DropDownOptions.All) || (filter == UISearchBox.DropDownOptions.Growable) ||
                (filter == UISearchBox.DropDownOptions.GrwbRico) || (filter == UISearchBox.DropDownOptions.Ploppable)
                || (filter == UISearchBox.DropDownOptions.Rico)))
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

            if (PropManager.exists && ((filter == UISearchBox.DropDownOptions.All) || (filter == UISearchBox.DropDownOptions.Prop) || (filter == UISearchBox.DropDownOptions.Decal)))
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

            if (TreeManager.exists && ((filter == UISearchBox.DropDownOptions.All) || (filter == UISearchBox.DropDownOptions.Tree)))
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

            if (NetManager.exists && ((filter == UISearchBox.DropDownOptions.All) || (filter == UISearchBox.DropDownOptions.Network)))
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

        public void UpdateAssetInstanceCount(Asset asset, bool forceUpdatePO = false)
        {
            if (!prefabInstanceCountDictionary.ContainsKey(asset.prefab)) asset.instanceCount = 0;
            else asset.instanceCount = prefabInstanceCountDictionary[asset.prefab];

            if ((Settings.includePOinstances || forceUpdatePO) && FindIt.isPOEnabled)
            {
                asset.poInstanceCount = ProceduralObjectsTool.GetPrefabPOInstanceCount(asset.prefab);
            }
        }

        /// <summary>
        /// Read the set of vehicle & tree props generated by tvp mod and update asset's prop type.
        /// Data is provided by TV Props Patch mod.
        /// </summary>
        public void SetTVPProps()
        {
            try
            {
                Type TVPropPatchModType = Type.GetType("TVPropPatch.Mod");
                HashSet<PropInfo> generatedVehicleProp = (HashSet<PropInfo>)TVPropPatchModType.GetField("generatedVehicleProp").GetValue(null);
                HashSet<PropInfo> generatedTreeProp = (HashSet<PropInfo>)TVPropPatchModType.GetField("generatedTreeProp").GetValue(null);
                Dictionary<PropInfo, VehicleInfo> propVehicleInfoTable = (Dictionary<PropInfo, VehicleInfo>)TVPropPatchModType.GetField("propVehicleInfoTable").GetValue(null);

                foreach (Asset asset in assets.Values)
                {
                    PropInfo propInfo = asset.prefab as PropInfo;
                    if (propInfo == null) continue;

                    if (generatedTreeProp.Contains(propInfo)) asset.propType = Asset.PropType.PropsTree;
                    if (generatedVehicleProp.Contains(propInfo)) asset.SetVehiclePropType(propVehicleInfoTable, propInfo);
                    asset.SetFindIt2Description();
                }
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }

            isTVPPatchModProcessed = true;
        }

        public void SetNext2Assets()
        {
            HashSet<string> next2AssetsNames = new HashSet<string>
            {
                "Two-Lane Alley",
                "One-Lane Oneway",
                "One-Lane Oneway With Parking",
                "Tiny Cul-De-Sac",
                "PlainStreet2L",
                "BasicRoadPntMdn",
                "One-Lane Oneway With Two Bicycle Lanes",
                "BasicRoadTL",
                "AsymRoadL1R2",
                "BasicRoadMdn",
                "BasicRoadMdn Decoration Grass",
                "BasicRoadMdn Decoration Trees",
                "Oneway3L",
                "Small Avenue",
                "AsymAvenueL2R4",
                "AsymAvenueL2R3",
                "AsymRoadL1R3",
                "Oneway4L",
                "Medium Avenue",
                "Medium Avenue TL",
                "Six-Lane Avenue Median",
                "Eight-Lane Avenue",
                "Small Rural Highway",
                "Rural Highway",
                "AsymHighwayL1R2",
                "Highway2L2W",
                "Four-Lane Highway",
                "Five-Lane Highway",
                "Large Highway",
                "Small Busway",
                "Small Busway Decoration Grass",
                "Small Busway Decoration Trees",
                "Small Busway OneWay",
                "Small Busway OneWay Decoration Grass",
                "Small Busway OneWay Decoration Trees",
                "Large Road With Bus Lanes",
                "Large Road Decoration Grass With Bus Lanes",
                "Large Road Decoration Trees With Bus Lanes",
                "Zonable Pedestrian Gravel Tiny",
                "Zonable Pedestrian Boardwalk Tiny",
                "Zonable Pedestrian Gravel",
                "Zonable Pedestrian Pavement Tiny",
                "Zonable Pedestrian Pavement",
                "Zonable Pedestrian Stone Tiny Road",
                "Zonable Promenade",
                "Medium Avenue Side Light",
                "Large Avenue Median Light",
                "BusLaneText"
            };

            foreach (Asset asset in assets.Values)
            {
                if (asset.assetType != Asset.AssetType.Network) continue;
                if (asset.prefab.m_isCustomContent) continue;
                if (next2AssetsNames.Contains(asset.prefab.name))
                {
                    next2Assets.Add(asset);
                }
            }
        }

        public void SetETSTAssets()
        {
            HashSet<string> etstAssetsNames = new HashSet<string>
            {
                "Station Track Eleva",
                "Station Track Elevated (C)",
                "Station Track Elevated (NP)",
                "Station Track Elevated (CNP)",
                "Station Track Elevated Narrow",
                "Station Track Elevated Narrow (C)",
                "Station Track Elevated Narrow (NP)",
                "Station Track Elevated Narrow (CNP)",
                "Station Track Sunken",
                "Station Track Sunken (NP)",
                "Train Station Track (C)",
                "Train Station Track (NP)",
                "Train Station Track (CNP)",
                "Station Track Tunnel"
            };
            foreach (Asset asset in assets.Values)
            {
                if (asset.assetType != Asset.AssetType.Network) continue;
                if (asset.prefab.m_isCustomContent) continue;
                if (etstAssetsNames.Contains(asset.prefab.name))
                {
                    etstAssets.Add(asset);
                }
            }
        }

        // One-Way Train Tracks mod
        public void SetOWTTAssets()
        {
            foreach (Asset asset in assets.Values)
            {
                if (asset.assetType != Asset.AssetType.Network) continue;
                if (asset.prefab.name.StartsWith("Rail1L") || asset.prefab.name.StartsWith("Oneway Train Track")) owttAssets.Add(asset);
            }
        }

        /// <summary>
        /// Set up access to airport roads. Modified from SamsamTS's Airport Roads mod
        /// </summary>
        public void SetAirplaneRoads(PrefabInfo prefab)
        {
            int constructionCost = 0;
            int maintenanceCost = 0;
            string thumbnail = "";

            if (prefab.name == "Airplane Runway")
            {
                constructionCost = 7000;
                maintenanceCost = 600;
                thumbnail = "Runway";
            }
            else if (prefab.name == "Aviation Club Runway")
            {
                constructionCost = 7000;
                maintenanceCost = 600;
                thumbnail = "Runway";
                prefab.m_dlcRequired = SteamHelper.DLC_BitMask.UrbanDLC; // Sunset Harbor
            }
            else if (prefab.name == "Airplane Taxiway")
            {
                constructionCost = 4000;
                maintenanceCost = 200;
                thumbnail = "Taxiway";
            }

            // Adding cost
            NetInfo netInfo = prefab as NetInfo;
            if (netInfo == null) return;
            PlayerNetAI netAI = netInfo.m_netAI as PlayerNetAI;
            netAI.m_constructionCost = constructionCost;
            netAI.m_maintenanceCost = maintenanceCost;

            // Making the prefab valid
            netInfo.m_availableIn = ItemClass.Availability.All;
            netInfo.m_placementStyle = ItemClass.Placement.Manual;
            typeof(NetInfo).GetField("m_UICategory", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(netInfo, "PublicTransportPlane");

            // Adding icons
            netInfo.m_Atlas = SamsamTS.UIUtils.GetAtlas("FindItAtlas");
            netInfo.m_Thumbnail = thumbnail;
            netInfo.m_InfoTooltipAtlas = SamsamTS.UIUtils.GetAtlas("FindItAtlas");

            // Adding missing locale
            Locale locale = (Locale)typeof(LocaleManager).GetField("m_Locale", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(SingletonLite<LocaleManager>.instance);
            Locale.Key key = new Locale.Key() { m_Identifier = "NET_TITLE", m_Key = prefab.name };
            if (!locale.Exists(key)) locale.AddLocalizedString(key, prefab.name);
            key = new Locale.Key() { m_Identifier = "NET_DESC", m_Key = prefab.name };
            if (!locale.Exists(key)) locale.AddLocalizedString(key, thumbnail);
        }

    }
}
