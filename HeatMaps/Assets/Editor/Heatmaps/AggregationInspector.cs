/// <summary>
/// Heat map data parser.
/// </summary>
/// This file opens a JSON file and processes it into an array
/// of point data.
/// OnGUI functionality displays the state of the data in the HeatMapper inspector.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityAnalytics
{
	public class AggregationInspector
	{
		private const string DATA_PATH_KEY = "UnityAnalyticsHeatMapAggregationDataPath";
		private string path;

		private Dictionary<string, HeatPoint[]> heatData;

		private int optionIndex = 0;
		private string[] optionKeys;

		public delegate void AggregationHandler (string[] strings);

		private AggregationHandler handler;

		private HeatMapDataParser parser = new HeatMapDataParser ();

		private string startDate = "";
		private string endDate = "";
		private float space = 10f;
		private float time = 10f;
		private bool disaggregateTime = false;

		private List<string> events = new List<string>{};


		public AggregationInspector (AggregationHandler handler)
		{
			this.handler = handler;
			path = EditorPrefs.GetString(DATA_PATH_KEY);

			endDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
			startDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0)));
		}

		public static AggregationInspector Init(AggregationHandler handler)
		{
			return new AggregationInspector (handler);
		}

		private void Dispatch()
		{
			handler (new string[]{});
		}

		public void OnGUI()
		{
			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Find File")) {
				path = EditorUtility.OpenFilePanel ("Locate a JSON file", "", "json");
				EditorPrefs.SetString (DATA_PATH_KEY, path);
			}
			EditorGUILayout.TextField (path);
			GUILayout.EndHorizontal ();


			startDate = EditorGUILayout.TextField ("Start Date (YYYY-MM-DD)", startDate);
			endDate = EditorGUILayout.TextField ("End Date (YYYY-MM-DD)", endDate);

			space = EditorGUILayout.FloatField ("Space Smooth", space);
			time = EditorGUILayout.FloatField ("Time Smooth", time);

			disaggregateTime = EditorGUILayout.Toggle ("Disaggregate Time", disaggregateTime);

			GUILayout.BeginVertical ("box");
			if (GUILayout.Button ("Limit To Events +")) {
				events.Add ("Event name");
			}
			for (var a = 0; a < events.Count; a++) {
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("-", GUILayout.MaxWidth(20f))) {
					events.RemoveAt (a);
					break;
				}
				events [a] = EditorGUILayout.TextField (events [a]);
				GUILayout.EndHorizontal ();
			}
			GUILayout.EndVertical ();

			if (GUILayout.Button ("Process")) {
				parser.LoadData (path, ParseHandler);
			}


			if (heatData != null && optionKeys != null && optionIndex > -1 && optionIndex < optionKeys.Length && heatData.ContainsKey(optionKeys[optionIndex])) {
				int oldIndex = optionIndex;
				optionIndex = EditorGUILayout.Popup("Option", optionIndex, optionKeys);
				if (optionIndex != oldIndex) {
					Dispatch ();
				}
			}
		}

		private void ParseHandler(Dictionary<string, HeatPoint[]> heatData, float maxDensity, float maxTime, string[] options) {
			this.heatData = heatData;
			if (heatData != null) {
				optionKeys = options;
				optionIndex = 0;
				Dispatch ();
			}
		}
	}
}

