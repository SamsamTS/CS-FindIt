// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;
using ColossalFramework.UI;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace FindIt.GUI
{
    public class UIQuickMenuPopUp : UIPanel
    {
        public static UIQuickMenuPopUp instance;
        private UIComponent m_button;

        private const float spacing = 5f;

        public override void Start()
        {
            name = "FindIt_UIQuickMenuPopUp";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericPanelWhite";
            size = new Vector2(480, 200);

            UILabel title = AddUIComponent<UILabel>();
            title.text = Translations.Translate("FIF_QM_TIT");
            title.textColor = new Color32(0, 0, 0, 255);
            title.relativePosition = new Vector3(spacing*3, spacing*3);

            UIButton close = AddUIComponent<UIButton>();
            close.size = new Vector2(30f, 30f);
            close.text = "X";
            close.textScale = 0.9f;
            close.textColor = new Color32(0, 0, 0, 255);
            close.focusedTextColor = new Color32(0, 0, 0, 255);
            close.hoveredTextColor = new Color32(109, 109, 109, 255);
            close.pressedTextColor = new Color32(128, 128, 128, 102);
            close.textPadding = new RectOffset(8, 8, 8, 8);
            close.canFocus = false;
            close.playAudioEvents = true;
            close.relativePosition = new Vector3(width - close.width, 0);
            close.eventClicked += (c, p) => Close();

            // Sort custom tag list alphabetically. Default = sort by number of assets in each tag
            UICheckBox customTagListSort = SamsamTS.UIUtils.CreateCheckBox(this);
            customTagListSort.isChecked = Settings.customTagListSort;
            customTagListSort.label.text = Translations.Translate("FIF_SET_CTLS");
            customTagListSort.label.textScale = 0.8f;
            customTagListSort.width = size.x;
            customTagListSort.label.textColor = new Color32(0, 0, 0, 255);
            customTagListSort.relativePosition = new Vector3(title.relativePosition.x, title.relativePosition.y + title.height + 20);
            customTagListSort.eventCheckChanged += (c, i) =>
            {
                Settings.customTagListSort = customTagListSort.isChecked;
                XMLUtils.SaveSettings();
                UIFilterTag.instance.UpdateCustomTagList();
                UISearchBox.instance.Search();
            };

            // Sort asset creator list alphabetically. Default = sort by number of assets in each tag
            UICheckBox assetCreatorListSort = SamsamTS.UIUtils.CreateCheckBox(this);
            assetCreatorListSort.isChecked = Settings.assetCreatorListSort;
            assetCreatorListSort.label.text = Translations.Translate("FIF_SET_ACLS");
            assetCreatorListSort.label.textScale = 0.8f;
            assetCreatorListSort.width = size.x;
            assetCreatorListSort.label.textColor = new Color32(0, 0, 0, 255);
            assetCreatorListSort.relativePosition = new Vector3(title.relativePosition.x, customTagListSort.relativePosition.y + customTagListSort.height + 10);
            assetCreatorListSort.eventCheckChanged += (c, i) =>
            {
                Settings.assetCreatorListSort = assetCreatorListSort.isChecked;
                XMLUtils.SaveSettings();
                UIFilterExtra.instance.UpdateAssetCreatorList();
                UISearchBox.instance.Search();
            };


            // Show prop markers in 'game' mode
            UICheckBox showPropMarker = SamsamTS.UIUtils.CreateCheckBox(this);
            showPropMarker.isChecked = Settings.showPropMarker;
            showPropMarker.label.text = Translations.Translate("FIF_SET_PM");
            showPropMarker.label.textScale = 0.8f;
            showPropMarker.width = size.x;
            showPropMarker.label.textColor = new Color32(0, 0, 0, 255);
            showPropMarker.tooltip = Translations.Translate("FIF_SET_PMTP");
            showPropMarker.relativePosition = new Vector3(title.relativePosition.x, assetCreatorListSort.relativePosition.y + assetCreatorListSort.height + 10);
            showPropMarker.eventCheckChanged += (c, i) =>
            {
                Settings.showPropMarker = showPropMarker.isChecked;
                XMLUtils.SaveSettings();
                UIFilterProp.instance.UpdateMarkerToggleVisibility();
            };

            // Show the number of existing instances of each asset
            UICheckBox showInstancesCounter = SamsamTS.UIUtils.CreateCheckBox(this);
            showInstancesCounter.isChecked = Settings.showInstancesCounter;
            showInstancesCounter.label.text = Translations.Translate("FIF_SET_IC");
            showInstancesCounter.label.textScale = 0.8f;
            showInstancesCounter.width = size.x;
            showInstancesCounter.tooltip = Translations.Translate("FIF_SET_ICTP");
            showInstancesCounter.label.textColor = new Color32(0, 0, 0, 255);
            showInstancesCounter.relativePosition = new Vector3(title.relativePosition.x, showPropMarker.relativePosition.y + showPropMarker.height + 10);
            showInstancesCounter.eventCheckChanged += (c, i) =>
            {
                Settings.showInstancesCounter = showInstancesCounter.isChecked;
                XMLUtils.SaveSettings();
                if (Settings.showInstancesCounter && AssetTagList.instance?.prefabInstanceCountDictionary != null)
                {
                    AssetTagList.instance.UpdatePrefabInstanceCount();
                }
                FindIt.instance.scrollPanel.Refresh();
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

        public static void ShowAt(UIComponent component)
        {
            if (instance == null)
            {
                instance = UIView.GetAView().AddUIComponent(typeof(UIQuickMenuPopUp)) as UIQuickMenuPopUp;
                instance.m_button = component;
                instance.Show(true);
                UIView.PushModal(instance);
            }
            else
            {
                instance.m_button = component;
                instance.Show(true);
            }

        }

    }
}
