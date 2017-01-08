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
            clipChildren = true;

            input = SamsamTS.UIUtils.CreateTextField(this);
            input.size = new Vector2(width - 45, 30);
            input.padding.top = 7;
            input.relativePosition = new Vector3(5, 5);

            input.eventTextChanged += OnTextChanged;

            UIButton button = AddUIComponent<UIButton>();
            button.size = new Vector2(43, 49);
            button.atlas = FindIt.instance.m_mainButton.atlas;
            button.normalFgSprite = "FindIt";
            button.hoveredFgSprite = "FindItFocused";
            button.pressedFgSprite = "FindItPressed";
            button.relativePosition = new Vector3(width - 41, -3);

            button.eventClick += (c, p) =>
            {
                input.Focus();
                input.SelectAll();
            };
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

            PrefabInfo current = null;
            int selected = -1;
            if(scrollPanel.selectedItem != null)
            {
                current = scrollPanel.selectedItem.objectUserData as PrefabInfo;
            }

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

                    if(asset.prefab == current)
                    {
                        selected = scrollPanel.itemsData.m_size - 1;
                    }
                }
            }

            scrollPanel.DisplayAt(0);
            scrollPanel.selectedIndex = selected;

            if (scrollPanel.selectedItem != null)
            {
                FindIt.SelectPrefab(scrollPanel.selectedItem.objectUserData as PrefabInfo);
            }
            else
            {
                ToolsModifierControl.SetTool<DefaultTool>();
            }
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
