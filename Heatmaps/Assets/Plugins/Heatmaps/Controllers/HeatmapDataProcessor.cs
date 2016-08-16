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

        const string k_SeparateUsersKey = "UnityAnalyticsHeatmapAggregationAggregateUserIDs";
        const string k_SeparateSessionKey = "UnityAnalyticsHeatmapAggregationAggregateSessionIDs";
        const string k_SeparateDebugKey = "UnityAnalyticsHeatmapAggregationAggregateDebug";
        const string k_SeparatePlatformKey = "UnityAnalyticsHeatmapAggregationAggregatePlatform";
        const string k_SeparateCustomKey = "UnityAnalyticsHeatmapAggregationAggregateCustom";

        const string k_ArbitraryFieldsKey = "UnityAnalyticsHeatmapAggregationArbitraryFields";

        const string k_RemapColorKey = "UnityAnalyticsHeatmapRemapColorKey";
        const string k_RemapOptionIndexKey = "UnityAnalyticsHeatmapRemapOptionIndexKey";
        const string k_RemapColorFieldKey = "UnityAnalyticsHeatmapRemapColorFieldKey";
        const string k_PercentileKey = "UnityAnalyticsHeatmapRemapPercentileKey";

        const float k_DefaultSpace = 10f;
        const float k_DefaultTime = 10f;
        const float k_DefaultRotation = 15f;


        public const int SMOOTH_VALUE = 0;
        public const int SMOOTH_NONE = 1;
        public const int SMOOTH_UNION = 2;

        public HeatmapViewModel m_ViewModel;
        public HeatmapAggregator m_Aggregator;
        public HeatmapDataParser m_DataParser;

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
            get{
                return _startDate;
            }
            set{
                _startDate = value;
            }
        }
        public string m_EndDate = "";

        int _smoothSpaceToggle;
        public int m_SmoothSpaceToggle
        {
            get{
                return _smoothSpaceToggle;
            }
            set{
                _smoothSpaceToggle = value;
                EditorPrefs.SetInt(k_SmoothSpaceKey, _smoothSpaceToggle);
            }
        }
        float _space;
        public float m_Space
        {
            get{
                return _space;
            }
            set{
                _space = value;
                EditorPrefs.SetFloat(k_SpaceKey, _space);
                ProcessAggregation();
            }
        }
        int _smoothRotationToggle;
        public int m_SmoothRotationToggle
        {
            get {
                return _smoothRotationToggle;
            }
            set {
                _smoothRotationToggle = value;
                EditorPrefs.SetInt(k_SmoothRotationKey, _smoothRotationToggle);
            }
        }
        float _rotation;
        public float m_Rotation
        {
            get {
                return _rotation;
            }
            set {
                _rotation = value;
                EditorPrefs.SetFloat(k_RotationKey, _rotation);
                ProcessAggregation();
            }
        }
        int _smoothTimeToggle;
        public int m_SmoothTimeToggle
        {
            get {
                return _smoothTimeToggle;
            }
            set {
                _smoothTimeToggle = value;
                EditorPrefs.SetInt(k_SmoothTimeKey, _smoothTimeToggle);
            }
        }
        float _time;
        public float m_Time
        {
            get {
                return _time;
            }
            set {
                _time = value;
                EditorPrefs.SetFloat(k_KeyToTime, _time);
                ProcessAggregation();
            }
        }

        bool _separateUsers = false;
        public bool m_SeparateUsers
        {
            get{
                return _separateUsers;
            }
            set{
                _separateUsers = value;
                EditorPrefs.SetBool(k_SeparateUsersKey, _separateUsers);
                ProcessAggregation();
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
                ProcessAggregation();
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
                ProcessAggregation();
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
                ProcessAggregation();
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
                ProcessAggregation();
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
                ProcessAggregation();
            }
        }

        bool _remapDensity;
        public bool m_RemapDensity
        {
            get {
                return _remapDensity;
            }
            set {
                _remapDensity = value;
                EditorPrefs.SetBool(k_RemapColorKey, _remapDensity);
                ProcessAggregation();
            }
        }
        string _remapColorField;
        public string m_RemapColorField
        {
            get
            {
                return _remapColorField;
            }
            set {
                _remapColorField = value;
                EditorPrefs.SetString(k_RemapColorFieldKey, _remapColorField);
                ProcessAggregation();
            }
        }
        int _remapOptionIndex;
        public int m_RemapOptionIndex
        {
            get {
                return _remapOptionIndex;
            }
            set {
                _remapOptionIndex = value;
                EditorPrefs.SetInt(k_RemapOptionIndexKey, _remapOptionIndex);
                ProcessAggregation();
            }
        }
        float _percentile;
        public float m_Percentile
        {
            get {
                return _percentile;
            }
            set {
                _percentile = value;
                EditorPrefs.SetFloat(k_PercentileKey, _percentile);
                ProcessAggregation();
            }
        }

        List<int> _heatmapOptions;
        public List<int> m_HeatmapOptions
        {
            get {
                return _heatmapOptions;
            }
            set {
                _heatmapOptions = value;
            }
        }
        public int m_HeatmapOptionIndex;
        public List<List<string>> m_SeparatedLists;
        public string[] m_OptionKeys;

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
        }

        public void RestoreSettings()
        {
            // Restore cached paths
            m_RawDataPath = EditorPrefs.GetString(k_DataPathKey);

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
            m_RemapDensity = EditorPrefs.GetBool(k_RemapColorKey);
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
            m_SeparationFields = new List<string>(arbitraryFieldsList);
        }

        /// <summary>
        /// Fetch the files within the currently specified date range.
        /// </summary>
        public void Fetch()
        {
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

            RawDataClient.GetInstance().m_DataPath = m_RawDataPath;
            m_ViewModel.m_RawDataFileList = RawDataClient.GetInstance().GetFiles(new UnityAnalyticsEventType[]{ UnityAnalyticsEventType.custom }, start, end);
            ProcessAggregation();
        }

        void ProcessAggregation()
        {
            if (m_ViewModel.m_RawDataFileList.Count == 0)
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
                if (m_RemapDensity && string.IsNullOrEmpty(m_RemapColorField))
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
                    aggregateOn.AddRange(m_SeparationFields);
                    groupOn.AddRange(m_SeparationFields);
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

                string remapToField = m_RemapDensity ? m_RemapColorField : "";
                int remapOption = m_RemapDensity ? m_RemapOptionIndex : 0;

                m_Aggregator.Process(OnAggregation, m_ViewModel.m_RawDataFileList, start, end,
                    aggregateOn, smoothOn, groupOn,
                    remapToField, m_RemapOptionIds[remapOption], m_Percentile);
            }
        }

        void OnAggregation(string jsonPath)
        {
            m_DataParser.LoadData(jsonPath, OnParsed, true);
        }

        void OnParsed(Dictionary<string, HeatPoint[]> data, string[] options)
        {
            m_ViewModel.m_Heatmaps = data;
            m_ViewModel.m_SeparationOptions = options;
            if (m_ViewModel.m_Heatmaps != null)
            {
                m_HeatmapOptionIndex = PickBestOption(options);
                ParseOptionList(options);
                OnPointData(m_ViewModel.m_Heatmaps[options[m_HeatmapOptionIndex]]);
            }
        }

        int PickBestOption(string[] options)
        {
            int bestOption = 0;
            if (m_HeatmapOptions != null)
            {
                string opt = m_HeatmapOptionIndex > options.Length ? "" : m_OptionKeys[m_HeatmapOptionIndex];
                ArrayList list = new ArrayList(options);
                int idx = list.IndexOf(opt);
                bestOption = idx == -1 ? 0 : idx;
            }
            return bestOption;
        }

        void ParseOptionList(string[] options)
        {
            string[] oldKey = BuildKey().Split('~');
            m_SeparatedLists = new List<List<string>>();
            m_HeatmapOptions = new List<int>();

            foreach(string opt in options)
            {
                string[] parts = opt.Split('~');

                for (int a = 0; a < parts.Length; a++)
                {
                    if (m_SeparatedLists.Count <= a)
                    {
                        m_SeparatedLists.Add(new List<string>());
                    }
                    if (m_SeparatedLists[a].IndexOf(parts[a]) == -1)
                    {
                        m_SeparatedLists[a].Add(parts[a]);
                    }
                }
            }
            for (int a = 0; a < m_SeparatedLists.Count; a++)
            {
                // Restore old indices when possible
                int index = 0;
                if (oldKey.Length > a)
                {
                    index = m_SeparatedLists[a].IndexOf(oldKey[a]);
                    index = Math.Max(0, index);
                }
                m_HeatmapOptions.Add(index);
            }
            m_OptionKeys = options;
        }

        string BuildKey()
        {
            string retv = "";
            if (m_SeparatedLists != null)
            {
                for (int a = 0; a < m_SeparatedLists.Count; a++)
                {
                    retv += m_SeparatedLists[a][m_HeatmapOptions[a]];
                    if (a < m_SeparatedLists.Count - 1)
                    {
                        retv += "~";
                    }
                }
            }
            return retv;
        }

        public void SelectList()
        {
            string key = BuildKey();
            if (m_ViewModel.m_Heatmaps != null &&
                m_ViewModel.m_Heatmaps.ContainsKey(key))
            {
                OnPointData(m_ViewModel.m_Heatmaps[key]);
            }
        }

        void OnPointData(HeatPoint[] heatData)
        {
            // Creating this data allows the renderer to use it on the next Update pass
            m_ViewModel.m_HeatData = heatData;
        }
    }
}

