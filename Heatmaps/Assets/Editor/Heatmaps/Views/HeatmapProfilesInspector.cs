using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapProfilesInspector
    {
        static HeatmapProfilesInspector s_Instance;
        List<HeatmapSettings> m_Profiles = new List<HeatmapSettings>();
        Heatmapper m_Heatmapper;
        bool m_Adding = false;
        bool m_NameNotUnique = false;
        string m_Name = "";

        GUIContent m_SaveCurrrentContent = new GUIContent("Save Current", "Store the Heatmapper's current settings");

        public HeatmapProfilesInspector(Heatmapper heatmapper)
        {
            m_Heatmapper = heatmapper;
        }

        public static HeatmapProfilesInspector Init(Heatmapper heatmapper)
        {
            if (s_Instance == null)
            {
                s_Instance = new HeatmapProfilesInspector(heatmapper);
            }
            return s_Instance;
        }

        public void OnEnable()
        {
            GenerateList();
        }

        public void OnGUI()
        {
            for (int a = 0; a < m_Profiles.Count; a++)
            {
                if (m_Profiles[a] == null)
                {
                    OnEnable();
                    return;
                }
                ProfileItem(m_Profiles[a].name, a);
            }
            if (m_Adding)
            {
                EditorGUILayout.LabelField(m_NameNotUnique ? "Please choose a unique name" : "Name this profile");

                using (new GUILayout.HorizontalScope())
                {
                    m_Name = EditorGUILayout.TextField(m_Name);
                    if (GUILayout.Button("Cancel", GUILayout.MaxWidth(50)))
                    {
                        CloseCreation();
                    }
                    if (GUILayout.Button("OK", GUILayout.MaxWidth(30)))
                    {
                        Create(m_Name);
                    }
                }
            }
            else if (GUILayout.Button(m_SaveCurrrentContent))
            {
                OpenCreation();
            }
        }

        void ProfileItem(string label, int id)
        {
            using(new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button(label))
                {
                    Apply(id);
                }
                if (GUILayout.Button("-", GUILayout.MaxWidth(50f)))
                {
                    RemoveAt(id);
                }
            }
        }

        void Apply(int id)
        {
            HeatmapInspectorViewModel.GetInstance().UpdateSettings(m_Profiles[id]);
            m_Heatmapper.SystemReset();
        }

        void RemoveAt(int id)
        {
            var profile = m_Profiles[id];
            m_Profiles.RemoveAt(id);
            var path = GetAssetPath(profile.name);
            AssetDatabase.DeleteAsset(path);
        }

        void OpenCreation()
        {
            m_Adding = true;
        }

        void CloseCreation()
        {
            m_Adding = false;
            m_NameNotUnique = false;
            m_Name = "";
        }

        void Create(string name)
        {
            if (AssetNameUnique(name) && !string.IsNullOrEmpty(name))
            {
                HeatmapSettings profile = HeatmapInspectorViewModel.GetInstance().RecordSettings();
                profile.name = name;
                var savePath = GetAssetPath(profile.name);
                AssetDatabase.CreateAsset(profile, savePath);
                EditorUtility.SetDirty(profile);
                GenerateList();
                CloseCreation();
            }
            else
            {
                m_NameNotUnique = true;
            }
        }

        void GenerateList()
        {
            m_Profiles = new List<HeatmapSettings>();
            var path = GetSavePath();
            var files = System.IO.Directory.GetFiles(path);

            for (var a = 0; a < files.Length; a++)
            {
                var assetPath = files[a];
                var profile = AssetDatabase.LoadAssetAtPath<HeatmapSettings>(assetPath);
                if (profile != null)
                {
                    m_Profiles.Add(profile);
                }
            }
        }

        string GetSavePath()
        {
            string path = System.IO.Path.Combine("Assets", "Editor");
            path = System.IO.Path.Combine(path, "Heatmaps");
            path = System.IO.Path.Combine(path, "Profiles");

            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            return path;
        }

        string GetAssetPath(string name)
        {
            var path = GetSavePath();
            return System.IO.Path.Combine(path, name + ".asset");
        }

        bool AssetNameUnique(string name)
        {
            for (var a = 0; a < m_Profiles.Count; a++)
            {
                if (m_Profiles[a].name == name)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

