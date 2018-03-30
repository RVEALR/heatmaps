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
using System.Linq;

namespace RVEALR.Heatmaps
{
    public class HeatmapDataProcessor
    {
        const string k_DataPathKey = "HeatmapDataPathKey";
		const string k_DataOutPathKey = "HeatmapDataOutPathKey";
        const string k_SpaceKey = "HeatmapAggregationSpace";
        const string k_KeyToTime = "HeatmapAggregationTime";
        const string k_RotationKey = "HeatmapAggregationRotation";
        const string k_SmoothSpaceKey = "HeatmapAggregationAggregateSpace";
        const string k_SmoothTimeKey = "HeatmapAggregationAggregateTime";
        const string k_SmoothRotationKey = "HeatmapAggregationAggregateRotation";


        const string k_SeparateSessionKey = "HeatmapAggregationAggregateSessionIDs";
        const string k_SeparateDebugKey = "HeatmapAggregationAggregateDebug";
        const string k_SeparatePlatformKey = "HeatmapAggregationAggregatePlatform";
        const string k_SeparateCustomKey = "HeatmapAggregationAggregateCustom";

        const string k_ArbitraryFieldsKey = "HeatmapAggregationArbitraryFields";

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
                m_Aggregator.SetDataInPath(_rawDataPath);
                EditorPrefs.SetString(k_DataPathKey, value);
            }
        }

		private string _dataOutPath = "";
		public string m_DataOutPath
		{
			get{
				return _dataOutPath;
			}
			set{
				_dataOutPath = value;
				m_Aggregator.SetDataOutPath(_dataOutPath);
				EditorPrefs.SetString(k_DataOutPathKey, value);
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

        void OnParsed(Dictionary<string, HeatPoint[]> data, string[] conjoinedOptions)
        {
            m_ViewModel.m_Heatmaps = data;
            m_InspectorViewModel.heatmapOptionConjoinedLabels = conjoinedOptions;
            if (m_ViewModel.m_Heatmaps != null)
            {
                ParseOptionList(conjoinedOptions);
                m_InspectorViewModel.heatmapOptionIndex = PickBestOption(conjoinedOptions);
                SelectHeatmap(m_ViewModel.m_Heatmaps[conjoinedOptions[m_InspectorViewModel.heatmapOptionIndex]]);
            }
        }

        /// <summary>
        /// Attempt to find a "preferred" index from the provided options list.
        /// </summary>
        /// Defaults to picking all 0s.
        /// <returns>The best option.</returns>
        /// <param name="conjoinedOptions">The list of options.</param>
        int PickBestOption(string[] conjoinedOptions)
        {
            int bestOption = 0;
            string opt = "";

            if (m_InspectorViewModel.heatmapOptions != null)
            {
                opt = (m_InspectorViewModel.heatmapOptionIndex >= conjoinedOptions.Length) ?
                    "" :
                    m_InspectorViewModel.heatmapOptionConjoinedLabels[m_InspectorViewModel.heatmapOptionIndex];
            }

            ArrayList list = new ArrayList(conjoinedOptions);
            int index = list.IndexOf(opt);
            bestOption = index == -1 ? 0 : index;
            return bestOption;
        }

//        string[] m_LastKey;

        /// <summary>
        /// Parses the conjoined options to generate two broken-out "lists of lists" of options.
        /// </summary>
        /// heatmapOptions represents the indices
        /// heatmapOptionLabels represents the strings
        /// <param name="conjoinedOptions">Options.</param>
        void ParseOptionList(string[] conjoinedOptions)
        {
            m_InspectorViewModel.heatmapOptionLabels = new List<List<string>>();
            foreach(string opt in conjoinedOptions)
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
            if (m_InspectorViewModel.OptionListIsNew && m_InspectorViewModel.heatmapOptions.Count == m_InspectorViewModel.heatmapOptionLabels.Count)
            {
                // new list from settings...just leave it alone.
                m_InspectorViewModel.OptionListIsNew = false;
            }
            else
            {
                List<int> optionsList = m_InspectorViewModel.heatmapOptionLabels.Select(x => 0).ToList();
                m_InspectorViewModel.heatmapOptions = optionsList;
            }

            // Leaving all this mess for now.
            // Might try to re-enable heuristic guessing
//            for (int a = 0; a < m_InspectorViewModel.heatmapOptionLabels.Count; a++)
//            {
//                // Restore old indices when possible
//                int index = 0;
//                if (m_LastKey != null && m_LastKey.Length > 0 && m_InspectorViewModel.heatmapOptionLabels.Count == m_LastKey.Length)
//                {
//
//                    Debug.Log(m_LastKey[a]);
//
//                    index = m_InspectorViewModel.heatmapOptionLabels[a].IndexOf(m_LastKey[a]);
//                    index = Math.Max(0, index);
//                }
//
//
//                int index = 0;
//                if (m_InspectorViewModel.OptionListIsNew && m_InspectorViewModel.heatmapOptions.Count == m_InspectorViewModel.heatmapOptionLabels.Count)
//                {
//                    m_InspectorViewModel.OptionListIsNew = false;
//                    for (int a = 0; a < m_InspectorViewModel.heatmapOptions.Count; a++)
//                    {
//                        int idx = m_InspectorViewModel.heatmapOptions[a];
//                        opt += m_InspectorViewModel.heatmapOptionLabels[a][idx] + "~";
//                    }
//
//                    if (opt.LastIndexOf("~") == opt.Length - 1)
//                        opt = opt.Substring(0, opt.Length - 1);
//
//                    index = m_InspectorViewModel.heatmapOptions[a];
//
//
//                }
//
//
//
//                optionsList.Add(index);
//            }
//            m_InspectorViewModel.heatmapOptions = optionsList;
//            m_LastKey = BuildKey().Split('~');
        }

        string BuildKey()
        {
            string retv = "";
            if (m_InspectorViewModel.heatmapOptionLabels != null)
            {
                for (int a = 0; a < m_InspectorViewModel.heatmapOptions.Count; a++)
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

        /// <summary>
        /// Selects the heatmap from among the list of all heatmaps
        /// </summary>
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

        /// <summary>
        /// Indexs from options.
        /// </summary>
        /// <returns>The from options.</returns>
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
            m_ViewModel.m_HeatData = heatData;
        }
    }
}

