// modified from SamsamTS's original Find It mod
// https://github.com/SamsamTS/CS-FindIt

using UnityEngine;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Reflection;


namespace FindIt.GUI
{
    public class UIDoubleClickMenu : UIPanel
    {
        public static UIDoubleClickMenu instance;
        private UIButton cancelButton;
        private UIButton meshInfoButton;
        private UIButton ricoButton;
        private static readonly int PanelWidth = 200;
        private static readonly int PanelHeight = 80;
        private Asset selectedAsset;

        public override void Start()
        {
            name = "FindIt_UIDoubleClickMenu";
            atlas = SamsamTS.UIUtils.GetAtlas("Ingame");
            backgroundSprite = "ButtonMenu";
            size = new Vector2(PanelWidth, PanelHeight);

            SetUpMeshInfoButton();
            SetUpRICOButton();

            cancelButton = SamsamTS.UIUtils.CreateButton(this);
            cancelButton.size = new Vector2(PanelWidth, 40);
            cancelButton.text = Translations.Translate("FIF_POP_CAN");
            cancelButton.relativePosition = new Vector3(0, ricoButton.relativePosition.y + ricoButton.height);
            cancelButton.eventClick += (c, p) =>
            {
                Close();
            };

            height = cancelButton.relativePosition.y + cancelButton.height;
            cancelButton.Focus();
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

        public static void ShowAt(UIComponent component, Asset asset)
        {
            if (instance == null)
            {
                instance = UIView.GetAView().AddUIComponent(typeof(UIDoubleClickMenu)) as UIDoubleClickMenu;
                instance.Show(true);
                instance.selectedAsset = asset;
                UIView.PushModal(instance);
            }
            else
            {
                instance.selectedAsset = asset;
                instance.Show(true);
            }

            float absoluteX = component.absolutePosition.x + component.width;
            if ((absoluteX + PanelWidth) > UIView.GetAView().fixedWidth)
            {
                absoluteX = component.absolutePosition.x - PanelWidth;
            }
            instance.absolutePosition = new Vector2(absoluteX, component.absolutePosition.y);

        }

        private void SetUpMeshInfoButton()
        {
            meshInfoButton = SamsamTS.UIUtils.CreateButton(this);
            meshInfoButton.size = new Vector2(PanelWidth, 40);
            meshInfoButton.text = Translations.Translate("FIF_DOU_MESH");
            meshInfoButton.relativePosition = new Vector3(0, 0);
            meshInfoButton.eventClick += (c, p) =>
            {
                if (FindIt.isMeshInfoEnabled) ViewInMeshInfo();
                meshInfoButton.tooltipBox.Hide();
                Close();
            };

            // set up tooltip
            meshInfoButton.Disable();
            if (!FindIt.isMeshInfoEnabled) meshInfoButton.tooltip = Translations.Translate("FIF_DOU_MESHNO");
            else if (!selectedAsset.prefab.m_isCustomContent ||
                selectedAsset.assetType == Asset.AssetType.Network ||
                selectedAsset.assetType == Asset.AssetType.Invalid)
            {
                meshInfoButton.tooltip = Translations.Translate("FIF_DOU_MESHTYPE");
            }
            else
            {
                meshInfoButton.tooltip = Translations.Translate("FIF_DOU_MESHOPEN");
                meshInfoButton.Enable();
            }
        }

        private void ViewInMeshInfo()
        {
            // get Mesh Info's main panel
            UIComponent meshInfoPanel = UIView.GetAView().FindUIComponent("MeshInfo");
            if (meshInfoPanel == null) return;
            meshInfoPanel.isVisible = true;
            UIDropDown[] dropdowns = meshInfoPanel.GetComponentsInChildren<UIDropDown>();
            UIDropDown typeDropdown = null;
            foreach (UIDropDown dropdown in dropdowns)
            {
                if (dropdown.items.Length == 4)
                {
                    typeDropdown = dropdown;
                    break;
                }
            }
            if (typeDropdown == null) return;
            if (selectedAsset.assetType == Asset.AssetType.Growable ||
                selectedAsset.assetType == Asset.AssetType.Ploppable ||
                selectedAsset.assetType == Asset.AssetType.Rico)
            {
                typeDropdown.selectedIndex = 0;
            }
            else if (selectedAsset.assetType == Asset.AssetType.Prop ||
                selectedAsset.assetType == Asset.AssetType.Decal)
            {
                typeDropdown.selectedIndex = 1;
            }
            else if (selectedAsset.assetType == Asset.AssetType.Tree)
            {
                typeDropdown.selectedIndex = 2;
            }
            UITextField textField = meshInfoPanel.GetComponentInChildren<UITextField>();
            if (textField == null) return;
            textField.text = selectedAsset.title;
            Type UIMainPanelType = Type.GetType("MeshInfo.GUI.UIMainPanel");
            MethodInfo InitializePreafabListsMI = UIMainPanelType.GetMethod("InitializePreafabLists", BindingFlags.NonPublic | BindingFlags.Instance);
            InitializePreafabListsMI.Invoke(meshInfoPanel, new object[] { });
            meshInfoPanel.BringToFront();
        }

        private void SetUpRICOButton()
        {
            ricoButton = SamsamTS.UIUtils.CreateButton(this);
            ricoButton.size = new Vector2(PanelWidth, 40);
            ricoButton.text = Translations.Translate("FIF_DOU_RICO");
            ricoButton.relativePosition = new Vector3(0, meshInfoButton.relativePosition.y + meshInfoButton.height);
            ricoButton.eventClick += (c, p) =>
            {
                if (FindIt.isRicoEnabled) ViewInRICORevisited();
                ricoButton.tooltipBox.Hide();
                Close();
            };

            // set up tooltip
            ricoButton.Disable();

            // check if old rico is installed
            Type ploppablericoType = Type.GetType("PloppableRICO.SettingsPanel, ploppablerico");

            if (!FindIt.isRicoEnabled || ploppablericoType == null) ricoButton.tooltip = Translations.Translate("FIF_DOU_RICONO");
            else if (selectedAsset.assetType != Asset.AssetType.Ploppable &&
                selectedAsset.assetType != Asset.AssetType.Growable &&
                selectedAsset.assetType != Asset.AssetType.Rico)
            {
                ricoButton.tooltip = Translations.Translate("FIF_DOU_RICOTYPE");
            }
            else
            {
                ricoButton.tooltip = Translations.Translate("FIF_DOU_RICOOPEN");
                ricoButton.Enable();
            }
        }

        private void ViewInRICORevisited()
        {
            Type SettingsPanelType = Type.GetType("PloppableRICO.SettingsPanel, ploppablerico");
            MethodInfo OpenMI = SettingsPanelType.GetMethod("Open", BindingFlags.NonPublic | BindingFlags.Static);
            BuildingInfo info = selectedAsset.prefab as BuildingInfo;
            OpenMI.Invoke(null, new object[] { info });
        }
    }
}
