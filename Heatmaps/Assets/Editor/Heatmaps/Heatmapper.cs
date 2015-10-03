/// <summary>
/// Heatmapper inspector.
/// </summary>
/// This code drives the Heatmapper inspector
/// The HeatmapDataParser handles loading and parsing the data.
/// The HeatmapRendererInspector speaks to the Renderer to achieve a desired look.

using UnityEngine;
using UnityEditor;
using UnityAnalyticsHeatmap;
using System.Collections.Generic;

public class Heatmapper : EditorWindow
{
	[MenuItem("Window/Heatmapper #%h")]
	private static void HeatmapperMenuOption()
	{
		EditorWindow.GetWindow (typeof(Heatmapper));
	}

	//Views
	//private RawEventInspector FetchView;
	private AggregationInspector AggregationView;
	private HeatmapDataParserInspector ParserView;
	private HeatmapRendererInspector RenderView;

	//Data handlers
	private RawEventClient EventClient = new RawEventClient ();
	private HeatmapAggregator Aggregator = new HeatmapAggregator ();

	private GameObject heatMapInstance;

	bool showAggregate = false;
	bool showRender = false;

	Dictionary<string, object> pointData;

	void OnGUI ()
	{
		GUILayout.BeginVertical ("box");
		GUILayout.BeginHorizontal ();
		if (GUILayout.Button ("Reset")) {
			SystemReset ();
		}
		if (GUILayout.Button ("Documentation")) {
			Application.OpenURL("https://bitbucket.org/strangeioc/heatmaps/wiki/Home");
		}
		if (GUILayout.Button ("Purge")) {
			PurgeData ();
		}
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();

		GUILayout.BeginVertical ("box");
		if (AggregationView == null) {
			AggregationView = AggregationInspector.Init (EventClient, Aggregator);
		}
		showAggregate = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showAggregate, "Aggregate Events", true);
		if (showAggregate) {
			AggregationView.OnGUI ();
			if (GUILayout.Button ("Fetch and Process")) {
				SystemProcess ();
			}
		}
		GUILayout.EndVertical ();

		GUILayout.BeginVertical ("box");
		if (ParserView == null) {
			ParserView = HeatmapDataParserInspector.Init (OnPointData);
		}
		if (RenderView == null) {
			RenderView = HeatmapRendererInspector.Init (this);
		}

		showRender = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showRender, "Render", true);
		if (showRender && ParserView != null) {
			ParserView.OnGUI ();
			RenderView.OnGUI ();
		}

		if (heatMapInstance) {
			RenderView.SetGameObject (heatMapInstance);
		}
		GUILayout.EndVertical ();
	}


	void Update() {
		if (heatMapInstance != null)
		{
			heatMapInstance.GetComponent<IHeatmapRenderer> ().RenderHeatmap ();
		}
		if (RenderView != null) {
			RenderView.Update ();
		}

		if (pointData != null) {
			if (heatMapInstance == null) {
				CreateHeatmapInstance ();
			}

			if (heatMapInstance.GetComponent<HeatmapMeshRenderer> () != null) {
				heatMapInstance.GetComponent<HeatmapMeshRenderer> ().UpdatePointData (pointData["heatData"] as HeatPoint[], (float)pointData["maxDensity"]);
			}

			if (RenderView != null) {
				RenderView.SetMaxTime ((float)pointData["maxTime"]);
				RenderView.SetGameObject (heatMapInstance);
				RenderView.Update (true);
			}

			pointData = null;
		}
	}

	void SystemProcess() {
		if (heatMapInstance == null) {
			CreateHeatmapInstance ();
		}
		if (AggregationView != null) {
			AggregationView.Fetch (OnAggregation);
		}
	}

	void SystemReset()
	{
		if (heatMapInstance) {
			heatMapInstance.transform.parent = null;
			DestroyImmediate (heatMapInstance);
		}
	}

	void PurgeData() {
		if (EditorUtility.DisplayDialog ("Destroy local data?", "You are about to delete your local heatmaps data cache, meaning you'll have to reload from the server. Are you sure?", "Purge", "Cancel")) {
			if (AggregationView != null) {
				AggregationView.PurgeData ();
			}
		}
	}

	void OnAggregation(string jsonPath) {
		ParserView.SetDataPath (jsonPath);
	}

	void OnPointData(HeatPoint[] heatData, float maxDensity, float maxTime)
	{
		//Creating this data allows the renderer to use it on the next Update pass
		pointData = new Dictionary<string, object> ();
		pointData ["heatData"] = heatData;
		pointData ["maxDensity"] = maxDensity;
		pointData ["maxTime"] = maxTime;
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
