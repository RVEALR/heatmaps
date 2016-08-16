/// <summary>
/// Heatmap view model.
/// </summary>

using System;
using System.Collections.Generic;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapViewModel
    {
        /// <summary>
        /// The complete dictionary of heatmaps from the currently loaded data set.
        /// </summary>
        public Dictionary<string, HeatPoint[]> m_Heatmaps;

        /// <summary>
        /// A list of labels representing all the heatmaps in the current set.
        /// </summary>
        public string[] m_SeparationOptions;

        /// <summary>
        /// The data directly represented in the current map.
        /// </summary>
        public HeatPoint[] m_HeatData;

        /// <summary>
        /// The raw data from which the map is generated.
        /// </summary>
        public CustomRawDatum[] m_RawData;
    }
}

