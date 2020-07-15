// modified from Elektrix's CS-NetPicker3
// https://github.com/CosignCosine/CS-NetPicker3
using ICities;
using UnityEngine;

namespace FindIt
{
    public class PickerLoader : LoadingExtensionBase
    {
        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            if (NetPickerTool.instance == null)
            {
                ToolController toolController = ToolsModifierControl.toolController;
                NetPickerTool.instance = toolController.gameObject.AddComponent<NetPickerTool>();
                NetPickerTool.instance.enabled = false;
            }
        }

        public override void OnReleased()
        {
            base.OnReleased();
            GameObject.Destroy(NetPickerTool.instance);
        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();
            GameObject.Destroy(NetPickerTool.instance);
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);
            if (LoadingManager.instance.m_loadingComplete)
            {
                if (NetPickerTool.instance == null)
                {
                    ToolController toolController = ToolsModifierControl.toolController;
                    NetPickerTool.instance = toolController.gameObject.AddComponent<NetPickerTool>();
                    NetPickerTool.instance.enabled = false;
                }
            }
        }
    }

    public class IngameKeybindingResolver : ThreadingExtensionBase
    {
        public override void OnUpdate(float realTimeDelta, float simulationTimeDelta)
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
               Debugging.Message("NetPicker - " + "Hotkey N pressed");
                NetPickerTool.instance.enabled = !NetPickerTool.instance.enabled;
                ToolsModifierControl.SetTool<NetPickerTool>();
            }
        }
    }
}
