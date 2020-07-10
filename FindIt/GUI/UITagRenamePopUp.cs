// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FindIt.GUI
{
    public class UITagsRenamePopUp : UIPanel
    {
        public static UITagsRenamePopUp instance;
        private UIComponent m_button;

        private const float spacing = 5f;
        private UIButton confirmButton;
        private UIButton cancelButton;
        private UITextField newTagInput;

        private UIDropDown tagDropDownMenu;
        private List<KeyValuePair<string, int>> customTagList;
        private string[] customTagListStrArray;

        private UIButton tagDropDownAddButton;
        private UILabel newTagNameLabel;
        private string oldTagName;
        private string newTagName;

        public override void Start()
        {
            name = "FindIt_TagsWindow";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericPanelWhite";
            size = new Vector2(380, 270);

            UILabel title = AddUIComponent<UILabel>();
            title.text = "Rename/Combine";
            title.textColor = new Color32(0, 0, 0, 255);
            title.relativePosition = new Vector3(spacing, spacing);

            UILabel message = AddUIComponent<UILabel>();
            message.text = "\nEnter the new tag name,\nor choose an existing tag to combine with.\nThis cannot be undone.";
            message.textColor = new Color32(0, 0, 0, 255);
            message.relativePosition = new Vector3(spacing, spacing + title.height + spacing);

            newTagInput = SamsamTS.UIUtils.CreateTextField(this);
            newTagInput.size = new Vector2(width - 2 * spacing, 30);
            newTagInput.padding.top = 7;
            newTagInput.tooltip = "Press enter to submit new tag name";
            newTagInput.relativePosition = new Vector3(spacing, message.relativePosition.y + message.height + spacing);
            newTagInput.submitOnFocusLost = false;
            newTagInput.eventTextSubmitted += (c, t) =>
            {
                // use the first string as the the tag name
                string[] tagsArr = Regex.Split(t, @"([^\w]|[_-]|\s)+", RegexOptions.IgnoreCase);
                if (tagsArr[0] != "")
                {
                    newTagName = tagsArr[0];
                }
                newTagNameLabel.text = "New Tag Name: " + newTagName;
            };

            // tag dropdown
            tagDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            tagDropDownMenu.normalBgSprite = "TextFieldPanelHovered";
            tagDropDownMenu.size = new Vector2(width - 2 * spacing - 50, 30);
            tagDropDownMenu.tooltip = "Use mouse wheel to scroll up/down";
            tagDropDownMenu.listHeight = 210;
            tagDropDownMenu.itemHeight = 30;
            tagDropDownMenu.relativePosition = new Vector3(spacing, newTagInput.relativePosition.y + newTagInput.height + spacing * 2);
            UpdateCustomTagList();

            // tag dropdown combine button
            tagDropDownAddButton = SamsamTS.UIUtils.CreateButton(this);
            tagDropDownAddButton.size = new Vector2(40, 30);
            tagDropDownAddButton.text = "Use";
            tagDropDownAddButton.tooltip = "Combine to this existing tag";
            tagDropDownAddButton.relativePosition = new Vector3(spacing + tagDropDownMenu.width + 5, tagDropDownMenu.relativePosition.y);
            tagDropDownAddButton.eventClick += (c, p) =>
            {
                newTagName = GetDropDownListKey();
                newTagNameLabel.text = "New Tag Name: " + newTagName;
            };

            // display new tag name for confirmation
            newTagNameLabel = AddUIComponent<UILabel>();
            newTagNameLabel.size = new Vector2(200, 50);
            newTagNameLabel.textColor = new Color32(0, 0, 0, 255);
            newTagNameLabel.text = "New Tag Name: " + oldTagName;
            newTagNameLabel.relativePosition = new Vector3(spacing, tagDropDownMenu.relativePosition.y + tagDropDownMenu.height + spacing * 2);

            confirmButton = SamsamTS.UIUtils.CreateButton(this);
            confirmButton.size = new Vector2(75, 40);
            confirmButton.text = "Confirm";
            confirmButton.relativePosition = new Vector3(spacing, newTagNameLabel.relativePosition.y + newTagNameLabel.height + spacing * 3);
            confirmButton.eventClick += (c, p) =>
            {
                RenameTag(oldTagName, newTagName);
                ((UIFilterTag)m_button.parent).UpdateCustomTagList();
                Close();
            };

            cancelButton = SamsamTS.UIUtils.CreateButton(this);
            cancelButton.size = new Vector2(75, 40);
            cancelButton.text = "Cancel";
            cancelButton.relativePosition = new Vector3(confirmButton.relativePosition.x + confirmButton.width + spacing * 2, confirmButton.relativePosition.y);
            cancelButton.eventClick += (c, p) =>
            {
                Close();
            };

        }

        private static void Close()
        {
            if (instance != null)
            {
                UIView.PopModal();

                instance.isVisible = false;
                Destroy(instance.gameObject);
                instance = null;
            }
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                p.Use();
                Close();
            }

            base.OnKeyDown(p);
        }

        public static void ShowAt(UIComponent component, string currentTag)
        {
            if (instance == null)
            {
                instance = UIView.GetAView().AddUIComponent(typeof(UITagsRenamePopUp)) as UITagsRenamePopUp;
                instance.m_button = component;
                instance.Show(true);
                UIView.PushModal(instance);
            }
            else
            {
                instance.m_button = component;
                instance.Show(true);
            }

            instance.oldTagName = currentTag;
        }

        private void UpdateCustomTagList()
        {
            customTagList = AssetTagList.instance.GetCustomTagList();

            List<string> list = new List<string>();

            foreach (KeyValuePair<string, int> entry in customTagList)
            {
                list.Add(entry.Key.ToString() + " (" + entry.Value.ToString() + ")");
            }

            customTagListStrArray = list.ToArray();
            tagDropDownMenu.items = customTagListStrArray;
            tagDropDownMenu.selectedIndex = 0;
        }
        private string GetDropDownListKey()
        {
            return customTagList[tagDropDownMenu.selectedIndex].Key;
        }

        // rename or a tag or combine it with another tag
        private void RenameTag(string oldTagName, string newTagName)
        {
            foreach (Asset asset in AssetTagList.instance.assets.Values)
            {
                if (!asset.tagsCustom.Contains(oldTagName)) continue;

                // remove old tag
                AssetTagList.instance.RemoveCustomTag(asset, oldTagName);

                // add new tag
                if (asset.tagsCustom.Contains(newTagName)) continue;
                AssetTagList.instance.AddCustomTags(asset, newTagName);
            }
        }
    }
}
