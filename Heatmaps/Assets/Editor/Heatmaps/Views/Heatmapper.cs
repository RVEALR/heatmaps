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
using System;

public class Heatmapper : EditorWindow
{

    [MenuItem("Window/Unity Analytics/Heatmapper #%h")]
    static void HeatmapperMenuOption()
    {
        EditorWindow.GetWindow(typeof(Heatmapper));
    }

    public Heatmapper()
    {
    }

    // Views
    AggregationInspector m_AggregationView;
    HeatmapRendererInspector m_RenderView;
    HeatmapProfilesInspector m_ProfileView;

    // Data handler
    HeatmapDataProcessor m_Processor;

    GameObject m_HeatMapInstance;

    bool m_ShowAggregate = false;
    bool m_ShowRender = false;
    bool m_IsPlayMode = false;
    bool m_ShowProfiles = false;

    Vector2 m_ScrollPosition;




    void OnEnable()
    {
        EnsureHeatmapInstance();
        m_Processor = new HeatmapDataProcessor();
        m_RenderView = HeatmapRendererInspector.Init(this, m_Processor);
        m_RenderView.OnEnable();
        m_AggregationView = AggregationInspector.Init(m_Processor);
        m_ProfileView = HeatmapProfilesInspector.Init(this);
        m_ProfileView.OnEnable();
        m_Processor.RestoreSettings();
        m_AggregationView.OnEnable();
    }

    void OnDisable()
    {
        if (m_RenderView != null)
        {
            m_RenderView.OnDisable();
        }
    }

    void OnFocus()
    {
        SystemProcess();
    }

    void OnGUI()
    {
        if (Event.current.type == EventType.Layout)
        {
            EnsureHeatmapInstance();
            if (m_RenderView != null)
            {
                m_RenderView.SetGameObject(m_HeatMapInstance);
            }
        }

        using (var scroll = new EditorGUILayout.ScrollViewScope(m_ScrollPosition))
        {
            m_ScrollPosition = scroll.scrollPosition;
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reset"))
                    {
                        SystemReset();
                    }
                    if (GUILayout.Button("Documentation"))
                    {
                        Application.OpenURL("https://bitbucket.org/Unity-Technologies/heatmaps/wiki/Home");
                    }
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                m_ShowAggregate = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), m_ShowAggregate, "Data", true);
                if (m_ShowAggregate)
                {
                    m_AggregationView.OnGUI();
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                m_ShowRender = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), m_ShowRender, "Render", true);
                if (m_ShowRender && m_RenderView != null)
                {
                    m_RenderView.OnGUI();
                }
            }

            using (new EditorGUILayout.VerticalScope("box"))
            {
                m_ShowProfiles = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), m_ShowProfiles, "Profiles", true);
                if (m_ShowProfiles && m_ProfileView != null)
                {
                    m_ProfileView.OnGUI();
                }
            }
        }
    }

    void Update()
    {
        EnsureHeatmapInstance();
        m_HeatMapInstance.GetComponent<IHeatmapRenderer>().RenderHeatmap();
        if (m_AggregationView != null)
        {
            m_AggregationView.Update();
        }
        if (m_RenderView != null)
        {
            bool hasNewData = m_Processor.m_ViewModel.m_HeatData != null;
            if (hasNewData)
            {
                m_RenderView.SetLimits(m_Processor.m_ViewModel.m_HeatData);
            }
            m_RenderView.Update(hasNewData);
            m_Processor.m_ViewModel.m_HeatData = null;
        }
        if (Application.isPlaying && !m_IsPlayMode) {
            m_IsPlayMode = true;
            SystemReset ();
        } else if (!Application.isPlaying && m_IsPlayMode) {
            m_IsPlayMode = false;
        }
    }

    void SystemProcess()
    {
        EnsureHeatmapInstance();
        m_Processor.Fetch();
    }

    public void SystemReset()
    {
        if (m_AggregationView != null) {
            m_AggregationView.SystemReset();
        }
        if (m_RenderView != null) {
            m_RenderView.SystemReset();
        }
        CreateHeatmapInstance(true);
        SystemProcess();
    }

    public void SwapRenderer(Type renderer)
    {
        AttemptReconnectWithHeatmapInstance();
        CreateHeatmapInstance(renderer);
        m_Processor.Fetch();
    }

    void EnsureHeatmapInstance()
    {
        AttemptReconnectWithHeatmapInstance();
        if (m_HeatMapInstance == null)
        {
            CreateHeatmapInstance();
        }
        if (m_RenderView != null)
        {
            m_RenderView.SetGameObject(m_HeatMapInstance);
        }
    }

    /// <summary>
    /// Attempts to reconnect with a heatmap instance.
    /// </summary>
    void AttemptReconnectWithHeatmapInstance()
    {
        m_HeatMapInstance = GameObject.Find("UnityAnalytics__Heatmap");
    }

    /// <summary>
    /// Creates the heat map instance.
    /// </summary>
    void CreateHeatmapInstance(bool force = false)
    {
        if (force)
        {
            DestroyHeatmapInstance();
        }
        CreateHeatmapInstance(typeof(HeatmapMeshRenderer));
    }

    void CreateHeatmapInstance (Type t)
    {
        DestroyHeatmapInstance();
        m_HeatMapInstance = new GameObject();
        m_HeatMapInstance.tag = "EditorOnly";
        m_HeatMapInstance.name = "UnityAnalytics__Heatmap";
        m_HeatMapInstance.AddComponent(t);
        m_HeatMapInstance.GetComponent<IHeatmapRenderer>().allowRender = true;
    }

    void DestroyHeatmapInstance()
    {
        if (m_HeatMapInstance)
        {
            m_HeatMapInstance.transform.parent = null;
            DestroyImmediate(m_HeatMapInstance);
        }
    }
}
