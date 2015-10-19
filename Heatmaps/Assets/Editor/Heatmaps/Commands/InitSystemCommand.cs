using System;
using strange.extensions.command.impl;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class InitSystemCommand : Command
    {
        //We inject this here to ensure that the aggregator has access to Application.persistentDatPath
        [Inject]
        public HeatmapAggregator aggregator { get; set; }

        public override void Execute()
        {
        }
    }
}

