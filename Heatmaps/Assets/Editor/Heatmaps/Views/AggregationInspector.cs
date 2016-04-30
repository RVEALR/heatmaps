/// <summary>
/// Inspector for the Aggregation portion of the Heatmapper.
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class AggregationInspector
    {
        const string k_UrlKey = "UnityAnalyticsHeatmapDataExportUrlKey";
        const string k_DataPathKey = "UnityAnalyticsHeatmapDataPathKey";
        const string k_UseCustomDataPathKey = "UnityAnalyticsHeatmapUsePersistentDataPathKey";

        const string k_SpaceKey = "UnityAnalyticsHeatmapAggregationSpace";
        const string k_KeyToTime = "UnityAnalyticsHeatmapAggregationTime";
        const string k_AngleKey = "UnityAnalyticsHeatmapAggregationAngle";
        const string k_AggregateTimeKey = "UnityAnalyticsHeatmapAggregationAggregateTime";
        const string k_AggregateAngleKey = "UnityAnalyticsHeatmapAggregationAggregateAngle";
        const string k_AggregateDevicesKey = "UnityAnalyticsHeatmapAggregationAggregateDeviceIDs";
        const string k_ArbitraryFieldsKey = "UnityAnalyticsHeatmapAggregationArbitraryFields";
        const string k_EventsKey = "UnityAnalyticsHeatmapAggregationEvents";

        const string k_RemapColorKey = "UnityAnalyticsHeatmapRemapColorKey";
        const string k_RemapOptionIndexKey = "UnityAnalyticsHeatmapRemapOptionIndexKey";
        const string k_RemapColorFieldKey = "UnityAnalyticsHeatmapRemapColorFieldKey";

        const float k_DefaultSpace = 10f;
        const float k_DefaultTime = 10f;
        const float k_DefaultAngle = 15f;

        string m_RawDataPath = "";
        string m_DataPath = "";
        bool m_UseCustomDataPath = true;

        Dictionary<string, HeatPoint[]> m_HeatData;

        public delegate void AggregationHandler(string jsonPath);

        AggregationHandler m_AggregationHandler;

        RawEventClient m_RawEventClient;
        HeatmapAggregator m_Aggregator;

        string m_StartDate = "";
        string m_EndDate = "";
        float m_Space = k_DefaultSpace;
        float m_Time = k_DefaultTime;
        float m_Angle = k_DefaultAngle;
        bool m_AggregateTime = true;
        bool m_AggregateAngle = true;
        bool m_AggregateDevices = true;

        bool m_RemapColor;
        string m_RemapColorField = "";
        int m_RemapOptionIndex = 0;
        GUIContent[] m_RemapOptions = new GUIContent[]{ new GUIContent("Increment"), new GUIContent("Cumulative"), new GUIContent("First Wins"), new GUIContent("Last Wins"), new GUIContent("Min Wins"), new GUIContent("Max Wins") };
        AggregationMethod[] m_RemapOptionIds = new AggregationMethod[]{ AggregationMethod.Increment, AggregationMethod.Cumulative, AggregationMethod.FirstWins, AggregationMethod.LastWins, AggregationMethod.MinWins, AggregationMethod.MaxWins };

        List<string> m_ArbitraryFields = new List<string>{ };
        List<string> m_Events = new List<string>{ };

        public AggregationInspector(RawEventClient client, HeatmapAggregator aggregator)
        {
            m_Aggregator = aggregator;
            m_RawEventClient = client;

            // Restore cached paths
            m_RawDataPath = EditorPrefs.GetString(k_UrlKey);
            m_UseCustomDataPath = EditorPrefs.GetBool(k_UseCustomDataPathKey);
            m_DataPath = EditorPrefs.GetString(k_DataPathKey);

            // Set dates based on today (should this be cached?)
            m_EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.UtcNow);
            m_StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.UtcNow.Subtract(new TimeSpan(5, 0, 0, 0)));

            // Restore other options
            m_Space = EditorPrefs.GetFloat(k_SpaceKey) == 0 ? k_DefaultSpace : EditorPrefs.GetFloat(k_SpaceKey);
            m_Time = EditorPrefs.GetFloat(k_KeyToTime) == 0 ? k_DefaultTime : EditorPrefs.GetFloat(k_KeyToTime);
            m_Angle = EditorPrefs.GetFloat(k_AngleKey) == 0 ? k_DefaultAngle : EditorPrefs.GetFloat(k_AngleKey);
            m_AggregateTime = EditorPrefs.GetBool(k_AggregateTimeKey);
            m_AggregateAngle = EditorPrefs.GetBool(k_AggregateAngleKey);
            m_AggregateDevices = EditorPrefs.GetBool(k_AggregateDevicesKey);
            m_RemapColor = EditorPrefs.GetBool(k_RemapColorKey);
            m_RemapColorField = EditorPrefs.GetString(k_RemapColorFieldKey);
            m_RemapOptionIndex = EditorPrefs.GetInt(k_RemapOptionIndexKey);

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
            m_ArbitraryFields = new List<string>(arbitraryFieldsList);

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
            m_Events = new List<string>(eventsList);
        }

        public static AggregationInspector Init(RawEventClient client, HeatmapAggregator aggregator)
        {
            return new AggregationInspector(client, aggregator);
        }

        public void SystemReset()
        {
            //TODO
        }

        public void Fetch(AggregationHandler handler, bool localOnly)
        {
            m_AggregationHandler = handler;

            EditorPrefs.SetString(k_UrlKey, m_RawDataPath);
            DateTime start, end;
            try
            {
                start = DateTime.Parse(m_StartDate).ToUniversalTime();
            }
            catch
            {
                throw new Exception("The start date is not properly formatted. Correct format is YYYY-MM-DD.");
            }
            try
            {
                // Add one day to include the whole of that day
                end = DateTime.Parse(m_EndDate).ToUniversalTime().Add(new TimeSpan(24, 0, 0));
            }
            catch
            {
                throw new Exception("The end date is not properly formatted. Correct format is YYYY-MM-DD.");
            }

            m_RawEventClient.Fetch(m_RawDataPath, localOnly, new UnityAnalyticsEventType[]{ UnityAnalyticsEventType.custom }, start, end, rawFetchHandler);
        }

        public void OnGUI()
        {
            string oldPath = m_RawDataPath;
            m_RawDataPath = EditorGUILayout.TextField(new GUIContent("Data Export URL", "Copy the URL from the 'Editing Project' page of your project dashboard"), m_RawDataPath);
            if (oldPath != m_RawDataPath && !string.IsNullOrEmpty(m_RawDataPath))
            {
                EditorPrefs.SetString(k_UrlKey, m_RawDataPath);
            }
            bool oldUseCustomDataPath = m_UseCustomDataPath;
            m_UseCustomDataPath = EditorGUILayout.Toggle(new GUIContent("Use custom data path", "By default, will use Application.persistentDataPath"), m_UseCustomDataPath);
            if (oldUseCustomDataPath != m_UseCustomDataPath)
            {
                EditorPrefs.SetBool(k_UseCustomDataPathKey, m_UseCustomDataPath);
            }

            if (!m_UseCustomDataPath)
            {
                m_DataPath = Application.persistentDataPath;
            }
            else
            {
                string oldDataPath = m_DataPath;
                m_DataPath = EditorGUILayout.TextField(new GUIContent("Save to path", "Where to save and retrieve data (defaults to Application.persistentDataPath"), m_DataPath);
                if (string.IsNullOrEmpty(m_DataPath))
                {
                    m_DataPath = Application.persistentDataPath;
                }
                if (oldDataPath != m_DataPath )
                {
                    EditorPrefs.SetString(k_DataPathKey, m_DataPath);
                }
            }
            m_Aggregator.SetDataPath(m_DataPath);
            m_RawEventClient.SetDataPath(m_DataPath);

            m_StartDate = EditorGUILayout.TextField(new GUIContent("Start Date (YYYY-MM-DD)", "Start date as ISO-8601 datetime"), m_StartDate);
            m_EndDate = EditorGUILayout.TextField(new GUIContent("End Date (YYYY-MM-DD)", "End date as ISO-8601 datetime"), m_EndDate);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Aggregate", EditorStyles.boldLabel);

            float oldSpace = m_Space;
            m_Space = EditorGUILayout.FloatField(new GUIContent("Space Smooth", "Divider to smooth out x/y/z data"), m_Space);
            if (oldSpace != m_Space)
            {
                EditorPrefs.SetFloat(k_SpaceKey, m_Space);
            }

            bool oldAggregateTime = m_AggregateTime;
            m_AggregateTime = EditorGUILayout.Toggle(new GUIContent("Time", "Units of space will aggregate, but units of time won't"), m_AggregateTime);
            if (oldAggregateTime != m_AggregateTime)
            {
                EditorPrefs.SetBool(k_AggregateTimeKey, m_AggregateTime);
            }
            if (!m_AggregateTime)
            {
                float oldTime = m_Time;
                m_Time = EditorGUILayout.FloatField(new GUIContent("Time Smooth", "Divider to smooth out time data"), m_Time);
                if (oldTime != m_Time)
                {
                    EditorPrefs.SetFloat(k_KeyToTime, m_Time);
                }
            }
            else
            {
                m_Time = 1f;
            }

            bool oldAggregateAngle = m_AggregateAngle;
            m_AggregateAngle = EditorGUILayout.Toggle(new GUIContent("Direction", "Units of space will aggregate, but different angles won't"), m_AggregateAngle);
            if (oldAggregateAngle != m_AggregateAngle)
            {
                EditorPrefs.SetBool(k_AggregateAngleKey, m_AggregateAngle);
            }
            if (!m_AggregateAngle)
            {
                float oldAngle = m_Angle;
                m_Angle = EditorGUILayout.FloatField(new GUIContent("Angle Smooth", "Divider to smooth out angle data"), m_Angle);
                if (oldAngle != m_Angle)
                {
                    EditorPrefs.SetFloat(k_AngleKey, m_Angle);
                }
            }
            else
            {
                m_Angle = 1f;
            }

            bool oldAggregateDevices = m_AggregateDevices;
            m_AggregateDevices = EditorGUILayout.Toggle(new GUIContent("Devices", "Separate each device into its own list. NOTE: Disaggregating device IDs can be slow!"), m_AggregateDevices);
            if (oldAggregateDevices != m_AggregateDevices)
            {
                EditorPrefs.SetBool(k_AggregateDevicesKey, m_AggregateDevices);
            }
            EditorGUILayout.EndVertical();



            bool oldRemapColor = m_RemapColor;
            m_RemapColor = EditorGUILayout.Toggle(new GUIContent("Remap color to field", "By default, heatmap color is determined by event density. Checking this box allows you to remap to a specific field (e.g., use to identify fps drops.)"), m_RemapColor);
            if (oldRemapColor != m_RemapColor)
            {
                EditorPrefs.SetBool(k_RemapColorKey, m_RemapColor);
            }
            if (m_RemapColor)
            {
                string oldRemapField = m_RemapColorField;
                int oldOptionIndex = m_RemapOptionIndex;
                m_RemapColorField = EditorGUILayout.TextField(new GUIContent("Remap to","Name the field to remap"), m_RemapColorField);
                m_RemapOptionIndex = EditorGUILayout.Popup(new GUIContent("Remap operation", "How should the remapped variable aggregate?"), m_RemapOptionIndex, m_RemapOptions);
                if (oldRemapField != m_RemapColorField)
                {
                    EditorPrefs.SetString(k_RemapColorFieldKey, m_RemapColorField);
                }
                if (oldOptionIndex != m_RemapOptionIndex)
                {
                    EditorPrefs.SetInt(k_RemapOptionIndexKey, m_RemapOptionIndex);
                }
            }

            string oldArbitraryFieldsString = string.Join("|", m_ArbitraryFields.ToArray());
            if (GUILayout.Button(new GUIContent("Separate on Field", "Specify arbitrary fields with which to bucket data.")))
            {
                m_ArbitraryFields.Add("Field name");
            }
            for (var a = 0; a < m_ArbitraryFields.Count; a++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                {
                    m_ArbitraryFields.RemoveAt(a);
                    break;
                }
                m_ArbitraryFields[a] = EditorGUILayout.TextField(m_ArbitraryFields[a]);
                GUILayout.EndHorizontal();
            }
            string currentArbitraryFieldsString = string.Join("|", m_ArbitraryFields.ToArray());

            if (oldArbitraryFieldsString != currentArbitraryFieldsString)
            {
                EditorPrefs.SetString(k_ArbitraryFieldsKey, currentArbitraryFieldsString);
            }

            string oldEventsString = string.Join("|", m_Events.ToArray());
            if (GUILayout.Button(new GUIContent("Add Whitelist Event", "Specify event names to include in the aggregation. By default all events are included.")))
            {
                m_Events.Add("Event name");
            }
            for (var a = 0; a < m_Events.Count; a++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                {
                    m_Events.RemoveAt(a);
                    break;
                }
                m_Events[a] = EditorGUILayout.TextField(m_Events[a]);
                GUILayout.EndHorizontal();
            }
            string currentEventsString = string.Join("|", m_Events.ToArray());

            if (oldEventsString != currentEventsString)
            {
                EditorPrefs.SetString(k_EventsKey, currentEventsString);
            }
        }

        void rawFetchHandler(List<string> fileList)
        {
            if (fileList.Count == 0)
            {
                Debug.LogWarning("No matching data found.");
            }
            else
            {
                DateTime start, end;
                try
                {
                    start = DateTime.Parse(m_StartDate).ToUniversalTime();
                }
                catch
                {
                    start = DateTime.Parse("2000-01-01").ToUniversalTime();
                }
                try
                {
                    end = DateTime.Parse(m_EndDate).ToUniversalTime().Add(new TimeSpan(24,0,0));
                }
                catch
                {
                    end = DateTime.UtcNow;
                }
                if (m_RemapColor && string.IsNullOrEmpty(m_RemapColorField))
                {
                    Debug.LogWarning("You have selected 'Remap color to field' but haven't specified a field name. No remapping can occur.");
                }

                var aggregateOn = new List<string>(){ "x", "y", "z", "t", "rx", "ry", "rz", "dx", "dy", "dz", "z" };

                // Specify smoothing properties (must be a subset of aggregateOn)
                var smoothOn = new Dictionary<string, float>();
                // Always smooth on space
                smoothOn.Add("x", m_Space);
                smoothOn.Add("y", m_Space);
                smoothOn.Add("z", m_Space);
                smoothOn.Add("dx", m_Space);
                smoothOn.Add("dy", m_Space);
                smoothOn.Add("dz", m_Space);
                // Time is optional
                if (!m_AggregateTime)
                {
                    smoothOn.Add("t", m_Time);
                }
                // Angle is optional
                if (!m_AggregateAngle)
                {
                    smoothOn.Add("rx", m_Angle);
                    smoothOn.Add("ry", m_Angle);
                    smoothOn.Add("rz", m_Angle);
                }

                string remapToField = m_RemapColor ? m_RemapColorField : "";
                int remapOption = m_RemapColor ? m_RemapOptionIndex : 0;

                // Specify groupings
                // Always group on eventName
                var groupOn = new List<string>(){ "eventName" };
                // deviceID is optional
                if (!m_AggregateDevices)
                {
                    aggregateOn.Add("deviceID");
                    groupOn.Add("deviceID");
                }
                // Arbitrary Fields are included if specified
                if (m_ArbitraryFields != null && m_ArbitraryFields.Count > 0)
                {
                    aggregateOn.AddRange(m_ArbitraryFields);
                    groupOn.AddRange(m_ArbitraryFields);
                }

                m_Aggregator.Process(aggregationHandler, fileList, start, end,
                    aggregateOn, smoothOn, groupOn,
                    remapToField, m_RemapOptionIds[remapOption], m_Events);
            }
        }

        void aggregationHandler(string jsonPath)
        {
            m_AggregationHandler(jsonPath);
        }
    }
}
