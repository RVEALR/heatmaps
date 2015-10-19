/// <summary>
/// Heatmapper inspector.
/// </summary>
/// This code drives the Heatmapper inspector
/// The HeatmapDataParser handles loading and parsing the data.
/// The HeatmapRendererInspector speaks to the Renderer to achieve a desired look.

using System.Collections.Generic;
using UnityAnalyticsHeatmap;
using UnityEditor;
using UnityEngine;
using strange.extensions.context.api;
using strange.extensions.signal.impl;
using strange.extensions.editor.impl;

public class Heatmapper : EditorView, IHeatmapperView
{

    
    // Views
    AggregationInspector m_AggregationView ;
    HeatmapRendererInspector m_RenderView;

    [MenuItem("Window/Heatmapper #%h")]
    static void HeatmapperMenuOption()
    {
        EditorWindow.GetWindow(typeof(Heatmapper));
    }

    override protected void OnFocus()
    {
        if (context == null)
        {
            m_AggregationView = new AggregationInspector();
            m_RenderView = new HeatmapRendererInspector(this);
            context = new HeatmapperContext(this);
        }
    }

    public void Init(IAggregationSettings aggregationSettings, IRendererSettings rendererSettings, IRenderInfo renderInfo, IRenderData renderData)
    {
        if (m_AggregationView == null)
            m_AggregationView = new AggregationInspector();

        m_AggregationView.settings = aggregationSettings;
        m_AggregationView.processSignal = processSignal;

        if (m_RenderView == null)
            m_RenderView = new HeatmapRendererInspector(this);

        m_RenderView.settings = rendererSettings;
        m_RenderView.renderSignal = renderSignal;
        m_RenderView.renderInfo = renderInfo;
        m_RenderView.renderData = renderData;
        m_RenderView.renderNewDataSignal = renderNewDataSignal;
    }

    private Signal m_ProcessSignal = new Signal();
    public Signal processSignal
    {
        get
        {
            return m_ProcessSignal;
        }
    }

    private Signal m_GoToDocumentationSignal = new Signal();
    public Signal goToDocumentationSignal
    {
        get
        {
            return m_GoToDocumentationSignal;
        }
    }

    private Signal m_PurgeMetadataSignal = new Signal();
    public Signal purgeMetadataSignal
    {
        get
        {
            return m_PurgeMetadataSignal;
        }
    }
    
    private Signal m_ResetSignal = new Signal();
    public Signal resetSignal
    {
        get
        {
            return m_ResetSignal;
        }
    }
    
    private Signal m_RenderSignal = new Signal();
    public Signal renderSignal
    {
        get
        {
            return m_RenderSignal;
        }
    }
    
    private Signal m_RenderNewDataSignal = new Signal();
    public Signal renderNewDataSignal
    {
        get
        {
            return m_RenderNewDataSignal;
        }
    }

    bool m_ShowAggregate = false;
    bool m_ShowRender = false;

    void OnGUI()
    {
        GUILayout.BeginVertical("box");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reset"))
        {
            context = null;
            OnFocus();
            resetSignal.Dispatch();
        }
        if (GUILayout.Button("Documentation"))
        {
            goToDocumentationSignal.Dispatch();
        }
        if (GUILayout.Button("Purge"))
        {
            if (EditorUtility.DisplayDialog("Destroy local data?", "You are about to delete your local heatmaps data cache, meaning you'll have to reload from the server. Are you sure?", "Purge", "Cancel"))
            {
                purgeMetadataSignal.Dispatch();
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");

        m_ShowAggregate = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), m_ShowAggregate, "Aggregate Events", true);
        if (m_ShowAggregate)
        {
            m_AggregationView.OnGUI();
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        m_ShowRender = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), m_ShowRender, "Render", true);
        if (m_ShowRender && m_RenderView != null)
        {
            m_RenderView.OnGUI();
        }

        GUILayout.EndVertical();
    }

    void Update()
    {
        if (m_RenderView != null)
        {
            m_RenderView.Update ();
        }
    }
}
