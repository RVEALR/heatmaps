using System;
using strange.extensions.command.impl;
using System.Collections.Generic;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class FetchDataCommand : Command
    {
        [Inject]
        public IAggregationSettings settings{ get; set; }

        [Inject]
        public RawEventClient rawEventClient{ get; set; }

        public override void Execute()
        {
            Retain ();
            if (!string.IsNullOrEmpty(settings.rawDataPath))
            {
                DateTime start, end;
                try
                {
                    start = DateTime.Parse(settings.startDate);
                }
                catch
                {
                    throw new Exception("The start date is not properly formatted. Correct format is YYYY-MM-DD.");
                }
                try
                {
                    end = DateTime.Parse(settings.endDate);
                }
                catch
                {
                    throw new Exception("The end date is not properly formatted. Correct format is YYYY-MM-DD.");
                }
                
                rawEventClient.Fetch(settings.rawDataPath, 
                                     settings.localOnly, 
                                     new UnityAnalyticsEventType[]{ UnityAnalyticsEventType.custom }, 
                                     start, end, rawFetchHandler);
            }
        }

        void rawFetchHandler(List<string> fileList)
        {
            settings.fileList = fileList;
            Release ();
        }
    }
}

