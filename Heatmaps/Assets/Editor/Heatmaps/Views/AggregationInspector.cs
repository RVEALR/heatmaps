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
        const string k_UseCustomDataPathKey = "UnityAnalyticsHeatmapUsePersistentDataPathKey";


        string m_DataPath = "";
        bool m_UseCustomDataPath = true;

        HeatmapDataProcessor m_Processor;
        HeatmapInspectorViewModel m_ViewModel;

        private GUIContent m_UseCustomDataPathContent = new GUIContent("Use custom data path", "By default, will use Application.persistentDataPath");
        private GUIContent m_DataPathContent = new GUIContent("Input path", "Where to retrieve data (defaults to Application.persistentDataPath");
        private GUIContent m_DatesContent = new GUIContent("Dates", "ISO-8601 datetimes (YYYY-MM-DD)");
        private GUIContent m_SeparateUsersContent = new GUIContent("Users", "Separate each user into their own list. NOTE: Separating user IDs can be quite slow!");
        private GUIContent m_SeparateSessionsContent = new GUIContent("Sessions", "Separate each session into its own list. NOTE: Separating unique sessions can be astonishingly slow!");
        private GUIContent m_SeparateDebugContent = new GUIContent("Is debug", "Separate debug devices from non-debug devices");
        private GUIContent m_SeparatePlatformContent = new GUIContent("Platform", "Separate data based on platform");
        private GUIContent m_SeparateCustomFieldContent = new GUIContent("On custom field", "Separate based on one or more parameter fields");

        private GUIContent m_RemapColorContent = new GUIContent("Color to field", "By default, heatmap color is determined by event density. Checking this box allows you to remap to a specific field (e.g., use to identify fps drops.)");
        private GUIContent m_RemapColorFieldContent = new GUIContent("Field","Name the field to remap");
        private GUIContent m_RemapOptionIndexContent = new GUIContent("Remap operation", "How should the remapped variable aggregate?");
        private GUIContent m_PercentileContent = new GUIContent("Percentile", "A value between 0 and 100");

        string m_StartDate = "";
        string m_EndDate = "";
        bool m_ValidDates = true;

        GUIStyle m_ValidDateStyle;
        GUIStyle m_InvalidDateStyle;


        float m_Space;
        float m_Time;
        float m_Rotation;

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

        List<string> m_ArbitraryFields = new List<string>{ };

        int m_FetchCount = 0;
        const int k_FetchInitValue = 30;

        public AggregationInspector(HeatmapDataProcessor processor)
        {
            m_Processor = processor;
            m_ViewModel = HeatmapInspectorViewModel.GetInstance();
        }

        public static AggregationInspector Init(HeatmapDataProcessor processor)
        {
            return new AggregationInspector(processor);
        }

        public void SystemReset()
        {
            //TODO
        }

        public void OnEnable()
        {
            // Restore cached paths
            m_UseCustomDataPath = EditorPrefs.GetBool(k_UseCustomDataPathKey);

            m_DataPath = m_Processor.m_RawDataPath;
            m_EndDate = m_Processor.m_EndDate;
            m_StartDate = m_Processor.m_StartDate;

            m_SeparatePlatform = m_Processor.m_SeparatePlatform;
            m_SeparateDebug = m_Processor.m_SeparateDebug;
            m_SeparateSessions = m_Processor.m_SeparateSessions;
            m_SeparateCustomField = m_Processor.m_SeparateCustomField;

            m_RemapColor = m_ViewModel.remapDensity;
            m_RemapColorField = m_ViewModel.remapColorField;
            m_RemapOptionIndex = m_ViewModel.remapOptionIndex;
            m_Percentile = m_ViewModel.remapPercentile;
            m_ArbitraryFields = m_Processor.m_SeparationFields;
        }

        public void Update()
        {
            HandleFetch();
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
                    AnalyticsSmootherControl.SmootherControl(m_ViewModel.smoothSpaceOption,
                        m_ViewModel.smoothSpace, "Space", "Divider to smooth out x/y/z data", SpaceChange, 2);
                    // ROTATION
                    AnalyticsSmootherControl.SmootherControl(m_ViewModel.smoothRotationOption,
                        m_ViewModel.smoothRotation, "Rotation", "Divider to smooth out angular data", RotationChange);
                    // TIME
                    AnalyticsSmootherControl.SmootherControl(m_ViewModel.smoothTimeOption,
                        m_ViewModel.smoothTime,
                        "Time", "Divider to smooth out passage of game time", TimeChange);
                }
            }

            // SEPARATION
            GUILayout.Label("Separate", EditorStyles.boldLabel);
            using (new GUILayout.HorizontalScope())
            {
                m_ViewModel.separateUsers = EditorGUIBinding.Toggle(m_SeparateUsersContent, m_ViewModel.separateUsers, SeparateUsersChange);
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
            GUILayout.Label("Remap", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope())
            {
                m_RemapColor = EditorGUIBinding.Toggle(m_RemapColorContent, m_RemapColor, RemapChange);
                if (m_RemapColor)
                {
                    m_RemapColorField = EditorGUIBinding.TextField(m_RemapColorFieldContent, m_RemapColorField, RemapFieldChange);
                    m_RemapOptionIndex = EditorGUIBinding.Popup(m_RemapOptionIndexContent, m_RemapOptionIndex, m_RemapOptions, RemapOptionIndexChange);
                    if (HeatmapDataProcessor.m_RemapOptionIds[m_RemapOptionIndex] == AggregationMethod.Percentile)
                    {
                        m_Percentile = EditorGUIBinding.FloatField(m_PercentileContent, m_Percentile, PercentileChange);
                        m_Percentile = Mathf.Clamp(m_Percentile, 0f, 100f);
                    }
                }
            }
        }

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
            m_Processor.m_RawDataPath = value;
            ScheduleFetch();
        }

        void StartDateChange(string value)
        {
            m_ValidDates = true;
            m_Processor.m_StartDate = value;
            ScheduleFetch();
        }

        void EndDateChange(string value)
        {
            m_ValidDates = true;
            m_Processor.m_EndDate = value;
            ScheduleFetch();
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
            // Midnight tonight
            var today = DateTime.Today.AddDays(1).ToUniversalTime();
            return startDate < endDate && endDate <= today;
        }

        void SpaceChange(int option, float value)
        {
            m_ViewModel.smoothSpaceOption = option;
            m_ViewModel.smoothSpace = value;
            ScheduleFetch();
        }

        void RotationChange(int option, float value)
        {
            m_ViewModel.smoothRotationOption = option;
            m_ViewModel.smoothRotation = value;
            ScheduleFetch();
        }

        void TimeChange(int option, float value)
        {
            m_ViewModel.smoothTimeOption = option;
            m_ViewModel.smoothTime = value;
            ScheduleFetch();
        }

        void SeparateUsersChange(bool value)
        {
            m_ViewModel.separateUsers = value;
            ScheduleFetch();
        }

        void SeparateSessionsChange(bool value)
        {
            m_Processor.m_SeparateSessions = value;
            ScheduleFetch();
        }

        void SeparateDebugChange(bool value)
        {
            m_Processor.m_SeparateDebug = value;
            ScheduleFetch();
        }

        void SeparatePlatformChange(bool value)
        {
            m_Processor.m_SeparatePlatform = value;
            ScheduleFetch();
        }

        void SeparateCustomFieldChange(bool value)
        {
            m_Processor.m_SeparateCustomField = value;
            ScheduleFetch();
        }

        void CustomFieldsChange(List<string> list)
        {
            m_Processor.m_SeparationFields = list;
            ScheduleFetch();
        }

        void RemapChange(bool value)
        {
            m_ViewModel.remapDensity = value;
            ScheduleFetch();
        }

        void RemapFieldChange(string value)
        {
            m_ViewModel.remapColorField = value;
            ScheduleFetch();
        }

        void RemapOptionIndexChange(int value)
        {
            m_ViewModel.remapOptionIndex = value;
            ScheduleFetch();
        }

        void PercentileChange(float value)
        {
            m_ViewModel.remapPercentile = m_Percentile;
            ScheduleFetch();
        }


        void ScheduleFetch()
        {
            m_FetchCount = k_FetchInitValue;
        }

        void HandleFetch()
        {
            if (m_FetchCount > 0)
            {
                m_FetchCount --;
                if (m_FetchCount == 0)
                {
                    CommitFetch();
                }
            }
        }

        void CommitFetch()
        {
            m_Processor.Fetch();
        }
    }
}