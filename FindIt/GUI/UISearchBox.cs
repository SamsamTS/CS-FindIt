using UnityEngine;

using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.UI;

using System.Collections.Generic;


namespace FindIt.GUI
{
    public class UISearchBox : UIPanel
    {
        public UITextField input;
        public UIScrollPanel scrollPanel;

        public override void Start()
        {
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "GenericTab";
            size = new Vector2(300, 40);

            input = SamsamTS.UIUtils.CreateTextField(this);
            input.size = new Vector2(width - 45, 30);
            input.padding.top = 7;
            input.relativePosition = new Vector3(5, 5);

            UISprite sprite = AddUIComponent<UISprite>();
            sprite.atlas = FindIt.instance.m_mainButton.atlas;
            sprite.spriteName = "FindIt";
            sprite.relativePosition = new Vector3(width - 41, -3);

            sprite.eventClick += (c, p) =>
            {
                input.Focus();
                input.SelectAll();
            };

            input.eventTextChanged += OnTextChanged;
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (input != null && !isVisible)
            {
                input.Unfocus();
            }

        }

        public void OnTextChanged(UIComponent c, string p)
        {
            List<Asset> matches = AssetTagList.instance.Find(p);
            scrollPanel.Clear();
            foreach (Asset asset in matches)
            {
                if (asset.prefab != null)
                {
                    UIScrollPanelItem.ItemData data = new UIScrollPanelItem.ItemData();
                    data.name = Asset.GetLocalizedTitle(asset.prefab);
                    data.tooltip = Asset.GetLocalizedTooltip(asset.prefab);

                    data.atlas = asset.prefab.m_Atlas;
                    if (data.atlas == null)
                    {
                        data.atlas = scrollPanel.atlas;
                    }

                    data.baseIconName = GetThumbNail(asset.prefab, data.atlas);

                    data.tooltipBox = GeneratedPanel.GetTooltipBox(TooltipHelper.GetHashCode(data.tooltip));
                    data.enabled = ToolsModifierControl.IsUnlocked(asset.prefab.GetUnlockMilestone());
                    data.verticalAlignment = scrollPanel.buttonsAlignment;
                    data.objectUserData = asset.prefab;

                    scrollPanel.itemsData.Add(data);
                }
            }
            scrollPanel.DisplayAt(0);
        }

        public static string GetThumbNail(PrefabInfo prefab, UITextureAtlas atlas)
        {
            string thumbnail = prefab.m_Thumbnail;

            if (thumbnail.IsNullOrWhiteSpace() || atlas[thumbnail] == null)
            {
                thumbnail = "ThumbnailBuildingDefault";
            }

            return thumbnail;
        }
    }
}
