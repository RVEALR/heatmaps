using System;
using UnityEngine;
using UnityEditor;

namespace UnityAnalyticsHeatmap
{
    public class RendererSettings : IRendererSettings
    {
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




        Color m_HighDensityColor = new Color(1f, 0, 0, .1f);
        public Color highDensityColor
        {
            get
            {
                return m_HighDensityColor;
            }
            set
            {
                m_HighDensityColor = SetAndSaveColor(k_HighColorDensityKey, m_HighDensityColor, value);
            }
        }

        Color m_MediumDensityColor = new Color(1f, 1f, 0, .1f);
        public Color mediumDensityColor
        {
            get
            {
                return m_MediumDensityColor;
            }
            set
            {
                m_MediumDensityColor = SetAndSaveColor(k_MediumColorDensityKey, m_MediumDensityColor, value);
            }
        }
        Color m_LowDensityColor = new Color(0, 1f, 1f, .1f);
        public Color lowDensityColor
        {
            get
            {
                return m_LowDensityColor;
            }
            set
            {
                m_LowDensityColor = SetAndSaveColor(k_LowColorDensityKey, m_LowDensityColor, value);
            }
        }
        
        float m_HighThreshold = .9f;
        public float highThreshold
        {
            get
            {
                return m_HighThreshold;
            }
            set
            {
                float old = m_HighThreshold;
                m_HighThreshold = value;
                if (old != m_HighThreshold)
                {
                    EditorPrefs.SetFloat(k_HighThresholdKey, m_HighThreshold);
                }
            }
        }

        float m_LowThreshold = .1f;
        public float lowThreshold
        {
            get
            {
                return m_LowThreshold;
            }
            set
            {
                float old = m_LowThreshold;
                m_LowThreshold = value;
                if (old != m_LowThreshold)
                {
                    EditorPrefs.SetFloat(k_LowThresholdKey, m_LowThreshold);
                }
            }
        }
        
        float m_StartTime = 0f;
        public float startTime
        {
            get
            {
                return m_StartTime;
            }
            set
            {
                float old = m_StartTime;
                m_StartTime = value;
                if (old != m_StartTime)
                {
                    EditorPrefs.SetFloat(k_StartTimeKey, m_StartTime);
                }
            }
        }

        float m_EndTime = 1f;
        public float endTime
        {
            get
            {
                return m_EndTime;
            }
            set
            {
                float old = m_EndTime;
                m_EndTime = value;
                if (old != m_EndTime)
                {
                    EditorPrefs.SetFloat(k_EndTimeKey, m_EndTime);
                }
            }
        }

        float m_MaxTime = 1f;
        public float maxTime
        {
            get
            {
                return m_MaxTime;
            }
            set
            {
                m_MaxTime = value;
            }
        }

        float m_ParticleSize = 1f;
        public float particleSize
        {
            get
            {
                return m_ParticleSize;
            }
            set
            {
                value = Mathf.Max(0.05f, value);
                float old = m_ParticleSize;
                m_ParticleSize = value;
                if (old != m_ParticleSize)
                {
                    EditorPrefs.SetFloat(k_ParticleSizeKey, m_ParticleSize);
                }
            }
        }

        int m_ParticleShapeIndex = 0;
        public int particleShapeIndex
        {
            get
            {
                return m_ParticleShapeIndex;
            }
            set
            {
                int old = m_ParticleShapeIndex;
                m_ParticleShapeIndex = value;
                if (old != m_ParticleShapeIndex)
                {
                    EditorPrefs.SetFloat(k_ParticleShapeKey, m_ParticleShapeIndex);
                }
            }
        }

        int m_ParticleDirectionIndex = 0;
        public int particleDirectionIndex
        {
            get
            {
                return m_ParticleDirectionIndex;
            }
            set
            {
                int old = m_ParticleDirectionIndex;
                m_ParticleDirectionIndex = value;
                if (old != m_ParticleDirectionIndex)
                {
                    EditorPrefs.SetFloat(k_ParticleDirectionKey, m_ParticleDirectionIndex);
                }
            }
        }

        float m_PlaySpeed = 1f;
        public float playSpeed
        {
            get
            {
                return m_PlaySpeed;
            }
            set
            {
                float old = m_PlaySpeed;
                m_PlaySpeed = value;
                if (old != m_PlaySpeed)
                {
                    EditorPrefs.SetFloat(k_PlaySpeedKey, m_PlaySpeed);
                }
            }
        }

        public bool isPlaying { get; set; }

        RenderShape[] m_ParticleShapeIds = new RenderShape[]{ RenderShape.Cube, RenderShape.Arrow, RenderShape.Square, RenderShape.Triangle };
        public RenderShape[] particleShapeIds
        {
            get
            {
                return m_ParticleShapeIds;
            }
        }

        RenderDirection[] m_ParticleDirectionIds = new RenderDirection[]{ RenderDirection.YZ, RenderDirection.XZ, RenderDirection.XY };
        public RenderDirection[] particleDirectionIds
        {
            get
            {
                return m_ParticleDirectionIds;
            }
        }

        public RendererSettings()
        {
            LoadSettings();
        }

        void LoadSettings()
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
        
        Color SetAndSaveColor(string key, Color currentColor, Color updatedColor)
        {
            var oldColor = currentColor;
            if (oldColor != updatedColor)
            {
                string colorString = FormatColorToString(updatedColor);
                EditorPrefs.SetString(key, colorString);
            }
            return updatedColor;
        }

    }
}

