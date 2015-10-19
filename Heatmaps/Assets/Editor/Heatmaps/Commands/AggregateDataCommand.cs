using System;
using strange.extensions.command.impl;
using UnityEngine;
using System.Collections.Generic;

namespace UnityAnalyticsHeatmap
{
    public class AggregateDataCommand : Command
    {
        [Inject]
        public HeatmapAggregator aggregator { get; set; }

        [Inject]
        public IAggregationSettings settings { get; set; }

        [Inject]
        public RenderSignal renderSignal { get; set; }


        public override void Execute()
        {
            Retain();

            if (settings.fileList.Count == 0)
            {
                Debug.LogWarning("No matching data found.");
                Release ();
            }
            else
            {
                DateTime start, end;
                try
                {
                    start = DateTime.Parse(settings.startDate);
                }
                catch
                {
                    start = DateTime.Parse("2000-01-01");
                }
                try
                {
                    end = DateTime.Parse(settings.endDate);
                }
                catch
                {
                    end = DateTime.UtcNow;
                }
                
                var aggregateOn = new List<string>(){ "x", "y", "z", "t", "rx", "ry", "rz" };
                
                // Specify smoothing properties (must be a subset of aggregateOn)
                var smoothOn = new Dictionary<string, float>();
                // Always smooth on space
                smoothOn.Add("x", settings.space);
                smoothOn.Add("y", settings.space);
                smoothOn.Add("z", settings.space);
                // Time is optional
                if (!settings.aggregateTime)
                {
                    smoothOn.Add("t", settings.time);
                }
                // Angle is optional
                if (!settings.aggregateAngle)
                {
                    smoothOn.Add("rx", settings.angle);
                    smoothOn.Add("ry", settings.angle);
                    smoothOn.Add("rz", settings.angle);
                }
                
                // Specify groupings
                // Always group on eventName
                var groupOn = new List<string>(){ "eventName" };
                // deviceID is optional
                if (!settings.groupDevices)
                {
                    aggregateOn.Add("deviceID");
                    groupOn.Add("deviceID");
                }
                // Arbitrary Fields are included if specified
                if (settings.arbitraryGroupFields != null && settings.arbitraryGroupFields.Count > 0)
                {
                    aggregateOn.AddRange(settings.arbitraryGroupFields);
                    groupOn.AddRange(settings.arbitraryGroupFields);
                }
                aggregator.Process(aggregationHandler, settings.fileList, start, end,
                                   aggregateOn, smoothOn, groupOn, settings.whiteListEvents);
            }
        }

        void aggregationHandler(string JSONPath)
        {
            settings.jsonPath = JSONPath;
            Release ();
        }
    }
}

