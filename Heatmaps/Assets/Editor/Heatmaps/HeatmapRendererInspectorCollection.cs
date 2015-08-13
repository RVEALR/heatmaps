/// <summary>
/// Maintains a list of one or more HeatmapRendererInspectors.
/// </summary>

using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace UnityAnalytics
{
	public class HeatmapRendererInspectorCollection
	{

		private List<HeatmapRendererInspector> inspectors;
		private List<bool> showInspector;
		private GameObject parentInstance;

		public HeatmapRendererInspectorCollection (GameObject parent)
		{
			parentInstance = parent;
			Reset ();
		}

		public static HeatmapRendererInspectorCollection Init(GameObject parent)
		{
			return new HeatmapRendererInspectorCollection (parent);
		}

		public void SetParent(GameObject parent) {
			parentInstance = parent;
		}

		public void OnGUI() {
			var removals = new List<int> ();
			for (int a = 0; a < inspectors.Count; a++) {
				HeatmapRendererInspector inspector = inspectors[a];
				showInspector[a] = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), showInspector[a], "Heatmap " + (a+1), true);
				if (showInspector[a]) {
					if (a > 0) {
						if (GUILayout.Button ("Delete")) {
							removals.Add (a);
						}
					}
					inspector.OnGUI ();
					inspector.SetParent (parentInstance);
				}
			}

			for (int b = removals.Count-1; b > -1; b--) {
				Remove (b);
			}
			EditorGUILayout.BeginVertical ("box");
			if (GUILayout.Button ("Add New Map")) {
				Add ();
			}
			EditorGUILayout.EndVertical ();
		}

		public void Render() {
			//No-op
		}

		public void Reset() {
			if (inspectors != null) {
				foreach (HeatmapRendererInspector inspector in inspectors) {
					inspector.Reset ();
				}
			}
			inspectors = new List<HeatmapRendererInspector>{ new HeatmapRendererInspector(parentInstance) };
			showInspector = new List<bool>{ true };
		}

		protected void Add() {
			inspectors.Add (new HeatmapRendererInspector (parentInstance));
			showInspector.Add (true);
		}

		protected void Remove(int index) {
			inspectors [index].Clean ();
			inspectors.RemoveAt (index);
			showInspector.RemoveAt (index);
		}
	}
}

