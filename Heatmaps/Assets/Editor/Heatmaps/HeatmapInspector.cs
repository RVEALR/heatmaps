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
	private HeatmapDataParserInspector m_ParseView;
	private HeatmapRendererInspector m_RenderView;

	private GameObject heatMapInstance;

	bool normalizeData;

	bool showFetch = false;
	bool showAggregate = false;
	bool showRender = false;


	void OnGUI ()
	{
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
		if (m_ParseView == null) {
			m_ParseView = HeatmapDataParserInspector.Init (PointDataHandler);
		}
		if (m_RenderView == null) {
			m_RenderView = HeatmapRendererInspector.Init ();
		}

		showRender = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showRender, "Render", true);
		if (showRender) {
			m_ParseView.OnGUI ();
			m_RenderView.OnGUI ();
		}

		if (heatMapInstance) {
			m_RenderView.SetGameObject (heatMapInstance);
		}
		GUILayout.EndVertical ();
	}


	void Update() {
		if (heatMapInstance)
		{
			heatMapInstance.GetComponent<IHeatmapRenderer> ().RenderHeatmap ();
		}
	}

	void SystemReset()
	{
		if (heatMapInstance) {
			heatMapInstance.transform.parent = null;
			DestroyImmediate (heatMapInstance);
			CreateHeatmapInstance ();
		}
	}

	void RawDataHandler(string[] paths) {
		// All the paths
	}

	void OnAggregation(string[] paths) {
		//The aggregated data
	}

	void PointDataHandler(HeatPoint[] heatData, float maxDensity, float maxTime)
	{
		if (heatMapInstance == null) {
			CreateHeatmapInstance ();
		}
		if (heatMapInstance.GetComponent<IHeatmapRenderer> () != null) {
			heatMapInstance.GetComponent<IHeatmapRenderer> ().UpdatePointData (heatData, maxDensity);
		}
		if (m_RenderView != null) {
			m_RenderView.SetMaxTime (maxTime);
		}
	}

	/// <summary>
	/// Creates the heat map instance.
	/// </summary>
	/// We've hard-coded the Component here. Everywhere else, we use the interface.
	/// If you want to write a custom Renderer, this is the place to sub it in.
	void CreateHeatmapInstance()
	{
		heatMapInstance = new GameObject ();
		heatMapInstance.tag = "EditorOnly";
		heatMapInstance.name = "UnityAnalytics__Heatmap";
		heatMapInstance.AddComponent<HeatmapMeshRenderer> ();
		heatMapInstance.GetComponent<IHeatmapRenderer> ().allowRender = true;
	}
}
