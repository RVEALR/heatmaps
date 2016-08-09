/// <summary>
/// Heatmap view model.
/// </summary>

using System;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapViewModel
    {
        /// <summary>
        /// The data directly represented in the map
        /// </summary>
        public HeatPoint[] m_HeatData;
        /// <summary>
        /// The raw data from which the map is generated.
        /// </summary>
        public CustomRawDatum[] m_RawData;
    }
}

