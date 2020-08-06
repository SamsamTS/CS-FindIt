// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.DataBinding;
using ColossalFramework.Globalization;

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
            Network,
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
            PropsMarker
        }

        public enum TreeType
        {
            Invalid = -1,
            SmallTree,
            MediumTree,
            LargeTree
        }

        public enum NetworkType
        {
            Invalid = -1,
            SmallRoads,
            MediumRoads,
            LargeRoads,
            Highway,
            Path,
            Fence,
            WaterStructures,
            Unsorted
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
                        if (buildingPrefab.m_generatedInfo?.m_heights != null)
                        {
                            if (buildingPrefab.m_generatedInfo.m_heights.Length != 0)
                            {
                                buildingHeight = buildingPrefab.m_generatedInfo.m_heights.Max();
                            }
                        }
                        SetFindIt2Description();
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
                        SetFindIt2Description();
                        return;
                    }

                    else if (m_prefab is TreeInfo)
                    {
                        assetType = AssetType.Tree;
                        TreeInfo info = m_prefab as TreeInfo;
                        treeType = SetTreeType(info);
                    }

                    else if (m_prefab is NetInfo)
                    {
                        assetType = AssetType.Network;
                        NetInfo info = m_prefab as NetInfo;
                        networkType = SetNetworkType(info);
                    }

                    SetFindIt2Description();
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
                    SetFindIt2Description();
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
        public NetworkType networkType = NetworkType.Invalid;
        public string findIt2Description;

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

        public static string GetLocalizedTooltip(Asset asset, PrefabInfo prefab, string title)
        {
            MilestoneInfo unlockMilestone = prefab.GetUnlockMilestone();

            string text = TooltipHelper.Format(new string[]
            {
                LocaleFormatter.Title,
                title,
                LocaleFormatter.Sprite,
                (!string.IsNullOrEmpty(prefab.m_InfoTooltipThumbnail)) ? prefab.m_InfoTooltipThumbnail : prefab.name,
                LocaleFormatter.Text,
                (asset.findIt2Description + Asset.GetLocalizedDescription(prefab)),
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
            else if (propEditorCategory.StartsWith("PropsMarker")) return Asset.PropType.PropsMarker;

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

        // check the type of network assets. A big ugly and messy but I don't know a better way to do the categorization.
        private Asset.NetworkType SetNetworkType(NetInfo info)
        {
            if (info == null) return Asset.NetworkType.Invalid;

            if (info.category.StartsWith("RoadsSmall")) return Asset.NetworkType.SmallRoads;
            else if (info.category.StartsWith("RoadsTiny")) return Asset.NetworkType.SmallRoads;
            else if (info.category.StartsWith("RoadsMedium")) return Asset.NetworkType.MediumRoads;
            else if (info.category.StartsWith("RoadsSmallHV")) return Asset.NetworkType.MediumRoads;
            else if (info.category.StartsWith("RoadsLarge")) return Asset.NetworkType.LargeRoads;
            else if (info.category.StartsWith("RoadsHighway")) return Asset.NetworkType.Highway;
            else if (info.m_class.name.StartsWith("Pedestrian")) return Asset.NetworkType.Path;
            else if (info.editorCategory.StartsWith("LandscapingWaterStructures")) return Asset.NetworkType.WaterStructures;
            else if (info.editorCategory.StartsWith("LandscapingFences")) return Asset.NetworkType.Fence;
            else if (info.m_class.name.StartsWith("Beautification")) return Asset.NetworkType.Fence;
            else if (info.m_class.name.StartsWith("Landscaping Quay")) return Asset.NetworkType.Fence;

            return NetworkType.Unsorted;
        }

        /// <summary>
        /// Add Find It 2 description: asset type, sub-type, size, height
        /// </summary>
        private void SetFindIt2Description()
        {
            if (assetType == AssetType.Decal)
            {
                findIt2Description = Translations.Translate("FIF_SE_ID");
            }
            else if (assetType == AssetType.Network)
            {
                findIt2Description = $"{Translations.Translate("FIF_SE_IN")}, ";

                if (networkType == Asset.NetworkType.SmallRoads) findIt2Description += Translations.Translate("FIF_NET_SMR");
                if (networkType == Asset.NetworkType.MediumRoads) findIt2Description += Translations.Translate("FIF_NET_MDR");
                if (networkType == Asset.NetworkType.LargeRoads) findIt2Description += Translations.Translate("FIF_NET_LGR");
                if (networkType == Asset.NetworkType.Highway) findIt2Description += Translations.Translate("FIF_NET_HGHW");
                if (networkType == Asset.NetworkType.Path) findIt2Description += Translations.Translate("FIF_NET_PATH");
                if (networkType == Asset.NetworkType.Fence) findIt2Description += Translations.Translate("FIF_NET_WALL");
                if (networkType == Asset.NetworkType.WaterStructures) findIt2Description += Translations.Translate("FIF_NET_WAT");
                if (networkType == Asset.NetworkType.Unsorted) findIt2Description += Translations.Translate("FIF_NET_UNS");
            }
            else if (assetType == AssetType.Growable || assetType == AssetType.Rico)
            {
                if (assetType == AssetType.Rico)
                {
                    findIt2Description = $"{Translations.Translate("FIF_SE_IR")}, ";
                }
                else
                {
                    findIt2Description = $"{Translations.Translate("FIF_SE_IG")}, ";
                }
                if (subService == ItemClass.SubService.ResidentialLow) findIt2Description += Translations.Translate("FIF_GR_LDR");
                if (subService == ItemClass.SubService.ResidentialHigh) findIt2Description += Translations.Translate("FIF_GR_HDR");
                if (subService == ItemClass.SubService.ResidentialLowEco) findIt2Description += Translations.Translate("FIF_GR_LDRE");
                if (subService == ItemClass.SubService.ResidentialHighEco) findIt2Description += Translations.Translate("FIF_GR_HDRE");
                if (subService == ItemClass.SubService.CommercialLow) findIt2Description += Translations.Translate("FIF_GR_LDC");
                if (subService == ItemClass.SubService.CommercialHigh) findIt2Description += Translations.Translate("FIF_GR_HDC");
                if (subService == ItemClass.SubService.CommercialEco) findIt2Description += Translations.Translate("FIF_GR_CE");
                if (subService == ItemClass.SubService.CommercialLeisure) findIt2Description += Translations.Translate("FIF_GR_LC");
                if (subService == ItemClass.SubService.CommercialTourist) findIt2Description += Translations.Translate("FIF_GR_TC");
                if (subService == ItemClass.SubService.IndustrialGeneric) findIt2Description += Translations.Translate("FIF_GR_GI");
                if (subService == ItemClass.SubService.IndustrialFarming) findIt2Description += Translations.Translate("FIF_GR_FAI");
                if (subService == ItemClass.SubService.IndustrialForestry) findIt2Description += Translations.Translate("FIF_GR_FOI");
                if (subService == ItemClass.SubService.IndustrialOil) findIt2Description += Translations.Translate("FIF_GR_OII");
                if (subService == ItemClass.SubService.IndustrialOre) findIt2Description += Translations.Translate("FIF_GR_ORI");
                if (subService == ItemClass.SubService.OfficeGeneric) findIt2Description += Translations.Translate("FIF_GR_O");
                if (subService == ItemClass.SubService.OfficeHightech) findIt2Description += Translations.Translate("FIF_GR_ITC");

                findIt2Description += $", {size.x}x{size.y}, {(int)buildingHeight} {Translations.Translate("FIF_EF_MET")} ({(int)(buildingHeight * 3.28084f)} {Translations.Translate("FIF_EF_FEE")})";
            }
            else if (assetType == AssetType.Prop)
            {
                findIt2Description = $"{Translations.Translate("FIF_SE_IPR")}, ";
                if (propType == Asset.PropType.PropsIndustrial) findIt2Description += Translations.Translate("FIF_PROP_IND");
                if (propType == Asset.PropType.PropsParks) findIt2Description += Translations.Translate("FIF_PROP_PAR");
                if (propType == Asset.PropType.PropsCommon) findIt2Description += Translations.Translate("FIF_PROP_COM");
                if (propType == Asset.PropType.PropsResidential) findIt2Description += Translations.Translate("FIF_PROP_RES");
                if (propType == Asset.PropType.PropsBillboards) findIt2Description += Translations.Translate("FIF_PROP_BIL");
                if (propType == Asset.PropType.PropsSpecialBillboards) findIt2Description += Translations.Translate("FIF_PROP_SPE");
                if (propType == Asset.PropType.PropsLights) findIt2Description += Translations.Translate("FIF_PROP_LIG");
                if (propType == Asset.PropType.Natural) findIt2Description += Translations.Translate("FIF_PROP_NAT");
                if (propType == Asset.PropType.Unsorted) findIt2Description += Translations.Translate("FIF_PROP_UNS");
            }
            else if (assetType == AssetType.Tree)
            {
                findIt2Description = $"{Translations.Translate("FIF_SE_IT")}, ";
                if (treeType == Asset.TreeType.SmallTree) findIt2Description += Translations.Translate("FIF_TREE_SM");
                if (treeType == Asset.TreeType.MediumTree) findIt2Description += Translations.Translate("FIF_TREE_MD");
                if (treeType == Asset.TreeType.LargeTree) findIt2Description += Translations.Translate("FIF_TREE_LG");
            }
            else if (assetType == AssetType.Ploppable)
            {
                findIt2Description = $"{Translations.Translate("FIF_SE_IP")}, ";

                if (service == ItemClass.Service.Electricity) findIt2Description += Translations.Translate("FIF_PLOP_E");
                if (service == ItemClass.Service.Water) findIt2Description += Translations.Translate("FIF_PLOP_W");
                if (service == ItemClass.Service.Garbage) findIt2Description += Translations.Translate("FIF_PLOP_G");
                if (service == ItemClass.Service.PlayerIndustry) findIt2Description += Translations.Translate("FIF_PLOP_I");
                if (service == ItemClass.Service.Fishing) findIt2Description += Translations.Translate("FIF_PLOP_FI");
                if (service == ItemClass.Service.HealthCare) findIt2Description += Translations.Translate("FIF_PLOP_H");
                if (service == ItemClass.Service.FireDepartment) findIt2Description += Translations.Translate("FIF_PLOP_F");
                if (service == ItemClass.Service.Disaster) findIt2Description += Translations.Translate("FIF_PLOP_D");
                if (service == ItemClass.Service.PoliceDepartment) findIt2Description += Translations.Translate("FIF_PLOP_P");
                if (service == ItemClass.Service.Education) findIt2Description += Translations.Translate("FIF_PLOP_ED");
                if (service == ItemClass.Service.PlayerEducation) findIt2Description += Translations.Translate("FIF_PLOP_C");
                if (service == ItemClass.Service.Museums) findIt2Description += Translations.Translate("FIF_PLOP_C");
                if (service == ItemClass.Service.VarsitySports) findIt2Description += Translations.Translate("FIF_PLOP_V");
                if (service == ItemClass.Service.PublicTransport) findIt2Description += Translations.Translate("FIF_PLOP_PT");
                if (service == ItemClass.Service.Tourism) findIt2Description += Translations.Translate("FIF_PLOP_PT");
                if (service == ItemClass.Service.Beautification) findIt2Description += Translations.Translate("FIF_PLOP_PPW");
                if (service == ItemClass.Service.Monument) findIt2Description += Translations.Translate("FIF_PLOP_U");

                findIt2Description += $", {size.x}x{size.y}, {(int)buildingHeight} {Translations.Translate("FIF_EF_MET")} ({(int)(buildingHeight * 3.28084f)} {Translations.Translate("FIF_EF_FEE")})";
            }

            findIt2Description += "\n";
        }
    }
}
