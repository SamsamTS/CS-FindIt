using ICities;
using UnityEngine;

using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using ColossalFramework;
using ColossalFramework.UI;
using ColossalFramework.Plugins;
using ColossalFramework.Globalization;

using FindIt.Redirection;
using FindIt.GUI;

namespace FindIt
{
    public class FindIt : MonoBehaviour
    {
        public const string settingsFileName = "FindIt";

        public static FindIt instance;
        //public static SavedBool detourGeneratedScrollPanels = new SavedBool("detourGeneratedScrollPanels", settingsFileName, true, true);
        public static SavedBool unlockAll = new SavedBool("unlockAll", settingsFileName, false, true);
        public static bool fixBadProps;
        public static UITextureAtlas atlas = LoadResources();
        public static bool inEditor = false;
        public static bool rico = false;
        //public static bool thumbnailFixRunning = false;

        public static AssetTagList list;

        public UIButton mainButton;
        public UISearchBox searchBox;
        public UIScrollPanel scrollPanel;

        private UIGroupPanel m_groupPanel;
        private BeautificationPanel m_beautificationPanel;

        public void Start()
        {
            try
            {
                rico = IsRicoEnabled();

                GameObject gameObject = GameObject.Find("FindItMainButton");
                if (gameObject != null)
                {
                    return;
                }

                list = AssetTagList.instance;

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

                    scrollPanel = UIScrollPanel.Create(m_groupPanel.GetComponentInChildren<UIScrollablePanel>(), UIVerticalAlignment.Middle);
                    scrollPanel.eventClicked += OnButtonClicked;
                    scrollPanel.eventVisibilityChanged += (c, p) =>
                    {
                        HideAllOptionPanels();

                        if (p && scrollPanel.selectedItem != null)
                        {
                            // Simulate item click
                            UIScrollPanelItem.ItemData item = scrollPanel.selectedItem;

                            UIScrollPanelItem panelItem = scrollPanel.GetItem(0);
                            panelItem.Display(scrollPanel.selectedItem, 0);
                            panelItem.component.SimulateClick();

                            scrollPanel.selectedItem = item;

                            scrollPanel.Refresh();
                        }
                    };

                    scrollPanel.eventTooltipEnter += (c, p) =>
                    {
                        UIScrollPanelItem.RefreshTooltipAltas(p.source);
                    };

                    searchBox = scrollPanel.parent.AddUIComponent<UISearchBox>();
                    searchBox.scrollPanel = scrollPanel;
                    searchBox.relativePosition = new Vector3(0, 0);
                    searchBox.Search();
                }
                else
                {
                    DebugUtils.Warning("GroupPanel not found");
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

                m_beautificationPanel = FindObjectOfType<BeautificationPanel>();

                DebugUtils.Log("Initialized");
            }
            catch (Exception e)
            {
                DebugUtils.Log("Start failed");
                DebugUtils.LogException(e);
                enabled = false;
            }
        }

        public static HashSet<PrefabInfo> thumbnailsToGenerate = new HashSet<PrefabInfo>();

        public void Update()
        {
            try
            {
                if (thumbnailsToGenerate.Count > 0)
                {
                    List<PrefabInfo> prefabs;
                    lock (thumbnailsToGenerate)
                    {
                        prefabs = new List<PrefabInfo>(thumbnailsToGenerate);
                    }

                    int count = 0;
                    foreach (PrefabInfo prefab in prefabs)
                    {
                        string name = Asset.GetName(prefab);
                        string baseIconName = prefab.m_Thumbnail;
                        if (!ImageUtils.CreateThumbnailAtlas(name, prefab) && !baseIconName.IsNullOrWhiteSpace())
                        {
                            prefab.m_Thumbnail = baseIconName;
                        }
                        lock (thumbnailsToGenerate)
                        {
                            thumbnailsToGenerate.Remove(prefab);
                        }
                        count++;

                        // Generate 7 thumbnails max
                        if (count > 7) break;
                    }

                    scrollPanel.Refresh();
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("Update failed");
                DebugUtils.LogException(e);
            }
        }

        public void HideAllOptionPanels()
        {
            OptionPanelBase[] panels = ToolsModifierControl.mainToolbar.m_OptionsBar.GetComponentsInChildren<OptionPanelBase>();

            foreach(OptionPanelBase panel in panels)
            {
                panel.Hide();
            }

            UIComponent brushPanel = ToolsModifierControl.mainToolbar.m_OptionsBar.Find("BrushPanel");
            if(brushPanel != null)
            {
                brushPanel.isVisible = false;
            }
            /*m_beautificationPanel = FindObjectOfType<BeautificationPanel>();
            if (m_beautificationPanel != null)
            {
               // Extra Lanscaping Tool hack
               m_beautificationPanel.component.isVisible = true;
               m_beautificationPanel.component.isVisible = false;
            }*/
        }

        public void OnButtonClicked(UIComponent c, UIMouseEventParameter p)
        {
            UIButton uIButton = p.source as UIButton;
            if (uIButton != null && uIButton.parent is UIScrollPanel)
            {
                HideAllOptionPanels();

                if (m_beautificationPanel != null)
                {
                    typeof(BeautificationPanel).GetMethod("OnButtonClicked", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(m_beautificationPanel, new object[] { uIButton });
                }
                else
                {
                    SelectPrefab(uIButton.objectUserData as PrefabInfo);
                }

            }
        }

        public static void SelectPrefab(PrefabInfo prefab)
        {
            BuildingInfo buildingInfo = prefab as BuildingInfo;
            NetInfo netInfo = prefab as NetInfo;
            TransportInfo transportInfo = prefab as TransportInfo;
            TreeInfo treeInfo = prefab as TreeInfo;
            PropInfo propInfo = prefab as PropInfo;
            if (buildingInfo != null)
            {
                BuildingTool buildingTool = ToolsModifierControl.SetTool<BuildingTool>();
                if (buildingTool != null)
                {                    
                    buildingTool.m_prefab = buildingInfo;
                    buildingTool.m_relocate = 0;
                }
            }
            if (netInfo != null)
            {
                NetTool netTool = ToolsModifierControl.SetTool<NetTool>();
                if (netTool != null)
                {
                    /*if (netInfo.GetSubService() == ItemClass.SubService.BeautificationParks)
                    {
                        //base.ShowPathsOptionPanel();
                        typeof(BeautificationPanel).GetMethod("ShowPathsOptionPanel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(beautificationPanel, null);
                    }
                    else if (netInfo.GetClassLevel() == ItemClass.Level.Level3)
                    {
                        //base.ShowFloodwallsOptionPanel();
                        typeof(BeautificationPanel).GetMethod("ShowFloodwallsOptionPanel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(beautificationPanel, null);
                    }
                    else if (netInfo.GetClassLevel() == ItemClass.Level.Level4)
                    {
                        //base.ShowQuaysOptionPanel();
                        typeof(BeautificationPanel).GetMethod("ShowQuaysOptionPanel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(beautificationPanel, null);
                    }
                    else
                    {
                        //base.ShowCanalsOptionPanel();
                        typeof(BeautificationPanel).GetMethod("ShowCanalsOptionPanel", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(beautificationPanel, null);
                    }*/
                    netTool.Prefab = netInfo;
                }
            }
            if (transportInfo != null)
            {
                TransportTool transportTool = ToolsModifierControl.SetTool<TransportTool>();
                if (transportTool != null)
                {
                    transportTool.m_prefab = transportInfo;
                    transportTool.m_building = 0;
                }
            }
            if (treeInfo != null)
            {
                TreeTool treeTool = ToolsModifierControl.SetTool<TreeTool>();
                if (treeTool != null)
                {
                    treeTool.m_prefab = treeInfo;
                    treeTool.m_mode = TreeTool.Mode.Single;
                }
            }
            if (propInfo != null)
            {
                PropTool propTool = ToolsModifierControl.SetTool<PropTool>();
                if (propTool != null)
                {
                    propTool.m_prefab = propInfo;
                    propTool.m_mode = PropTool.Mode.Single;
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

                if (Input.GetKeyDown(KeyCode.Escape) && searchBox.isVisible)
                {
                    searchBox.input.Unfocus();
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
        }

        public static bool IsRicoEnabled()
        {
            foreach(PluginManager.PluginInfo plugin in PluginManager.instance.GetPluginsInfo())
            {
                foreach(Assembly assembly in plugin.GetAssemblies())
                {
                    if(assembly.GetName().Name.ToLower() == "ploppablerico")
                    {
                        DebugUtils.Log("Rico found");
                        return plugin.isEnabled;
                    }
                }
            }

            return false;
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

        public static UITextureAtlas LoadResources()
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
                    "Tag",
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

            return atlas;
        }
    }

    public class FindItLoader : LoadingExtensionBase
    {
        public override void OnCreated(ILoading loading)
        {
            if ((ToolManager.instance.m_properties.m_mode & ItemClass.Availability.GameAndMap) != ItemClass.Availability.None)
            {
                //if (FindIt.detourGeneratedScrollPanels.value)
                //{
                    Redirector<Detours.GeneratedScrollPanelDetour>.Deploy();
                //}
            }
        }

        public override void OnReleased()
        {
            Redirector<Detours.GeneratedScrollPanelDetour>.Revert();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            AssetTagList.instance.Init();

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

                if(UITagsWindow.instance == null)
                {
                    UITagsWindow.instance = UIView.GetAView().AddUIComponent(typeof(UITagsWindow)) as UITagsWindow;
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