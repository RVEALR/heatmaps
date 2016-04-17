/// <summary>
/// Heat map data parser.
/// </summary>
/// This portion of the Heatmapper opens a JSON file and processes it into an array
/// of point data.
/// OnGUI functionality displays the state of the data in the Heatmapper inspector.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapDataParserInspector
    {
        const string k_DataPathKey = "UnityAnalyticsHeatmapDataPath";

        string m_Path = "";

        Dictionary<string, HeatPoint[]> m_HeatData;
        Vector3 m_LowSpace;
        Vector3 m_HighSpace;


        int m_OptionIndex = 0;
        string[] m_OptionKeys;

        public delegate void PointHandler(HeatPoint[] heatData);

        PointHandler m_PointHandler;

        HeatmapDataParser m_DataParser = new HeatmapDataParser();


        public HeatmapDataParserInspector(PointHandler handler)
        {
            m_PointHandler = handler;
            m_Path = EditorPrefs.GetString(k_DataPathKey);
        }

        public static HeatmapDataParserInspector Init(PointHandler handler)
        {
            return new HeatmapDataParserInspector(handler);
        }

        void Dispatch()
        {
            m_PointHandler(m_HeatData[m_OptionKeys[m_OptionIndex]]);
        }

        public void SetDataPath(string jsonPath)
        {
            m_Path = jsonPath;
            m_DataParser.LoadData(m_Path, ParseHandler);
        }

        public void OnGUI()
        {
            if (m_HeatData != null && m_OptionKeys != null && m_OptionIndex > -1 && m_OptionIndex < m_OptionKeys.Length && m_HeatData.ContainsKey(m_OptionKeys[m_OptionIndex]))
            {
                int oldIndex = m_OptionIndex;
                m_OptionIndex = EditorGUILayout.Popup("Option", m_OptionIndex, m_OptionKeys);
                if (m_OptionIndex != oldIndex)
                {
                    Dispatch();
                }
            }
        }

        void ParseHandler(Dictionary<string, HeatPoint[]> heatData, string[] options)
        {
            m_HeatData = heatData;
            if (heatData != null)
            {
                if (m_OptionKeys != null)
                {
                    string opt = m_OptionIndex > m_OptionKeys.Length ? "" : m_OptionKeys[m_OptionIndex];
                    ArrayList list = new ArrayList(options);
                    int idx = list.IndexOf(opt);
                    m_OptionIndex = idx == -1 ? 0 : idx;
                }
                else
                {
                    m_OptionIndex = 0;
                }
                m_OptionKeys = options;
                Dispatch();
            }
        }
    }
}
