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
using System.Linq;

namespace UnityAnalyticsHeatmap
{
    public class AggregationInspector
    {
        const string k_UrlKey = "UnityAnalyticsHeatmapDataExportUrlKey";
        const string k_DataPathKey = "UnityAnalyticsHeatmapDataPathKey";
        const string k_UseCustomDataPathKey = "UnityAnalyticsHeatmapUsePersistentDataPathKey";

        const string k_SpaceKey = "UnityAnalyticsHeatmapAggregationSpace";
        const string k_KeyToTime = "UnityAnalyticsHeatmapAggregationTime";
        const string k_RotationKey = "UnityAnalyticsHeatmapAggregationRotation";
        const string k_SmoothSpaceKey = "UnityAnalyticsHeatmapAggregationAggregateSpace";
        const string k_SmoothTimeKey = "UnityAnalyticsHeatmapAggregationAggregateTime";
        const string k_SmoothRotationKey = "UnityAnalyticsHeatmapAggregationAggregateRotation";

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
        const string k_PercentileKey = "UnityAnalyticsHeatmapRemapPercentileKey";

        const float k_DefaultSpace = 10f;
        const float k_DefaultTime = 10f;
        const float k_DefaultRotation = 15f;

        string m_DataPath = "";
        bool m_UseCustomDataPath = true;

        Dictionary<string, HeatPoint[]> m_HeatData;

        public delegate void AggregationHandler(string jsonPath);

        AggregationHandler m_AggregationHandler;

        HeatmapAggregator m_Aggregator;



        private GUIContent m_UseCustomDataPathContent = new GUIContent("Use custom data path", "By default, will use Application.persistentDataPath");
        private GUIContent m_DataPathContent = new GUIContent("Input path", "Where to retrieve data (defaults to Application.persistentDataPath");
        private GUIContent m_DatesContent = new GUIContent("Dates", "ISO-8601 datetimes (YYYY-MM-DD)");
        private GUIContent m_SeparateUsersContent = new GUIContent("Users", "Separate each user into their own list. NOTE: Separating user IDs can be quite slow!");
        private GUIContent m_SeparateSessionsContent = new GUIContent("Sessions", "Separate each session into its own list. NOTE: Separating unique sessions can be astonishingly slow!");
        private GUIContent m_SeparateDebugContent = new GUIContent("Is Debug", "Separate debug devices from non-debug devices");
        private GUIContent m_SeparatePlatformContent = new GUIContent("Platform", "Separate data based on platform");
        private GUIContent m_SeparateCustomFieldContent = new GUIContent("On Custom Field", "Separate based on one or more parameter fields");

        private GUIContent m_RemapColorContent = new GUIContent("Remap color to field", "By default, heatmap color is determined by event density. Checking this box allows you to remap to a specific field (e.g., use to identify fps drops.)");
        private GUIContent m_RemapColorFieldContent = new GUIContent("Field","Name the field to remap");
        private GUIContent m_RemapOptionIndexContent = new GUIContent("Remap operation", "How should the remapped variable aggregate?");
        private GUIContent m_PercentileContent = new GUIContent("Percentile", "A value between 0 and 100");

        string m_StartDate = "";
        string m_EndDate = "";
        bool m_ValidDates = true;

        GUIStyle m_ValidDateStyle;
        GUIStyle m_InvalidDateStyle;


        float m_Space = k_DefaultSpace;
        float m_Time = k_DefaultTime;
        float m_Rotation = k_DefaultRotation;

        public const int SMOOTH_VALUE = 0;
        public const int SMOOTH_NONE = 1;
        public const int SMOOTH_UNION = 2;

        int m_SmoothSpaceToggle = SMOOTH_VALUE;
        int m_SmoothTimeToggle = SMOOTH_UNION;
        int m_SmoothRotationToggle = SMOOTH_UNION;

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
            new GUIContent("Min"),
            new GUIContent("Max"),
            new GUIContent("First"),
            new GUIContent("Last"),
            new GUIContent("Percentile")
        };
        AggregationMethod[] m_RemapOptionIds = new AggregationMethod[]{
            AggregationMethod.Increment,
            AggregationMethod.Cumulative,
            AggregationMethod.Average,
            AggregationMethod.Min,
            AggregationMethod.Max,
            AggregationMethod.First,
            AggregationMethod.Last,
            AggregationMethod.Percentile
        };

        List<string> m_ArbitraryFields = new List<string>{ };

        public AggregationInspector(HeatmapAggregator aggregator)
        {
            m_Aggregator = aggregator;

            // Restore cached paths
            m_UseCustomDataPath = EditorPrefs.GetBool(k_UseCustomDataPathKey);
            m_DataPath = EditorPrefs.GetString(k_DataPathKey);

            // Set dates based on today (should this be cached?)
            m_EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.UtcNow);
            m_StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.UtcNow.Subtract(new TimeSpan(5, 0, 0, 0)));

            // Restore other options
            m_Space = EditorPrefs.GetFloat(k_SpaceKey) == 0 ? k_DefaultSpace : EditorPrefs.GetFloat(k_SpaceKey);
            m_Time = EditorPrefs.GetFloat(k_KeyToTime) == 0 ? k_DefaultTime : EditorPrefs.GetFloat(k_KeyToTime);
            m_Rotation = EditorPrefs.GetFloat(k_RotationKey) == 0 ? k_DefaultRotation : EditorPrefs.GetFloat(k_RotationKey);
            m_SmoothSpaceToggle = EditorPrefs.GetInt(k_SmoothSpaceKey);
            m_SmoothTimeToggle = EditorPrefs.GetInt(k_SmoothTimeKey);
            m_SmoothRotationToggle = EditorPrefs.GetInt(k_SmoothRotationKey);
            m_SeparateUsers = EditorPrefs.GetBool(k_SeparateUsersKey);
            m_RemapColor = EditorPrefs.GetBool(k_RemapColorKey);
            m_RemapColorField = EditorPrefs.GetString(k_RemapColorFieldKey);
            m_RemapOptionIndex = EditorPrefs.GetInt(k_RemapOptionIndexKey);
            m_Percentile = EditorPrefs.GetFloat(k_PercentileKey);

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

        public static AggregationInspector Init(HeatmapAggregator aggregator)
        {
            return new AggregationInspector(aggregator);
        }

        public void SystemReset()
        {
            //TODO
        }

        public void Fetch(AggregationHandler handler, bool localOnly)
        {
            m_AggregationHandler = handler;
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

            RawDataClient.GetInstance().m_DataPath = m_DataPath;
            var fileList = RawDataClient.GetInstance().GetFiles(new UnityAnalyticsEventType[]{ UnityAnalyticsEventType.custom }, start, end);
            ProcessAggregation(fileList);
        }

        public void OnGUI()
        {
            if (m_ValidDateStyle == null)
            {
                m_ValidDateStyle = new GUIStyle("box");
                m_InvalidDateStyle = new GUIStyle("box");
                m_InvalidDateStyle.normal.textColor = Color.red;
            }

            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.HorizontalScope())
                {
                    m_UseCustomDataPath = EditorGUIBinding.Toggle(m_UseCustomDataPathContent, m_UseCustomDataPath, UseCustomDataPathChange);
                    if (GUILayout.Button("Open Folder"))
                    {
                        EditorUtility.RevealInFinder(m_DataPath);
                    }
                }
                if (m_UseCustomDataPath)
                {
                    m_DataPath = EditorGUIBinding.TextField(m_DataPathContent, m_DataPath, DataPathChange);
                }

                EditorGUILayout.LabelField(m_DatesContent, EditorStyles.boldLabel, GUILayout.Width(35));
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUIStyle dateFieldStyle = m_ValidDates ? m_ValidDateStyle : m_InvalidDateStyle;
                    m_StartDate = AnalyticsDatePicker.DatePicker(m_StartDate, dateFieldStyle, StartDateChange, DateFailure, DateValidationStart);
                    EditorGUILayout.LabelField("-", GUILayout.Width(10));
                    m_EndDate = AnalyticsDatePicker.DatePicker(m_EndDate, dateFieldStyle, EndDateChange, DateFailure, DateValidationEnd);
                }
            }

            // SMOOTHERS (SPACE, ROTATION, TIME)
            using (new GUILayout.VerticalScope())
            {
                GUILayout.Label("Smooth/Unionize", EditorStyles.boldLabel);
                using (new GUILayout.HorizontalScope())
                {
                    // SPACE
                    AnalyticsSmootherControl.SmootherControl(ref m_SmoothSpaceToggle, ref m_Space, "Space", "Divider to smooth out x/y/z data", k_SmoothSpaceKey, k_SpaceKey, 2);
                    // ROTATION
                    AnalyticsSmootherControl.SmootherControl(ref m_SmoothRotationToggle, ref m_Rotation, "Rotation", "Divider to smooth out angular data", k_SmoothRotationKey, k_RotationKey);
                    // TIME
                    AnalyticsSmootherControl.SmootherControl(ref m_SmoothTimeToggle, ref m_Time, "Time", "Divider to smooth out passage of game time", k_SmoothTimeKey, k_KeyToTime);
                }
            }

            // SEPARATION
            GUILayout.Label("Separate", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                m_SeparateUsers = EditorGUIBinding.Toggle(m_SeparateUsersContent, m_SeparateUsers, SeparateUsersChange);
                m_SeparateSessions = EditorGUIBinding.Toggle(m_SeparateSessionsContent, m_SeparateSessions, SeparateSessionsChange);
            }
            using (new GUILayout.HorizontalScope())
            {
                m_SeparateDebug = EditorGUIBinding.Toggle(m_SeparateDebugContent, m_SeparateDebug, SeparateDebugChange);
                m_SeparatePlatform = EditorGUIBinding.Toggle(m_SeparatePlatformContent, m_SeparatePlatform, SeparatePlatformChange);
            }
            m_SeparateCustomField = EditorGUIBinding.Toggle(m_SeparateCustomFieldContent, m_SeparateCustomField, SeparateCustomFieldChange);

            if (m_SeparateCustomField)
            {
                m_ArbitraryFields = AnalyticsTextFieldList.TextFieldList(m_ArbitraryFields, CustomFieldsChange);
            }

            // COLOR REMAPPING
            using (new GUILayout.VerticalScope("box"))
            {
                m_RemapColor = EditorGUIBinding.Toggle(m_RemapColorContent, m_RemapColor, RemapChange);
                if (m_RemapColor)
                {
                    m_RemapColorField = EditorGUIBinding.TextField(m_RemapColorFieldContent, m_RemapColorField, RemapFieldChange);
                    m_RemapOptionIndex = EditorGUIBinding.Popup(m_RemapOptionIndexContent, m_RemapOptionIndex, m_RemapOptions, RemapOptionIndexChange);
                    if (m_RemapOptionIds[m_RemapOptionIndex] == AggregationMethod.Percentile)
                    {
                        m_Percentile = EditorGUIBinding.FloatField(m_PercentileContent, m_Percentile, PercentileChange);
                        m_Percentile = Mathf.Clamp(m_Percentile, 0f, 100f);
                    }
                }
            }
        }


        void ProcessAggregation(List<string> fileList)
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

                // When these are the same, points where these values match will be aggregated to the same point
                var aggregateOn = new List<string>(){ "x", "y", "z", "t", "rx", "ry", "rz", "dx", "dy", "dz", "z" };
                // Specify groupings for unique lists
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

                // Specify smoothing properties (must be a subset of aggregateOn)
                var smoothOn = new Dictionary<string, float>();
                // Smooth space
                if (m_SmoothSpaceToggle == SMOOTH_VALUE || m_SmoothSpaceToggle == SMOOTH_NONE)
                {
                    float spaceSmoothValue = (m_SmoothSpaceToggle == SMOOTH_NONE) ? 0f : m_Space;
                    smoothOn.Add("x", spaceSmoothValue);
                    smoothOn.Add("y", spaceSmoothValue);
                    smoothOn.Add("z", spaceSmoothValue);
                    smoothOn.Add("dx", spaceSmoothValue);
                    smoothOn.Add("dy", spaceSmoothValue);
                    smoothOn.Add("dz", spaceSmoothValue);
                }
                // Smooth rotation
                if (m_SmoothRotationToggle == SMOOTH_VALUE || m_SmoothRotationToggle == SMOOTH_NONE)
                {
                    float rotationSmoothValue = (m_SmoothRotationToggle == SMOOTH_NONE) ? 0f : m_Rotation;
                    smoothOn.Add("rx", rotationSmoothValue);
                    smoothOn.Add("ry", rotationSmoothValue);
                    smoothOn.Add("rz", rotationSmoothValue);
                }
                // Smooth time
                if (m_SmoothTimeToggle == SMOOTH_VALUE || m_SmoothTimeToggle == SMOOTH_NONE)
                {
                    float timeSmoothValue = (m_SmoothTimeToggle == SMOOTH_NONE) ? 0f : m_Time;
                    smoothOn.Add("t", timeSmoothValue);
                }

                string remapToField = m_RemapColor ? m_RemapColorField : "";
                int remapOption = m_RemapColor ? m_RemapOptionIndex : 0;

                m_Aggregator.Process(aggregationHandler, fileList, start, end,
                    aggregateOn, smoothOn, groupOn,
                    remapToField, m_RemapOptionIds[remapOption], m_Percentile);
            }
        }

        void aggregationHandler(string jsonPath)
        {
            m_AggregationHandler(jsonPath);
        }

        #region change handlers
        void UseCustomDataPathChange(bool value)
        {
            EditorPrefs.SetBool(k_UseCustomDataPathKey, value);
            DataPathChange(m_DataPath);
        }

        void DataPathChange(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                m_DataPath = Application.persistentDataPath;
            }
            EditorPrefs.SetString(k_DataPathKey, value);
            m_Aggregator.SetDataPath(m_DataPath);
        }

        void StartDateChange(string value)
        {
            m_ValidDates = true;
        }

        void EndDateChange(string value)
        {
            m_ValidDates = true;
        }

        void DateFailure()
        {
            m_ValidDates = false;
        }

        bool DateValidationStart(string value)
        {
            return DateValidation(value, m_EndDate);
        }

        bool DateValidationEnd(string value)
        {
            return DateValidation(m_StartDate, value);
        }

        bool DateValidation(string start, string end)
        {
            DateTime startDate;
            DateTime endDate;
            try
            {
                startDate = DateTime.Parse(start);
                endDate = DateTime.Parse(end);
            }
            catch
            {
                return false;
            }
            var now = DateTime.UtcNow;
            var today = new DateTime(now.Year, now.Month, now.Day + 1);
            return startDate < endDate && endDate <= today;
        }

        void SeparateUsersChange(bool value)
        {
            EditorPrefs.SetBool(k_SeparateUsersKey, value);
        }

        void SeparateSessionsChange(bool value)
        {
            EditorPrefs.SetBool(k_SeparateSessionKey, value);
        }

        void SeparateDebugChange(bool value)
        {
            EditorPrefs.SetBool(k_SeparateDebugKey, value);
        }

        void SeparatePlatformChange(bool value)
        {
            EditorPrefs.SetBool(k_SeparatePlatformKey, value);
        }

        void SeparateCustomFieldChange(bool value)
        {
            EditorPrefs.SetBool(k_SeparateCustomKey, value);
        }

        void CustomFieldsChange(List<string> list)
        {
            string currentArbitraryFieldsString = string.Join("|", list.ToArray());
            EditorPrefs.SetString(k_ArbitraryFieldsKey, currentArbitraryFieldsString);
        }

        void RemapChange(bool value)
        {
            EditorPrefs.SetBool(k_RemapColorKey, value);
        }

        void RemapFieldChange(string value)
        {
            EditorPrefs.SetString(k_RemapColorFieldKey, value);
        }

        void RemapOptionIndexChange(int value)
        {
            EditorPrefs.SetInt(k_RemapOptionIndexKey, value);
        }

        void PercentileChange(float value)
        {
            EditorPrefs.SetFloat(k_PercentileKey, m_Percentile);
        }

        #endregion
    }
}