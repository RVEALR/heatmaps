/// <summary>
/// Inspector for the Aggregation portion of the Heatmapper.
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityAnalytics;

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

        const string k_SeparateUsersKey = "UnityAnalyticsHeatmapAggregationAggregateUserIDs";
        const string k_SeparateSessionKey = "UnityAnalyticsHeatmapAggregationAggregateSessionIDs";
        const string k_SeparateDebugKey = "UnityAnalyticsHeatmapAggregationAggregateDebug";
        const string k_SeparatePlatformKey = "UnityAnalyticsHeatmapAggregationAggregatePlatform";
        const string k_SeparateCustomKey = "UnityAnalyticsHeatmapAggregationAggregateCustom";

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

        Texture2D unionIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/union.png") as Texture2D;
        Texture2D numberIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/number.png") as Texture2D;
        Texture2D noneIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/none.png") as Texture2D;

        string m_StartDate = "";
        string m_EndDate = "";
        float m_Space = k_DefaultSpace;
        float m_Time = k_DefaultTime;
        float m_Angle = k_DefaultAngle;
        bool m_AggregateTime = true;
        bool m_AggregateAngle = true;

        bool m_SeparateUsers = false;
        bool m_SeparateSessions = false;
        bool m_SeparatePlatform = false;
        bool m_SeparateDebug = false;
        bool m_SeparateCustomField = false;

        bool m_RemapColor;
        string m_RemapColorField = "";
        int m_RemapOptionIndex = 0;
        float m_Percentile = 50f;
        GUIContent[] m_RemapOptions = new GUIContent[]{
            new GUIContent("Increment"),
            new GUIContent("Cumulative"),
            new GUIContent("Average"),
            new GUIContent("First Wins"),
            new GUIContent("Last Wins"),
            new GUIContent("Min Wins"),
            new GUIContent("Max Wins"),
            new GUIContent("Percentile")
        };
        AggregationMethod[] m_RemapOptionIds = new AggregationMethod[]{
            AggregationMethod.Increment,
            AggregationMethod.Cumulative,
            AggregationMethod.Average,
            AggregationMethod.FirstWins,
            AggregationMethod.LastWins,
            AggregationMethod.MinWins,
            AggregationMethod.MaxWins,
            AggregationMethod.Percentile
        };

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
            m_SeparateUsers = EditorPrefs.GetBool(k_SeparateUsersKey);
            m_RemapColor = EditorPrefs.GetBool(k_RemapColorKey);
            m_RemapColorField = EditorPrefs.GetString(k_RemapColorFieldKey);
            m_RemapOptionIndex = EditorPrefs.GetInt(k_RemapOptionIndexKey);

            // Restore list of arbitrary separation fields
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

        int m_SpaceToggle = 0;
        int m_AngleToggle = 0;
        int m_TimeToggle = 0;

        public void OnGUI()
        {
            //GUIStyle stretch = new GUIStyle(GUI.skin.box);
            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            bool oldUseCustomDataPath = m_UseCustomDataPath;
            m_UseCustomDataPath = EditorGUILayout.Toggle(new GUIContent("Use custom data path", "By default, will use Application.persistentDataPath"), m_UseCustomDataPath);
            if (oldUseCustomDataPath != m_UseCustomDataPath)
            {
                EditorPrefs.SetBool(k_UseCustomDataPathKey, m_UseCustomDataPath);
            }
            if (GUILayout.Button("Open Folder"))
            {
                EditorUtility.RevealInFinder(m_DataPath);
            }
            GUILayout.EndHorizontal();

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

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Dates", "ISO-8601 datetimes (YYYY-MM-DD)"), GUILayout.Width(35));
            m_StartDate = EditorGUILayout.TextField(m_StartDate);
            EditorGUILayout.LabelField("-", GUILayout.Width(10));
            m_EndDate = EditorGUILayout.TextField(m_EndDate);
            GUILayout.EndHorizontal();


            GUILayout.BeginVertical("box");
            GUILayout.Label("Smooth", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();

            // SPACE
            SmootherControl(ref m_SpaceToggle, ref m_Space, "Space", "Divider to smooth out x/y/z data", k_SpaceKey);

            // ROTATION
            SmootherControl(ref m_AngleToggle, ref m_Angle, "Rotation", "Divider to smooth out angular data", k_SpaceKey);


            // TIME
            SmootherControl(ref m_TimeToggle, ref m_Time, "Time", "Divider to smooth out passage of game time", k_SpaceKey);

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();


            // SEPARATION
            GUILayout.Label("Separate", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GroupControl(ref m_SeparateUsers,
                "Users", "Separate each user into their own list. NOTE: Separating user IDs can be quite slow!",
                k_SeparateUsersKey);
            GroupControl(ref m_SeparateSessions,
                "Sessions", "Separate each session into its own list. NOTE: Separating unique sessions can be astonishly slow!",
                k_SeparateSessionKey);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GroupControl(ref m_SeparateDebug,
                "Is Debug", "Separate debug devices from non-debug devices",
                k_SeparateDebugKey);
            GroupControl(ref m_SeparatePlatform,
                "Platform", "Separate data based on platform",
                k_SeparatePlatformKey);
            GUILayout.EndHorizontal();


            GroupControl(ref m_SeparateCustomField,
                "On Custom Field", "Separate based on one or more parameter fields",
                k_SeparateCustomKey);


            if (m_SeparateCustomField)
            {
                string oldArbitraryFieldsString = string.Join("|", m_ArbitraryFields.ToArray());
                if (m_ArbitraryFields.Count == 0)
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
                    if (a == m_ArbitraryFields.Count-1 && GUILayout.Button(new GUIContent("+", "Add field")))
                    {
                        m_ArbitraryFields.Add("Field name");
                    }
                    GUILayout.EndHorizontal();
                }




                string currentArbitraryFieldsString = string.Join("|", m_ArbitraryFields.ToArray());

                if (oldArbitraryFieldsString != currentArbitraryFieldsString)
                {
                    EditorPrefs.SetString(k_ArbitraryFieldsKey, currentArbitraryFieldsString);
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("Box");
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
                m_RemapColorField = EditorGUILayout.TextField(new GUIContent("Field","Name the field to remap"), m_RemapColorField);
                m_RemapOptionIndex = EditorGUILayout.Popup(new GUIContent("Remap operation", "How should the remapped variable aggregate?"), m_RemapOptionIndex, m_RemapOptions);

                if (m_RemapOptionIds[m_RemapOptionIndex] == AggregationMethod.Percentile)
                {
                    m_Percentile = EditorGUILayout.FloatField("Percentile", m_Percentile);
                }
                if (oldRemapField != m_RemapColorField)
                {
                    EditorPrefs.SetString(k_RemapColorFieldKey, m_RemapColorField);
                }
                if (oldOptionIndex != m_RemapOptionIndex)
                {
                    EditorPrefs.SetInt(k_RemapOptionIndexKey, m_RemapOptionIndex);
                }
            }
            GUILayout.EndVertical();
        }

        void SmootherControl(ref int toggler, ref float value, string label, string tooltip, string key)
        {
            GUILayout.BeginVertical();
            toggler = GUILayout.Toolbar(toggler, new GUIContent[] {
                new GUIContent(unionIcon, "Union"), 
                new GUIContent(numberIcon, "Smooth to value"),
                new GUIContent(noneIcon, "No smoothing")
            }, GUILayout.MaxWidth(100));
            float oldValue = value;


            float lw = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;
            float fw = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.fieldWidth = 20;
            value = EditorGUILayout.FloatField(new GUIContent(label, tooltip), value);
            EditorGUIUtility.labelWidth = lw;
            EditorGUIUtility.fieldWidth = fw;

            if (oldValue != value)
            {
                EditorPrefs.SetFloat(key, value);
            }
            GUILayout.EndVertical();
        }

        void GroupControl(ref bool groupParam, string label, string tooltip, string key)
        {
            bool oldValue = groupParam;
            groupParam = EditorGUILayout.Toggle(new GUIContent(label, tooltip), groupParam);
            if (groupParam != oldValue)
            {
                EditorPrefs.SetBool(key, groupParam);
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
                // userID is optional
                if (m_SeparateUsers)
                {
                    aggregateOn.Add("userID");
                    groupOn.Add("userID");
                }
                if (m_SeparateSessions)
                {
                    aggregateOn.Add("sessionID");
                    groupOn.Add("sessionID");
                }
                if (m_SeparateDebug)
                {
                    aggregateOn.Add("debug");
                    groupOn.Add("debug");
                }
                if (m_SeparatePlatform)
                {
                    aggregateOn.Add("platform");
                    groupOn.Add("platform");
                }
                // Arbitrary Fields are included if specified
                if (m_SeparateCustomField)
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
