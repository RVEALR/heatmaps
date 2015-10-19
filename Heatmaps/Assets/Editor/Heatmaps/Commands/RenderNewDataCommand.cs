
using System;
using strange.extensions.command.impl;
using System.Collections.Generic;


namespace UnityAnalyticsHeatmap
{
    public class RenderNewDataCommand : Command
    {
        [Inject]
        public IRenderData renderData { get; set; }

        [Inject]
        public IRendererSettings settings { get; set; }


        public override void Execute ()
        {
            settings.startTime = 0f;
            settings.endTime = renderData.maxTime;
            settings.maxTime = renderData.maxTime;
        }
    }
}

