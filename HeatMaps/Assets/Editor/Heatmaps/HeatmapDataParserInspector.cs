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
	public class HeatMapDataParserInspector
	{
		private const string DATA_PATH_KEY = "UnityAnalyticsHeatMapDataPath";
		private string path;

		private Dictionary<string, HeatPoint[]> heatData;
		private float maxDensity = 0;
		private float maxTime = 0;
		private int optionIndex = 0;
		private string[] optionKeys;

		public delegate void PointHandler (HeatPoint[] heatData, float maxDensity, float maxTime);

		private PointHandler handler;

		private HeatMapDataParser parser = new HeatMapDataParser ();


		public HeatMapDataParserInspector (PointHandler handler)
		{
			this.handler = handler;
			path = EditorPrefs.GetString(DATA_PATH_KEY);
		}

		public static HeatMapDataParserInspector Init(PointHandler handler)
		{
			return new HeatMapDataParserInspector (handler);
		}

		private void Dispatch()
		{
			handler (heatData [optionKeys [optionIndex]], maxDensity, maxTime);
		}

		public void OnGUI()
		{
			

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Find File")) {
				path = EditorUtility.OpenFilePanel ("Locate a JSON file", "", "json");
				EditorPrefs.SetString (DATA_PATH_KEY, path);
			}
			path = EditorGUILayout.TextField (path);
			GUILayout.EndHorizontal ();

			if (heatData != null && optionKeys != null && optionIndex > -1 && optionIndex < optionKeys.Length && heatData.ContainsKey(optionKeys[optionIndex])) {
				int oldIndex = optionIndex;
				optionIndex = EditorGUILayout.Popup("Option", optionIndex, optionKeys);
				if (optionIndex != oldIndex) {
					Dispatch ();
				}
			}
			if (GUILayout.Button ("Load")) {
				parser.LoadData (path, ParseHandler);
			}
		}

		private void ParseHandler(Dictionary<string, HeatPoint[]> heatData, float maxDensity, float maxTime, string[] options) {
			this.heatData = heatData;
			if (heatData != null) {
				optionKeys = options;
				optionIndex = 0;
				this.maxDensity = maxDensity;
				this.maxTime = maxTime;
				Dispatch ();
			}
		}
	}
}

