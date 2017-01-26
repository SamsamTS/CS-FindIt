using ICities;
using UnityEngine;

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Globalization;

using FindIt.Redirection;
using FindIt.GUI;

namespace FindIt
{
    public class FindIt : MonoBehaviour
    {
        public const string settingsFileName = "FindIt";

        public static FindIt instance;
        public static SavedBool unlockAll = new SavedBool("unlockAll", settingsFileName, false, true);
        public static bool fixBadProps;
        public static UITextureAtlas atlas;
        public static bool inEditor = false;
        public static bool thumbnailFixRunning = false;

        public static AssetTagList list;

        public UIButton mainButton;
        public UISearchBox searchBox;

        private UIGroupPanel m_groupPanel;

        public void Start()
        {
            try
            {
                GameObject gameObject = GameObject.Find("FindItMainButton");
                if (gameObject != null)
                {
                    return;
                }

                list = AssetTagList.instance;
                list.Init();

                StartCoroutine("FixFocusedThumbnails");
                LoadResources();

                UITabstrip tabstrip = ToolsModifierControl.mainToolbar.GetComponentInChildren<UITabstrip>();

                GameObject asGameObject = UITemplateManager.GetAsGameObject("MainToolbarButtonTemplate");
                GameObject asGameObject2 = UITemplateManager.GetAsGameObject("ScrollableSubPanelTemplate");

                mainButton = tabstrip.AddTab("FindItMainButton", asGameObject, asGameObject2, new Type[] { typeof(UIGroupPanel) }) as UIButton;
                mainButton.atlas = atlas;

                Locale locale = (Locale)typeof(LocaleManager).GetField("m_Locale", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(LocaleManager.instance);
                Locale.Key key = new Locale.Key
                {
                    m_Identifier = "TUTORIAL_ADVISER_TITLE",
                    m_Key = mainButton.name
                };
                if (!locale.Exists(key))
                {
                    locale.AddLocalizedString(key, "Find It! " + ModInfo.version);
                }
                key = new Locale.Key
                {
                    m_Identifier = "TUTORIAL_ADVISER",
                    m_Key = mainButton.name
                };
                if (!locale.Exists(key))
                {
                    locale.AddLocalizedString(key, "Thanks for subscribing to Find It!\n\nStart typing some keywords into the input field to find the desired asset.\n\nIf you like the mod please consider leaving a rating on the steam workshop.");
                }

                FieldInfo m_ObjectIndex = typeof(MainToolbar).GetField("m_ObjectIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                m_ObjectIndex.SetValue(ToolsModifierControl.mainToolbar, (int)m_ObjectIndex.GetValue(ToolsModifierControl.mainToolbar) + 1);

                mainButton.gameObject.GetComponent<TutorialUITag>().tutorialTag = name;
                m_groupPanel = tabstrip.GetComponentInContainer(mainButton, typeof(UIGroupPanel)) as UIGroupPanel;

                if (m_groupPanel != null)
                {
                    m_groupPanel.name = "FindItGroupPanel";
                    m_groupPanel.enabled = true;
                    m_groupPanel.component.isInteractive = true;
                    m_groupPanel.m_OptionsBar = ToolsModifierControl.mainToolbar.m_OptionsBar;
                    m_groupPanel.m_DefaultInfoTooltipAtlas = ToolsModifierControl.mainToolbar.m_DefaultInfoTooltipAtlas;
                    if (ToolsModifierControl.mainToolbar.enabled)
                    {
                        m_groupPanel.RefreshPanel();
                    }

                    UIScrollPanel scrollPanel = UIScrollPanel.Create(m_groupPanel.GetComponentInChildren<UIScrollablePanel>(), UIVerticalAlignment.Middle);
                    scrollPanel.eventClicked += OnButtonClicked;
                    scrollPanel.eventVisibilityChanged += (c, p) =>
                    {
                        if (p && scrollPanel.selectedItem != null)
                        {
                            SelectPrefab(scrollPanel.selectedItem.objectUserData as PrefabInfo);
                        }
                    };

                    scrollPanel.eventTooltipEnter += (c, p) =>
                    {
                        UIScrollPanelItem.RefreshTooltipAltas(p.source);
                    };

                    searchBox = scrollPanel.parent.AddUIComponent<UISearchBox>();
                    searchBox.scrollPanel = scrollPanel;
                    searchBox.relativePosition = new Vector3(0, -45);
                    searchBox.Search();
                }

                mainButton.normalBgSprite = "ToolbarIconGroup6Normal";
                mainButton.focusedBgSprite = "ToolbarIconGroup6Focused";
                mainButton.hoveredBgSprite = "ToolbarIconGroup6Hovered";
                mainButton.pressedBgSprite = "ToolbarIconGroup6ressed";
                mainButton.disabledBgSprite = "ToolbarIconGroup6Disabled";

                mainButton.normalFgSprite = "FindIt";
                mainButton.focusedFgSprite = "FindItFocused";
                mainButton.hoveredFgSprite = "FindItHovered";
                mainButton.pressedFgSprite = "FindItPressed";
                mainButton.disabledFgSprite = "FindItDisabled";

                mainButton.tooltip = "Find It! " + ModInfo.version;

                DebugUtils.Log("Initialized");
            }
            catch (Exception e)
            {
                DebugUtils.Log("Start failed");
                DebugUtils.LogException(e);
                enabled = false;
            }
        }

        private IEnumerator FixFocusedThumbnails()
        {
            DebugUtils.Log("FixFocusedThumbnails started");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            thumbnailFixRunning = true;
            int fixedCount = 0;

            Stopwatch stopWatch2 = new Stopwatch();
            stopWatch2.Start();

            foreach (Asset asset in list.assets.Values)
            {
                if(ImageUtils.FixFocusedTexture(asset.prefab))
                {
                    fixedCount++;
                }

                if (stopWatch2.ElapsedMilliseconds > 3)
                {
                    yield return null;
                    stopWatch2.Reset();
                    stopWatch2.Start();
                }
            }
            thumbnailFixRunning = false;
            stopWatch.Stop();
            DebugUtils.Log("FixFocusedThumbnails ended. Fixed " + fixedCount + " thumbnails in " + (stopWatch.ElapsedMilliseconds / 1000) + "s");
        }

        public void OnButtonClicked(UIComponent c, UIMouseEventParameter p)
        {
            UIButton uIButton = p.source as UIButton;
            if (uIButton != null && uIButton.parent is UIScrollPanel)
            {
                PrefabInfo prefab = uIButton.objectUserData as PrefabInfo;

                string key = Asset.GetName(prefab);
                if (AssetTagList.instance.assets.ContainsKey(key) && AssetTagList.instance.assets[key].onButtonClicked != null)
                {
                    AssetTagList.instance.assets[key].onButtonClicked(uIButton);
                }
                else
                {
                    SelectPrefab(prefab);
                }
            }
        }

        public static void SelectPrefab(PrefabInfo prefab)
        {
            if (prefab is BuildingInfo)
            {
                BuildingTool tool = ToolsModifierControl.SetTool<BuildingTool>();
                if (tool != null)
                {
                    tool.m_prefab = prefab as BuildingInfo;
                }
            }
            else if (prefab is PropInfo)
            {
                PropTool tool = ToolsModifierControl.SetTool<PropTool>();
                if (tool != null)
                {
                    tool.m_prefab = prefab as PropInfo;
                }
            }
            else if (prefab is TreeInfo)
            {
                TreeTool tool = ToolsModifierControl.SetTool<TreeTool>();
                if (tool != null)
                {
                    tool.m_prefab = prefab as TreeInfo;
                }
            }
        }

        public void OnGUI()
        {
            try
            {
                if (!UIView.HasModalInput() &&
                    (!UIView.HasInputFocus() || (UIView.activeComponent != null && UIView.activeComponent.parent is UISearchBox)))
                {
                    Event e = Event.current;

                    // Checking key presses
                    if (OptionsKeymapping.search.IsPressed(e))
                    {
                        if (!searchBox.isVisible)
                        {
                            mainButton.SimulateClick();
                        }
                        searchBox.searchButton.SimulateClick();
                    }
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
        }

        public static void FixBadProps()
        {
            PropInstance[] buffer = PropManager.instance.m_props.m_buffer;
            uint size = PropManager.instance.m_props.m_size;

            string log = "";

            for (uint i = 0; i < size; i++)
            {
                try
                {
                    if (buffer[i].m_flags != 0)
                    {
                        PropInfo info = buffer[i].Info;

                        if(info == null) continue;

                        if (info.m_requireWaterMap && info.m_lodWaterHeightMap == null)
                        {
                            PropManager.instance.ReleaseProp((ushort)i);
                            log += "Removed " + info.name + "\n";
                        }
                    }
                }
                catch
                { }
            }
            
            if(log != "") DebugUtils.Log(log);
        }

        public static void LoadResources()
        {
            if (atlas == null)
            {
                string[] spriteNames = new string[]
			    {
				    "FindIt",
				    "FindItDisabled",
				    "FindItFocused",
				    "FindItHovered",
				    "FindItPressed",
				    "IconPolicyLeisureDisabled",
				    "IconPolicyTouristDisabled"
			    };

                atlas = ResourceLoader.CreateTextureAtlas("FindItAtlas", spriteNames, "FindIt.Icons.");

                UITextureAtlas defaultAtlas = ResourceLoader.GetAtlas("Ingame");
                Texture2D[] textures = new Texture2D[]
                {
                    defaultAtlas["ToolbarIconGroup6Focused"].texture,
                    defaultAtlas["ToolbarIconGroup6Hovered"].texture,
                    defaultAtlas["ToolbarIconGroup6Normal"].texture,
                    defaultAtlas["ToolbarIconGroup6Pressed"].texture,
                    defaultAtlas["IconPolicyLeisure"].texture,
                    defaultAtlas["IconPolicyTourist"].texture
                };

                ResourceLoader.AddTexturesInAtlas(atlas, textures);
            }
        }
    }

    public class FindItLoader : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            if ((ToolManager.instance.m_properties.m_mode & ItemClass.Availability.GameAndMap) != ItemClass.Availability.None)
            {
                Redirector<Detours.GeneratedScrollPanelDetour>.Deploy();
            }
        }

        public override void OnReleased()
        {
            Redirector<Detours.GeneratedScrollPanelDetour>.Revert();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if(FindIt.fixBadProps)
            {
                DebugUtils.Log("Fixing bad props");
                FindIt.FixBadProps();
                FindIt.fixBadProps = false;
                DebugUtils.Log("Bad props fixed");
            }

            if ((ToolManager.instance.m_properties.m_mode & ItemClass.Availability.GameAndMap) != ItemClass.Availability.None)
            //if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario)
            //if (mode != LoadMode.NewAsset && mode != LoadMode.LoadAsset)
            {
                FindIt.inEditor = false;

                if (FindIt.instance == null)
                {
                    // Creating the instance
                    FindIt.instance = new GameObject("FindIt").AddComponent<FindIt>();
                }
                else
                {
                    FindIt.instance.Start();
                }
            }
            else if (mode == LoadMode.NewAsset || mode == LoadMode.LoadAsset)
            {
                FindIt.inEditor = true;

                ToolsModifierControl.toolController.eventEditPrefabChanged += (p) =>
                {
                    DebugUtils.Log("eventEditPrefabChanged");
                    SimulationManager.instance.AddAction(() =>
                    {
                        if (FindIt.instance == null)
                        {
                            // Creating the instance
                            FindIt.instance = new GameObject("FindIt").AddComponent<FindIt>();
                        }
                        else
                        {
                            FindIt.instance.Start();
                        }
                    });
                };
            }
        }
    }
}