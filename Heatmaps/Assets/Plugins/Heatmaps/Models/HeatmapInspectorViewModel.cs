/// <summary>
/// Stores and dispatches changes to the Inspector, so 3rd parties can listen in.
/// </summary>

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    internal class HeatmapsInspectorSettingsKeys
    {
        internal static string k_HeatmapInFrontKey = "UnityAnalyticsHeatmapInFront";

        internal static string k_MaskTypeKey = "UnityAnalyticsHeatmapMaskOption";
        internal static string k_MaskRadiusKey = "UnityAnalyticsHeatmapMaskRadius";
        internal static string k_MaskFollowType = "UnityAnalyticsHeatmapMaskFollowType";

        internal static string k_ParticleSizeKey = "UnityAnalyticsHeatmapParticleSize";
        internal static string k_ParticleShapeKey = "UnityAnalyticsHeatmapParticleShape";
        internal static string k_ParticleDirectionKey = "UnityAnalyticsHeatmapParticleDirection";
        internal static string k_ParticleProjectionKey = "UnityAnalyticsHeatmapParticleProjection";

        internal static string k_RemapColorKey = "UnityAnalyticsHeatmapRemapColorKey";
        internal static string k_RemapOptionIndexKey = "UnityAnalyticsHeatmapRemapOptionIndexKey";
        internal static string k_RemapColorFieldKey = "UnityAnalyticsHeatmapRemapColorFieldKey";
        internal static string k_PercentileKey = "UnityAnalyticsHeatmapRemapPercentileKey";
    }


    public class HeatmapInspectorViewModel
    {
        static HeatmapInspectorViewModel m_Instance;

        HeatmapSettings _settings;
        HeatmapSettings m_Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = new HeatmapSettings();
                }
                return _settings;
            }
        }

        public delegate void HeatmapSettingsChangeHandler(object sender, EventArgs e);
        public event EventHandler<HeatmapSettings> SettingsChanged;

        public static HeatmapInspectorViewModel GetInstance()
        {
            if (m_Instance == null)
            {
                m_Instance = new HeatmapInspectorViewModel();
            }
            return m_Instance;
        }

        public bool heatmapInFront
        {
            get
            {
                return m_Settings.heatmapInFront;
            }
            set
            {
                m_Settings.heatmapInFront = value;
                Dispatch();
            }
        }

        public int heatmapOptionIndex
        {
            get
            {
                return m_Settings.heatmapOptionIndex;
            }
            set
            {
                m_Settings.heatmapOptionIndex = value;
                Dispatch();
            }
        }

        public List<int> heatmapOptions
        {
            get {
                return m_Settings.heatmapOptions;
            }
            set {
                m_Settings.heatmapOptions = value;
            }
        }

        public int maskFollowType
        {
            get
            {
                return m_Settings.maskFollowType;
            }
            set
            {
                if (value != m_Settings.maskFollowType)
                {
                    EditorPrefs.SetInt(HeatmapsInspectorSettingsKeys.k_MaskFollowType, value);
                    m_Settings.maskFollowType = value;
                    Dispatch();
                }
            }
        }

        public int maskType
        {
            get
            {
                return m_Settings.maskType;
            }
            set
            {
                if (value != m_Settings.maskType)
                {
                    EditorPrefs.SetInt(HeatmapsInspectorSettingsKeys.k_MaskTypeKey, value);
                    m_Settings.maskType = value;
                    Dispatch();
                }
            }
        }

        public float maskRadius
        {
            get
            {
                return m_Settings.maskRadius;
            }
            set
            {
                if (value != m_Settings.maskRadius)
                {
                    EditorPrefs.SetFloat(HeatmapsInspectorSettingsKeys.k_MaskRadiusKey, value);
                    m_Settings.maskRadius = value;
                    Dispatch();
                }
            }
        }

        public float particleSize
        {
            get
            {
                return m_Settings.particleSize;
            }
            set
            {
                value = Mathf.Max(0.05f, value);
                if (value != m_Settings.particleSize)
                {
                    EditorPrefs.SetFloat(HeatmapsInspectorSettingsKeys.k_ParticleSizeKey, value);
                    m_Settings.particleSize = value;
                    Dispatch();
                }
            }
        }

        public int particleShape
        {
            get
            {
                return m_Settings.particleShape;
            }
            set
            {
                if (value != m_Settings.particleShape)
                {
                    EditorPrefs.SetInt(HeatmapsInspectorSettingsKeys.k_ParticleShapeKey, value);
                    m_Settings.particleShape = value;
                    Dispatch();
                }
            }
        }

        public int particleDirection
        {
            get
            {
                return m_Settings.particleDirection;
            }
            set
            {
                if (value != m_Settings.particleDirection)
                {
                    EditorPrefs.SetInt(HeatmapsInspectorSettingsKeys.k_ParticleDirectionKey, value);
                    m_Settings.particleDirection = value;
                    Dispatch();
                }
            }
        }

        public int particleProjection
        {
            get
            {
                return m_Settings.particleProjection;
            }
            set
            {
                if (value != m_Settings.particleProjection)
                {
                    EditorPrefs.SetInt(HeatmapsInspectorSettingsKeys.k_ParticleProjectionKey, value);
                    m_Settings.particleProjection = value;
                    Dispatch();
                }
            }
        }

        public bool remapDensity
        {
            get {
                return m_Settings.remapDensity;
            }
            set {
                m_Settings.remapDensity = value;
                EditorPrefs.SetBool(HeatmapsInspectorSettingsKeys.k_RemapColorKey, m_Settings.remapDensity);
            }
        }

        public string remapColorField
        {
            get
            {
                return m_Settings.remapColorField;
            }
            set {
                m_Settings.remapColorField = value;
                EditorPrefs.SetString(HeatmapsInspectorSettingsKeys.k_RemapColorFieldKey, m_Settings.remapColorField);
            }
        }

        public int remapOptionIndex
        {
            get {
                return m_Settings.remapOptionIndex;
            }
            set {
                m_Settings.remapOptionIndex = value;
                EditorPrefs.SetInt(HeatmapsInspectorSettingsKeys.k_RemapOptionIndexKey, m_Settings.remapOptionIndex);
            }
        }

        public float remapPercentile
        {
            get {
                return m_Settings.remapPercentile;
            }
            set {
                m_Settings.remapPercentile = value;
                EditorPrefs.SetFloat(HeatmapsInspectorSettingsKeys.k_PercentileKey, m_Settings.remapPercentile);
            }
        }

        void Dispatch()
        {
            if (SettingsChanged != null)
            {
                SettingsChanged(this, m_Settings);
            }
        }
    }

    public class HeatmapSettings : EventArgs
    {
        public HeatmapSettings()
        {
            heatmapInFront = EditorPrefs.GetBool(HeatmapsInspectorSettingsKeys.k_HeatmapInFrontKey);

            heatmapOptionIndex = 0;
            heatmapOptions = new List<int>();

            maskFollowType = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_MaskFollowType);
            maskRadius = EditorPrefs.GetFloat(HeatmapsInspectorSettingsKeys.k_MaskRadiusKey, 1.0f);
            maskType = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_MaskTypeKey);

            particleSize = EditorPrefs.GetFloat(HeatmapsInspectorSettingsKeys.k_ParticleSizeKey);
            particleShape = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_ParticleShapeKey);
            particleDirection = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_ParticleSizeKey);
            particleProjection = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_ParticleProjectionKey);

            remapDensity = EditorPrefs.GetBool(HeatmapsInspectorSettingsKeys.k_RemapColorKey);
            remapColorField = EditorPrefs.GetString(HeatmapsInspectorSettingsKeys.k_RemapColorFieldKey);
            remapOptionIndex = EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_RemapOptionIndexKey);
            remapPercentile = EditorPrefs.GetFloat(HeatmapsInspectorSettingsKeys.k_PercentileKey);
        }

        public bool heatmapInFront;

        public int heatmapOptionIndex;
        public List<int> heatmapOptions;

        public int maskFollowType;
        public float maskRadius;
        public int maskType;

        public float particleSize;
        public int particleShape;
        public int particleDirection;
        public int particleProjection;

        public bool remapDensity;
        public string remapColorField;
        public int remapOptionIndex;
        public float remapPercentile;
    }
}

