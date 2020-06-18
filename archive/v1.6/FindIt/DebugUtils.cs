﻿using UnityEngine;
using ColossalFramework.Plugins;

using System;
using ColossalFramework;

namespace FindIt
{
    public class DebugUtils
    {
        public const string modPrefix = "[Find It! " + ModInfo.version + "] ";

        public static SavedBool hideDebugMessages = new SavedBool("hideDebugMessages", FindIt.settingsFileName, true, true);

        public static void Log(string message)
        {
            if (hideDebugMessages.value) return;

            if (message == m_lastLog)
            {
                m_duplicates++;
            }
            else if (m_duplicates > 0)
            {
                Debug.Log(modPrefix + m_lastLog + "(x" + (m_duplicates + 1) + ")");
                Debug.Log(modPrefix + message);
                m_duplicates = 0;
            }
            else
            {
                Debug.Log(modPrefix + message);
            }
            m_lastLog = message;
        }

        public static void Warning(string message)
        {
            if (message != m_lastWarning)
            {
                Debug.Log(modPrefix + message);
            }
            m_lastWarning = message;
        }

        public static void LogException(Exception e)
        {
            Debug.Log(modPrefix + "Intercepted exception (not game breaking):");
            Debug.LogException(e);
        }

        private static string m_lastWarning;
        private static string m_lastLog;
        private static int m_duplicates = 0;
    }
}
