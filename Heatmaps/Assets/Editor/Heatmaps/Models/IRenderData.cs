
using System;
using System.Collections.Generic;


namespace UnityAnalyticsHeatmap
{
    public interface IRenderData
    {
        Dictionary<string, HeatPoint[]> data { get; set; }
        string[] options { get; set; }
        HeatPoint[] currentPoints { get; }
        int currentOptionIndex { get; set; }

        float maxDensity { get; }
        float maxTime { get; }
    }
}

