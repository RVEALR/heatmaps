using System;
using strange.extensions.signal.impl;

namespace UnityAnalyticsHeatmap
{
    public interface IHeatmapperView
    {
        Signal processSignal { get; }

        Signal goToDocumentationSignal { get; }

        Signal purgeMetadataSignal { get; }
        
        Signal resetSignal { get; }
        
        Signal renderSignal { get; }
        
        Signal renderNewDataSignal { get; }

        void Init(IAggregationSettings aggregationSettings, IRendererSettings rendererSettings, IRenderInfo renderInfo, IRenderData renderData);
    }
}

