using UnityEngine;

using ColossalFramework.UI;
using System.Linq;

namespace FindIt.GUI
{
    public class UITagsWindow : UIPanel
    {
        public static UITagsWindow instance;
        
        public UITextField input;
        private UIDragHandle m_dragHandle;

        private UIComponent m_tagSprite;

        private UIPanel m_tagsPanel;

        private Asset m_asset;

        private const float spacing = 5f;

        public override void Start()
        {
            name = "FindIt_TagsWindow";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericPanelWhite";
            size = new Vector2(300, 180);
            isVisible = false;

            UILabel title = AddUIComponent<UILabel>();
            title.text = "Custom Tags";
            title.textScale = 0.9f;
            title.textColor = new Color32(0, 0, 0, 255);
            title.relativePosition = new Vector3(spacing, spacing);

            m_dragHandle = AddUIComponent<UIDragHandle>();
            m_dragHandle.target = parent;
            m_dragHandle.relativePosition = Vector3.zero;

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

            close.eventClicked += (c, p) =>
            {
                Hide();
                UIView.PopModal();
            };

            m_tagsPanel = AddUIComponent<UIPanel>();
            m_tagsPanel.size = new Vector2(width - 2 * spacing, height - 50);

            m_tagsPanel.autoFitChildrenVertically = true;
            m_tagsPanel.autoLayout = true;
            m_tagsPanel.autoLayoutDirection = LayoutDirection.Horizontal;
            m_tagsPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            m_tagsPanel.autoLayoutStart = LayoutStart.TopLeft;
            m_tagsPanel.wrapLayout = true;

            m_tagsPanel.relativePosition = new Vector3(spacing, title.relativePosition.y + title.height + spacing);

            input = SamsamTS.UIUtils.CreateTextField(this);
            input.size = new Vector2(width - 2 * spacing, 30);
            input.padding.top = 7;
            input.relativePosition = new Vector3(spacing, height - input.height - spacing);
            input.submitOnFocusLost = false;

            input.eventTextSubmitted += (c, t) =>
            {
                AssetTagList.instance.AddCustomTags(m_asset, t);
                Display(m_asset);
            };
        }

        protected override void OnKeyDown(UIKeyEventParameter p)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                p.Use();
                UIView.PopModal();
                Hide();
            }

            base.OnKeyDown(p);
        }

        public static void ShowAt(Asset asset, UIComponent component)
        {
            instance.m_tagSprite = component;
            instance.Display(asset);

            instance.Show(true);

            UIView.PushModal(instance);
        }

        public void Display(Asset asset)
        {
            if (asset == null) return;

            m_asset = asset;

            m_tagsPanel.autoLayout = false;

            UITag[] tags = m_tagsPanel.GetComponentsInChildren<UITag>();

            foreach(UITag tag in tags)
            {
                DestroyImmediate(tag.gameObject);
            }

            if (asset.tagsCustom.Count == 0)
            {
                m_tagsPanel.height = 0;
            }
            else
            {
                foreach (string t in asset.tagsCustom.OrderBy(s => s))
                {
                    UITag tag = m_tagsPanel.AddUIComponent<UITag>();
                    tag.text = t;
                    tag.asset = asset;
                }

                m_tagsPanel.autoLayout = true;
                m_tagsPanel.Reset();
            }

            Refresh(asset);
        }

        public void Refresh(Asset asset)
        {
            if (asset.tagsCustom.Count == 0)
            {
                m_tagSprite.isVisible = false;
            }
            else
            {
                m_tagSprite.opacity = 0.5f;
                m_tagSprite.isVisible = true;
            }

            height = m_tagsPanel.relativePosition.y + m_tagsPanel.height + 2 * spacing + input.height + spacing;
            m_dragHandle.size = size;
            input.relativePosition = new Vector3(spacing, height - input.height - spacing);

            input.text = "";
            input.Focus();

            absolutePosition = new Vector3(m_tagSprite.absolutePosition.x, m_tagSprite.absolutePosition.y - instance.height);
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (input == null) return;

            if(isVisible)
            {
                input.Focus();
            }
            else
            {
                input.text = "";
                input.Unfocus();
            }
        }
    }
}
