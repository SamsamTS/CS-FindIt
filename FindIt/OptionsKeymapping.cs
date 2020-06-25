// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.UI;

using System.Reflection;
using UnityEngine;

namespace FindIt
{
    public class OptionsKeymapping : UICustomControl
    {
        private static readonly string kKeyBindingTemplate = "KeyBindingTemplate";

        private InputKey m_EditingBinding;

        private string m_EditingBindingCategory;

        private int count = 0;

        private void Awake()
        {
            AddKeymapping("Search", SavedInputKey.Encode((KeyCode)Settings.searchKey.keyCode, Settings.searchKey.control, Settings.searchKey.shift, Settings.searchKey.alt));
        }

        private void AddKeymapping(string label, InputKey inputKey)
        {
            UIPanel uIPanel = component.AttachUIComponent(UITemplateManager.GetAsGameObject(kKeyBindingTemplate)) as UIPanel;
            if (count++ % 2 == 1) uIPanel.backgroundSprite = null;

            UILabel uILabel = uIPanel.Find<UILabel>("Name");
            UIButton uIButton = uIPanel.Find<UIButton>("Binding");
            uIButton.eventKeyDown += new KeyPressHandler(this.OnBindingKeyDown);
            uIButton.eventMouseDown += new MouseEventHandler(this.OnBindingMouseDown);

            uILabel.text = label;
            uIButton.text = SavedInputKey.ToLocalizedString("KEYNAME", inputKey);
            uIButton.objectUserData = inputKey;
        }

        private void OnEnable()
        {
            LocaleManager.eventLocaleChanged += new LocaleManager.LocaleChangedHandler(this.OnLocaleChanged);
        }

        private void OnDisable()
        {
            LocaleManager.eventLocaleChanged -= new LocaleManager.LocaleChangedHandler(this.OnLocaleChanged);
        }

        private void OnLocaleChanged()
        {
            this.RefreshBindableInputs();
        }

        private bool IsModifierKey(KeyCode code)
        {
            return code == KeyCode.LeftControl || code == KeyCode.RightControl || code == KeyCode.LeftShift || code == KeyCode.RightShift || code == KeyCode.LeftAlt || code == KeyCode.RightAlt;
        }

        private bool IsControlDown()
        {
            return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        }

        private bool IsShiftDown()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        }

        private bool IsAltDown()
        {
            return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        private bool IsUnbindableMouseButton(UIMouseButton code)
        {
            return code == UIMouseButton.Left || code == UIMouseButton.Right;
        }

        private KeyCode ButtonToKeycode(UIMouseButton button)
        {
            if (button == UIMouseButton.Left)
            {
                return KeyCode.Mouse0;
            }
            if (button == UIMouseButton.Right)
            {
                return KeyCode.Mouse1;
            }
            if (button == UIMouseButton.Middle)
            {
                return KeyCode.Mouse2;
            }
            if (button == UIMouseButton.Special0)
            {
                return KeyCode.Mouse3;
            }
            if (button == UIMouseButton.Special1)
            {
                return KeyCode.Mouse4;
            }
            if (button == UIMouseButton.Special2)
            {
                return KeyCode.Mouse5;
            }
            if (button == UIMouseButton.Special3)
            {
                return KeyCode.Mouse6;
            }
            return KeyCode.None;
        }

        private void OnBindingKeyDown(UIComponent comp, UIKeyEventParameter p)
        {
            if (this.m_EditingBinding != 0 && !this.IsModifierKey(p.keycode))
            {
                p.Use();
                UIView.PopModal();
                KeyCode keycode = p.keycode;
                InputKey inputKey = (p.keycode == KeyCode.Escape) ? this.m_EditingBinding : SavedInputKey.Encode(keycode, p.control, p.shift, p.alt);
                if (p.keycode == KeyCode.Backspace)
                {
                    inputKey = SavedInputKey.Empty;
                }
                this.m_EditingBinding = inputKey;
                UITextComponent uITextComponent = p.source as UITextComponent;
                uITextComponent.text = SavedInputKey.ToLocalizedString("KEYNAME", m_EditingBinding);

                // Apply and save.
                Settings.searchKey = new KeyBinding { keyCode = inputKey & 0xFFFFFFF, control = (inputKey & 0x40000000) != 0, shift = (inputKey & 0x20000000) != 0, alt = (inputKey & 0x10000000) != 0 };
                XMLUtils.SaveSettings();

                this.m_EditingBinding = 0;
                this.m_EditingBindingCategory = string.Empty;
            }
        }

        private void OnBindingMouseDown(UIComponent comp, UIMouseEventParameter p)
        {
            if (this.m_EditingBinding == 0)
            {
                p.Use();
                this.m_EditingBinding = (InputKey)p.source.objectUserData;
                this.m_EditingBindingCategory = p.source.stringUserData;
                UIButton uIButton = p.source as UIButton;
                uIButton.buttonsMask = (UIMouseButton.Left | UIMouseButton.Right | UIMouseButton.Middle | UIMouseButton.Special0 | UIMouseButton.Special1 | UIMouseButton.Special2 | UIMouseButton.Special3);
                uIButton.text = "Press any key";
                p.source.Focus();
                UIView.PushModal(p.source);
            }
            else if (!this.IsUnbindableMouseButton(p.buttons))
            {
                p.Use();
                UIView.PopModal();
                InputKey inputKey = SavedInputKey.Encode(this.ButtonToKeycode(p.buttons), this.IsControlDown(), this.IsShiftDown(), this.IsAltDown());

                this.m_EditingBinding = inputKey;
                UIButton uIButton2 = p.source as UIButton;
                uIButton2.text = SavedInputKey.ToLocalizedString("KEYNAME", m_EditingBinding);
                uIButton2.buttonsMask = UIMouseButton.Left;
                this.m_EditingBinding = 0;
                this.m_EditingBindingCategory = string.Empty;
            }
        }

        // Called from OnLocaleChanged.
        private void RefreshBindableInputs()
        {
            foreach (UIComponent current in component.GetComponentsInChildren<UIComponent>())
            {
                UITextComponent uITextComponent = current.Find<UITextComponent>("Binding");
                if (uITextComponent != null)
                {
                    InputKey inputKey = (InputKey)uITextComponent.objectUserData;
                    if (inputKey != 0)
                    {
                        uITextComponent.text = SavedInputKey.ToLocalizedString("KEYNAME", inputKey);
                    }
                }
                UILabel uILabel = current.Find<UILabel>("Name");
                if (uILabel != null)
                {
                    uILabel.text = Locale.Get("KEYMAPPING", uILabel.stringUserData);
                }
            }
        }

        internal InputKey GetDefaultEntry(string entryName)
        {
            FieldInfo field = typeof(DefaultSettings).GetField(entryName, BindingFlags.Static | BindingFlags.Public);
            if (field == null)
            {
                return 0;
            }
            object value = field.GetValue(null);
            if (value is InputKey)
            {
                return (InputKey)value;
            }
            return 0;
        }

        private void RefreshKeyMapping()
        {
            foreach (UIComponent current in component.GetComponentsInChildren<UIComponent>())
            {
                UITextComponent uITextComponent = current.Find<UITextComponent>("Binding");
                InputKey inputKey = (InputKey)uITextComponent.objectUserData;
                if (this.m_EditingBinding != inputKey)
                {
                    uITextComponent.text = SavedInputKey.ToLocalizedString("KEYNAME", inputKey);
                }
            }
        }
    }
}