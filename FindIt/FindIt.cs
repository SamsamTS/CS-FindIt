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


namespace FindIt
{
    public class FindIt : MonoBehaviour
    {
        public const string settingsFileName = "FindIt";

        public static FindIt instance;

        public static AssetTagList list;

        public void Start()
        {
            try
            {
                list = AssetTagList.instance;
                list.Init();

                DebugUtils.Log("Initialized");
            }
            catch(Exception e)
            {
                DebugUtils.Log("Start failed");
                DebugUtils.LogException(e);
                enabled = false;
            }
        }

        public void OnGUI()
        {
            try
            {
                if (!UIView.HasModalInput() && !UIView.HasInputFocus())
                {
                    Event e = Event.current;

                    // Checking key presses
                    if (OptionsKeymapping.search.IsPressed(e))
                    {

                    }
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnGUI failed");
                DebugUtils.LogException(e);
            }
        }
    }

    public class FineRoadAnarchyLoader : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
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