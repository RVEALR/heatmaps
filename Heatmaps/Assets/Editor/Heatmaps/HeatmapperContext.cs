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
        injectionBinder.Bind<IAggregationSettings>().To<AggregationSettings>().ToSingleton();
        injectionBinder.Bind<IRendererSettings>().To<RendererSettings>().ToSingleton();
        injectionBinder.Bind<IRenderInfo>().To<RenderInfo>().ToSingleton();
        injectionBinder.Bind<IRenderData>().To<RenderData>().ToSingleton();
        injectionBinder.Bind<RawEventClient>().ToSingleton();
        injectionBinder.Bind<HeatmapAggregator>().ToSingleton();
        injectionBinder.Bind<HeatmapDataParser>().ToSingleton();


        //Commands
        commandBinder.Bind<StartSignal>().To<InitSystemCommand>().To<CreateHeatmapInstanceCommand>();
        commandBinder.Bind<PurgeMetadataSignal>().To<PurgeMetadataCommand>();
        commandBinder.Bind<GoToDocumentationSignal>().To<GoToDocumentationCommand>();

        // TODO: what else to reset?
        // Restore factory settings in AggregationSettings and RendererSettings
        commandBinder.Bind<ResetSignal>().To<ResetCommand>().To<CreateHeatmapInstanceCommand>();
        commandBinder.Bind<RenderSignal>().To<RenderCommand>().Pooled();
        commandBinder.Bind<RenderNewDataSignal>().To<RenderNewDataCommand>();

        commandBinder.Bind<ProcessSignal>()
                .To<FetchDataCommand>()
                .To<AggregateDataCommand>()
                .To<LoadAggregationCommand>()
                .To<RenderNewDataCommand>()
                .To<RenderCommand>()
                .InSequence();

        //Mediation
        mediationBinder.Bind<Heatmapper>().ToAbstraction<IHeatmapperView>().To<HeatmapperMediator>();
    }

    public override void Launch()
    {
        injectionBinder.GetInstance<StartSignal>().Dispatch();
    }
}

