/// <summary>
/// Stores and dispatches changes to the Inspector, so 3rd parties can listen in.
/// </summary>

using System;
using System.Collections.Generic;

namespace UnityAnalyticsHeatmap
{
    internal class HeatmapsInspectorSettingsKeys
    {
        internal static string k_MaskTypeKey = "UnityAnalyticsHeatmapMaskOption";
        internal static string k_MaskRadiusKey = "UnityAnalyticsHeatmapMaskRadius";
        internal static string k_MaskFollowType = "UnityAnalyticsHeatmapMaskFollowType";

        internal static string k_ParticleSizeKey = "UnityAnalyticsHeatmapParticleSize";
        internal static string k_ParticleShapeKey = "UnityAnalyticsHeatmapParticleShape";
        internal static string k_ParticleDirectionKey = "UnityAnalyticsHeatmapParticleDirection";
        internal static string k_ParticleProjectionKey = "UnityAnalyticsHeatmapParticleProjection";
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
                    UnityEditor.EditorPrefs.SetInt(HeatmapsInspectorSettingsKeys.k_MaskFollowType, value);
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
                    UnityEditor.EditorPrefs.SetInt(HeatmapsInspectorSettingsKeys.k_MaskTypeKey, value);
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
                    UnityEditor.EditorPrefs.SetFloat(HeatmapsInspectorSettingsKeys.k_MaskRadiusKey, value);
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
                if (value != m_Settings.particleSize)
                {
                    UnityEditor.EditorPrefs.SetFloat(HeatmapsInspectorSettingsKeys.k_ParticleSizeKey, value);
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
                    UnityEditor.EditorPrefs.SetInt(HeatmapsInspectorSettingsKeys.k_ParticleShapeKey, value);
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
                    UnityEditor.EditorPrefs.SetInt(HeatmapsInspectorSettingsKeys.k_ParticleDirectionKey, value);
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
                    UnityEditor.EditorPrefs.SetInt(HeatmapsInspectorSettingsKeys.k_ParticleProjectionKey, value);
                    m_Settings.particleProjection = value;
                    Dispatch();
                }
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
            maskFollowType = UnityEditor.EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_MaskFollowType);
            maskRadius = UnityEditor.EditorPrefs.GetFloat(HeatmapsInspectorSettingsKeys.k_MaskRadiusKey, 1.0f);
            maskType = UnityEditor.EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_MaskTypeKey);


            particleSize = UnityEditor.EditorPrefs.GetFloat(HeatmapsInspectorSettingsKeys.k_ParticleSizeKey);
            particleShape = UnityEditor.EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_ParticleShapeKey);
            particleDirection = UnityEditor.EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_ParticleSizeKey);
            particleProjection = UnityEditor.EditorPrefs.GetInt(HeatmapsInspectorSettingsKeys.k_ParticleProjectionKey);
        }

        public int maskFollowType;
        public float maskRadius;
        public int maskType;

        public float particleSize;
        public int particleShape;
        public int particleDirection;
        public int particleProjection;
    }
}

