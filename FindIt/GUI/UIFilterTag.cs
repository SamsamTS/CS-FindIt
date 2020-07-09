// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;

namespace FindIt.GUI
{
    public class UIFilterTag : UIPanel
    {
        public static UIFilterTag instance;

        public UICheckBox tagDropDownCheckBox;
        public UIDropDown tagDropDownMenu;
        public UIButton refreshButton;
        public UIButton renameButton;
        public UIButton deleteButton;

        public List<KeyValuePair<string, int>> customTagList;
        public string[] customTagListStrArray;

        public override void Start()
        {
            instance = this;

            // tag dropdown filter checkbox
            tagDropDownCheckBox = SamsamTS.UIUtils.CreateCheckBox(this);
            tagDropDownCheckBox.isChecked = false;
            tagDropDownCheckBox.width = 20;
            tagDropDownCheckBox.tooltip = "Only show assets with this tag";
            tagDropDownCheckBox.relativePosition = new Vector3(10, 10);
            tagDropDownCheckBox.eventCheckChanged += (c, i) =>
            {
                if(customTagListStrArray.Length == 0)
                {
                    tagDropDownCheckBox.isChecked = false;
                }
                ((UISearchBox)parent).Search();
            };

            // tag dropdown
            tagDropDownMenu = SamsamTS.UIUtils.CreateDropDown(this);
            tagDropDownMenu.size = new Vector2(200, 25);
            tagDropDownMenu.tooltip = "Use mouse wheel to scroll up/down";
            tagDropDownMenu.listHeight = 210;
            tagDropDownMenu.itemHeight = 30;
            UpdateCustomTagList();
            tagDropDownMenu.items = customTagListStrArray;
            tagDropDownMenu.relativePosition = new Vector3(tagDropDownCheckBox.relativePosition.x + tagDropDownCheckBox.width + 5, 5);
            tagDropDownMenu.eventSelectedIndexChanged += (c, p) =>
            {
                if (tagDropDownCheckBox.isChecked)
                {
                    ((UISearchBox)parent).Search();
                }
            };

            // refresh button 
            refreshButton = SamsamTS.UIUtils.CreateButton(this);
            refreshButton.size = new Vector2(80, 25);
            refreshButton.text = "Refresh";
            refreshButton.textScale = 0.8f;
            refreshButton.tooltip = "Refresh custom tag drop-down list";
            refreshButton.relativePosition = new Vector3(tagDropDownMenu.relativePosition.x + tagDropDownMenu.width + 5, 5);
            refreshButton.eventClick += (c, p) => 
            {
                UpdateCustomTagList();
                ((UISearchBox)parent).Search();
            };

            // rename button 
            renameButton = SamsamTS.UIUtils.CreateButton(this);
            renameButton.size = new Vector2(80, 25);
            renameButton.text = "Rename";
            renameButton.textScale = 0.8f;
            renameButton.tooltip = "Rename this tag";
            renameButton.relativePosition = new Vector3(refreshButton.relativePosition.x + refreshButton.width + 5, 5);
            renameButton.eventClick += (c, p) =>
            {
                if (customTagListStrArray.Length == 0) return;
                RenameTag();
                UpdateCustomTagList();
                ((UISearchBox)parent).Search();
            };

            // delete button 
            deleteButton = SamsamTS.UIUtils.CreateButton(this);
            deleteButton.size = new Vector2(80, 25);
            deleteButton.text = "Delete";
            deleteButton.textScale = 0.8f;
            deleteButton.tooltip = "Delete this tag";
            deleteButton.relativePosition = new Vector3(renameButton.relativePosition.x + renameButton.width + 5, 5);
            deleteButton.eventClick += (c, p) =>
            {
                if (customTagListStrArray.Length != 0)
                {
                    UITagsDeletePopUp.ShowAt(deleteButton);
                }
            };
        }

        // Update custom tag list 
        public void UpdateCustomTagList()
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

            if (customTagListStrArray.Length == 0)
            {
                tagDropDownCheckBox.isChecked = false;
            }
        }

        public string GetDropDownListKey()
        {
            return customTagList[tagDropDownMenu.selectedIndex].Key;
        }

        // delete a tag and remove it from all tagged assets
        public void DeleteTag()
        {
            string tag = GetDropDownListKey();

            foreach (Asset asset in AssetTagList.instance.assets.Values)
            {
                if (!asset.tagsCustom.Contains(tag)) continue;

                // remove tag
                AssetTagList.instance.RemoveCustomTag(asset, tag);
            }
        }

        // rename or a tag or combine it with another tag
        private void RenameTag()
        {
            string tag = GetDropDownListKey();
            string newTag = tag;

            foreach (Asset asset in AssetTagList.instance.assets.Values)
            {
                if (!asset.tagsCustom.Contains(tag)) continue;

                // remove tag
                AssetTagList.instance.RemoveCustomTag(asset, tag);

                // add new tag
                if (asset.tagsCustom.Contains(newTag)) continue;
                AssetTagList.instance.AddCustomTags(asset, newTag);
            }
        }
    }
}
