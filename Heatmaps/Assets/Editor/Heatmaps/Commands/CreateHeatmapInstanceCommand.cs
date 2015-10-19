
using System;
using strange.extensions.command.impl;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class CreateHeatmapInstanceCommand : Command
    {

        override public void Execute()
        {
            if (injectionBinder.GetBinding<IHeatmapRenderer>() != null)
            {
                injectionBinder.Unbind<IHeatmapRenderer>();
            }

            GameObject original = GameObject.Find ("UnityAnalytics__Heatmap");
            if (original != null)
            {
                original.transform.parent = null;
                GameObject.DestroyImmediate(original);
            }

            var go = new GameObject();
            go.tag = "EditorOnly";
            go.name = "UnityAnalytics__Heatmap";
            IHeatmapRenderer renderer = go.AddComponent<HeatmapMeshRenderer>();
            renderer.allowRender = true;
            injectionBinder.Bind<IHeatmapRenderer>().ToValue(renderer);
        }
    }
}
