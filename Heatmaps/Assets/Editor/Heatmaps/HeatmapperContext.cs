using System;
using strange.extensions.editor.impl;
using UnityEditor;
using UnityAnalyticsHeatmap;
using UnityEngine;
using strange.extensions.mediation.api;


public class HeatmapperContext : EditorMVCSContext
{
    public HeatmapperContext(object view)
        : base(view)
    {
    }

    protected static HeatmapperContext instance;

    protected override void mapBindings()
    {
        base.mapBindings();

        //Injections
        injectionBinder.Bind<AggregationSettings>().ToSingleton();
        injectionBinder.Bind<RendererSettings>().ToSingleton();
        injectionBinder.Bind<RawEventClient>().ToSingleton();



        //Commands
        commandBinder.Bind<StartSignal>().To<InitSystemCommand>();
        commandBinder.Bind<PurgeMetadataSignal>().To<PurgeMetadataCommand>();
        commandBinder.Bind<GoToDocumentationSignal>().To<GoToDocumentationCommand>();
        commandBinder.Bind<ResetSignal>().To<ResetCommand>();

        commandBinder.Bind<ProcessSignal>().To<FetchDataCommand>().To<AggregateDataCommand>().To<RenderCommand>().InSequence();

        //Mediation
        mediationBinder.Bind<Heatmapper>().ToAbstraction<IHeatmapperView>().To<HeatmapperMediator>();
    }

    public override void Launch()
    {
        injectionBinder.GetInstance<StartSignal>().Dispatch();
    }
}

