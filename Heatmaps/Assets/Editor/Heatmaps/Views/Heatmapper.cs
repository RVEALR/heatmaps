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
	public class Heatmapper : EditorWindow
	{
		static Heatmapper s_Instance;

	    [MenuItem("Window/Analytics/Heatmapper #%h")]
	    static void HeatmapperMenuOption()
	    {
			s_Instance = EditorWindow.GetWindow(typeof(Heatmapper)) as Heatmapper;
	    }

	    public Heatmapper()
	    {
			s_Instance = this;
	    }

	    // Views
		//DataInputOutputInspector m_DataInputOutputView;
		List<HeatmapView> m_HeatmapViews;
	    public HeatmapProfilesInspector m_ProfileView;

	    bool m_ShowProfiles = false;

	    Vector2 m_ScrollPosition;

		public static Heatmapper Instance()
		{
			if (s_Instance == null)
			{
				s_Instance = new Heatmapper();
			}
			return s_Instance;
		}

	    void OnEnable()
	    {
			if (m_HeatmapViews == null)
			{
				m_HeatmapViews = new List<HeatmapView>();
				AddHeatmap();
			}

			foreach (HeatmapView heatmapView in m_HeatmapViews)
			{
				heatmapView.OnEnable();
			}

			m_ProfileView = HeatmapProfilesInspector.Init();
			m_ProfileView.OnEnable();
	    }

	    void OnDisable()
	    {
			foreach (HeatmapView heatmapView in m_HeatmapViews)
			{
				heatmapView.m_RenderView.OnEnable();
				if (heatmapView.m_RenderView != null)
				{
					heatmapView.m_RenderView.OnDisable();
				}
			}
	    }

	    void OnFocus()
	    {
	        //SystemProcess();
	    }

	    void OnGUI()
	    {
	        using (var scroll = new EditorGUILayout.ScrollViewScope(m_ScrollPosition))
	        {
	            m_ScrollPosition = scroll.scrollPosition;
	            using (new EditorGUILayout.VerticalScope("box"))
	            {
	                using (new EditorGUILayout.HorizontalScope())
					{				
						if (GUILayout.Button("(+) Add Heatmap"))
						{
							AddHeatmap();
						}
						if (GUILayout.Button("Reset All Heatmaps"))
	                    {
	                        SystemReset();
	                    }
	                    if (GUILayout.Button("Documentation"))
	                    {
	                        Application.OpenURL("https://bitbucket.org/Unity-Technologies/heatmaps/wiki/Home");
	                    }
	                }
	            }

			/*	using (new EditorGUILayout.VerticalScope("box"))
				{
					m_ShowInOut = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), m_ShowAggregate, "Input/Output Data", true);
					if (m_ShowInOut)
					{
						m_DataInputOutputView.OnGUI();
					}
				} */

				if (m_HeatmapViews != null && m_HeatmapViews.Count > 0)
				{
					foreach (HeatmapView heatmapView in m_HeatmapViews)
					{
						using (new EditorGUILayout.VerticalScope("box"))
						{
							heatmapView.m_ShowView = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), heatmapView.m_ShowView, heatmapView.m_Identifier, true);
							if (heatmapView.m_ShowView)
							{
								heatmapView.OnGUI();
							}
						}

						if (GUILayout.Button("(A) Apply Heatmap Changes"))
						{
							ApplyHeatmap(heatmapView);
						}

						if (m_HeatmapViews.Count > 1 && GUILayout.Button("(-) Remove Heatmap"))
						{
							RemoveHeatmap(heatmapView);
						}
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
			foreach (HeatmapView heatmapView in m_HeatmapViews)
			{
				heatmapView.Update();
			}
	    }

		private void AddHeatmap()
		{
			HeatmapView view = new HeatmapView("Heatmap_#" + (m_HeatmapViews.Count + 1).ToString());
			m_HeatmapViews.Add(view);
			view.OnEnable();
			ApplyHeatmap(view);
		}

		private void ApplyHeatmap(HeatmapView heatmapView)
		{
			heatmapView.Apply();
		}

		private void RemoveHeatmap(HeatmapView heatmapView)
		{
			m_HeatmapViews.Remove(heatmapView);
		}

	    public void SystemReset()
	    {
			m_HeatmapViews.Clear();
			AddHeatmap();
	    }
	}
}
