/// <summary>
/// Inspector for the Aggregation portion of the Heatmapper.
/// </summary>

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
		private const string URL_KEY = "UnityAnalyticsHeatmapDataExportUrlKey";

		private const string LAST_IMPORT_PATH_KEY = "UnityAnalyticsHeatmapAggregationLastImportPath";
		private const string SPACE_KEY = "UnityAnalyticsHeatmapAggregationSpace";
		private const string KEY_TO_TIME = "UnityAnalyticsHeatmapAggregationTime";
		private const string ANGLE_KEY = "UnityAnalyticsHeatmapAggregationAngle";
		private const string AGGREGATE_TIME_KEY = "UnityAnalyticsHeatmapAggregationAggregateTime";
		private const string AGGREGATE_ANGLE_KEY = "UnityAnalyticsHeatmapAggregationAggregateAngle";
		private const string EVENTS_KEY = "UnityAnalyticsHeatmapAggregationEvents";

		private string rawDataPath = "";

		private const float DEFAULT_SPACE = 10f;
		private const float DEFAULT_TIME = 10f;
		private const float DEFAULT_ANGLE = 15f;

		private Dictionary<string, HeatPoint[]> heatData;

		public delegate void AggregationHandler (string jsonPath);
		private AggregationHandler handler;

		private RawEventClient rawEventClient;
		private HeatmapAggregator aggregator;

		private string startDate = "";
		private string endDate = "";
		private float space = DEFAULT_SPACE;
		private float time = DEFAULT_TIME;
		private float angle = DEFAULT_ANGLE;
		private bool aggregateTime = true;
		private bool aggregateAngle = true;

		private List<string> events = new List<string>{};

		public AggregationInspector (RawEventClient client, HeatmapAggregator aggregator)
		{
			this.aggregator = aggregator;
			this.rawEventClient = client;

			// Restore cached paths
			rawDataPath = EditorPrefs.GetString(URL_KEY);

			// Set dates based on today (should this be cached?)
			endDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
			startDate = String.Format("{0:yyyy-MM-dd}", DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0)));

			// Restore other options
			space = EditorPrefs.GetFloat (SPACE_KEY) == 0 ? DEFAULT_SPACE : EditorPrefs.GetFloat (SPACE_KEY);
			time = EditorPrefs.GetFloat (KEY_TO_TIME) == 0 ?  DEFAULT_TIME : EditorPrefs.GetFloat (KEY_TO_TIME);
			angle = EditorPrefs.GetFloat (ANGLE_KEY) == 0 ?  DEFAULT_ANGLE : EditorPrefs.GetFloat (ANGLE_KEY);
			aggregateTime = EditorPrefs.GetBool (AGGREGATE_TIME_KEY);
			aggregateAngle = EditorPrefs.GetBool (AGGREGATE_ANGLE_KEY);

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

		public static AggregationInspector Init(RawEventClient client, HeatmapAggregator aggregator)
		{
			return new AggregationInspector (client, aggregator);
		}

		public void PurgeData() {
			rawEventClient.PurgeData ();
		}

		public void Fetch(AggregationHandler handler) {
			this.handler = handler;
			if (!string.IsNullOrEmpty (rawDataPath)) {
				EditorPrefs.SetString (URL_KEY, rawDataPath);
				DateTime start, end;
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

				rawEventClient.Fetch (rawDataPath, new UnityAnalyticsEventType[]{ UnityAnalyticsEventType.custom }, start, end, rawFetchHandler);
			}
		}

		public void OnGUI()
		{
			string oldPath = rawDataPath;
			rawDataPath = EditorGUILayout.TextField (new GUIContent("Data Export URL", "Copy the URL from the 'Editing Project' page of your project dashboard"), rawDataPath);
			if (oldPath != rawDataPath && !string.IsNullOrEmpty (rawDataPath)) {
				EditorPrefs.SetString (URL_KEY, rawDataPath);
			}

			startDate = EditorGUILayout.TextField (new GUIContent("Start Date (YYYY-MM-DD)",  "Start date as ISO-8601 datetime"), startDate);
			endDate = EditorGUILayout.TextField (new GUIContent("End Date (YYYY-MM-DD)",  "End date as ISO-8601 datetime"), endDate);

			float oldSpace = space;
			space = EditorGUILayout.FloatField (new GUIContent("Space Smooth", "Divider to smooth out x/y/z data"), space);
			if (oldSpace != space) {
				EditorPrefs.SetFloat (SPACE_KEY, space);
			}

			GUILayout.BeginHorizontal ();
			bool oldAggregateTime = aggregateTime;
			aggregateTime = EditorGUILayout.Toggle (new GUIContent("Aggregate Time", "Units of space will aggregate, but units of time won't"), aggregateTime);
			if (oldAggregateTime != aggregateTime) {
				EditorPrefs.SetBool (AGGREGATE_TIME_KEY, aggregateTime);
			}
			if (!aggregateTime) {
				float oldTime = time;
				time = EditorGUILayout.FloatField (new GUIContent ("Smooth", "Divider to smooth out time data"), time);
				if (oldTime != time) {
					EditorPrefs.SetFloat (KEY_TO_TIME, time);
				}
			} else {
				time = 1f;
			}
			GUILayout.EndHorizontal ();

			GUILayout.BeginHorizontal ();
			bool oldAggregateAngle = aggregateAngle;
			aggregateAngle = EditorGUILayout.Toggle (new GUIContent("Aggregate Direction", "Units of space will aggregate, but different angles won't"), aggregateAngle);
			if (oldAggregateAngle != aggregateAngle) {
				EditorPrefs.SetBool (AGGREGATE_ANGLE_KEY, aggregateAngle);
			}
			if (!aggregateAngle) {
				float oldAngle = angle;
				angle = EditorGUILayout.FloatField (new GUIContent ("Smooth", "Divider to smooth out angle data"), angle);
				if (oldAngle != angle) {
					EditorPrefs.SetFloat (ANGLE_KEY, angle);
				}
			} else {
				angle = 1f;
			}
			GUILayout.EndHorizontal ();

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
		}

		private void rawFetchHandler(List<string> fileList) {

			if (fileList.Count == 0) {
				Debug.LogWarning ("No matching data found.");
			} else {
				DateTime start, end;
				try {
					start = DateTime.Parse (startDate);
				} catch {
					start = DateTime.Parse ("2000-01-01");
				}
				try {
					end = DateTime.Parse (endDate);
				} catch {
					end = DateTime.UtcNow;
				}

				aggregator.Process (aggregationHandler, fileList, start, end, space, time, angle, !aggregateTime, !aggregateAngle, events);
			}
		}

		private void aggregationHandler(string jsonPath) {
			handler (jsonPath);
		}
	}
}
