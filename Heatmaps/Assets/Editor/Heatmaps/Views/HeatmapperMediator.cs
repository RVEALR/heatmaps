using System;
using strange.extensions.editor.impl;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapperMediator : EditorMediator
    {
        [Inject]
        public IHeatmapperView view{ get; set; }

        [Inject]
        public ProcessSignal processSignal{ get; set; }

        [Inject]
        public GoToDocumentationSignal goToDocumentationSignal{ get; set; }
        
        [Inject]
        public ResetSignal resetSignal { get; set; }
        
        [Inject]
        public RenderSignal renderSignal { get; set; }
        
        [Inject]
        public RenderSignal renderNewDataSignal { get; set; }

        [Inject]
        public PurgeMetadataSignal purgeMetadataSignal { get; set; }
        
        [Inject]
        public IAggregationSettings aggregationSettings { get; set; }
        
        [Inject]
        public IRendererSettings rendererSettings { get; set; }
        
        [Inject]
        public IRenderInfo renderInfo { get; set; }
        
        [Inject]
        public IRenderData renderData { get; set; }

        public override void OnRegister()
        {
            view.processSignal.AddListener(Process);
            view.goToDocumentationSignal.AddListener(GoToDocumentation);
            view.resetSignal.AddListener(Reset);
            view.renderSignal.AddListener(Render);
            view.renderNewDataSignal.AddListener(RenderNewData);
            view.purgeMetadataSignal.AddListener(PurgeMetadata);

            view.Init(aggregationSettings, rendererSettings, renderInfo, renderData);
        }

        public override void OnRemove()
        {
            view.processSignal.RemoveListener(Process);
            view.goToDocumentationSignal.RemoveListener(GoToDocumentation);
            view.resetSignal.RemoveListener(Reset);
            view.renderSignal.RemoveListener(Render);
            view.renderNewDataSignal.RemoveListener(RenderNewData);
            view.purgeMetadataSignal.RemoveListener(PurgeMetadata);
        }

        void Process()
        {
            processSignal.Dispatch();
        }

        void GoToDocumentation()
        {
            goToDocumentationSignal.Dispatch();
        }

        void Reset()
        {
            Debug.Log("Reset");
            resetSignal.Dispatch();
        }
        
        void Render()
        {
            renderSignal.Dispatch();
        }
        
        void RenderNewData()
        {
            renderNewDataSignal.Dispatch();
        }

        void PurgeMetadata()
        {
            purgeMetadataSignal.Dispatch();
        }


    }
}

