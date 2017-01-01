using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

using ColossalFramework;
using ColossalFramework.PlatformServices;
using ColossalFramework.Packaging;

namespace FindIt
{
    public class Asset
    {
        public string name;
        public PrefabInfo prefab;
        public ulong steamID;
        public string author;
        public float score;
    }

    public class AssetTagList
    {
        public static AssetTagList instance;

        public Dictionary<string, HashSet<Asset>> tagsTitle = new Dictionary<string, HashSet<Asset>>();
        public Dictionary<string, HashSet<Asset>> tagsDesc = new Dictionary<string, HashSet<Asset>>();
        public Dictionary<string, HashSet<Asset>> authors = new Dictionary<string, HashSet<Asset>>();
        public Dictionary<string, Asset> assets = new Dictionary<string, Asset>();

        public List<Asset> matches = new List<Asset>();

        public string m_search = "";

        public string search
        {
            set
            {
                m_search = value;
                matches.Clear();

                if(value.IsNullOrWhiteSpace()) return;

                string[] values = value.ToLower().Split(' ');

                Find(values, authors, 10, false);
                Find(values, tagsTitle, 5, true);
                Find(values, tagsDesc, 1, true);

                matches = matches.OrderByDescending(s => s.score).ToList();
            }

            get
            {
                return m_search;
            }
        }

        public AssetTagList()
        {
            foreach (Package.Asset current in PackageManager.FilterAssets(new Package.AssetType[] { UserAssetType.CustomAssetMetaData }))
            {
                PublishedFileId id = current.package.GetPublishedFileID();
                string author = null;

                if (!current.package.packageAuthor.IsNullOrWhiteSpace())
                {
                    ulong authorID;
                    if (UInt64.TryParse(current.package.packageAuthor.Substring("steamid:".Length), out authorID))
                    {
                        author = new Friend(new UserID(authorID)).personaName;
                    }
                }

                if (!assets.ContainsKey(current.fullName))
                {
                    assets[current.fullName] = new Asset()
                    {
                        name = current.fullName,
                        steamID = id.AsUInt64,
                        author = author
                    };
                }
            }
        }

        public void Init()
        {
            foreach (Asset asset in assets.Values)
            {
                asset.prefab = null;
            }

            tagsTitle.Clear();
            tagsDesc.Clear();
            authors.Clear();
            
            GetPrefabs<BuildingInfo>();
            //GetPrefab<NetInfo>();
            GetPrefabs<PropInfo>();
            GetPrefabs<TreeInfo>();

            foreach (Asset asset in assets.Values)
            {
                if (asset.prefab != null)
                {
                    AddAssetTags(asset, tagsTitle, asset.prefab.GetLocalizedTitle());
                    AddAssetTags(asset, tagsDesc, asset.prefab.GetLocalizedDescription());

                    if (!asset.author.IsNullOrWhiteSpace())
                    {
                        string author = Regex.Replace(asset.author.ToLower().Trim(), @"([^\w]|\s)+", "_");
                        if (!authors.ContainsKey(author))
                        {
                            authors[author] = new HashSet<Asset>();
                        }
                        authors[author].Add(asset);
                    }
                }
            }

            CleanDictionary(tagsTitle);
            CleanDictionary(tagsDesc);
        }

        private void Find(string[] tags, Dictionary<string, HashSet<Asset>> dico, float scoreMultiplier, bool weight)
        {
            foreach (string tag in dico.Keys)
            {
                float score = 0;
                foreach (string t in tags)
                {
                    int index = tag.IndexOf(t);

                    if (index >= 0)
                    {
                        float w = 1f;
                        if(weight)
                        {
                            w = dico[tag].Count;
                        }
                        score += scoreMultiplier / w * ((tag.Length - index) / (float)tag.Length) * (t.Length / (float)tag.Length);
                    }
                }

                if (score > 0)
                {
                    foreach (Asset asset in dico[tag])
                    {
                        if (matches.Contains(asset))
                        {
                            asset.score += score;
                        }
                        else
                        {
                            matches.Add(asset);
                            asset.score = score;
                        }
                    }
                }
            }
        }

        private void GetPrefabs<T>() where T : PrefabInfo
        {
            for (uint i = 0; i < PrefabCollection<T>.PrefabCount(); i++)
            {
                T prefab = PrefabCollection<T>.GetPrefab(i);

                if (prefab == null) continue;

                string name = prefab.name;
                if(name.EndsWith("_Data"))
                {
                    name = name.Substring(0, name.LastIndexOf("_Data"));
                }

                if (assets.ContainsKey(name))
                {
                    assets[name].prefab = prefab;
                }
                else
                {
                    assets[name] = new Asset()
                    {
                        name = name,
                        prefab = prefab,
                        steamID = GetSteamID(prefab)
                    };
                }
            }
        }

        private void AddAssetTags(Asset asset, Dictionary<string, HashSet<Asset>> dico, string text)
        {
            text = Regex.Replace(text, "([A-Z][a-z]+)", " $1");

            string[] tagsArr = Regex.Split(text, @"([^\w]|\s)+", RegexOptions.IgnoreCase);

            foreach (string t in tagsArr)
            {
                string tag = t.ToLower().Trim();

                if (tag.Length > 0 && !tag.Contains("_"))
                {
                    if (!dico.ContainsKey(tag))
                    {
                        dico[tag] = new HashSet<Asset>();
                    }
                    dico[tag].Add(asset);
                }
            }
        }

        private void CleanDictionary(Dictionary<string, HashSet<Asset>> dico)
        {
            List<string> keys = new List<string>(dico.Keys);
            foreach (string key in keys)
            {
                if (key.EndsWith("s"))
                {
                    string tag = key.Substring(0, key.Length - 1);
                    if (dico.ContainsKey(tag))
                    {
                        dico[tag].UnionWith(dico[key]);
                        dico.Remove(key);
                    }
                }
            }
        }

        private static ulong GetSteamID(PrefabInfo prefab)
        {
            ulong id = 0;

            if (prefab.name.Contains("."))
            {
                string steamID = prefab.name.Substring(0, prefab.name.IndexOf("."));
                UInt64.TryParse(steamID, out id);
            }

            return id;
        }
    }
}
