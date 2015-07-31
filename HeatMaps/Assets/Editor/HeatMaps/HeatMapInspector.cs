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

	private HeatMapDataParserInspector m_ParseView;
	private HeatMapRendererInspector m_RenderView;

	private GameObject heatMapInstance;

	bool normalizeData;

	void OnGUI ()
	{
		if (GUILayout.Button ("Reset")) {
			SystemReset ();
		}


		if (m_ParseView == null) {
			m_ParseView = HeatMapDataParserInspector.Init (PointDataHandler);
		}
		m_ParseView.OnGUI ();



		if (m_RenderView == null) {
			m_RenderView = HeatMapRendererInspector.Init ();
		}
		m_RenderView.OnGUI ();
		if (heatMapInstance) {
			m_RenderView.SetGameObject (heatMapInstance);
		}
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
