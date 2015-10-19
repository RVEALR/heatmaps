
using System;
using System.Collections.Generic;
using UnityEngine;


namespace UnityAnalyticsHeatmap
{
    public class RenderData : IRenderData
    {

        Dictionary<string, HeatPoint[]> m_HeatData;
        public Dictionary<string, HeatPoint[]> data {
            get
            {
                return m_HeatData;
            }
            set{
                m_HeatData = value;
            }
        }

        string[] m_Options;
        public string[] options
        {
            get
            {
                return m_Options;
            }
            set
            {
                m_Options = value;
            }
        }

        HeatPoint[] m_CurrentPoints;
        public HeatPoint[] currentPoints
        {
            get
            {
                return m_CurrentPoints;
            }
        }

        int m_CurrentOption;
        public int currentOptionIndex
        {
            get
            {
                return m_CurrentOption;
            }
            set {
                m_CurrentOption = value;
                m_CurrentPoints = data[options[m_CurrentOption]];
                CalculateMax();
            }
        }
        
        float m_MaxDensity = 0f;
        public float maxDensity
        {
            get
            {
                return m_MaxDensity;
            }
        }
        
        float m_MaxTime = 0f;
        public float maxTime
        {
            get
            {
                return m_MaxTime;
            }
        }
        
        void CalculateMax()
        {
            m_MaxDensity = 0f;
            m_MaxTime = 0f;
            
            for (int i = 0; i < m_CurrentPoints.Length; i++)
            {
                m_MaxDensity = Mathf.Max(m_MaxDensity, m_CurrentPoints[i].density);
                m_MaxTime = Mathf.Max(m_MaxTime, m_CurrentPoints[i].time);
            }
        }
    }
}

