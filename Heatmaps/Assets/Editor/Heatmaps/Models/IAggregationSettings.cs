



using System;
using System.Collections.Generic;


namespace UnityAnalyticsHeatmap
{
    public interface IAggregationSettings
    {
        string rawDataPath { get; set; }
        string startDate { get; set; }
        string endDate { get; set; }
        bool localOnly { get; set; }

        float space { get; set; }
        float time { get; set; }
        float angle { get; set; }

        bool aggregateTime { get; set; }
        bool aggregateAngle { get; set; }
        bool groupDevices { get; set; }

        List<string> arbitraryGroupFields { get; set; }
        List<string> whiteListEvents { get; set; }

        List<string> fileList { get; set; }
        string jsonPath { get; set; }
    }
}

