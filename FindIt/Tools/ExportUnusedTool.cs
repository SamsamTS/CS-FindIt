﻿using System;
using System.Collections.Generic;
using System.IO;
using ColossalFramework;
using FindIt.GUI;
using ColossalFramework.UI;
using ColossalFramework.IO;

namespace FindIt
{
    public static class ExportUnusedTool
    {
        public static void ExportUnused(bool exportAllUnused)
        {
            // get steam id of all workship assets
            HashSet<ulong> steamIds = new HashSet<ulong>();

            if (exportAllUnused)
            {
                foreach (Asset asset in AssetTagList.instance.assets.Values)
                {
                    if (!asset.prefab.m_isCustomContent) continue;
                    if (asset.steamID == 0) continue;
                    steamIds.Add(asset.steamID);
                }
            }

            else // only export assets from last search
            {
                foreach (Asset asset in UISearchBox.instance.matches)
                {
                    if (!asset.prefab.m_isCustomContent) continue;
                    if (asset.steamID == 0) continue;
                    steamIds.Add(asset.steamID);
                }
            }

            // update instance count
            AssetTagList.instance.UpdatePrefabInstanceCount(UISearchBox.DropDownOptions.All);
            if (FindIt.isPOEnabled) ProceduralObjectsTool.UpdatePOInfoList();

            // filter out used assets
            Dictionary<ulong, int> unusedIDs = new Dictionary<ulong, int>();
            foreach (Asset asset in AssetTagList.instance.assets.Values)
            {
                if (!asset.prefab.m_isCustomContent) continue;
                if (asset.steamID == 0) continue;
                AssetTagList.instance.UpdateAssetInstanceCount(asset, true);
                if (asset.instanceCount > 0 || asset.poInstanceCount > 0) 
                {
                    steamIds.Remove(asset.steamID);
                }
                else
                {
                    if (unusedIDs.ContainsKey(asset.steamID)) unusedIDs[asset.steamID] += 1;
                    else unusedIDs.Add(asset.steamID, 1);
                }
            }

            string currentTime = GetFormattedDateTime();
            string path = Path.Combine(DataLocation.localApplicationData, $"FindItExportUnusedWorkshopID_{currentTime}.html");
            if (File.Exists(path)) File.Delete(path);

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
            {
                file.WriteLine($"City Name: {GetCityName()}<br>");
                file.WriteLine($"Export Date: {currentTime}<br>");
                file.WriteLine($"<br>This list only considers asset types that are monitored by Find It 2<br>");
                file.WriteLine($"<br>Some mods are bundled with assets for other purpose. Ignore mods in the list<br>");
                file.WriteLine($"<br>If you ever copied assets from the workshop download folder to the local asset folder, the information here can be inaccurate<br>");
                if (FindIt.isPOEnabled)
                {
                    file.WriteLine($"<br>It seems like you're using Procedural Objects. This list already considers POs<br>");
                }
                
                foreach (ulong id in steamIds)
                {
                    if (unusedIDs.ContainsKey(id))
                    {
                        file.WriteLine($"<br><a href=\"https://steamcommunity.com/sharedfiles/filedetails/?id={id}\">{id}</a><br>\n");
                        file.WriteLine($"This workshop ID contains {unusedIDs[id]} asset(s). All unused<br>\n");
                    }
                }
            }

            ExceptionPanel panel = UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel");
            panel.SetMessage("Find It 2", $"FindItExportUnusedWorkshopID.html is exported.\n\nIt only considers asset types that are monitored by Find It 2.\n\n{path}", false);
        }

        private static string GetCityName()
        {
            if (Singleton<SimulationManager>.exists)
            {
                string cityName = Singleton<SimulationManager>.instance.m_metaData.m_CityName;
                if (cityName != null) return cityName;
            }
            return "(Unknown)";
        }

        private static string GetFormattedDateTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd_hh-mm-ss");
        }
    }
}
