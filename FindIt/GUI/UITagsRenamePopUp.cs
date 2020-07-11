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

        private List<KeyValuePair<string, int>> customTagList;
        private string[] customTagListStrArray;

        private UILabel newTagNameLabel;
        private string oldTagName;
        private string newTagName;

        public override void Start()
        {
            name = "FindIt_TagsWindow";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericPanelWhite";
            size = new Vector2(380, 210);

            UILabel title = AddUIComponent<UILabel>();
            title.text = "Rename";
            title.textColor = new Color32(0, 0, 0, 255);
            title.relativePosition = new Vector3(spacing, spacing);

            UILabel message = AddUIComponent<UILabel>();
            message.text = "\nType a new tag name then press Enter.\nThis cannot be undone.";
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

                if (newTagName == "")
                {
                    newTagNameLabel.text = "Please enter a new tag name";
                    newTagNameLabel.textColor = new Color32(255, 0, 0, 255);
                    return;
                }
                newTagNameLabel.textColor = new Color32(0, 0, 0, 255);
                newTagNameLabel.text = "New Tag Name: " + newTagName;
            };

            // display new tag name for confirmation
            newTagNameLabel = AddUIComponent<UILabel>();
            newTagNameLabel.size = new Vector2(200, 50);
            newTagNameLabel.textColor = new Color32(0, 0, 0, 255);
            newTagNameLabel.text = "New Tag Name: ";
            newTagNameLabel.relativePosition = new Vector3(spacing, newTagInput.relativePosition.y + newTagInput.height + spacing * 2);

            confirmButton = SamsamTS.UIUtils.CreateButton(this);
            confirmButton.size = new Vector2(75, 40);
            confirmButton.text = "Confirm";
            confirmButton.relativePosition = new Vector3(spacing, newTagNameLabel.relativePosition.y + newTagNameLabel.height + spacing * 3);
            confirmButton.eventClick += (c, p) =>
            {
                if (newTagName == "")
                {
                    newTagNameLabel.text = "Please enter a new tag name";
                    newTagNameLabel.textColor = new Color32(255, 0, 0, 255);
                    return;
                }

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
            instance.newTagName = "";
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
