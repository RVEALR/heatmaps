/// <summary>
/// Inspector for the Aggregation portion of the Heatmapper.
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using strange.extensions.signal.impl;

namespace UnityAnalyticsHeatmap
{
    public class AggregationInspector
    {

        public IAggregationSettings settings { get; set; }

        public Signal processSignal { get; set; }


        public AggregationInspector()
        {
        }

        public void OnGUI()
        {
            if (settings == null)
            {
                return;
            }

            settings.rawDataPath = EditorGUILayout.TextField(new GUIContent("Data Export URL", "Copy the URL from the 'Editing Project' page of your project dashboard"), settings.rawDataPath);
            settings.startDate = EditorGUILayout.TextField(new GUIContent("Start Date (YYYY-MM-DD)", "Start date as ISO-8601 datetime"), settings.startDate);
            settings.endDate = EditorGUILayout.TextField(new GUIContent("End Date (YYYY-MM-DD)", "End date as ISO-8601 datetime"), settings.endDate);
            settings.space = EditorGUILayout.FloatField(new GUIContent("Space Smooth", "Divider to smooth out x/y/z data"), settings.space);

            GUILayout.BeginHorizontal();
            settings.aggregateTime = EditorGUILayout.Toggle(new GUIContent("Aggregate Time", "Units of space will aggregate, but units of time won't"), settings.aggregateTime);
            if (!settings.aggregateTime)
            {
                settings.time = EditorGUILayout.FloatField(new GUIContent("Smooth", "Divider to smooth out time data"), settings.time);
            }
            else
            {
                settings.time = 1f;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            settings.aggregateAngle = EditorGUILayout.Toggle(new GUIContent("Aggregate Direction", "Units of space will aggregate, but different angles won't"), settings.aggregateAngle);

            if (!settings.aggregateAngle)
            {
                settings.angle = EditorGUILayout.FloatField(new GUIContent("Smooth", "Divider to smooth out angle data"), settings.angle);
            }
            else
            {
                settings.angle = 1f;
            }
            GUILayout.EndHorizontal();

            settings.groupDevices = EditorGUILayout.Toggle(new GUIContent("Aggregate Devices", "Takes no account of unque device IDs. NOTE: Disaggregating device IDs can be slow!"), settings.groupDevices);

            if (GUILayout.Button(new GUIContent("Add Arbitrary Field", "Specify arbitrary additional fields on which to aggregate.")))
            {
                settings.arbitraryGroupFields.Add("Field name");
            }
            for (var a = 0; a < settings.arbitraryGroupFields.Count; a++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                {
                    settings.arbitraryGroupFields.RemoveAt(a);
                    break;
                }
                settings.arbitraryGroupFields[a] = EditorGUILayout.TextField(settings.arbitraryGroupFields[a]);
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button(new GUIContent("Add Whitelist Event", "Specify events to include in the aggregation. If specified, all other events will be excluded.")))
            {
                settings.whiteListEvents.Add("Event name");
            }
            for (var a = 0; a < settings.whiteListEvents.Count; a++)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                {
                    settings.whiteListEvents.RemoveAt(a);
                    break;
                }
                settings.whiteListEvents[a] = EditorGUILayout.TextField(settings.whiteListEvents[a]);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            
            settings.localOnly = GUILayout.Toggle(settings.localOnly, new GUIContent("Local only", "If checked, don't attempt to download raw data from the server."));
            string fetchButtonText = settings.localOnly ? "Process" : "Fetch and Process";
            if (GUILayout.Button(fetchButtonText))
            {
                processSignal.Dispatch();
            }
            GUILayout.EndHorizontal();
        }
    }
}
