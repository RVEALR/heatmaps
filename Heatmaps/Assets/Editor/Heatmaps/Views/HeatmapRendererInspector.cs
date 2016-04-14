/// <summary>
/// Heat map renderer inspector.
/// </summary>
/// This code manages the portion of the inspector that
/// controls the Heat Map renderer.

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapRendererInspector
    {
        const string k_StartTimeKey = "UnityAnalyticsHeatmapStartTime";
        const string k_EndTimeKey = "UnityAnalyticsHeatmapEndTime";
        const string k_PlaySpeedKey = "UnityAnalyticsHeatmapPlaySpeed";

        const string k_ParticleSizeKey = "UnityAnalyticsHeatmapParticleSize";
        const string k_ParticleShapeKey = "UnityAnalyticsHeatmapParticleShape";
        const string k_ParticleDirectionKey = "UnityAnalyticsHeatmapParticleDirection";

        const string k_LowXKey = "UnityAnalyticsHeatmapLowX";
        const string k_HighXKey = "UnityAnalyticsHeatmapHighX";
        const string k_LowYKey = "UnityAnalyticsHeatmapLowY";
        const string k_HighYKey = "UnityAnalyticsHeatmapHighY";
        const string k_LowZKey = "UnityAnalyticsHeatmapLowZ";
        const string k_HighZKey = "UnityAnalyticsHeatmapHighZ";

        Heatmapper m_Heatmapper;

        float m_StartTime = 0f;
        float m_EndTime = 1f;
        float m_MaxTime = 1f;

        float m_ParticleSize = 1f;
        int m_ParticleShapeIndex = 0;
        GUIContent[] m_ParticleShapeOptions = new GUIContent[]{ new GUIContent("Cube"), new GUIContent("Arrow"), new GUIContent("Point To Point"), new GUIContent("Square"), new GUIContent("Triangle") };
        RenderShape[] m_ParticleShapeIds = new RenderShape[]{ RenderShape.Cube, RenderShape.Arrow, RenderShape.PointToPoint, RenderShape.Square, RenderShape.Triangle };

        float m_LowX = 0f;
        float m_HighX = 1f;
        float m_LowY = 0f;
        float m_HighY = 1f;
        float m_LowZ = 0f;
        float m_HighZ = 1f;
        Vector3 m_LowSpace = Vector3.zero;
        Vector3 m_HighSpace = Vector3.one;

        int m_ParticleDirectionIndex = 0;
        GUIContent[] m_ParticleDirectionOptions = new GUIContent[]{ new GUIContent("YZ"), new GUIContent("XZ"), new GUIContent("XY") };
        RenderDirection[] m_ParticleDirectionIds = new RenderDirection[]{ RenderDirection.YZ, RenderDirection.XZ, RenderDirection.XY };

        bool m_IsPlaying = false;
        float m_PlaySpeed = 1f;

        GameObject m_GameObject;


        public HeatmapRendererInspector()
        {
            m_StartTime = EditorPrefs.GetFloat(k_StartTimeKey, m_StartTime);
            m_EndTime = EditorPrefs.GetFloat(k_EndTimeKey, m_EndTime);
            m_PlaySpeed = EditorPrefs.GetFloat(k_PlaySpeedKey, m_PlaySpeed);

            m_ParticleSize = EditorPrefs.GetFloat(k_ParticleSizeKey, m_ParticleSize);

            m_ParticleShapeIndex = EditorPrefs.GetInt(k_ParticleShapeKey, m_ParticleShapeIndex);
            m_ParticleDirectionIndex = EditorPrefs.GetInt(k_ParticleDirectionKey, m_ParticleDirectionIndex);

            m_LowX = EditorPrefs.GetFloat(k_LowXKey, m_LowX);
            m_LowY = EditorPrefs.GetFloat(k_LowYKey, m_LowY);
            m_LowZ = EditorPrefs.GetFloat(k_LowZKey, m_LowZ);
            m_HighX = EditorPrefs.GetFloat(k_HighXKey, m_HighX);
            m_HighY = EditorPrefs.GetFloat(k_HighXKey, m_HighY);
            m_HighZ = EditorPrefs.GetFloat(k_HighXKey, m_HighZ);
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
            SerializedProperty colorGradient = null;
            if (m_GameObject != null)
            {
                SerializedObject serializedGradient = new SerializedObject(m_GameObject.GetComponent<GradientContainer>());
                if (serializedGradient != null)
                {
                    colorGradient = serializedGradient.FindProperty("ColorGradient");
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(colorGradient, false);
                    if(EditorGUI.EndChangeCheck())
                    {
                        serializedGradient.ApplyModifiedProperties();
                    }
                }
            }
            EditorGUILayout.EndVertical();

            // TIME WINDOW
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Time", EditorStyles.boldLabel);
            var oldStartTime = m_StartTime;
            var oldEndTime = m_EndTime;
            RenderMinMaxSlider(ref m_StartTime, ref m_EndTime, k_StartTimeKey, k_EndTimeKey, 0f, m_MaxTime);
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
            }
            if (oldEndTime != m_EndTime)
            {
                forceTime = true;
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

            if (m_ParticleShapeIndex > 2)
            {
                var oldParticleDirectionIndex = m_ParticleDirectionIndex;
                m_ParticleDirectionIndex = EditorGUILayout.Popup(new GUIContent("Billboard plane", "For 2D shapes, the facing direction of an individual data point"), m_ParticleDirectionIndex, m_ParticleDirectionOptions);
                if (oldParticleDirectionIndex != m_ParticleDirectionIndex)
                {
                    EditorPrefs.SetInt(k_ParticleDirectionKey, m_ParticleDirectionIndex);
                }
            }
            // POSITION MASKING
            EditorGUILayout.LabelField("Masking");
            RenderMinMaxSlider(ref m_LowX, ref m_HighX, k_LowXKey, k_HighXKey, m_LowSpace.x, m_HighSpace.x);
            RenderMinMaxSlider(ref m_LowY, ref m_HighY, k_LowYKey, k_HighYKey, m_LowSpace.y, m_HighSpace.y);
            RenderMinMaxSlider(ref m_LowZ, ref m_HighZ, k_LowZKey, k_HighZKey, m_LowSpace.z, m_HighSpace.z);
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
                r.UpdateGradient(SafeGradientValue(colorGradient ));
                r.pointSize = m_ParticleSize;
                r.UpdateRenderMask(m_LowX, m_HighX, m_LowY, m_HighY, m_LowZ, m_HighZ);
                r.UpdateRenderStyle(m_ParticleShapeIds[m_ParticleShapeIndex], m_ParticleDirectionIds[m_ParticleDirectionIndex]);
            }
        }

        // Access to SerializedProperty's internal gradientValue property getter, in a manner that'll only soft break (returning null) if the property changes or disappears in future Unity revs.
        static Gradient SafeGradientValue(SerializedProperty sp)
        {
            BindingFlags instanceAnyPrivacyBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            PropertyInfo propertyInfo = typeof(SerializedProperty).GetProperty(
                "gradientValue",
                instanceAnyPrivacyBindingFlags,
                null,
                typeof(Gradient),
                new Type[0],
                null
                );
            if (propertyInfo == null)
                return null;
            
            Gradient gradientValue = propertyInfo.GetValue(sp, null) as Gradient;
            return gradientValue;
        }

        protected void RenderMinMaxSlider(ref float lowValue, ref float highValue, string lowKey, string highKey, float minValue, float maxValue)
        {
            EditorGUILayout.BeginHorizontal();
            float oldLow = lowValue;
            float oldHigh = highValue;

            lowValue = EditorGUILayout.FloatField(lowValue, GUILayout.MaxWidth(50f));
            highValue = EditorGUILayout.FloatField(highValue, GUILayout.Width(50f));
            EditorGUILayout.MinMaxSlider(ref lowValue, ref highValue, minValue, maxValue);


            highValue = Mathf.Max(lowValue, highValue);
            lowValue = Mathf.Min(lowValue, highValue);

            // Needed to solve small rounding error in the MinMaxSlider
            highValue = (Mathf.Abs(oldHigh - highValue) < .0001f) ? oldHigh : highValue;
            if (GUILayout.Button("Max"))
            {
                lowValue = minValue;
                highValue = maxValue;
            }
            if (oldLow != lowValue)
            {
                EditorPrefs.SetFloat(lowKey, lowValue);
            }
            if (oldHigh != highValue)
            {
                EditorPrefs.SetFloat(highKey, highValue);
            }
            EditorGUILayout.EndHorizontal();
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
                        EditorGUI.FocusTextInControl("");
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

        public void SetSpaceLimits(Vector3 lowSpace, Vector3 highSpace)
        {
            m_LowX = lowSpace.x;
            m_LowY = lowSpace.y;
            m_LowZ = lowSpace.z;
            m_HighX = highSpace.x;
            m_HighY = highSpace.y;
            m_HighZ = highSpace.z;

            m_LowSpace = lowSpace;
            m_HighSpace = highSpace;
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
    }
}
