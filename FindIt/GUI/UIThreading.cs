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
            UIButton mainButton = FindIt.instance?.mainButton;
            UISearchBox searchBox = FindIt.instance?.searchBox;
            if (searchBox == null || mainButton == null) return;

            KeyCode searchKey = (KeyCode)(Settings.searchKey.keyCode);
            KeyCode allKey = (KeyCode)(Settings.allKey.keyCode);
            KeyCode networkKey = (KeyCode)(Settings.networkKey.keyCode);
            KeyCode ploppableKey = (KeyCode)(Settings.ploppableKey.keyCode);
            KeyCode growableKey = (KeyCode)(Settings.growableKey.keyCode);
            KeyCode ricoKey = (KeyCode)(Settings.ricoKey.keyCode);
            KeyCode grwbRicoKey = (KeyCode)(Settings.grwbRicoKey.keyCode);
            KeyCode propKey = (KeyCode)(Settings.propKey.keyCode);
            KeyCode decalKey = (KeyCode)(Settings.decalKey.keyCode);
            KeyCode treeKey = (KeyCode)(Settings.treeKey.keyCode);
            KeyCode randomSelectionKey = (KeyCode)(Settings.randomSelectionKey.keyCode);

            // Null checks for safety.
            // Check modifier keys according to settings.
            bool altPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt) || Input.GetKey(KeyCode.AltGr);
            bool ctrlPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool shiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // Has hotkey been pressed?
            if (searchKey != KeyCode.None && Input.GetKey(searchKey) && CheckHotkey(Settings.searchKey, altPressed, ctrlPressed, shiftPressed))
                OpenFindIt(-1);
            else if (allKey != KeyCode.None && Input.GetKey(allKey) && CheckHotkey(Settings.allKey, altPressed, ctrlPressed, shiftPressed))
                OpenFindIt(0);
            else if (networkKey != KeyCode.None && Input.GetKey(networkKey) && CheckHotkey(Settings.networkKey, altPressed, ctrlPressed, shiftPressed))
                OpenFindIt(1);
            else if (ploppableKey != KeyCode.None && Input.GetKey(ploppableKey) && CheckHotkey(Settings.ploppableKey, altPressed, ctrlPressed, shiftPressed))
                OpenFindIt(2);
            else if (growableKey != KeyCode.None && Input.GetKey(growableKey) && CheckHotkey(Settings.growableKey, altPressed, ctrlPressed, shiftPressed))
                OpenFindIt(3);
            else if (ricoKey != KeyCode.None && Input.GetKey(ricoKey) && CheckHotkey(Settings.ricoKey, altPressed, ctrlPressed, shiftPressed))
                OpenFindIt(4);
            else if (grwbRicoKey != KeyCode.None && Input.GetKey(grwbRicoKey) && CheckHotkey(Settings.grwbRicoKey, altPressed, ctrlPressed, shiftPressed))
                OpenFindIt(5);
            else if (propKey != KeyCode.None && Input.GetKey(propKey) && CheckHotkey(Settings.propKey, altPressed, ctrlPressed, shiftPressed))
                OpenFindIt(6);
            else if (decalKey != KeyCode.None && Input.GetKey(decalKey) && CheckHotkey(Settings.decalKey, altPressed, ctrlPressed, shiftPressed))
                OpenFindIt(7);
            else if (treeKey != KeyCode.None && Input.GetKey(treeKey) && CheckHotkey(Settings.treeKey, altPressed, ctrlPressed, shiftPressed))
                OpenFindIt(8);
            else if (randomSelectionKey != KeyCode.None && Input.GetKey(randomSelectionKey) && CheckHotkey(Settings.randomSelectionKey, altPressed, ctrlPressed, shiftPressed))
            {
                OpenFindIt(-2);
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
            }
        }

        public bool CheckHotkey(KeyBinding keyBinding, bool altPressed, bool ctrlPressed, bool shiftPressed)
        {
            if ((altPressed != keyBinding.alt) || (ctrlPressed != keyBinding.control) || (shiftPressed != keyBinding.shift)) return false;
            return true;
        }

        public void OpenFindIt(int index)
        {
            // Cancel if key input is already queued for processing.
            if (_processed) return;

            _processed = true;
            try
            {
                if (index > -1)
                {
                    if (index > 5 && !FindIt.isRicoEnabled)
                    {
                        index -= 2;
                    }
                    FindIt.instance.searchBox.typeFilter.selectedIndex = index;

					//restore stored search query individually for each asset type
					//this is only done when opening FindIt via one of the specific hotkeys
					if (FindIt.instance.searchBox.storedQueries.TryGetValue((UISearchBox.DropDownOptions)index, out string storedQuery))
					{
						Debugging.Message($"restore stored query for category {index} (cast: '{(UISearchBox.DropDownOptions)index}': \"{storedQuery}\"");
						FindIt.instance.searchBox.input.text = storedQuery;
					}
                }

                // If the searchbox isn't visible, simulate a click on the main button.
                if (!FindIt.instance.searchBox.isVisible)
                {
                    FindIt.instance.mainButton.SimulateClick();
                }

                if (index == -2)
                {
                    UISearchBox.instance.PickRandom();
                }
                else
                {
                    // Simulate a search
                    //FindIt.instance.searchBox.Search();
                    FindIt.instance.searchBox.input.Focus();
                    FindIt.instance.searchBox.input.SelectAll();
                }
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
            }
        }
       
    }
}