// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;
using ColossalFramework.UI;

namespace FindIt.GUI
{
    public class UIWarningWindow : UIPanel
    {
        public static UIWarningWindow instance;
        private UIComponent m_button;

        private const float spacing = 5f;

        private UIButton continueButton;

        private string messageStr = "";

        public override void Start()
        {
            name = "FindIt_TagsWindow";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericPanelWhite";
            size = new Vector2(500, 200);

            UILabel title = AddUIComponent<UILabel>();
            title.text = "Find It 2 Warning\n";
            title.textColor = new Color32(255, 0, 0, 255);
            title.relativePosition = new Vector3(spacing, spacing);

            UILabel message = AddUIComponent<UILabel>();
            message.text = messageStr;
            message.textColor = new Color32(0, 0, 0, 255);
            message.relativePosition = new Vector3(spacing, spacing + title.height + spacing);

            continueButton = SamsamTS.UIUtils.CreateButton(this);
            continueButton.size = new Vector2(120, 40);
            continueButton.text = "Continue";
            continueButton.relativePosition = new Vector3(spacing, message.relativePosition.y + message.height + spacing * 2);
            continueButton.eventClick += (c, p) =>
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

        public static void ShowAt(UIComponent component, string messageStr)
        {
            if (instance == null)
            {
                instance = UIView.GetAView().AddUIComponent(typeof(UIWarningWindow)) as UIWarningWindow;
                instance.m_button = component;
                instance.Show(true);
                UIView.PushModal(instance);
            }
            else
            {
                instance.m_button = component;
                instance.Show(true);
            }
            instance.messageStr = messageStr;
        }

    }
}
