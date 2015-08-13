/// <summary>
/// Heat mapper inspector.
/// </summary>
/// This code drives the Heat Mapper inspector
/// The HeatmapDataParser handles loading and parsing the data.
/// The HeatmapRendererInspector speaks to the Renderer to achieve a desired look.

using UnityEngine;
using UnityEditor;
using UnityAnalytics;

public class Heatmapper : EditorWindow
{
	[MenuItem("Window/Heatmapper #%h")]
	private static void HeatmapperMenuOption()
	{
		EditorWindow.GetWindow (typeof(Heatmapper));
	}

	private RawEventInspector m_FetchView;
	private AggregationInspector m_AggregateView;
	private HeatmapRendererInspectorCollection m_RenderView;

	bool normalizeData;

	bool showFetch = false;
	bool showAggregate = false;
	bool showRender = false;

	GameObject heatmapInstance;


	void OnGUI ()
	{
		if (heatmapInstance == null) {
			heatmapInstance = new GameObject();
			heatmapInstance.name = "UnityAnalytics__Heatmaps";
		}

		GUILayout.BeginVertical ("box");
		if (GUILayout.Button ("Reset")) {
			SystemReset ();
		}
		GUILayout.EndVertical ();

		GUILayout.BeginVertical ("box");
		if (m_FetchView == null) {
			m_FetchView = RawEventInspector.Init ();
		}
		showFetch = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showFetch, "Fetch Raw Custom Events", true);
		if (showFetch) {
			m_FetchView.OnGUI ();
		}
		GUILayout.EndVertical ();

		GUILayout.BeginVertical ("box");
		if (m_AggregateView == null) {
			m_AggregateView = AggregationInspector.Init (OnAggregation);
		}
		showAggregate = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showAggregate, "Aggregate Events", true);
		if (showAggregate) {
			m_AggregateView.OnGUI ();
		}
		GUILayout.EndVertical ();

		GUILayout.BeginVertical ("box");

		if (m_RenderView == null) {
			m_RenderView = HeatmapRendererInspectorCollection.Init (heatmapInstance);
		}
		m_RenderView.SetParent (heatmapInstance);

		showRender = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showRender, "Render", true);
		if (showRender) {
			m_RenderView.OnGUI ();
		}
		GUILayout.EndVertical ();
	}


	void Update() {
		if (m_RenderView != null)
		{
			m_RenderView.Render ();
		}
	}

	void SystemReset()
	{
		if (m_RenderView != null) {
			m_RenderView.Reset ();
		}
	}

	void RawDataHandler(string[] paths) {
		// All the paths
	}

	void OnAggregation(string[] paths) {
		//The aggregated data
	}
}
