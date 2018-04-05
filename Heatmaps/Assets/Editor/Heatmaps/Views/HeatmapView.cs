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

namespace RVEALR.Heatmaps
{
	public class HeatmapView
	{
		public AggregationInspector m_AggregationView;
		public HeatmapRendererInspector m_RenderView;
		public HeatmapDataProcessor m_Processor;
		public bool m_ShowView;
		public string m_Identifier;

		public GameObject m_HeatMapInstance;

		bool m_ShowRender = false;
		bool m_ShowAggregate = false;
		bool m_IsPlayMode = false;

		public HeatmapView(string id)
		{
			if (string.IsNullOrEmpty(id))
				m_Identifier = "UnityAnalytics__Heatmap";
			else
				m_Identifier = id;
			Init(new HeatmapDataProcessor());
		}

		public void Init(HeatmapDataProcessor proc)
		{
			m_Processor = proc;
			m_AggregationView = AggregationInspector.Init(m_Processor);
			m_RenderView = HeatmapRendererInspector.Init(this, m_Processor);
		}

		public HeatmapView(HeatmapDataProcessor proc, AggregationInspector agg, HeatmapRendererInspector renderer)
		{
			m_Processor = proc;
			m_AggregationView = agg;
			m_RenderView = renderer;
		}

		public void OnGUI()
		{
			using (new EditorGUILayout.VerticalScope("box"))
			{
				m_ShowAggregate = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), m_ShowAggregate, "Aggregate Data", true);
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
		}

		public void Apply()
		{
			EnsureHeatmapInstance();
			m_Processor.Fetch();
			SaveHeatmapDetails();
		}

		public void OnEnable()
		{
			EnsureHeatmapInstance();
			m_RenderView.OnEnable();
			m_Processor.RestoreSettings();
			m_AggregationView.OnEnable();
			Update();
		}

		public void SwapRenderer(Type renderer)
		{
			AttemptReconnectWithHeatmapInstance();
			CreateHeatmapInstance(renderer);
			m_Processor.Fetch();
		}

		public void Update()
		{
			EnsureHeatmapInstance();
			m_HeatMapInstance.GetComponent<IHeatmapRenderer>().RenderHeatmap();

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

			if (Application.isPlaying && !m_IsPlayMode) 
			{
				m_IsPlayMode = true;
				Apply();
			} 
			else if (!Application.isPlaying && m_IsPlayMode) 
			{
				m_IsPlayMode = false;
			}
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

		void SaveHeatmapDetails()
		{
			HeatmapViewController viewController = m_HeatMapInstance.GetComponent<HeatmapViewController>();
			if (viewController == null)
			{
				viewController = m_HeatMapInstance.AddComponent<HeatmapViewController>();
			}
				
			viewController.settings = HeatmapProfilesInspector.Init().Create(m_Identifier);
		}


		/// <summary>
		/// Attempts to reconnect with a heatmap instance.
		/// </summary>
		void AttemptReconnectWithHeatmapInstance()
		{
			m_HeatMapInstance = m_HeatMapInstance == null ? GameObject.Find(m_Identifier) : m_HeatMapInstance;
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
			m_HeatMapInstance.name = m_Identifier;
			m_HeatMapInstance.AddComponent(t);
			m_HeatMapInstance.GetComponent<IHeatmapRenderer>().allowRender = true;
		}

		void DestroyHeatmapInstance()
		{
			if (m_HeatMapInstance)
			{
				m_HeatMapInstance.transform.parent = null;
				UnityEngine.Object.DestroyImmediate(m_HeatMapInstance);
			}
		}
	}
}
