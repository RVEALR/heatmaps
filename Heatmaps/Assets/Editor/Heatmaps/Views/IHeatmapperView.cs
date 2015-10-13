using System;
using strange.extensions.signal.impl;

namespace UnityAnalyticsHeatmap
{
    public interface IHeatmapperView
    {
        Signal processSignal{ get; }

        Signal goToDocumentationSignal{ get; }

        Signal purgeMetadataSignal{ get; }

        Signal resetSignal{ get; }
    }
}

