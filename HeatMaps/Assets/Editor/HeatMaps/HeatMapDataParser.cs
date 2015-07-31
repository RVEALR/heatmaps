/// <summary>
/// Heat map data parser.
/// </summary>
/// This file opens a JSON file and processes it into an array
/// of point data.
/// OnGUI functionality displays the state of the data in the HeatMapper inspector.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MiniJSON;
using UnityEditor;
using UnityEngine;

namespace UnityAnalytics
{
	public class HeatMapDataParser
	{
		private const string DATA_PATH_KEY = "UnityAnalyticsHeatMapDataPath";
		private string path;


		private Dictionary<string, HeatPoint[]> heatData;
		private float maxDensity;
		private float maxTime;
		private int optionIndex = 0;
		private string[] optionKeys;

		public delegate void PointHandler (HeatPoint[] heatData, float maxDensity, float maxTime);

		private PointHandler handler;


		public HeatMapDataParser (PointHandler handler)
		{
			this.handler = handler;
			path = EditorPrefs.GetString(DATA_PATH_KEY);
		}

		public static HeatMapDataParser Init(PointHandler handler)
		{
			return new HeatMapDataParser (handler);
		}

		private void Dispatch()
		{
			handler (heatData [optionKeys [optionIndex]], maxDensity, maxTime);
		}

		public void OnGUI()
		{
			GUILayout.Label ("Data", EditorStyles.boldLabel);
			path = EditorGUILayout.TextField ("Source", path);

			GUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Find File")) {
				path = EditorUtility.OpenFilePanel ("Locate a JSON file", "", "json");
				EditorPrefs.SetString (DATA_PATH_KEY, path);
			}
			if (GUILayout.Button ("Load")) {
				LoadData ();
			}
			GUILayout.EndHorizontal ();

			if (heatData != null && optionKeys != null && optionIndex > -1 && optionIndex < optionKeys.Length && heatData.ContainsKey(optionKeys[optionIndex])) {
				int oldIndex = optionIndex;
				optionIndex = EditorGUILayout.Popup("Option", optionIndex, optionKeys);
				if (optionIndex != oldIndex) {
					Dispatch ();
				}
			}
		}

		void LoadData() {
			if (!string.IsNullOrEmpty (path)) {
				// Handle any problems that might arise when reading the text
				try
				{
					StreamReader reader = new StreamReader(path);
					using (reader)
					{
						ConsumeHeatmapData (reader.ReadToEnd());
					}
				}
				// If anything broke in the try block, we throw an exception with information
				// on what didn't work
				catch (Exception e)
				{
					Debug.Log(e.Message);
				}
			}
		}

		void ConsumeHeatmapData(string text) 
		{
			heatData = new Dictionary<string, HeatPoint[]> ();
			ArrayList keys = new ArrayList();
			maxDensity = 0;
			maxTime = 0;

			try
			{
				Dictionary<string, object> data = Json.Deserialize(text) as Dictionary<string, object>;
				foreach(KeyValuePair<string, object> kv in data)
				{
					keys.Add(kv.Key);

					var pointList = kv.Value as List<object>;
					HeatPoint[] array = new HeatPoint[pointList.Count];
					for (int a = 0, aa = pointList.Count; a < aa; a++)
					{
						array[a] = new HeatPoint();
						float x = 0, y = 0, z = 0, t = 0;
						float d = 0;
						Dictionary<string, object> pt = pointList [a] as Dictionary<string, object>;

						foreach (KeyValuePair<string,object> pointKV in pt) {
							switch (pointKV.Key)
							{
								case "x":
									x = (float)(Double)pointKV.Value;
									break;
								case "y":
									y = (float)(Double)pointKV.Value;
									break;
								case "z":
									z = (float)(Double)pointKV.Value;
									break;
								case "t":
									t = (float)(Double)pointKV.Value;
									break;
								case "d":
									d = (float)(Int64)pointKV.Value;
									break;
							}
						}
						array[a].position = new Vector3(x,y,z);
						array[a].density = d;
					array [a].time = t;
						maxDensity = Mathf.Max (d, maxDensity);
						maxTime = Mathf.Max (array[a].time, maxTime);
					}
					heatData[kv.Key] = array;
				}
			}
			catch (Exception e)
			{
				Debug.LogError("JSON Parse error. " + e.Message);
			}

			if (heatData != null) {
				optionKeys = keys.ToArray(typeof(string)) as string[];
				optionIndex = 0;
				Dispatch ();
			}
		}
	}
}

