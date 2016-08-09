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

namespace UnityAnalyticsHeatmap
{
    public class HeatmapDataProcessor
    {
        public string m_RawDataPath = "";
        public string m_DataPath = "";
        public string m_EndDate = "";

        public bool m_SeparateUsers = false;
        public bool m_SeparateSessions = false;
        public bool m_SeparatePlatform = false;
        public bool m_SeparateDebug = false;
        public bool m_SeparateCustomField = false;

        public List<string> m_SeparationFields = new List<string>();

        public bool m_RemapDensity;
        public string m_RemapColorField = "";
        public AggregationMethod m_RemapMethod;
        public float m_Percentile;


        public DateTime m_StartDate;


        public HeatmapDataProcessor()
        {
        }
    }
}

