using System;
using UnityEngine;
using ICities;
using ColossalFramework.UI;
using FindIt.GUI;


namespace FindIt
{
    public class UIThreading : ThreadingExtensionBase
    {
        // Flag.
        private bool _processed = false;


        /// <summary>
        /// Look for keypress to open GUI.
        /// </summary>
        /// <param name="realTimeDelta"></param>
        /// <param name="simulationTimeDelta"></param>
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            // Local reference.
            KeyCode searchKey = (KeyCode)(Settings.searchKey.keyCode);

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
                        if (!UIView.HasModalInput() &&
                            (!UIView.HasInputFocus() || (UIView.activeComponent != null && UIView.activeComponent.parent is UISearchBox)))
                        {
                            if (!FindIt.instance.searchBox.isVisible)
                            {
                                FindIt.instance.mainButton.SimulateClick();
                            }
                            FindIt.instance.searchBox.searchButton.SimulateClick();
                        }

                        if (Input.GetKeyDown(KeyCode.Escape) && FindIt.instance.searchBox.isVisible)
                        {
                            FindIt.instance.searchBox.input.Unfocus();
                        }
                    }
                    catch (Exception e)
                    {
                        Debugging.Message("OnGUI failed");
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
        }
    }

}