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
        private UIButton refreshButton;
        private UIButton renameButton;
        private UIButton combineButton;
        private UIButton deleteButton;

        private List<KeyValuePair<string, int>> customTagList;
        public string[] customTagListStrArray;

        public override void Start()
        {
            instance = this;

            // tag dropdown filter checkbox
            tagDropDownCheckBox = SamsamTS.UIUtils.CreateCheckBox(this);
            tagDropDownCheckBox.isChecked = false;
            tagDropDownCheckBox.width = 20;
            tagDropDownCheckBox.tooltip = Translations.Translate("FIF_TAG_DDTP");
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
            tagDropDownMenu.tooltip = Translations.Translate("FIF_POP_SCR");
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
            refreshButton.text = Translations.Translate("FIF_TAG_REF");
            refreshButton.textScale = 0.8f;
            refreshButton.tooltip = Translations.Translate("FIF_TAG_REFTP");
            refreshButton.relativePosition = new Vector3(tagDropDownMenu.relativePosition.x + tagDropDownMenu.width + 5, 5);
            refreshButton.eventClick += (c, p) => 
            {
                UpdateCustomTagList();
                ((UISearchBox)parent).Search();
            };

            // rename button 
            renameButton = SamsamTS.UIUtils.CreateButton(this);
            renameButton.size = new Vector2(80, 25);
            renameButton.text = Translations.Translate("FIF_TAG_REN");
            renameButton.textScale = 0.8f;
            renameButton.tooltip = Translations.Translate("FIF_TAG_RENTP");
            renameButton.relativePosition = new Vector3(refreshButton.relativePosition.x + refreshButton.width + 5, 5);
            renameButton.eventClick += (c, p) =>
            {
                if (customTagListStrArray.Length != 0)
                {
                    UITagsRenamePopUp.ShowAt(renameButton, GetDropDownListKey());
                }
                else
                {
                    Debugging.Message("Custom tag rename button pressed, but no custom tag exists");
                }
            };

            // combine button 
            combineButton = SamsamTS.UIUtils.CreateButton(this);
            combineButton.size = new Vector2(80, 25);
            combineButton.text = Translations.Translate("FIF_TAG_COM");
            combineButton.textScale = 0.8f;
            combineButton.tooltip = Translations.Translate("FIF_TAG_COMTP");
            combineButton.relativePosition = new Vector3(renameButton.relativePosition.x + renameButton.width + 5, 5);
            combineButton.eventClick += (c, p) =>
            {
                if (customTagListStrArray.Length != 0)
                {
                    UITagsCombinePopUp.ShowAt(combineButton, GetDropDownListKey());
                }
                else
                {
                    Debugging.Message("Custom tag combine button pressed, but no custom tag exists");
                }
            };

            // delete button 
            deleteButton = SamsamTS.UIUtils.CreateButton(this);
            deleteButton.size = new Vector2(80, 25);
            deleteButton.text = Translations.Translate("FIF_TAG_DEL");
            deleteButton.textScale = 0.8f;
            deleteButton.tooltip = Translations.Translate("FIF_TAG_DELTP");
            deleteButton.relativePosition = new Vector3(combineButton.relativePosition.x + combineButton.width + 5, 5);
            deleteButton.eventClick += (c, p) =>
            {
                if (customTagListStrArray.Length != 0)
                {
                    UITagsDeletePopUp.ShowAt(deleteButton, GetDropDownListKey());
                }
                else
                {
                    Debugging.Message("Custom tag delete button pressed, but no custom tag exists");
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
        
    }
}
