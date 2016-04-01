﻿/// <summary>
/// Heat map renderer inspector.
/// </summary>
/// This code manages the portion of the inspector that
/// controls the Heat Map renderer.

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapRendererInspector
    {
        const string k_RemapColorKey = "UnityAnalyticsHeatmapRemapColorKey";
        const string k_RemapColorFieldKey = "UnityAnalyticsHeatmapRemapColorFieldKey";

        const string k_HighColorDensityKey = "UnityAnalyticsHeatmapHighDensityColor";
        const string k_MediumColorDensityKey = "UnityAnalyticsHeatmapMediumDensityColor";
        const string k_LowColorDensityKey = "UnityAnalyticsHeatmapLowDensityColor";

        const string k_HighThresholdKey = "UnityAnalyticsHeatmapHighThreshold";
        const string k_LowThresholdKey = "UnityAnalyticsHeatmapLowThreshold";

        const string k_StartTimeKey = "UnityAnalyticsHeatmapStartTime";
        const string k_EndTimeKey = "UnityAnalyticsHeatmapEndTime";
        const string k_PlaySpeedKey = "UnityAnalyticsHeatmapPlaySpeed";

        const string k_ParticleSizeKey = "UnityAnalyticsHeatmapParticleSize";
        const string k_ParticleShapeKey = "UnityAnalyticsHeatmapParticleShape";
        const string k_ParticleDirectionKey = "UnityAnalyticsHeatmapParticleDirection";

        Heatmapper m_Heatmapper;


        bool m_RemapColor;

        Color m_HighDensityColor = new Color(1f, 0, 0, .1f);
        Color m_MediumDensityColor = new Color(1f, 1f, 0, .1f);
        Color m_LowDensityColor = new Color(0, 1f, 1f, .1f);

        float m_HighThreshold = .9f;
        float m_LowThreshold = .1f;

        float m_StartTime = 0f;
        float m_EndTime = 1f;
        float m_MaxTime = 1f;

        float m_ParticleSize = 1f;
        int m_ParticleShapeIndex = 0;
        GUIContent[] m_ParticleShapeOptions = new GUIContent[]{ new GUIContent("Cube"), new GUIContent("Arrow"), new GUIContent("Point To Point"), new GUIContent("Square"), new GUIContent("Triangle") };
        RenderShape[] m_ParticleShapeIds = new RenderShape[]{ RenderShape.Cube, RenderShape.Arrow, RenderShape.PointToPoint, RenderShape.Square, RenderShape.Triangle };

        int m_ParticleDirectionIndex = 0;
        GUIContent[] m_ParticleDirectionOptions = new GUIContent[]{ new GUIContent("YZ"), new GUIContent("XZ"), new GUIContent("XY") };
        RenderDirection[] m_ParticleDirectionIds = new RenderDirection[]{ RenderDirection.YZ, RenderDirection.XZ, RenderDirection.XY };

        bool m_IsPlaying = false;
        float m_PlaySpeed = 1f;

        GameObject m_GameObject;


        public HeatmapRendererInspector()
        {
            m_HighDensityColor = GetColorFromString(EditorPrefs.GetString(k_HighColorDensityKey), m_HighDensityColor);
            m_MediumDensityColor = GetColorFromString(EditorPrefs.GetString(k_MediumColorDensityKey), m_MediumDensityColor);
            m_LowDensityColor = GetColorFromString(EditorPrefs.GetString(k_LowColorDensityKey), m_LowDensityColor);

            m_HighThreshold = EditorPrefs.GetFloat(k_HighThresholdKey, m_HighThreshold);
            m_LowThreshold = EditorPrefs.GetFloat(k_LowThresholdKey, m_LowThreshold);

            m_StartTime = EditorPrefs.GetFloat(k_StartTimeKey, m_StartTime);
            m_EndTime = EditorPrefs.GetFloat(k_EndTimeKey, m_EndTime);
            m_PlaySpeed = EditorPrefs.GetFloat(k_PlaySpeedKey, m_PlaySpeed);

            m_ParticleSize = EditorPrefs.GetFloat(k_ParticleSizeKey, m_ParticleSize);

            m_ParticleShapeIndex = EditorPrefs.GetInt(k_ParticleShapeKey, m_ParticleShapeIndex);
            m_ParticleDirectionIndex = EditorPrefs.GetInt(k_ParticleDirectionKey, m_ParticleDirectionIndex);
        }

        public static HeatmapRendererInspector Init(Heatmapper heatmapper)
        {
            var inspector = new HeatmapRendererInspector();
            inspector.m_Heatmapper = heatmapper;
            return inspector;
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical("box");
            // COLORS
            EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            m_LowDensityColor = SetAndSaveColor(new GUIContent("", "Color for low density data"), k_LowColorDensityKey, m_LowDensityColor);
            m_MediumDensityColor = SetAndSaveColor(new GUIContent("", "Color for medium density data"), k_MediumColorDensityKey, m_MediumDensityColor);
            m_HighDensityColor = SetAndSaveColor(new GUIContent("", "Color for high density data"), k_HighColorDensityKey, m_HighDensityColor);

            EditorGUILayout.EndHorizontal();

            // THRESHOLDS

            EditorGUILayout.LabelField("Color Thresholds");
            var oldLowThreshold = m_LowThreshold;
            var oldHighThreshold = m_HighThreshold;
            EditorGUILayout.BeginHorizontal();
            m_LowThreshold = EditorGUILayout.FloatField(m_LowThreshold);
            m_HighThreshold = EditorGUILayout.FloatField(m_HighThreshold);

            m_LowThreshold = Mathf.Min(m_LowThreshold, m_HighThreshold);
            m_HighThreshold = Mathf.Max(m_LowThreshold, m_HighThreshold);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.MinMaxSlider(ref m_LowThreshold, ref m_HighThreshold, 0f, 1f);
            if (oldLowThreshold != m_LowThreshold)
            {
                EditorPrefs.SetFloat(k_LowThresholdKey, m_LowThreshold);
            }
            if (oldHighThreshold != m_HighThreshold)
            {
                EditorPrefs.SetFloat(k_HighThresholdKey, m_HighThreshold);
            }

            m_RemapColor = EditorGUILayout.Toggle(new GUIContent("Remap color to field", "Do stuff"), m_RemapColor);
            EditorGUILayout.EndVertical();


            // TIME WINDOW
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Time", EditorStyles.boldLabel);
            var oldStartTime = m_StartTime;
            var oldEndTime = m_EndTime;

            m_StartTime = EditorGUILayout.FloatField(new GUIContent("Start", "Show only data after this time"), m_StartTime);
            m_EndTime = EditorGUILayout.FloatField(new GUIContent("End", "Show only data before this time"), m_EndTime);

            m_StartTime = Mathf.Min(m_StartTime, m_EndTime);
            m_EndTime = Mathf.Max(m_StartTime, m_EndTime);

            EditorGUILayout.MinMaxSlider(ref m_StartTime, ref m_EndTime, 0f, m_MaxTime);
            if (GUILayout.Button(new GUIContent("Max Time", "Set time to maximum extents")))
            {
                m_StartTime = 0;
                m_EndTime = m_MaxTime;
            }
            var oldPlaySpeed = m_PlaySpeed;
            m_PlaySpeed = EditorGUILayout.FloatField(new GUIContent("Play Speed", "Speed at which playback occurs"), m_PlaySpeed);

            if (oldPlaySpeed != m_PlaySpeed)
            {
                EditorPrefs.SetFloat(k_PlaySpeedKey, m_PlaySpeed);
            }
            EditorGUILayout.BeginHorizontal();
            GUIContent restartContent = new GUIContent("<<", "Back to Start");
            if (GUILayout.Button(restartContent))
            {
                Restart();
                m_IsPlaying = false;
            }

            string playTip = m_IsPlaying ? "Pause" : "Play";
            string playText = m_IsPlaying ? "||" : ">";
            GUIContent playContent = new GUIContent(playText, playTip);
            if (GUILayout.Button(playContent))
            {
                if (m_StartTime < m_MaxTime && m_EndTime == m_MaxTime)
                {
                    Restart();
                }
                m_IsPlaying = !m_IsPlaying;
            }
            EditorGUILayout.EndHorizontal();

            bool forceTime = false;
            if (oldStartTime != m_StartTime)
            {
                forceTime = true;
                EditorPrefs.SetFloat(k_StartTimeKey, m_StartTime);
            }
            if (oldEndTime != m_EndTime)
            {
                forceTime = true;
                EditorPrefs.SetFloat(k_EndTimeKey, m_EndTime);
            }

            Update(forceTime);

            EditorGUILayout.EndVertical();

            // PARTICLE SIZE/SHAPE
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Particle", EditorStyles.boldLabel);
            var oldParticleSize = m_ParticleSize;
            m_ParticleSize = EditorGUILayout.FloatField(new GUIContent("Size", "The display size of an individual data point"), m_ParticleSize);
            m_ParticleSize = Mathf.Max(0.05f, m_ParticleSize);
            if (oldParticleSize != m_ParticleSize)
            {
                EditorPrefs.SetFloat(k_ParticleSizeKey, m_ParticleSize);
            }

            var oldParticleShapeIndex = m_ParticleShapeIndex;
            m_ParticleShapeIndex = EditorGUILayout.Popup(new GUIContent("Shape", "The display shape of an individual data point"), m_ParticleShapeIndex, m_ParticleShapeOptions);
            if (oldParticleShapeIndex != m_ParticleShapeIndex)
            {
                EditorPrefs.SetInt(k_ParticleShapeKey, m_ParticleShapeIndex);
            }

            if (m_ParticleShapeIndex > 0)
            {
                var oldParticleDirectionIndex = m_ParticleDirectionIndex;
                m_ParticleDirectionIndex = EditorGUILayout.Popup(new GUIContent("Billboard plane", "For 2D shapes, the facing direction of an individual data point"), m_ParticleDirectionIndex, m_ParticleDirectionOptions);
                if (oldParticleDirectionIndex != m_ParticleDirectionIndex)
                {
                    EditorPrefs.SetInt(k_ParticleDirectionKey, m_ParticleDirectionIndex);
                }
            }
            EditorGUILayout.EndVertical();

            if (m_GameObject != null && m_GameObject.GetComponent<IHeatmapRenderer>() != null)
            {
                int total = m_GameObject.GetComponent<IHeatmapRenderer>().totalPoints;
                int current = m_GameObject.GetComponent<IHeatmapRenderer>().currentPoints;
                GUILayout.Label("Points in current set: " + total);
                GUILayout.Label("Points currently displayed: " + current);
            }

            // PASS VALUES TO RENDERER
            if (m_GameObject != null)
            {
                IHeatmapRenderer r = m_GameObject.GetComponent<IHeatmapRenderer>() as IHeatmapRenderer;
                r.UpdateColors(new Color[]{ m_LowDensityColor, m_MediumDensityColor, m_HighDensityColor });
                r.UpdateThresholds(new float[]{ m_LowThreshold, m_HighThreshold });
                r.pointSize = m_ParticleSize;
                r.UpdateRenderStyle(m_ParticleShapeIds[m_ParticleShapeIndex], m_ParticleDirectionIds[m_ParticleDirectionIndex]);
            }
        }
        
        public void SystemReset()
        {
            //TODO
        }

        public void Update(bool forceUpdate = false)
        {
            if (m_GameObject != null)
            {
                float oldStartTime = m_StartTime;
                float oldEndTime = m_EndTime;
                UpdateTime();
                if (forceUpdate || oldStartTime != m_StartTime || oldEndTime != m_EndTime)
                {
                    IHeatmapRenderer r = m_GameObject.GetComponent<IHeatmapRenderer>() as IHeatmapRenderer;
                    if (r != null)
                    {
                        r.UpdateTimeLimits(m_StartTime, m_EndTime);
                        m_Heatmapper.Repaint();
                    }
                }
            }
        }

        void UpdateTime()
        {
            if (m_IsPlaying)
            {
                m_StartTime += m_PlaySpeed;
                m_EndTime += m_PlaySpeed;
            }
            if (m_EndTime >= m_MaxTime)
            {
                float diff = m_EndTime - m_StartTime;
                m_EndTime = m_MaxTime;
                m_StartTime = Mathf.Max(m_EndTime - diff, 0);
                m_IsPlaying = false;
            }
        }

        public void SetMaxTime(float maxTime)
        {
            m_EndTime = m_MaxTime = maxTime;
            m_StartTime = 0f;
        }

        void Restart()
        {
            float diff = m_EndTime - m_StartTime;
            m_StartTime = 0;
            m_EndTime = m_StartTime + diff;
        }

        public void SetGameObject(GameObject go)
        {
            m_GameObject = go;
        }

        string FormatColorToString(Color c)
        {
            return c.r + "|" + c.g + "|" + c.b + "|" + c.a;
        }

        Color GetColorFromString(string s, Color defaultColor)
        {
            if (string.IsNullOrEmpty(s))
            {
                return defaultColor;
            }

            string[] cols = s.Split('|');

            float r = 0, g = 0, b = 0, a = 1;
            try
            {
                r = float.Parse(cols[0]);
                g = float.Parse(cols[1]);
                b = float.Parse(cols[2]);
                a = float.Parse(cols[3]);
            }
            catch
            {
                r = 1f;
                g = 1f;
                b = 0f;
                a = 1f;
            }
            return new Color(r, g, b, a);
            ;
        }

        Color SetAndSaveColor(GUIContent content, string key, Color currentColor)
        {
            var oldColor = currentColor;
            Color updatedColor = EditorGUILayout.ColorField(currentColor);
            if (oldColor != updatedColor)
            {
                string colorString = FormatColorToString(updatedColor);
                EditorPrefs.SetString(key, colorString);
            }
            return updatedColor;
        }
    }
}
