
using System;
using strange.extensions.command.impl;
using System.Collections.Generic;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class LoadAggregationCommand : Command
    {

        [Inject]
        public IAggregationSettings aggregationSettings { get; set; }

        [Inject]
        public IRendererSettings rendererSettings { get; set; }

        [Inject]
        public HeatmapDataParser parser { get; set; }

        [Inject]
        public IRenderData renderData { get; set; }

        override public void Execute()
        {
            Retain ();
            parser.LoadData(aggregationSettings.jsonPath, OnPointData);
        }
        
        void OnPointData(Dictionary<string, HeatPoint[]> heatData, float maxDensity, float maxTime, string[] options)
        {
            renderData.data = heatData;
            renderData.options = options;
            renderData.currentOptionIndex = 0;
            Release ();


//            if (heatData != null)
//            {
//                m_OptionKeys = options;
//                m_OptionIndex = 0;
//                rendererSettings.m = maxDensity;
//                rendererSettings.maxTime = maxTime;
//
//            }
        }
    }
}

