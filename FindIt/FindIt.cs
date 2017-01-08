using ICities;
using UnityEngine;

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.UI;

using FindIt.Redirection;
using FindIt.GUI;

namespace FindIt
{
    public class FindIt : MonoBehaviour
    {
        public const string settingsFileName = "FindIt";

        public static FindIt instance;

        public static AssetTagList list;

        public UIButton m_mainButton;
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

                UITabstrip tabstrip = ToolsModifierControl.mainToolbar.GetComponentInChildren<UITabstrip>();

                GameObject asGameObject = UITemplateManager.GetAsGameObject("MainToolbarButtonTemplate");
                GameObject asGameObject2 = UITemplateManager.GetAsGameObject("ScrollableSubPanelTemplate");

                m_mainButton = tabstrip.AddTab("FindItMainButton", asGameObject, asGameObject2, new Type[] { typeof(UIGroupPanel) }) as UIButton;
                m_mainButton.atlas = LoadResources();

                FieldInfo m_ObjectIndex = typeof(MainToolbar).GetField("m_ObjectIndex", BindingFlags.Instance | BindingFlags.NonPublic);
                m_ObjectIndex.SetValue(ToolsModifierControl.mainToolbar, (int)m_ObjectIndex.GetValue(ToolsModifierControl.mainToolbar) + 1);

                m_mainButton.gameObject.GetComponent<TutorialUITag>().tutorialTag = name;
                m_groupPanel = tabstrip.GetComponentInContainer(m_mainButton, typeof(UIGroupPanel)) as UIGroupPanel;

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

                    UISearchBox searchBox = scrollPanel.parent.AddUIComponent<UISearchBox>();
                    searchBox.scrollPanel = scrollPanel;
                    searchBox.relativePosition = new Vector3(5, -40);
                    searchBox.OnTextChanged(searchBox, "");
                }

                m_mainButton.normalBgSprite = "ToolbarIconGroup6Normal";
                m_mainButton.focusedBgSprite = "ToolbarIconGroup6Focused";
                m_mainButton.hoveredBgSprite = "ToolbarIconGroup6Hovered";
                m_mainButton.pressedBgSprite = "ToolbarIconGroup6ressed";
                m_mainButton.disabledBgSprite = "ToolbarIconGroup6Disabled";

                m_mainButton.normalFgSprite = "FindIt";
                m_mainButton.focusedFgSprite = "FindItFocused";
                m_mainButton.hoveredFgSprite = "FindItHovered";
                m_mainButton.pressedFgSprite = "FindItPressed";
                m_mainButton.disabledFgSprite = "FindItDisabled";

                m_mainButton.tooltip = "Find It! " + ModInfo.version;

                DebugUtils.Log("Initialized");
            }
            catch (Exception e)
            {
                DebugUtils.Log("Start failed");
                DebugUtils.LogException(e);
                enabled = false;
            }
        }

        public void OnButtonClicked(UIComponent c, UIMouseEventParameter p)
        {
            UIButton uIButton = p.source as UIButton;
            if (uIButton != null && uIButton.parent is UIScrollPanel)
            {
                SelectPrefab(uIButton.objectUserData as PrefabInfo);
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
                        m_mainButton.SimulateClick();
                    }
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
        }

        private UITextureAtlas LoadResources()
        {
            string[] spriteNames = new string[]
			{
				"FindIt",
				"FindItDisabled",
				"FindItFocused",
				"FindItHovered",
				"FindItPressed"
			};

            UITextureAtlas atlas = ResourceLoader.CreateTextureAtlas("FindIt", spriteNames, "FindIt.Icons.");

            UITextureAtlas defaultAtlas = ResourceLoader.GetAtlas("Ingame");
            Texture2D[] textures = new Texture2D[]
            {
                defaultAtlas["ToolbarIconGroup6Focused"].texture,
                defaultAtlas["ToolbarIconGroup6Hovered"].texture,
                defaultAtlas["ToolbarIconGroup6Normal"].texture,
                defaultAtlas["ToolbarIconGroup6Pressed"].texture
            };

            ResourceLoader.AddTexturesInAtlas(atlas, textures);

            return atlas;
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
            if ((ToolManager.instance.m_properties.m_mode & ItemClass.Availability.GameAndMap) != ItemClass.Availability.None)
            //if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame || mode == LoadMode.NewGameFromScenario)
            //if (mode != LoadMode.NewAsset && mode != LoadMode.LoadAsset)
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
            }
        }
    }
}