using System;
using UnityEngine;
using ICities;
using ColossalFramework.UI;
using FindIt.GUI;

namespace FindIt
{
    public class UIThreading : ThreadingExtensionBase
    {
        // Flags.
        private bool _processed = false;


        /// <summary>
        /// Look for keypress to open GUI.
        /// </summary>
        /// <param name="realTimeDelta"></param>
        /// <param name="simulationTimeDelta"></param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // Local references.
            KeyCode searchKey = (KeyCode)(Settings.searchKey.keyCode);
            UIButton mainButton = FindIt.instance?.mainButton;
            UISearchBox searchBox = FindIt.instance?.searchBox;

            // Null checks for safety.
            if (searchBox != null && mainButton != null)
            {

                // Has hotkey been pressed?
                if (searchKey != KeyCode.None && Input.GetKey(searchKey))
                {
                    // Check modifier keys according to settings.
                    bool altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
                    bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                    bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

                    // Modifiers have to *exactly match* settings, e.g. "alt-F" should not trigger on "ctrl-alt-F".
                    if ((altPressed == Settings.searchKey.alt) && (ctrlPressed == Settings.searchKey.control) && (shiftPressed == Settings.searchKey.shift))
                    {
                        // Cancel if key input is already queued for processing.
                        if (_processed) return;

                        _processed = true;
                        try
                        {
                            // If the searchbox isn't visible, simulate a click on the main button.
                            if (!searchBox.isVisible)
                            {
                                mainButton.SimulateClick();
                            }

                            // Simulate click on searchbox to focus and select contents.
                            searchBox.searchButton.SimulateClick();
                        }
                        catch (Exception e)
                        {
                            Debugging.LogException(e);
                        }
                    }
                    else
                    {
                        // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                        _processed = false;
                    }
                }
                else
                {
                    // Relevant keys aren't pressed anymore; this keystroke is over, so reset and continue.
                    _processed = false;
                }

                // Check for escape press.
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (searchBox.hasFocus)
                    {
                        // If the search box is focussed, unfocus.
                        searchBox.input.Unfocus();
                    }
                    /*
                    else if (searchBox.isVisible)
                    {
                        // Otherwise, if the searchbox is visible, simulate a main button click to hide.
                        mainButton.SimulateClick();
                    }
                    */
                }
            }
        }
    }
}