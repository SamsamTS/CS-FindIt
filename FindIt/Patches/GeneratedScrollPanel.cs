using System;
using System.Reflection;
using UnityEngine;
using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.PlatformServices;
using HarmonyLib;
using FindIt.GUI;

namespace FindIt
{
    // This patch adds some of Find It's own UI stuff (like the custom tag and steam sprites) to the game's default panels
    [HarmonyPatch(typeof(GeneratedScrollPanel))]
    [HarmonyPatch("CreateButton")]
    [HarmonyPatch(new Type[] { typeof(string), typeof(string), typeof(string), typeof(int), typeof(UITextureAtlas), typeof(UIComponent), typeof(bool) })]
    internal static class CreateButtonPatch
    {
        private static void Postfix(UIButton __result, GeneratedScrollPanel __instance, string name, string tooltip, string baseIconName, int index, UITextureAtlas atlas, UIComponent tooltipBox, bool enabled)
        {
            if (Settings.hideExtraUIonVP) return;

            __result.eventVisibilityChanged += new PropertyChangedEventHandler<bool>(Init);

            GeneratedScrollPanel panel = __instance;
            SimulationManager.instance.AddAction(() =>
            {
                if (__result.objectUserData is PrefabInfo prefab)
                {
                    string key = Asset.GetName(prefab);

                    if (AssetTagList.instance.assets.ContainsKey(key))
                    {
                        if (AssetTagList.instance.assets[key].onButtonClicked == null)
                        {
                            MethodInfo onButtonClicked = panel.GetType().GetMethod("OnButtonClicked", BindingFlags.NonPublic | BindingFlags.Instance);
                            AssetTagList.instance.assets[key].onButtonClicked = Delegate.CreateDelegate(typeof(Asset.OnButtonClicked), panel, onButtonClicked, false) as Asset.OnButtonClicked;
                        }
                    }
                }
            });
        }

        private static UIComponent tsContainer;

        private static void Init(UIComponent component, bool b)
        {
            component.eventVisibilityChanged -= new PropertyChangedEventHandler<bool>(Init);

            try
            {
                if (component.objectUserData is PrefabInfo prefab)
                {
                    string name = Asset.GetName(prefab);

                    if (AssetTagList.instance.assets.ContainsKey(name))
                    {
                        ImageUtils.FixThumbnails(prefab, component as UIButton);

                        Asset asset = AssetTagList.instance.assets[name];

                        component.eventVisibilityChanged += (c, p) =>
                        {
                            if (Settings.unlockAll)
                            {
                                c.isEnabled = true;
                            }
                            else
                            {
                                c.isEnabled = ToolsModifierControl.IsUnlocked(prefab.GetUnlockMilestone());
                            }
                        };

                        // Fixing focused texture
                        component.eventClicked += new MouseEventHandler(FixFocusedTexture);

                        // Find TSContainer
                        if (tsContainer == null)
                        {
                            tsContainer = component.parent.parent.parent.parent.parent;
                        }
                        float uiScale = 1.0f;
                        if (tsContainer != null) uiScale = tsContainer.transform.localScale.x;

                        // Adding custom tag icon
                        UISprite tagSprite = component.AddUIComponent<UISprite>();
                        tagSprite.size = new Vector2(20 * uiScale, 16 * uiScale);
                        tagSprite.atlas = FindIt.atlas;
                        tagSprite.spriteName = "Tag";
                        tagSprite.opacity = 0.5f;
                        tagSprite.tooltipBox = UIView.GetAView().defaultTooltipBox;
                        tagSprite.relativePosition = new Vector3(component.width - tagSprite.width - (5 * uiScale), 5 * uiScale);
                        tagSprite.isVisible = false;

                        if (CustomTagsLibrary.assetTags.ContainsKey(name))
                        {
                            tagSprite.tooltip = CustomTagsLibrary.assetTags[name];
                        }
                        else
                        {
                            tagSprite.tooltip = null;
                        }

                        tagSprite.eventMouseEnter += (c, p) =>
                        {
                            tagSprite.opacity = 1f;
                        };

                        tagSprite.eventMouseLeave += (c, p) =>
                        {
                            tagSprite.opacity = 0.5f;
                        };

                        tagSprite.eventClick += (c, p) =>
                        {
                            p.Use();

                            UITagsWindow.ShowAt(asset, tagSprite);
                        };

                        component.eventMouseEnter += (c, p) =>
                        {
                            tagSprite.isVisible = true;
                        };

                        component.eventMouseLeave += (c, p) =>
                        {
                            if (asset.tagsCustom.Count == 0)
                            {
                                tagSprite.isVisible = false;
                            }
                        };

                        // adding DLC/steam icon
                        UISprite m_dlcSprite = component.AddUIComponent<UISprite>();
                        m_dlcSprite.size = new Vector2(16 * uiScale, 16 * uiScale);
                        m_dlcSprite.atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
                        m_dlcSprite.opacity = 0.8f;
                        m_dlcSprite.tooltipBox = UIView.GetAView().defaultTooltipBox;
                        m_dlcSprite.relativePosition = new Vector3(component.width - m_dlcSprite.width - (3 * uiScale), component.height - m_dlcSprite.height - (3 * uiScale));
                        m_dlcSprite.isVisible = false;
                        m_dlcSprite.eventMouseLeave += (c, p) =>
                        {
                            m_dlcSprite.tooltipBox.Hide();
                        };

                        m_dlcSprite.tooltip = null;
                        m_dlcSprite.isVisible = false;

                        if (asset != null)
                        {
                            // next 2 assets, show blue steam icon
                            if (FindIt.isNext2Enabled && AssetTagList.instance.next2Assets.Contains(asset))
                            {
                                m_dlcSprite.isVisible = true;
                                m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";
                                m_dlcSprite.tooltip = "Network Extension 2 Mod";
                            }
                            // etst assets, show blue steam icon
                            else if (FindIt.isETSTEnabled && AssetTagList.instance.etstAssets.Contains(asset))
                            {
                                m_dlcSprite.isVisible = true;
                                m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";
                                m_dlcSprite.tooltip = "Extra Train Station Tracks Mod";
                            }
                            // owtt assets, show blue steam icon
                            else if (FindIt.isOWTTEnabled && AssetTagList.instance.owttAssets.Contains(asset))
                            {
                                m_dlcSprite.isVisible = true;
                                m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";
                                m_dlcSprite.tooltip = "One-Way Train Tracks Mod";
                            }
                            // tvp patch assets, show blue steam icon
                            else if ((FindIt.isTVPPatchEnabled || FindIt.isTVP2Enabled) && asset.assetType == Asset.AssetType.Prop && AssetTagList.instance.tvppAssets.Contains(asset))
                            {
                                m_dlcSprite.isVisible = true;
                                m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";

                                // if based on vanilla assets, show blue steam icon
                                if (!asset.prefab.m_isCustomContent)
                                {
                                    m_dlcSprite.tooltip = "Tree & Vehicle Props Mod";

                                    if (asset.prefab.m_dlcRequired == SteamHelper.DLC_BitMask.AirportDLC)
                                    {
                                        m_dlcSprite.tooltip += "\nAirports DLC";
                                    }
                                    else if (asset.prefab.m_dlcRequired == SteamHelper.DLC_BitMask.ModderPack10)
                                    {
                                        m_dlcSprite.tooltip += "\nVehicles of the World CCP by bsquiklehausen";
                                    }
                                }
                                else
                                {
                                    if (!asset.author.IsNullOrWhiteSpace() && (asset.steamID != 0))
                                    {
                                        m_dlcSprite.tooltip = "Tree & Vehicle Props Mod\nBy " + asset.author + "\n" + "ID: " + asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                    }
                                    else if (asset.steamID != 0)
                                    {
                                        m_dlcSprite.tooltip = "Tree & Vehicle Props Mod\n" + "ID: " + asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                    }
                                    else
                                    {
                                        m_dlcSprite.tooltip = "Tree & Vehicle Props Mod\n" + Translations.Translate("FIF_UIS_CNWS");
                                    }
                                }
                            }
                            // ntcp assets, show blue steam icon
                            else if (FindIt.isNTCPEnabled && asset.assetType == Asset.AssetType.Prop && AssetTagList.instance.ntcpAssets.Contains(asset))
                            {
                                m_dlcSprite.isVisible = true;
                                m_dlcSprite.spriteName = "UIFilterWorkshopItemsFocusedHovered";

                                // if based on vanilla assets, show blue steam icon
                                if (!asset.prefab.m_isCustomContent)
                                {
                                    m_dlcSprite.tooltip = "Non-terrain Conforming Props Mod";
                                }
                                else
                                {
                                    if (!asset.author.IsNullOrWhiteSpace() && (asset.steamID != 0))
                                    {
                                        m_dlcSprite.tooltip = "Non-terrain Conforming Props Mod\nBy " + asset.author + "\n" + "ID: " + asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                    }
                                    else if (asset.steamID != 0)
                                    {
                                        m_dlcSprite.tooltip = "Non-terrain Conforming Props Mod" + "\n" + "ID: " + asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                    }
                                    else
                                    {
                                        m_dlcSprite.tooltip = "Non-terrain Conforming Props Mod\n" + Translations.Translate("FIF_UIS_CNWS");
                                    }
                                }
                            }
                            // vanilla assets, show corresponding dlc icons
                            else if (!asset.prefab.m_isCustomContent)
                            {
                                SetDLCSprite(m_dlcSprite, asset.prefab.m_dlcRequired);
                            }
                            // custom assets, show steam icon(has workshop info) or yellow cogwheel icon(no workshop info)
                            else
                            {
                                if (!asset.author.IsNullOrWhiteSpace() && (asset.steamID != 0))
                                {
                                    m_dlcSprite.opacity = 0.2f;
                                    m_dlcSprite.isVisible = true;
                                    m_dlcSprite.spriteName = "UIFilterWorkshopItems";
                                    m_dlcSprite.tooltip = "By " + asset.author + "\n" + "ID: " + asset.steamID + "\n" + Translations.Translate("FIF_UIS_WS");
                                }
                                else
                                {
                                    m_dlcSprite.opacity = 0.45f;
                                    m_dlcSprite.isVisible = true;
                                    m_dlcSprite.spriteName = "UIFilterProcessingBuildings";
                                    m_dlcSprite.tooltip = Translations.Translate("FIF_UIS_CNWS");
                                }
                            }
                        }

                        if (PlatformService.IsOverlayEnabled() && m_dlcSprite.spriteName == "UIFilterWorkshopItems")
                        {
                            m_dlcSprite.eventMouseUp += (c, p) =>
                            {
                                if (!p.used && p.buttons == UIMouseButton.Right)
                                {
                                    PublishedFileId publishedFileId = new PublishedFileId(asset.steamID);

                                    if (publishedFileId != PublishedFileId.invalid)
                                    {
                                        if (!Settings.useDefaultBrowser)
                                        {
                                            PlatformService.ActivateGameOverlayToWorkshopItem(publishedFileId);
                                        }
                                        else
                                        {
                                            // UnityEngine.Application.OpenURL("https://steamcommunity.com/sharedfiles/filedetails/?id=" + publishedFileId);
                                            System.Diagnostics.Process.Start("https://steamcommunity.com/sharedfiles/filedetails/?id=" + publishedFileId);
                                        }
                                        p.Use();
                                    }
                                }
                            };
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }
        }

        private static void FixFocusedTexture(UIComponent component, UIMouseEventParameter p)
        {
            component.eventClicked -= new MouseEventHandler(FixFocusedTexture);

            try
            {
                if (component.objectUserData is PrefabInfo prefab)
                {
                    if (ImageUtils.FixFocusedTexture(prefab))
                    {
                        // Debugging.Message("Fixed focused texture: " + prefab.name);
                    }
                    UIScrollPanelItem.fixedFocusedTexture.Add(prefab);
                }
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }
        }

        private static void SetDLCSprite(UISprite sprite, SteamHelper.DLC_BitMask dlc)
        {
            if (dlc == SteamHelper.DLC_BitMask.None) return;

            sprite.isVisible = true;

            if (dlc == SteamHelper.DLC_BitMask.DeluxeDLC)
            {
                sprite.tooltip = "Deluxe Upgrade Pack";
                sprite.spriteName = "DeluxeIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.AfterDarkDLC)
            {
                sprite.tooltip = "After Dark DLC";
                sprite.spriteName = "ADIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.SnowFallDLC)
            {
                sprite.tooltip = "Snow Fall DLC";
                sprite.spriteName = "WWIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.NaturalDisastersDLC)
            {
                sprite.tooltip = "Natural Disasters DLC";
                sprite.spriteName = "NaturalDisastersIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.InMotionDLC)
            {
                sprite.tooltip = "Mass Transit DLC";
                sprite.spriteName = "MassTransitIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.GreenCitiesDLC)
            {
                sprite.tooltip = "Green Cities DLC";
                sprite.spriteName = "GreenCitiesIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.ParksDLC)
            {
                sprite.tooltip = "Parklife DLC";
                sprite.spriteName = "ParkLifeIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.IndustryDLC)
            {
                sprite.tooltip = "Industries DLC";
                sprite.spriteName = "IndustriesIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.CampusDLC)
            {
                sprite.tooltip = "Campus DLC";
                sprite.spriteName = "CampusIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.UrbanDLC)
            {
                sprite.tooltip = "Sunset Harbor DLC";
                sprite.spriteName = "DonutIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.AirportDLC)
            {
                sprite.tooltip = "Airports DLC";
                sprite.spriteName = "AirportIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.Football)
            {
                sprite.tooltip = "Match Day DLC";
                sprite.spriteName = "MDIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.Football2345)
            {
                sprite.tooltip = "Stadiums: European Club Pack DLC";
                sprite.spriteName = "StadiumsDLCIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.OrientalBuildings)
            {
                sprite.tooltip = "Pearls from the East DLC";
                sprite.spriteName = "ChineseBuildingsTagIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.MusicFestival)
            {
                sprite.tooltip = "Concerts DLC";
                sprite.spriteName = "ConcertsIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.ModderPack1)
            {
                sprite.tooltip = "Art Deco Content Creator Pack by Shroomblaze";
                sprite.spriteName = "ArtDecoIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.ModderPack2)
            {
                sprite.tooltip = "High-Tech Buildings Content Creator Pack by GCVos";
                sprite.spriteName = "HighTechIcon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.ModderPack3)
            {
                sprite.tooltip = "European Suburbias Content Creator Pack by Avanya";
                sprite.spriteName = "Modderpack3Icon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.ModderPack4)
            {
                sprite.tooltip = "University City Content Creator Pack by KingLeno";
                sprite.spriteName = "Modderpack4Icon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.ModderPack5)
            {
                sprite.tooltip = "Modern City Center Content Creator Pack by AmiPolizeiFunk";
                sprite.spriteName = "Modderpack5Icon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.ModderPack6)
            {
                sprite.tooltip = "Modern Japan Content Creator Pack by Ryuichi Kaminogi";
                sprite.spriteName = "Modderpack6Icon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.ModderPack7)
            {
                sprite.tooltip = "Train Stations Content Creator Pack by BadPeanut";
                sprite.spriteName = "Modderpack7Icon";
            }
            else if (dlc == SteamHelper.DLC_BitMask.ModderPack8)
            {
                sprite.tooltip = "Bridges & Piers Content Creator Pack by Armesto";
                sprite.spriteName = "Modderpack8Icon";
            }
            else
            {
                sprite.tooltip = "Unknown DLC";
                sprite.spriteName = "ToolbarIconHelp";
            }
        }
    }
}
