/// <summary>
/// Heatmap view model.
/// </summary>

using System;
using System.Collections.Generic;

namespace RVEALR.Heatmaps
{
    public class HeatmapViewModel
    {
        /// <summary>
        /// The list of raw data files
        /// </summary>
        public List<string> m_RawDataFileList = new List<string>();

        /// <summary>
        /// The complete dictionary of heatmaps from the currently loaded data set.
        /// </summary>
        public Dictionary<string, HeatPoint[]> m_Heatmaps;

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

