// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework;
using System;

namespace FindIt.GUI
{
    public class UITagsExportUnusedPopUp : UIPanel
    {
        public static UITagsExportUnusedPopUp instance;
        private UIComponent m_button;

        private const float spacing = 5f;

        private UIButton closeButton;
        private UIButton openFileButton;

        public override void Start()
        {
            name = "FindIt_TagsExportUnusedPopUp";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericPanelWhite";
            size = new Vector2(400, 145);

            UILabel title = AddUIComponent<UILabel>();
            title.text = Translations.Translate("FIF_EF_UNEXP");
            title.textColor = new Color32(0, 0, 0, 255);
            title.relativePosition = new Vector3(spacing * 2, spacing * 2);

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

            UILabel message = AddUIComponent<UILabel>();
            message.text = "\n" + GetCityName() + "\n" + GetFormattedDateTime();
            message.textColor = new Color32(0, 0, 0, 255);
            message.relativePosition = new Vector3(spacing * 2, spacing + title.height + spacing);

            closeButton = SamsamTS.UIUtils.CreateButton(this);
            closeButton.size = new Vector2(100, 40);
            closeButton.text = Translations.Translate("FIF_UNEXP_CLO");
            closeButton.relativePosition = new Vector3(spacing * 2, message.relativePosition.y + message.height + spacing * 2);
            closeButton.eventClick += (c, p) =>
            {
                Close();
            };

            openFileButton = SamsamTS.UIUtils.CreateButton(this);
            openFileButton.size = new Vector2(200, 40);
            openFileButton.text = Translations.Translate("FIF_SET_CTFOP");
            openFileButton.relativePosition = new Vector3(closeButton.relativePosition.x + closeButton.width + spacing * 4, closeButton.relativePosition.y);
            openFileButton.eventClick += (c, p) =>
            {

            };

            height = openFileButton.relativePosition.y + openFileButton.height + 10;
            openFileButton.Focus();
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
                instance = UIView.GetAView().AddUIComponent(typeof(UITagsExportUnusedPopUp)) as UITagsExportUnusedPopUp;
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

        private static string GetCityName()
        {
            if (Singleton<SimulationManager>.exists)
            {
                string cityName = Singleton<SimulationManager>.instance.m_metaData.m_CityName;
                if (cityName != null) return cityName;
            }
            return "";
        }

        private static string GetFormattedDateTime()
        {
            return DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
        }
    }
}
