/// <summary>
/// Model to store Data retrieval and Aggregation values
/// </summary>

using System;
using UnityEditor;
using System.Collections.Generic;

namespace UnityAnalyticsHeatmap
{
    public class AggregationSettings : IAggregationSettings
    {
        // Data retrieval keys
        const string k_UrlKey = "UnityAnalyticsHeatmapDataExportUrlKey";

        // Aggregation keys
        const string k_SpaceKey = "UnityAnalyticsHeatmapAggregationSpace";
        const string k_KeyToTime = "UnityAnalyticsHeatmapAggregationTime";
        const string k_AngleKey = "UnityAnalyticsHeatmapAggregationAngle";
        const string k_AggregateTimeKey = "UnityAnalyticsHeatmapAggregationAggregateTime";
        const string k_AggregateAngleKey = "UnityAnalyticsHeatmapAggregationAggregateAngle";
        const string k_AggregateDevicesKey = "UnityAnalyticsHeatmapAggregationAggregateDeviceIDs";
        const string k_ArbitraryFieldsKey = "UnityAnalyticsHeatmapAggregationArbitraryFields";
        const string k_EventsKey = "UnityAnalyticsHeatmapAggregationEvents";
        
        const float k_DefaultSpace = 10f;
        const float k_DefaultTime = 10f;
        const float k_DefaultAngle = 15f;

        // Data retrieval properties
        string m_RawDataPath = "";
        public string rawDataPath {
            get
            {
                return m_RawDataPath;
            }
            set
            {
                string old = m_RawDataPath;
                m_RawDataPath = value;
                if (rawDataPath != old)
                {
                    EditorPrefs.SetString(k_UrlKey, m_RawDataPath);
                }
            }
        }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public bool localOnly { get; set; }

        // Aggregation properties
        float m_Space = k_DefaultSpace;
        public float space {
            get {
                return m_Space;
            }
            set {
                float old = m_Space;
                m_Space = value;
                if (old != m_Space)
                {
                    EditorPrefs.SetFloat(k_SpaceKey, m_Space);
                }
            }
        }
        float m_Time = k_DefaultTime;
        public float time {
            get {
                return m_Time;
            }
            set {
                float old = m_Time;
                m_Time = value;
                if (old != m_Time)
                {
                    EditorPrefs.SetFloat(k_KeyToTime, m_Time);
                }
            }
        }
        float m_Angle = k_DefaultAngle;
        public float angle {
            get {
                return m_Angle;
            }
            set {
                float old = m_Angle;
                m_Angle = value;
                if (old != m_Angle)
                {
                    EditorPrefs.SetFloat(k_AngleKey, m_Angle);
                }
            }
        }
        bool m_AggregateTime = true;
        public bool aggregateTime {
            get {
                return m_AggregateTime;
            }
            set {
                bool old = m_AggregateTime;
                m_AggregateTime = value;
                if (old != m_AggregateTime)
                {
                    EditorPrefs.SetBool(k_AggregateTimeKey, m_AggregateTime);
                }
            }
        }
        bool m_AggregateAngle = true;
        public bool aggregateAngle {
            get {
                return m_AggregateAngle;
            }
            set {
                bool old = m_AggregateAngle;
                m_AggregateAngle = value;
                if (old != m_AggregateAngle)
                {
                    EditorPrefs.SetBool(k_AggregateAngleKey, m_AggregateAngle);
                }
            }
        }
        bool m_GroupDevices = true;
        public bool groupDevices {
            get {
                return m_GroupDevices;
            }
            set {
                bool old = m_GroupDevices;
                m_GroupDevices = value;
                if (old != m_GroupDevices)
                {
                    EditorPrefs.SetBool(k_AggregateDevicesKey, m_GroupDevices);
                }
            }
        }
        List<string> m_ArbitraryGroupFields = new List<string>();
        string m_ArbitraryGroupFieldsAsString = "";
        public List<string> arbitraryGroupFields {
            get {
                return m_ArbitraryGroupFields;
            }
            set {
                string old = m_ArbitraryGroupFieldsAsString;
                m_ArbitraryGroupFields = value;
                m_ArbitraryGroupFieldsAsString = string.Join("|", m_ArbitraryGroupFields.ToArray());
                if (old != m_ArbitraryGroupFieldsAsString)
                {
                    EditorPrefs.SetString(k_ArbitraryFieldsKey, m_ArbitraryGroupFieldsAsString);
                }
            }
        }
        List<string> m_WhiteListEvents = new List<string>{ };
        string m_WhileListEventsAsString = "";
        public List<string> whiteListEvents {
            get {
                return m_WhiteListEvents;
            }
            set {
                string old = m_WhileListEventsAsString;
                m_WhiteListEvents = value;
                m_WhileListEventsAsString = string.Join("|", m_WhiteListEvents.ToArray());
                if (old != m_WhileListEventsAsString)
                {
                    EditorPrefs.SetString(k_EventsKey, m_WhileListEventsAsString);
                }
            }
        }

        public List<string> fileList { get; set; }
        public string jsonPath { get; set; }

        public AggregationSettings()
        {
            LoadSettings();
        }

        void LoadSettings()
        {
            // Restore cached paths
            rawDataPath = EditorPrefs.GetString(k_UrlKey);
            
            // Set dates based on today (should this be cached?)
            endDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
            startDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0)));
            
            // Restore other options
            space = EditorPrefs.GetFloat(k_SpaceKey) == 0 ? k_DefaultSpace : EditorPrefs.GetFloat(k_SpaceKey);
            time = EditorPrefs.GetFloat(k_KeyToTime) == 0 ? k_DefaultTime : EditorPrefs.GetFloat(k_KeyToTime);
            angle = EditorPrefs.GetFloat(k_AngleKey) == 0 ? k_DefaultAngle : EditorPrefs.GetFloat(k_AngleKey);
            aggregateTime = EditorPrefs.GetBool(k_AggregateTimeKey);
            aggregateAngle = EditorPrefs.GetBool(k_AggregateAngleKey);
            groupDevices = EditorPrefs.GetBool(k_AggregateDevicesKey);
            
            // Restore list of arbitrary aggregation fields
            string loadedArbitraryFields = EditorPrefs.GetString(k_ArbitraryFieldsKey);
            string[] arbitraryFieldsList;
            if (string.IsNullOrEmpty(loadedArbitraryFields))
            {
                arbitraryFieldsList = new string[]{ };
            }
            else
            {
                arbitraryFieldsList = loadedArbitraryFields.Split('|');
            }
            arbitraryGroupFields = new List<string>(arbitraryFieldsList);
            
            // Restore list of events
            string loadedEvents = EditorPrefs.GetString(k_EventsKey);
            string[] eventsList;
            if (string.IsNullOrEmpty(loadedEvents))
            {
                eventsList = new string[]{ };
            }
            else
            {
                eventsList = loadedEvents.Split('|');
            }
            whiteListEvents = new List<string>(eventsList);
        }
    }
}

