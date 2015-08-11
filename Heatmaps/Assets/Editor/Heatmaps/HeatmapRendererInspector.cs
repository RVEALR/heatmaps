﻿/// <summary>
/// Heat map renderer inspector.
/// </summary>
/// This code manages the portion of the inspector that
/// controls the Heat Map renderer.

using System;
using UnityEngine;
using System.Collections;
using UnityEditor;

namespace UnityAnalytics
{
	public class HeatmapRendererInspector
	{
		private const string HIGH_DENSITY_COLOR_KEY = "UnityAnalyticsHeatmapHighDensityColor";
		private const string MEDIUM_DENSITY_COLOR_KEY = "UnityAnalyticsHeatmapMediumDensityColor";
		private const string LOW_DENSITY_COLOR_KEY = "UnityAnalyticsHeatmapLowDensityColor";

		private const string HIGH_THRESHOLD_KEY = "UnityAnalyticsHeatmapHighThreshold";
		private const string LOW_THRESHOLD_KEY = "UnityAnalyticsHeatmapLowThreshold";

		private const string START_TIME_KEY = "UnityAnalyticsHeatmapStartTime";
		private const string END_TIME_KEY = "UnityAnalyticsHeatmapEndTime";

		private const string PARTICLE_SIZE_KEY = "UnityAnalyticsHeatmapParticleSize";
		private const string PARTICLE_SHAPE_KEY = "UnityAnalyticsHeatmapParticleShape";

		Color HighDensityColor = new Color(1f, 0, 0, .1f);
		Color MediumDensityColor = new Color(1f, 1f, 0, .1f);
		Color LowDensityColor = new Color(0, 1f, 1f, .1f);

		float HighThreshold = .9f;
		float LowThreshold = .1f;

		float StartTime = 0f;
		float EndTime = 1f;
		float MaxTime = 1f;

		float ParticleSize = 1f;
		int ParticleShapeIndex = 0;
		string[] particleShapeOptions = new string[]{"Cube", "Square", "Triangle"};
		RenderShape[] particleShapeIds = new RenderShape[]{RenderShape.CUBE, RenderShape.SQUARE, RenderShape.TRI};

		private GameObject gameObject;


		public HeatmapRendererInspector ()
		{
			HighDensityColor =  GetColorFromString(EditorPrefs.GetString(HIGH_DENSITY_COLOR_KEY));
			MediumDensityColor =  GetColorFromString(EditorPrefs.GetString(MEDIUM_DENSITY_COLOR_KEY));
			LowDensityColor =  GetColorFromString(EditorPrefs.GetString(LOW_DENSITY_COLOR_KEY));

			HighThreshold = EditorPrefs.GetFloat (HIGH_THRESHOLD_KEY);
			LowThreshold = EditorPrefs.GetFloat (LOW_THRESHOLD_KEY);

			StartTime = EditorPrefs.GetFloat (START_TIME_KEY);
			EndTime = EditorPrefs.GetFloat (END_TIME_KEY);

			ParticleSize = EditorPrefs.GetFloat (PARTICLE_SIZE_KEY);

			ParticleShapeIndex = EditorPrefs.GetInt (PARTICLE_SHAPE_KEY);
		}

		public static HeatmapRendererInspector Init()
		{
			return new HeatmapRendererInspector ();
		}

		public void OnGUI()
		{
			//COLORS
			EditorGUILayout.BeginVertical ("box");
			HighDensityColor = SetAndSaveColor ("High Color", HIGH_DENSITY_COLOR_KEY, HighDensityColor);
			MediumDensityColor = SetAndSaveColor ("Medium Color", MEDIUM_DENSITY_COLOR_KEY, MediumDensityColor);
			LowDensityColor = SetAndSaveColor ("Low Color", LOW_DENSITY_COLOR_KEY, LowDensityColor);

			//THRESHOLDS
			var oldLowThreshold = LowThreshold;
			var oldHighThreshold = HighThreshold;

			LowThreshold = EditorGUILayout.FloatField ("Low Threshold", LowThreshold);
			HighThreshold = EditorGUILayout.FloatField ("High Threshold", HighThreshold);

			EditorGUILayout.MinMaxSlider(ref LowThreshold, ref HighThreshold, 0f, 1f);
			if (oldLowThreshold != LowThreshold) {
				EditorPrefs.SetFloat (LOW_THRESHOLD_KEY, LowThreshold);
			}
			if (oldHighThreshold != HighThreshold) {
				EditorPrefs.SetFloat (HIGH_THRESHOLD_KEY, HighThreshold);
			}
			EditorGUILayout.EndVertical ();


			//TIME WINDOW
			EditorGUILayout.BeginVertical ("box");
			var oldStartTime = StartTime;
			var oldEndTime = EndTime;

			StartTime = EditorGUILayout.FloatField ("Start Time", StartTime);
			EndTime = EditorGUILayout.FloatField ("End Time", EndTime);

			EditorGUILayout.MinMaxSlider(ref StartTime, ref EndTime, 0f, MaxTime);
			if (GUILayout.Button ("Max Time")) {
				StartTime = 0;
				EndTime = MaxTime;
			}
			if (oldStartTime != StartTime) {
				EditorPrefs.SetFloat (START_TIME_KEY, StartTime);
			}
			if (oldEndTime != EndTime) {
				EditorPrefs.SetFloat (END_TIME_KEY, EndTime);
			}
			EditorGUILayout.EndVertical ();

			//PARTICLE SIZE/SHAPE
			EditorGUILayout.BeginVertical ("box");
			var oldParticleSize = ParticleSize;
			ParticleSize = EditorGUILayout.FloatField ("Particle Size", ParticleSize);
			if (oldParticleSize != ParticleSize) {
				EditorPrefs.SetFloat (PARTICLE_SIZE_KEY, ParticleSize);
			}

			var oldParticleShapeIndex = ParticleShapeIndex;
			ParticleShapeIndex = EditorGUILayout.Popup ("Particle Shape", ParticleShapeIndex, particleShapeOptions);
			if (oldParticleShapeIndex != ParticleShapeIndex) {
				EditorPrefs.SetInt (PARTICLE_SHAPE_KEY, ParticleShapeIndex);
			}
			EditorGUILayout.EndVertical ();

			//PASS VALUES TO RENDERER
			if (gameObject != null) {
				IHeatmapRenderer r = gameObject.GetComponent<IHeatmapRenderer> () as IHeatmapRenderer;
				r.UpdateColors (new Color[]{LowDensityColor, MediumDensityColor, HighDensityColor});
				r.UpdateThresholds (new float[]{LowThreshold, HighThreshold});
				r.pointSize = ParticleSize;
				r.UpdateTimeLimits (StartTime, EndTime);
				r.UpdateRenderStyle(particleShapeIds[ParticleShapeIndex]);
				SceneView.RepaintAll ();
			}
		}

		public void SetMaxTime(float maxTime) {
			MaxTime = maxTime;
		}

		public void SetGameObject(GameObject go) {
			gameObject = go;
		}

		private string FormatColorToString(Color c) {
			return c.r + "|" + c.g + "|" + c.b + "|" + c.a;
		}

		private Color GetColorFromString(string s) {
			string[] cols = s.Split ('|');

			float r = 0, g = 0, b = 0, a = 1;
			try {
				r = float.Parse(cols [0]);
				g = float.Parse(cols [1]);
				b = float.Parse(cols [2]);
				a = float.Parse(cols [3]);
			}
			catch {
				r = 1f;
				g = 1f;
				b = 0f;
				a = 1f;
			}
			return new Color (r, g, b, a);;
		}

		private Color SetAndSaveColor(string label, string key, Color currentColor) {
			var oldColor = currentColor;
			Color updatedColor = EditorGUILayout.ColorField (label, currentColor);
			if (oldColor != updatedColor) {
				string colorString = FormatColorToString (updatedColor);
				EditorPrefs.SetString (key, colorString);
			}
			return updatedColor;
		}
	}
}
