// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;
using ColossalFramework.UI;

namespace FindIt.GUI
{
    public class UIUpdatePopUp : UIPanel
    {
        public static UIUpdatePopUp instance;
        private UIComponent m_button;

        private const float spacing = 5f;

        private UIButton continueButton;
        private UIButton noShowButton;

        private string titleStr = "";
        private string messageStr = "";

        public override void Start()
        {
            name = "FindIt_TagsWindow";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericPanelWhite";
            

            UILabel title = AddUIComponent<UILabel>();
            title.text = titleStr;
            title.textColor = new Color32(255, 0, 0, 255);
            title.relativePosition = new Vector3(spacing*3, spacing * 2);

            UILabel message = AddUIComponent<UILabel>();
            message.text = messageStr;
            message.textColor = new Color32(0, 0, 0, 255);
            message.relativePosition = new Vector3(spacing*3, spacing * 2 + title.height + spacing);

            continueButton = SamsamTS.UIUtils.CreateButton(this);
            continueButton.size = new Vector2(100, 40);
            continueButton.text = "Continue";
            continueButton.relativePosition = new Vector3(spacing*3, message.relativePosition.y + message.height + spacing * 2);
            continueButton.eventClick += (c, p) =>
            {
                Close();
            };

            noShowButton = SamsamTS.UIUtils.CreateButton(this);
            noShowButton.size = new Vector2(260, 40);
            noShowButton.text = "Don't show this message again";
            noShowButton.relativePosition = new Vector3(continueButton.relativePosition.x + continueButton.width + spacing * 2, continueButton.relativePosition.y);
            noShowButton.eventClick += (c, p) =>
            {
                Settings.lastUpdateNotificationVersion = ModInfo.versionFloat;
                XMLUtils.SaveSettings();
                Close();
            };

            size = new Vector2(700, title.height + message.height + continueButton.height + spacing * 8);
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

        public static void ShowAt(UIComponent component, string titleStr, string messageStr)
        {
            if (instance == null)
            {
                instance = UIView.GetAView().AddUIComponent(typeof(UIUpdatePopUp)) as UIUpdatePopUp;
                instance.m_button = component;
                instance.Show(true);
                UIView.PushModal(instance);
            }
            else
            {
                instance.m_button = component;
                instance.Show(true);
            }
            instance.titleStr = titleStr;
            instance.messageStr = messageStr;
        }

    }
}
