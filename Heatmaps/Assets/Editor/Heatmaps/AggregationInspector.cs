

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace UnityAnalyticsHeatmap
{
	public class AggregationInspector
	{
		private const string DATA_PATH_KEY = "UnityAnalyticsHeatmapAggregationDataPath";
		private const string LAST_IMPORT_PATH_KEY = "UnityAnalyticsHeatmapAggregationLastImportPath";
		private const string SPACE_KEY = "UnityAnalyticsHeatmapAggregationSpace";
		private const string KEY_TO_TIME = "UnityAnalyticsHeatmapAggregationTime";
		private const string DISAGGREGATE_KEY = "UnityAnalyticsHeatmapAggregationDisaggregate";
		private const string EVENTS_KEY = "UnityAnalyticsHeatmapAggregationEvents";
		private const string TRIM_DATES_KEY = "UnityAnalyticsHeatmapAggregationTrimDates";

		private const string NEW_PATH_TEXT = "New file path";

		private const float DEFAULT_SPACE = 10f;
		private const float DEFAULT_TIME = 10f;

		private Dictionary<string, HeatPoint[]> heatData;

		public delegate void AggregationHandler (string[] strings);

		private AggregationHandler handler;

		private HeatmapAggregator processor = new HeatmapAggregator ();

		private List<string> inputFiles = new List<string>{NEW_PATH_TEXT};
		private string lastImportPath = "";
		private string startDate = "";
		private string endDate = "";
		private float space = DEFAULT_SPACE;
		private float time = DEFAULT_TIME;
		private bool disaggregateTime = false;
		private bool trimDates = false;

		private List<string> events = new List<string>{};


		public AggregationInspector (AggregationHandler handler)
		{
			this.handler = handler;

			// Restore cached paths
			string loadedPath = EditorPrefs.GetString (DATA_PATH_KEY);
			string[] paths;
			if (string.IsNullOrEmpty(loadedPath)) {
				paths = new string[]{};
			} else {
				paths = loadedPath.Split ('|');
			}
			inputFiles = new List<string>(paths);

			// Set dates based on today (should this be cached?)
			endDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
			startDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0)));

			// Restore other options
			space = EditorPrefs.GetFloat (SPACE_KEY) == 0 ? DEFAULT_SPACE : EditorPrefs.GetFloat (SPACE_KEY);
			time = EditorPrefs.GetFloat (KEY_TO_TIME) == 0 ?  DEFAULT_TIME : EditorPrefs.GetFloat (KEY_TO_TIME);
			disaggregateTime = EditorPrefs.GetBool (DISAGGREGATE_KEY);
			trimDates = EditorPrefs.GetBool (TRIM_DATES_KEY);

			// Restore list of events
			string loadedEvents = EditorPrefs.GetString (EVENTS_KEY);
			string[] eventsList;
			if (string.IsNullOrEmpty (loadedEvents)) {
				eventsList = new string[]{};
			} else {
				eventsList = loadedEvents.Split ('|');
			}
			events = new List<string>(eventsList);
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
			GUILayout.BeginVertical ("box");
			if (GUILayout.Button ("Add File")) {
				int insertPoint = inputFiles.Count;

				if (inputFiles.Count == 1 && inputFiles [0] == NEW_PATH_TEXT) {
					insertPoint = 0;
				} else {
					string newFilePath = EditorUtility.OpenFilePanel ("Locate your downloaded file", lastImportPath, "txt");
					if (!string.IsNullOrEmpty (newFilePath)) {
						inputFiles.Add (NEW_PATH_TEXT);
						inputFiles [insertPoint] = newFilePath;
						lastImportPath = Path.GetDirectoryName(newFilePath);
						EditorPrefs.SetString (LAST_IMPORT_PATH_KEY, newFilePath);
					}
				}
				SavePaths ();
			}
			for (var a = 0; a < inputFiles.Count; a++) {
				GUILayout.BeginHorizontal ();
				if (GUILayout.Button ("-", GUILayout.MaxWidth(20f))) {
					inputFiles.RemoveAt (a);
					SavePaths ();
					break;
				}
				inputFiles [a] = EditorGUILayout.TextField (inputFiles [a]);
				GUILayout.EndHorizontal ();
			}
			GUILayout.EndVertical ();

			bool oldTrimDates = trimDates;
			trimDates = EditorGUILayout.Toggle (new GUIContent("Trim Dates", "Exclude certain date ranges"), trimDates);
			if (oldTrimDates != trimDates) {
				EditorPrefs.SetBool (TRIM_DATES_KEY, trimDates);
			}
			if (trimDates) {
				startDate = EditorGUILayout.TextField (new GUIContent("Start Date (YYYY-MM-DD)", "Start date as ISO-8601 datetime"), startDate);
				endDate = EditorGUILayout.TextField (new GUIContent("End Date (YYYY-MM-DD)", "End date as ISO-8601 datetime"), endDate);
			}

			float oldSpace = space;
			space = EditorGUILayout.FloatField (new GUIContent("Space Smooth", "Divider to smooth out x/y/z data"), space);
			if (oldSpace != space) {
				EditorPrefs.SetFloat (SPACE_KEY, space);
			}

			float oldTime = time;
			time = EditorGUILayout.FloatField (new GUIContent("Time Smooth", "Divider to smooth out time data"), time);
			if (oldTime != time) {
				EditorPrefs.SetFloat (KEY_TO_TIME, time);
			}

			bool oldDisaggregateTime = disaggregateTime;
			disaggregateTime = EditorGUILayout.Toggle (new GUIContent("Disaggregate Time", "Units of space will aggregate, but units of time won't"), disaggregateTime);
			if (oldDisaggregateTime != disaggregateTime) {
				EditorPrefs.SetBool (DISAGGREGATE_KEY, disaggregateTime);
			}

			GUILayout.BeginVertical ("box");
			string oldEventsString = string.Join ("|", events.ToArray());
			if (GUILayout.Button (new GUIContent("Limit To Events", "Specify events to include in the aggregation. If specified, all other events will be excluded."))) {
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
			string currentEventsString = string.Join ("|", events.ToArray());

			if (oldEventsString != currentEventsString) {
				EditorPrefs.SetString (EVENTS_KEY, currentEventsString);
			}

			GUILayout.EndVertical ();

			if (GUILayout.Button (new GUIContent("Process", "Aggregate as specified above"))) {
				DateTime start, end;

				if (trimDates) {
					try {
						start = DateTime.Parse (startDate);
					} catch {
						throw new Exception ("The start date is not properly formatted. Correct format is YYYY-MM-DD.");
					}
					try {
						end = DateTime.Parse (endDate);
					} catch {
						throw new Exception ("The end date is not properly formatted. Correct format is YYYY-MM-DD.");
					}
				} else {
					start = DateTime.Parse ("2000-01-01");
					end = DateTime.UtcNow;
				}
				processor.Process (inputFiles, start, end, space, time, disaggregateTime, events);
			}
		}

		void SavePaths() {
			string pathsString = string.Join ("|", inputFiles.ToArray());
			EditorPrefs.SetString (DATA_PATH_KEY, pathsString);
		}
	}
}

