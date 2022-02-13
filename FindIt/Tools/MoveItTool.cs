using System;
using System.Reflection;
using UnityEngine;

namespace FindIt
{
    public static class MoveItTool
    {
        public static bool initialized = false;
        private static ToolController toolController;
        private static Component moveItTool;
        private static MethodInfo pasteFromExternalMethod;
        public static bool Init()
        {
            try
            {
                toolController = UnityEngine.Object.FindObjectOfType<ToolController>();
                moveItTool = toolController.GetComponent("MoveItTool");
                if (moveItTool == null)
                {
                    Debugging.Message("MoveItTool is null");
                    return false;
                }
                Type pasteFromExternalType = Type.GetType("MoveIt.MoveItTool, MoveIt", false);
                if (pasteFromExternalType == null)
                {
                    Debugging.Message("pasteFromExternalType is null");
                    return false;
                }
                pasteFromExternalMethod = pasteFromExternalType.GetMethod("PasteFromExternal");
                if (pasteFromExternalMethod == null)
                {
                    Debugging.Message("pasteFromExternalMethod is null");
                    return false;
                }
            }

            catch (Exception ex)
            {
                Debugging.LogException(ex);
                return false;
            }

            initialized = true;
            return true;
        }

        public static bool MoveItClone(PrefabInfo prefab)
        {
            if (!initialized) return false;
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