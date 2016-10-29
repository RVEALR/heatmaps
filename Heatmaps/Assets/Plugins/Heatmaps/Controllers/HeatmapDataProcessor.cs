/// <summary>
/// Manages the loading and processing of Heatmap data
/// </summary>
/// 
/// Heatmap data is loaded from GZip or text files and stored in the
/// HeatmapViewModel as a CustomRawData[]. From there, it is aggregated
/// into a HeatPoint[] before being sent to the renderer.
/// 
/// This class manages all that loading and processing, working out the
/// minimum work required to dynamically update the map.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityAnalytics;
using UnityEditor;
using System.Collections;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapDataProcessor
    {
        const string k_DataPathKey = "UnityAnalyticsHeatmapDataPathKey";
        const string k_SpaceKey = "UnityAnalyticsHeatmapAggregationSpace";
        const string k_KeyToTime = "UnityAnalyticsHeatmapAggregationTime";
        const string k_RotationKey = "UnityAnalyticsHeatmapAggregationRotation";
        const string k_SmoothSpaceKey = "UnityAnalyticsHeatmapAggregationAggregateSpace";
        const string k_SmoothTimeKey = "UnityAnalyticsHeatmapAggregationAggregateTime";
        const string k_SmoothRotationKey = "UnityAnalyticsHeatmapAggregationAggregateRotation";


        const string k_SeparateSessionKey = "UnityAnalyticsHeatmapAggregationAggregateSessionIDs";
        const string k_SeparateDebugKey = "UnityAnalyticsHeatmapAggregationAggregateDebug";
        const string k_SeparatePlatformKey = "UnityAnalyticsHeatmapAggregationAggregatePlatform";
        const string k_SeparateCustomKey = "UnityAnalyticsHeatmapAggregationAggregateCustom";

        const string k_ArbitraryFieldsKey = "UnityAnalyticsHeatmapAggregationArbitraryFields";

        const float k_DefaultSpace = 10f;
        const float k_DefaultTime = 10f;
        const float k_DefaultRotation = 15f;


        public const int SMOOTH_VALUE = 0;
        public const int SMOOTH_NONE = 1;
        public const int SMOOTH_UNION = 2;

        public HeatmapViewModel m_ViewModel;
        HeatmapInspectorViewModel m_InspectorViewModel;
        public HeatmapAggregator m_Aggregator;
        public HeatmapDataParser m_DataParser;


        bool m_DateChangeHasOccurred = true;

        private string _rawDataPath = "";
        public string m_RawDataPath
        {
            get{
                return _rawDataPath;
            }
            set{
                _rawDataPath = value;
                m_Aggregator.SetDataPath(_rawDataPath);
                EditorPrefs.SetString(k_DataPathKey, value);
            }
        }
        string _startDate = "";
        public string m_StartDate
        {
            get {
                return _startDate;
            }
            set {
                string oldDate = _startDate;
                _startDate = value;
                if (_startDate != oldDate)
                {
                    m_DateChangeHasOccurred = true;
                }
            }
        }

        string _endDate = "";
        public string m_EndDate
        {
            get {
                return _endDate;
            }
            set {
                string oldDate = _endDate;
                _endDate = value;
                if (_endDate != oldDate)
                {
                    m_DateChangeHasOccurred = true;
                }
            }
        }

        bool _separateSessions = false;
        public bool m_SeparateSessions{
            get{
                return _separateSessions;
            }
            set{
                _separateSessions = value;
                EditorPrefs.SetBool(k_SeparateSessionKey, _separateSessions);
            }
        }
        bool _separatePlatform;
        public bool m_SeparatePlatform
        {
            get {
                return _separatePlatform;
            }
            set {
                _separatePlatform = value;
                EditorPrefs.SetBool(k_SeparateSessionKey, _separatePlatform);
            }
        }
        bool _separateDebug;
        public bool m_SeparateDebug
        {
            get {
                return _separateDebug;
            }
            set {
                _separateDebug = value;
                EditorPrefs.SetBool(k_SeparateDebugKey, _separateDebug);
            }
        }
        bool _separateCustomField;
        public bool m_SeparateCustomField
        {
            get {
                return _separateCustomField;
            }
            set {
                _separateCustomField = value;
                EditorPrefs.SetBool(k_SeparateCustomKey, _separateCustomField);
            }
        }

        List<string> _separationFields = new List<string>();
        public List<string> m_SeparationFields
        {
            get {
                return _separationFields;
            }
            set {
                _separationFields = value;
                string currentArbitraryFieldsString = string.Join("|", _separationFields.ToArray());
                EditorPrefs.SetString(k_ArbitraryFieldsKey, currentArbitraryFieldsString);
            }
        }

        public delegate void AggregationHandler(string jsonPath);
        public delegate void PointHandler(HeatPoint[] heatData);


        static public AggregationMethod[] m_RemapOptionIds = new AggregationMethod[]{
            AggregationMethod.Increment,
            AggregationMethod.Cumulative,
            AggregationMethod.Average,
            AggregationMethod.Min,
            AggregationMethod.Max,
            AggregationMethod.First,
            AggregationMethod.Last,
            AggregationMethod.Percentile
        };

        public HeatmapDataProcessor()
        {
            m_ViewModel = new HeatmapViewModel();
            m_Aggregator = new HeatmapAggregator(m_RawDataPath);
            m_DataParser = new HeatmapDataParser();
            m_InspectorViewModel = HeatmapInspectorViewModel.GetInstance();
            m_InspectorViewModel.SettingsChanged += OnSettingsUpdate;
        }

        void OnSettingsUpdate(object sender, HeatmapSettings settings)
        {
            SelectList();
        }

        public void RestoreSettings()
        {
            // Restore cached paths
            m_RawDataPath = EditorPrefs.GetString(k_DataPathKey);

            // Set dates based on today (should this be cached?)
            m_EndDate = String.Format("{0:yyyy-MM-dd}", DateTime.UtcNow);
            m_StartDate = String.Format("{0:yyyy-MM-dd}", DateTime.UtcNow.Subtract(new TimeSpan(5, 0, 0, 0)));

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
            m_SeparationFields = new List<string>(arbitraryFieldsList);
        }

        /// <summary>
        /// Fetch the files within the currently specified date range.
        /// </summary>
        public void Fetch()
        {
            RawDataClient.GetInstance().m_DataPath = m_RawDataPath;
            ProcessAggregation();
        }

        void ProcessAggregation()
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

            if (m_DateChangeHasOccurred || RawDataClient.GetInstance().m_ManifestInvalidated ||
                m_ViewModel.m_RawDataFileList == null || m_ViewModel.m_RawDataFileList.Count == 0)
            {
                RawDataClient.GetInstance().m_DataPath = m_RawDataPath;
                m_ViewModel.m_RawDataFileList = RawDataClient.GetInstance().GetFiles(
                    new UnityAnalyticsEventType[]{ UnityAnalyticsEventType.custom }, start, end);
                m_DateChangeHasOccurred = false;
                if (m_ViewModel.m_RawDataFileList.Count == 0)
                {
                    return;
                }
            }

            if (m_InspectorViewModel.remapDensity && string.IsNullOrEmpty(m_InspectorViewModel.remapColorField))
            {
                Debug.LogWarning("You have selected 'Remap color to field' but haven't specified a field name. No remapping can occur.");
            }

            // When these are the same, points where these values match will be aggregated to the same point
            var aggregateOn = new List<string>(){ "x", "y", "z", "t", "rx", "ry", "rz", "dx", "dy", "dz", "z" };
            // Specify groupings for unique lists
            var groupOn = new List<string>(){ "eventName" };

            // userID is optional
            if (m_InspectorViewModel.separateUsers)
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
                aggregateOn.AddRange(m_SeparationFields);
                groupOn.AddRange(m_SeparationFields);
            }

            // Specify smoothing properties (must be a subset of aggregateOn)
            var smoothOn = new Dictionary<string, float>();
            // Smooth space
            if (m_InspectorViewModel.smoothSpaceOption == SMOOTH_VALUE || m_InspectorViewModel.smoothSpaceOption == SMOOTH_NONE)
            {
                float spaceSmoothValue = (m_InspectorViewModel.smoothSpaceOption == SMOOTH_NONE) ? 0f : m_InspectorViewModel.smoothSpace;
                smoothOn.Add("x", spaceSmoothValue);
                smoothOn.Add("y", spaceSmoothValue);
                smoothOn.Add("z", spaceSmoothValue);
                smoothOn.Add("dx", spaceSmoothValue);
                smoothOn.Add("dy", spaceSmoothValue);
                smoothOn.Add("dz", spaceSmoothValue);
            }
            // Smooth rotation
            if (m_InspectorViewModel.smoothRotationOption == SMOOTH_VALUE || m_InspectorViewModel.smoothRotationOption == SMOOTH_NONE)
            {
                float rotationSmoothValue = (m_InspectorViewModel.smoothRotationOption == SMOOTH_NONE) ? 0f : m_InspectorViewModel.smoothRotation;
                smoothOn.Add("rx", rotationSmoothValue);
                smoothOn.Add("ry", rotationSmoothValue);
                smoothOn.Add("rz", rotationSmoothValue);
            }
            // Smooth time
            if (m_InspectorViewModel.smoothTimeOption == SMOOTH_VALUE || m_InspectorViewModel.smoothTimeOption == SMOOTH_NONE)
            {
                float timeSmoothValue = (m_InspectorViewModel.smoothTimeOption == SMOOTH_NONE) ? 0f : m_InspectorViewModel.smoothTime;
                smoothOn.Add("t", timeSmoothValue);
            }

            string remapToField = m_InspectorViewModel.remapDensity ? m_InspectorViewModel.remapColorField : "";
            int remapOption = m_InspectorViewModel.remapDensity ? m_InspectorViewModel.remapOptionIndex : 0;

            m_Aggregator.Process(OnAggregated, m_ViewModel.m_RawDataFileList, start, end,
                aggregateOn, smoothOn, groupOn,
                remapToField, m_RemapOptionIds[remapOption], m_InspectorViewModel.remapPercentile);
        }

        void OnAggregated(string jsonString)
        {
            string labelName = (m_InspectorViewModel.remapDensity) ? m_InspectorViewModel.remapColorField : "";
            m_DataParser.LoadData(jsonString, OnParsed, HeatmapDataParser.k_AsData, labelName);
        }

        void OnParsed(Dictionary<string, HeatPoint[]> data, string[] options)
        {
            m_ViewModel.m_Heatmaps = data;
            m_InspectorViewModel.heatmapOptionConjoinedLabels = options;
            if (m_ViewModel.m_Heatmaps != null)
            {
                m_InspectorViewModel.heatmapOptionIndex = PickBestOption(options);
                ParseOptionList(options);
                SelectHeatmap(m_ViewModel.m_Heatmaps[options[m_InspectorViewModel.heatmapOptionIndex]]);
            }
        }

        int PickBestOption(string[] options)
        {
            int bestOption = 0;
            if (m_InspectorViewModel.heatmapOptions != null)
            {
                string opt = (m_InspectorViewModel.heatmapOptionIndex >= options.Length) ? "" : m_InspectorViewModel.heatmapOptionConjoinedLabels[m_InspectorViewModel.heatmapOptionIndex];
                ArrayList list = new ArrayList(options);
                int idx = list.IndexOf(opt);
                bestOption = idx == -1 ? 0 : idx;
            }
            return bestOption;
        }

        void ParseOptionList(string[] options)
        {
            string[] oldKey = BuildKey().Split('~');
            m_InspectorViewModel.heatmapOptionLabels = new List<List<string>>();
            var optionsList = new List<int>();
            foreach(string opt in options)
            {
                string[] parts = opt.Split('~');
                for (int a = 0; a < parts.Length; a++)
                {
                    if (m_InspectorViewModel.heatmapOptionLabels.Count <= a)
                    {
                        m_InspectorViewModel.heatmapOptionLabels.Add(new List<string>());
                    }
                    if (m_InspectorViewModel.heatmapOptionLabels[a].IndexOf(parts[a]) == -1)
                    {
                        m_InspectorViewModel.heatmapOptionLabels[a].Add(parts[a]);
                    }
                }
            }
            for (int a = 0; a < m_InspectorViewModel.heatmapOptionLabels.Count; a++)
            {
                // Restore old indices when possible
                int index = 0;
                if (oldKey.Length > a)
                {
                    index = m_InspectorViewModel.heatmapOptionLabels[a].IndexOf(oldKey[a]);
                    index = Math.Max(0, index);
                }
                optionsList.Add(index);
            }
            m_InspectorViewModel.heatmapOptions = optionsList;
        }

        string BuildKey()
        {
            string retv = "";
            if (m_InspectorViewModel.heatmapOptionLabels != null)
            {
                for (int a = 0; a < m_InspectorViewModel.heatmapOptionLabels.Count; a++)
                {
                    retv += m_InspectorViewModel.heatmapOptionLabels[a][m_InspectorViewModel.heatmapOptions[a]];
                    if (a < m_InspectorViewModel.heatmapOptionLabels.Count - 1)
                    {
                        retv += "~";
                    }
                }
            }
            return retv;
        }

        void SelectList()
        {
            m_InspectorViewModel.heatmapOptionIndex = IndexFromOptions();
            string key = BuildKey();
            if (m_ViewModel.m_Heatmaps != null &&
                m_ViewModel.m_Heatmaps.ContainsKey(key))
            {
                SelectHeatmap(m_ViewModel.m_Heatmaps[key]);
            }
        }

        int IndexFromOptions()
        {
            int index = 0;
            string key = "";
            for (var a = 0; a < m_InspectorViewModel.heatmapOptions.Count; a++)
            {
                key += m_InspectorViewModel.heatmapOptionLabels[a][m_InspectorViewModel.heatmapOptions[a]];
                if (a < m_InspectorViewModel.heatmapOptions.Count-1)
                {
                    key += "~";
                }
            }
            if (m_ViewModel.m_Heatmaps.ContainsKey(key))
            {
                index = new List<string>(m_InspectorViewModel.heatmapOptionConjoinedLabels).IndexOf(key);
            }

            return index;
        }

        void SelectHeatmap(HeatPoint[] heatData)
        {
            // Creating this data allows the renderer to use it on the next Update pass
            m_ViewModel.m_HeatData = heatData;
        }
    }
}

