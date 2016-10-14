﻿/// <summary>
/// Heat map renderer inspector.
/// </summary>
/// This code manages the portion of the inspector that
/// controls the Heat Map renderer.

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace UnityAnalyticsHeatmap
{
    public class HeatmapRendererInspector
    {
        const string k_Renderer = "UnityAnalyticsHeatmapRenderer";

        const string k_StartTimeKey = "UnityAnalyticsHeatmapStartTime";
        const string k_EndTimeKey = "UnityAnalyticsHeatmapEndTime";
        const string k_PlaySpeedKey = "UnityAnalyticsHeatmapPlaySpeed";

        const string k_ParticleSizeKey = "UnityAnalyticsHeatmapParticleSize";
        const string k_ParticleShapeKey = "UnityAnalyticsHeatmapParticleShape";
        const string k_ParticleDirectionKey = "UnityAnalyticsHeatmapParticleDirection";

        const string k_MaskOptionKey = "UnityAnalyticsHeatmapMaskOption";
        const string k_MaskRadiusKey = "UnityAnalyticsHeatmapMaskRadius";
        const string k_MaskWillFollowKey = "UnityAnalyticsHeatmapMaskWillFollow";

        const string k_LowXKey = "UnityAnalyticsHeatmapLowX";
        const string k_HighXKey = "UnityAnalyticsHeatmapHighX";
        const string k_LowYKey = "UnityAnalyticsHeatmapLowY";
        const string k_HighYKey = "UnityAnalyticsHeatmapHighY";
        const string k_LowZKey = "UnityAnalyticsHeatmapLowZ";
        const string k_HighZKey = "UnityAnalyticsHeatmapHighZ";

        const string k_ShowTipsKey = "UnityAnalyticsHeatmapShowRendererTooltips";

        Heatmapper m_Heatmapper;
        HeatmapDataProcessor m_Processor;

        Type[] m_Renderers = new Type[]{ typeof(HeatmapMeshRenderer), typeof(InstancedHeatmapMeshRenderer) };
        GUIContent[] m_RendererOptions = new GUIContent[]{ new GUIContent("Mesh Renderer"), new GUIContent("GPU Instanced Renderer (Requires 5.5+)") };
        int m_RendererIndex = 0;

        float m_StartTime = 0f;
        float m_EndTime = 1f;
        float m_MaxTime = 1f;

        float m_ParticleSize = 1f;
        int m_ParticleShapeIndex = 0;
        GUIContent[] m_ParticleShapeOptions = new GUIContent[]{ new GUIContent("Cube"), new GUIContent("Arrow"), new GUIContent("Point To Point"), new GUIContent("Square"), new GUIContent("Triangle") };
        RenderShape[] m_ParticleShapeIds = new RenderShape[]{ RenderShape.Cube, RenderShape.Arrow, RenderShape.PointToPoint, RenderShape.Square, RenderShape.Triangle };

        int m_MaskOption = 0;
        float m_MaskRadius = 1.0f;
        Vector3 m_MaskRadiusSource = Vector3.zero;
        bool m_MaskWillFollow = false;

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

        bool m_Tips = false;

        GameObject m_GameObject;
        SerializedObject m_SerializedGradient = null;
        SerializedProperty m_ColorGradient = null;

        Texture2D darkSkinPlayIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/play_dark.png") as Texture2D;
        Texture2D darkSkinPauseIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/pause_dark.png") as Texture2D;
        Texture2D darkSkinRewindIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/rwd_dark.png") as Texture2D;

        Texture2D lightSkinPlayIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/play_light.png") as Texture2D;
        Texture2D lightSkinPauseIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/pause_light.png") as Texture2D;
        Texture2D lightSkinRewindIcon = EditorGUIUtility.Load("Assets/Editor/Heatmaps/Textures/rwd_light.png") as Texture2D;

        GUIContent m_ParticleSizeContent = new GUIContent("Size", "The display size of an individual data point");
        GUIContent m_ParticleShapeContent = new GUIContent("Shape", "The display shape of an individual data point");
        GUIContent m_ParticleDirectionContent = new GUIContent("Billboard plane", "For 2D shapes, the facing direction of an individual data point");
        GUIContent m_PlaySpeedContent = new GUIContent("Play speed", "Speed at which playback occurs");
        GUIContent m_TipsContent = new GUIContent("Hot tips", "When enabled, see individual point information on rollover. Caution: can be costly! Also note, submap must be selected to see hot tips.");
        GUIContent m_TipsTextContent = new GUIContent("Points (displayed/total): 0 / 0");
        GUIContent m_RestartContent;
        GUIContent m_PlayContent;
        GUIContent m_PauseContent;
        GUIContent m_MaskOptionContentRadius = new GUIContent("Radius", "Check this to filter data by position and radius");
        GUIContent m_MaskOptionContentSlice = new GUIContent("Slice", "Check this to filter data by global x/y/x positions");
        GUIContent m_MaskRadiusContent = new GUIContent("Radius", "Radius to draw");

        public HeatmapRendererInspector()
        {
            m_RendererIndex = EditorPrefs.GetInt(k_Renderer, m_RendererIndex);

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

            m_MaskOption = EditorPrefs.GetInt(k_MaskOptionKey, m_MaskOption);
            m_MaskRadius = EditorPrefs.GetFloat(k_MaskRadiusKey, m_MaskRadius);
            m_MaskWillFollow = EditorPrefs.GetBool(k_MaskWillFollowKey, m_MaskWillFollow);

            m_Tips = EditorPrefs.GetBool(k_ShowTipsKey, false);

            var playIcon = lightSkinPlayIcon;
            var pauseIcon = lightSkinPauseIcon;
            var rwdIcon = lightSkinRewindIcon;
            if (EditorGUIUtility.isProSkin)
            {
                playIcon = darkSkinPlayIcon;
                pauseIcon = darkSkinPauseIcon;
                rwdIcon = darkSkinRewindIcon;
            }

            m_RestartContent = new GUIContent(rwdIcon, "Back to Start");
            m_PlayContent = new GUIContent(playIcon, "Play");
            m_PauseContent = new GUIContent(pauseIcon, "Pause");
        }

        public static HeatmapRendererInspector Init(Heatmapper heatmapper, HeatmapDataProcessor processor)
        {
            var inspector = new HeatmapRendererInspector();
            inspector.m_Processor = processor;
            inspector.m_Heatmapper = heatmapper;
            return inspector;
        }

        public void OnEnable()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        public void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView view)
        {
            if (m_MaskWillFollow)
            {
                var originalSource = m_MaskRadiusSource;
                m_MaskRadiusSource = view.camera.transform.position;
                if (Vector3.Equals(originalSource, m_MaskRadiusSource) == false)
                {
                    m_Heatmapper.Repaint();
                }
            }
        }

        public void OnGUI()
        {
            m_RendererIndex = EditorGUIBinding.Popup(m_RendererIndex, m_RendererOptions, RendererChange);
            using (new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Data set options", EditorStyles.boldLabel);
                m_Processor.m_HeatmapOptions = AnalyticsListGroup.ListGroup(m_Processor.m_HeatmapOptions,
                    m_Processor.m_SeparatedLists, OptionsChange);
            }

            // PARTICLE SIZE/SHAPE
            using(new EditorGUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Render options", EditorStyles.boldLabel);

                if (m_GameObject == null)
                {
                    EditorGUILayout.LabelField("No heatmap. Can't show gradient.", EditorStyles.boldLabel);
                }
                else if (m_ColorGradient != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(m_ColorGradient, false);
                    if(EditorGUI.EndChangeCheck())
                    {
                        m_SerializedGradient.ApplyModifiedProperties();
                    }
                }

                var oldParticleSize = m_ParticleSize;
                m_ParticleSize = EditorGUILayout.FloatField(m_ParticleSizeContent, m_ParticleSize);
                m_ParticleSize = Mathf.Max(0.05f, m_ParticleSize);
                if (oldParticleSize != m_ParticleSize)
                {
                    EditorPrefs.SetFloat(k_ParticleSizeKey, m_ParticleSize);
                }

                var oldParticleShapeIndex = m_ParticleShapeIndex;
                m_ParticleShapeIndex = EditorGUILayout.Popup(m_ParticleShapeContent, m_ParticleShapeIndex, m_ParticleShapeOptions);
                if (oldParticleShapeIndex != m_ParticleShapeIndex)
                {
                    EditorPrefs.SetInt(k_ParticleShapeKey, m_ParticleShapeIndex);
                }

                if (m_ParticleShapeIndex > 2)
                {
                    var oldParticleDirectionIndex = m_ParticleDirectionIndex;
                    m_ParticleDirectionIndex = EditorGUILayout.Popup(m_ParticleDirectionContent, m_ParticleDirectionIndex, m_ParticleDirectionOptions);
                    if (oldParticleDirectionIndex != m_ParticleDirectionIndex)
                    {
                        EditorPrefs.SetInt(k_ParticleDirectionKey, m_ParticleDirectionIndex);
                    }
                }
            }

            // POSITION MASKING
            using(new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Filtering", EditorStyles.boldLabel);

                GUIContent[] maskOptionContent = new GUIContent[]{m_MaskOptionContentSlice, m_MaskOptionContentRadius};
                int oldMaskOption = m_MaskOption;
                m_MaskOption = GUILayout.Toolbar(m_MaskOption, maskOptionContent, GUILayout.MaxWidth(200));
                if (oldMaskOption != m_MaskOption)
                {
                    EditorPrefs.SetInt(k_MaskOptionKey, m_MaskOption);
                }

                if (m_MaskOption == 1)
                {
                    var oldMaskRadius = m_MaskRadius;
                    EditorGUI.BeginChangeCheck();
                    m_MaskWillFollow = EditorGUILayout.Toggle("Follow camera", m_MaskWillFollow);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorPrefs.SetBool(k_MaskWillFollowKey, m_MaskWillFollow);
                    }

                    EditorGUI.BeginDisabledGroup(m_MaskWillFollow);
                    m_MaskRadiusSource = EditorGUILayout.Vector3Field("Source", m_MaskRadiusSource);
                    EditorGUI.EndDisabledGroup();

                    m_MaskRadius = EditorGUILayout.FloatField(m_MaskRadiusContent, m_MaskRadius);
                    if (oldMaskRadius != m_MaskRadius)
                    {
                        EditorPrefs.SetFloat(k_MaskRadiusKey, m_MaskRadius);
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("Slice (x/y/z)");
                    RenderMinMaxSlider(ref m_LowX, ref m_HighX, k_LowXKey, k_HighXKey, m_LowSpace.x, m_HighSpace.x);
                    RenderMinMaxSlider(ref m_LowY, ref m_HighY, k_LowYKey, k_HighYKey, m_LowSpace.y, m_HighSpace.y);
                    RenderMinMaxSlider(ref m_LowZ, ref m_HighZ, k_LowZKey, k_HighZKey, m_LowSpace.z, m_HighSpace.z);
                }
            }

            // TIME WINDOW
            var oldStartTime = m_StartTime;
            var oldEndTime = m_EndTime;
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Time", EditorStyles.boldLabel);
                RenderMinMaxSlider(ref m_StartTime, ref m_EndTime, k_StartTimeKey, k_EndTimeKey, 0f, m_MaxTime);
                var oldPlaySpeed = m_PlaySpeed;
                m_PlaySpeed = EditorGUILayout.FloatField(m_PlaySpeedContent, m_PlaySpeed);
                if (oldPlaySpeed != m_PlaySpeed)
                {
                    EditorPrefs.SetFloat(k_PlaySpeedKey, m_PlaySpeed);
                }
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(m_RestartContent))
                    {
                        Restart();
                        m_IsPlaying = false;
                    }

                    GUIContent playButtonContent = m_IsPlaying ? m_PauseContent : m_PlayContent;
                    if (GUILayout.Button(playButtonContent))
                    {
                        if (m_StartTime < m_MaxTime && m_EndTime == m_MaxTime)
                        {
                            Restart();
                        }
                        m_IsPlaying = !m_IsPlaying;
                    }
                }
            }
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

            // REPORTING AND TIPS
            EditorGUILayout.LabelField(m_TipsTextContent);
            bool oldTips = m_Tips;
            m_Tips = EditorGUILayout.Toggle(m_TipsContent, m_Tips);
            if (oldTips != m_Tips)
            {
                EditorPrefs.SetBool(k_ShowTipsKey, m_Tips);
            }

            if (Event.current.type == EventType.Layout)
            {
                if (m_GameObject != null && m_GameObject.GetComponent<IHeatmapRenderer>() != null)
                {
                    int total = m_GameObject.GetComponent<IHeatmapRenderer>().totalPoints;
                    int current = m_GameObject.GetComponent<IHeatmapRenderer>().currentPoints;
                    m_TipsTextContent = new GUIContent("Points (displayed/total): " + current + " / " + total);
                }

                // PASS VALUES TO RENDERER
                if (m_GameObject != null)
                {
                    IHeatmapRenderer r = m_GameObject.GetComponent<IHeatmapRenderer>() as IHeatmapRenderer;
                    r.UpdateGradient(SafeGradientValue(m_ColorGradient));
                    r.pointSize = m_ParticleSize;
                    r.activateTips = m_Tips;
                    if (m_MaskOption == 1)
                    {
                        r.UpdateRenderMask(m_MaskRadiusSource, m_MaskRadius);
                    }
                    else
                    {
                        r.UpdateRenderMask(m_LowX, m_HighX, m_LowY, m_HighY, m_LowZ, m_HighZ);
                    }
                    r.UpdateRenderStyle(m_ParticleShapeIds[m_ParticleShapeIndex], m_ParticleDirectionIds[m_ParticleDirectionIndex]);
                }
            }
        }

        public void SetLimits(HeatPoint[] points)
        {
            float maxDensity = 0;
            m_MaxTime = 0;
            m_LowSpace = new Vector3();
            m_HighSpace = new Vector3();

            for (int a = 0; a < points.Length; a++)
            {
                maxDensity = Mathf.Max(maxDensity, points[a].density);
                m_MaxTime = Mathf.Max(m_MaxTime, points[a].time);
                m_LowSpace = Vector3.Min(m_LowSpace, points[a].position);
                m_HighSpace = Vector3.Max(m_HighSpace, points[a].position);
            }

            SetSpaceLimits(m_LowSpace, m_HighSpace);
            SetMaxTime(m_MaxTime);

            if (m_GameObject != null && m_GameObject.GetComponent<IHeatmapRenderer>() != null)
            {
                m_GameObject.GetComponent<IHeatmapRenderer>().UpdatePointData(points, maxDensity);
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
            using (new EditorGUILayout.HorizontalScope())
            {
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

        void SetMaxTime(float maxTime)
        {
            m_MaxTime = maxTime;
            if (m_StartTime == 0 && m_EndTime == 0)
            {
                m_EndTime = m_MaxTime;
                m_StartTime = 0f;
            }
            else
            {
                m_EndTime = Mathf.Clamp(m_EndTime, 0f, m_MaxTime);
                m_StartTime = Mathf.Clamp(m_StartTime, 0f, m_MaxTime);
            }
        }

        public void SetSpaceLimits(Vector3 lowSpace, Vector3 highSpace)
        {
            m_LowX = Mathf.Clamp(m_LowX, lowSpace.x, highSpace.x);
            m_LowY = Mathf.Clamp(m_LowY, lowSpace.y, highSpace.y);
            m_LowZ = Mathf.Clamp(m_LowZ, lowSpace.z, highSpace.z);
            m_HighX = Mathf.Clamp(m_HighX, lowSpace.x, highSpace.x);
            m_HighY = Mathf.Clamp(m_HighY, lowSpace.y, highSpace.y);
            m_HighZ = Mathf.Clamp(m_HighZ, lowSpace.z, highSpace.z);

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
            if (m_GameObject != null)
            {
                m_SerializedGradient = new SerializedObject(m_GameObject.GetComponent<GradientContainer>());
                m_ColorGradient = m_SerializedGradient.FindProperty("ColorGradient");
            }
        }

        #region change handlers
        void RendererChange(int value)
        {
            EditorPrefs.SetInt(k_Renderer, value);
            m_Heatmapper.SwapRenderer(m_Renderers[value]);
        }

        void OptionsChange(List<int> value)
        {
            m_Processor.SelectList();
        }
        #endregion
    }
}
