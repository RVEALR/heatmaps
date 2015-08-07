/// <summary>
/// Heat mapper inspector.
/// </summary>
/// This code drives the Heat Mapper inspector
/// The HeatMapDataParser handles loading and parsing the data.
/// The HeatMapRendererInspector speaks to the Renderer to achieve a desired look.

using UnityEngine;
using UnityEditor;
using UnityAnalytics;

public class HeatMapper : EditorWindow
{
	[MenuItem("Window/Heat Mapper #%h")]
	private static void HeatMapperMenuOption()
	{
		EditorWindow.GetWindow (typeof(HeatMapper));
	}

	private RawEventInspector m_FetchView;
	private AggregationInspector m_AggregateView;
	private HeatMapDataParserInspector m_ParseView;
	private HeatMapRendererInspector m_RenderView;

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
			m_FetchView = RawEventInspector.Init (RawDataHandler);
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
			m_ParseView = HeatMapDataParserInspector.Init (PointDataHandler);
		}
		if (m_RenderView == null) {
			m_RenderView = HeatMapRendererInspector.Init ();
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
			heatMapInstance.GetComponent<IHeatMapRenderer> ().RenderHeatMap ();
		}
	}

	void SystemReset()
	{
		if (heatMapInstance) {
			heatMapInstance.transform.parent = null;
			DestroyImmediate (heatMapInstance);
			CreateHeatMapInstance ();
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
			CreateHeatMapInstance ();
		}
		if (heatMapInstance.GetComponent<IHeatMapRenderer> () != null) {
			heatMapInstance.GetComponent<IHeatMapRenderer> ().UpdatePointData (heatData, maxDensity);
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
	void CreateHeatMapInstance()
	{
		heatMapInstance = new GameObject ();
		heatMapInstance.tag = "EditorOnly";
		heatMapInstance.name = "UnityAnalytics__HeatMap";
		heatMapInstance.AddComponent<HeatMapMeshRenderer> ();
		heatMapInstance.GetComponent<IHeatMapRenderer> ().allowRender = true;
	}
}
