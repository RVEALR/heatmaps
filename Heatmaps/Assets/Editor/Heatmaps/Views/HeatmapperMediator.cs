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
        public PurgeMetadataSignal purgeMetadataSignal { get; set; }

        public override void OnRegister()
        {
            view.processSignal.AddListener(Process);
            view.goToDocumentationSignal.AddListener(GoToDocumentation);
            view.resetSignal.AddListener(Reset);
            view.purgeMetadataSignal.AddListener(PurgeMetadata);
        }

        public override void OnRemove()
        {
            view.processSignal.RemoveListener(Process);
            view.goToDocumentationSignal.RemoveListener(GoToDocumentation);
            view.resetSignal.RemoveListener(Reset);
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

        void PurgeMetadata()
        {
            purgeMetadataSignal.Dispatch();
        }


    }
}

