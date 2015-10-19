/// <summary>
/// Heat map renderer inspector.
/// </summary>
/// This code manages the portion of the inspector that
/// controls the Heat Map renderer.

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using strange.extensions.signal.impl;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapRendererInspector
    {
        public IRendererSettings settings { get; set; }

        public IRenderInfo renderInfo { get; set; }

        public IRenderData renderData { get; set; }

        public Signal renderSignal { get; set; }

        public Signal renderNewDataSignal { get; set; }

        Heatmapper m_Heatmapper;


        GUIContent[] m_ParticleShapeOptions = new GUIContent[]{ new GUIContent("Cube"), new GUIContent("Arrow"), new GUIContent("Square"), new GUIContent("Triangle") };
        GUIContent[] m_ParticleDirectionOptions = new GUIContent[]{ new GUIContent("YZ"), new GUIContent("XZ"), new GUIContent("XY") };

        public HeatmapRendererInspector(Heatmapper heatmapper)
        {
            m_Heatmapper = heatmapper;
        }

        public void OnGUI()
        {
            if (settings == null)
            {
                return;
            }

            if (renderData != null && renderData.options != null)
            {
                int oldIndex = renderData.currentOptionIndex;
                renderData.currentOptionIndex = EditorGUILayout.Popup("Option", renderData.currentOptionIndex, renderData.options);
                if (renderData.currentOptionIndex != oldIndex)
                {
                    renderNewDataSignal.Dispatch();
                    SetToMaxTime();
                }
            }

            // COLORS
            EditorGUILayout.BeginVertical("box");
            settings.highDensityColor = EditorGUILayout.ColorField(new GUIContent("High Color", "Color for high density data"), settings.highDensityColor);
            settings.mediumDensityColor = EditorGUILayout.ColorField(new GUIContent("Medium Color", "Color for medium density data"), settings.mediumDensityColor);
            settings.lowDensityColor = EditorGUILayout.ColorField(new GUIContent("Low Color", "Color for low density data"), settings.lowDensityColor);

            // THRESHOLDS
            float lowThreshold = settings.lowThreshold;
            float highThreshold = settings.highThreshold;
            lowThreshold = EditorGUILayout.FloatField(new GUIContent("Low Threshold", "Normalized threshold between low-density and medium-density data"), lowThreshold);
            highThreshold = EditorGUILayout.FloatField(new GUIContent("High Threshold", "Normalized threshold between medium-density and high-density data"), highThreshold);
            lowThreshold = Mathf.Min(lowThreshold, settings.highThreshold);
            highThreshold = Mathf.Max(lowThreshold, highThreshold);
            EditorGUILayout.MinMaxSlider(ref lowThreshold, ref highThreshold, 0f, 1f);
            settings.lowThreshold = lowThreshold;
            settings.highThreshold = highThreshold;
            EditorGUILayout.EndVertical();

            // TIME WINDOW
            EditorGUILayout.BeginVertical("box");
            float oldStartTime = settings.startTime;
            float startTime = settings.startTime;
            float oldEndTime = settings.endTime;
            float endTime = settings.endTime;

            startTime = EditorGUILayout.FloatField(new GUIContent("Start Time", "Show only data after this time"), startTime);
            endTime = EditorGUILayout.FloatField(new GUIContent("End Time", "Show only data before this time"), endTime);

            startTime = Mathf.Min(startTime, endTime);
            endTime = Mathf.Max(startTime, endTime);

            EditorGUILayout.MinMaxSlider(ref startTime, ref endTime, 0f, settings.maxTime);
            if (GUILayout.Button(new GUIContent("Max Time", "Set time to maximum extents")))
            {
                SetToMaxTime();
            }
            settings.startTime = startTime;
            settings.endTime = endTime;
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box");

            settings.playSpeed = EditorGUILayout.FloatField(new GUIContent("Play Speed", "Speed at which playback occurs"), settings.playSpeed);

            EditorGUILayout.BeginHorizontal();
            GUIContent restartContent = new GUIContent("<<", "Back to Start");
            if (GUILayout.Button(restartContent))
            {
                Restart();
                settings.isPlaying = false;
            }

            string playTip = settings.isPlaying ? "Pause" : "Play";
            string playText = settings.isPlaying ? "||" : ">";
            GUIContent playContent = new GUIContent(playText, playTip);
            if (GUILayout.Button(playContent))
            {
                if (settings.endTime == settings.maxTime)
                {
                    Restart();
                }
                settings.isPlaying = !settings.isPlaying;
            }
            EditorGUILayout.EndHorizontal();

            bool forceTime = (oldStartTime != settings.startTime || oldEndTime != settings.endTime);
            Update(forceTime);

            EditorGUILayout.EndVertical();

            // PARTICLE SIZE/SHAPE
            EditorGUILayout.BeginVertical("box");

            settings.particleSize = EditorGUILayout.FloatField(new GUIContent("Particle Size", "The display size of an individual data point"), settings.particleSize);
            settings.particleShapeIndex = EditorGUILayout.Popup(new GUIContent("Particle Shape", "The display shape of an individual data point"), settings.particleShapeIndex, m_ParticleShapeOptions);
            if (settings.particleShapeIndex > 0)
            {
                settings.particleDirectionIndex = EditorGUILayout.Popup(new GUIContent("Billboard plane", "For 2D shapes, the facing direction of an individual data point"), settings.particleDirectionIndex, m_ParticleDirectionOptions);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Label("Points in current set: " + renderInfo.totalPoints);
            GUILayout.Label("Points currently displayed: " + renderInfo.currentPoints);
        }

        public void Update(bool forceUpdate = false)
        {
            if (settings == null)
            {
                return;
            }


            if (settings.isPlaying)
            {
                UpdateTime();
            }

            if (settings.isPlaying || forceUpdate)
            {
                renderSignal.Dispatch();
                m_Heatmapper.Repaint();
            }
        }

        void UpdateTime()
        {
            if (settings.isPlaying)
            {
                settings.startTime += settings.playSpeed;
                settings.endTime += settings.playSpeed;
            }
            if (settings.endTime >= settings.maxTime)
            {
                float diff = settings.endTime - settings.startTime;
                settings.endTime = settings.maxTime;
                settings.startTime = Mathf.Max(settings.endTime - diff, 0);
                settings.isPlaying = false;
            }
        }

        void SetToMaxTime()
        {
            settings.startTime = 0f;
            settings.endTime = renderData.maxTime;
            settings.maxTime = renderData.maxTime;
        }

        void Restart()
        {
            float diff = settings.endTime - settings.startTime;
            settings.startTime = 0;
            settings.endTime = settings.startTime + diff;
        }
    }
}
