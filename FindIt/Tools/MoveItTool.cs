using System;
using System.Reflection;
using UnityEngine;

namespace FindIt
{
    public static class MoveItTool
    {
        private static bool initialized = false;
        private static ToolController toolController;
        private static Component moveItTool;
        private static MethodInfo pasteFromExternalMethod;
        private static void Init()
        {
            try
            {
                toolController = UnityEngine.Object.FindObjectOfType<ToolController>();
                moveItTool = toolController.GetComponent("MoveItTool");
                Type pasteFromExternalType = Type.GetType("MoveIt.MoveItTool, MoveIt", false);
                pasteFromExternalMethod = pasteFromExternalType.GetMethod("PasteFromExternal");
            }
            catch (Exception ex)
            {
                Debugging.LogException(ex);
            }

            initialized = true;
        }

        public static bool MoveItClone(PrefabInfo prefab)
        {
            if (!initialized) Init();

            try
            {
                bool result = (bool)pasteFromExternalMethod.Invoke(null, new object[] { prefab });
                if (result) toolController.CurrentTool = moveItTool as ToolBase;
                return result;
            }
            catch (Exception e)
            {
                Debugging.LogException(e);
                return false;
            }
        }
    }
}