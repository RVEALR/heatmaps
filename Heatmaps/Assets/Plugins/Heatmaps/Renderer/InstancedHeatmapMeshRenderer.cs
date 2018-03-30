/// <summary>
/// Instanced heatmap mesh renderer.
/// </summary>
/// An improvement over the original HeatmapMeshRenderer that
/// employs GPU instancing. (Requires Unity 5.5+)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RVEALR.Heatmaps
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(GradientContainer))]
	public class InstancedHeatmapMeshRenderer : MonoBehaviour, IHeatmapRenderer
	{
	    const string k_ShaderName = "Heatmaps/Particles/SimplestInstancedShader";
	    const string k_ColorProperty = "_Color";

	    const int k_NotRendering = 0;
	    const int k_BeginRenderer = 1;
	    const int k_RenderInProgress = 2;
	    const int k_UpdateMaterials = 4;
	    
	    // Unity limit of shapes which can be drawn in a single instancing
	    const int k_ShapesPerDraw = 1023;
	    
	    // Density Thresholds
	    float m_HighThreshold;
	    float m_LowThreshold;
	    
	    // Time limits
	    float m_StartTime = 0f;
	    float m_EndTime = 1.0f;
	    
	    [Range(0.1f, 100f)]
	    float m_ParticleSize = 1.0f;
	    
	    bool m_Tips = false;
	    
	    // Particle Rendering Data
	    HeatPoint[] m_Data;
	    float m_MaxDensity = 0f;
	    float m_LowX = 0f;
	    float m_LowY = 0f;
	    float m_LowZ = 0f;
	    float m_HighX = 1f;
	    float m_HighY = 1f;
	    float m_HighZ = 1f;

	    Vector3 m_MaskSource;
	    
	    RenderShape m_RenderStyle = RenderShape.Cube;
	    RenderDirection m_RenderDirection = RenderDirection.YZ;

	    RenderProjection m_Projection = RenderProjection.FirstPerson;
	    
	    Shader m_Shader;
	    public Material m_Material;
	    Gradient m_Gradient;
	    int m_ColorID;
	    Mesh m_Mesh;

	    List<Matrix4x4[]> m_Matrices;
	    List<MaterialPropertyBlock> m_Properties;

	    bool m_HeatmapInFront;

	    int _r = k_NotRendering;
	    int m_RenderState
	    {
	        get
	        {
	            return _r;
	        }
	        set
	        {
	            _r = value;
	        }
	    }
	    
	    void OnEnable()
	    {
	        m_Shader = Shader.Find(k_ShaderName);
	        allowRender = true;
	    }
	    
	    public void UpdatePointData(HeatPoint[] newData, float newMaxDensity)
	    {
	        m_Data = newData;
	        m_MaxDensity = newMaxDensity;
	        m_RenderState = k_BeginRenderer;
	    }
	    
	    public void UpdateColors(Color[] colors)
	    {
	        // No-op
	    }
	    
	    public void UpdateThresholds(float[] threshholds)
	    {
	        //No-op
	    }
	    
	    public void UpdateGradient(Gradient gradient)
	    {
	        if (m_Material == null)
	        {
	            m_Shader = Shader.Find(k_ShaderName);
	            m_Material = new Material(m_Shader);
	            m_ColorID = Shader.PropertyToID(k_ColorProperty);
	        }
	        if (gradient == null || !GradientUtils.CompareGradients(gradient, m_Gradient))
	        {
	            m_Gradient = gradient;
	            m_RenderState = k_BeginRenderer;
	        }
	    }
	    
	    public void UpdateRenderMask(float lowX, float highX, float lowY, float highY, float lowZ, float highZ)
	    {
	        if (lowX != m_LowX)
	        {
	            m_LowX = lowX;
	            m_RenderState = k_BeginRenderer;
	        }
	        if (lowY != m_LowY)
	        {
	            m_LowY = lowY;
	            m_RenderState = k_BeginRenderer;
	        }
	        if (lowZ != m_LowZ)
	        {
	            m_LowZ = lowZ;
	            m_RenderState = k_BeginRenderer;
	        }
	        if (highX != m_HighX)
	        {
	            m_HighX = highX;
	            m_RenderState = k_BeginRenderer;
	        }
	        if (highY != m_HighY)
	        {
	            m_HighY = highY;
	            m_RenderState = k_BeginRenderer;
	        }
	        if (highZ != m_HighZ)
	        {
	            m_HighZ = highZ;
	            m_RenderState = k_BeginRenderer;
	        }
	    }

	    public void UpdateRenderMask(float radius)
	    {
	        //TODO
	    }

	    public void UpdateCameraPosition(Vector3 pos)
	    {
	        if (Vector3.Equals(pos, m_MaskSource) == false)
	        {
	            m_MaskSource = pos;
	            m_RenderState = k_BeginRenderer;
	        }
	    }

	    public void UpdateProjection(RenderProjection projection)
	    {
	        // TODO
	    }
	    
	    public float pointSize
	    {
	        get
	        {
	            return m_ParticleSize;
	        }
	        set
	        {
	            if (m_ParticleSize != value)
	            {
	                m_ParticleSize = value;
	                RenderMesh();
	                m_RenderState = k_BeginRenderer;
	            }
	        }
	    }
	    
	    public bool activateTips
	    {
	        get
	        {
	            return m_Tips;
	        }
	        set
	        {
	            if (m_Tips != value)
	            {
	                m_Tips = value;
	                if (m_Tips)
	                {
	                    m_RenderState = k_BeginRenderer;
	                }
	                else
	                {
	                    //Remove colliders
	                    MeshCollider[] children = transform.GetComponentsInChildren<MeshCollider>();
	                    foreach (MeshCollider child in children)
	                    {
	                        DestroyImmediate(child.gameObject.GetComponent<MeshCollider>());
	                    }
	                }
	            }
	        }
	    }

	    public bool heatmapInFront {
	        get
	        {
	            return m_HeatmapInFront;
	        }

	        set {
	            if (value != m_HeatmapInFront)
	            {
	                m_HeatmapInFront = value;
	                //CreateMaterials();
	                m_RenderState = k_UpdateMaterials;
	            }
	        }
	    }

	    public string remapLabel{ get; set; }

	    bool _allowRender = true;
	    public bool allowRender
	    {
	        get
	        {
	            return _allowRender;
	        }
	        set
	        {
	            _allowRender = value;
	        }
	    }
	    
	    public int currentPoints { get; set; }
	    
	    public int totalPoints { get; set; }
	    
	    public void UpdateTimeLimits(float startTime, float endTime)
	    {
	        if (m_StartTime != startTime || m_EndTime != endTime)
	        {
	            m_StartTime = startTime;
	            m_EndTime = endTime;
	            m_RenderState = k_BeginRenderer;
	        }
	    }
	    
	    public void UpdateRenderStyle(RenderShape style, RenderDirection direction)
	    {
	        if (style != m_RenderStyle || direction != m_RenderDirection)
	        {
	            m_RenderDirection = direction;
	            m_RenderStyle = style;
	            RenderMesh();
	            m_RenderState = k_BeginRenderer;
	        }
	    }

	    void RenderMesh()
	    {
	        if (m_Mesh == null)
	        {
	            m_Mesh = new Mesh();
	        }

	        // Draw the shape once
	        int[] tris;
	        Vector3[] vectors;
	        switch (m_RenderStyle)
	        {
	            case RenderShape.Cube:
	                vectors =  RenderShapeMeshUtils.AddCubeVectorsToMesh(m_ParticleSize, 0, 0, 0);
	                tris = RenderShapeMeshUtils.AddCubeTrisToMesh(0);
	                break;
	            case RenderShape.Arrow:
	                vectors = RenderShapeMeshUtils.AddArrowVectorsToMesh(m_ParticleSize, Vector3.zero, Vector3.zero, m_Projection);
	                tris = RenderShapeMeshUtils.AddArrowTrisToMesh(0, m_Projection);
	                break;
	            case RenderShape.Square:
	                vectors = RenderShapeMeshUtils.AddSquareVectorsToMesh(m_ParticleSize, m_RenderDirection, Vector3.zero, Vector3.zero);
	                tris = RenderShapeMeshUtils.AddSquareTrisToMesh(0);
	                break;
	            case RenderShape.PointToPoint:
	                // FIXME: This needs more thinking. Obviously destination isn't fixed.
	                // In fact, can we even use do this in the Instanced renderer?
	                vectors = RenderShapeMeshUtils.AddP2PVectorsToMesh(m_ParticleSize, Vector3.zero, Vector3.one, false);
	                tris = RenderShapeMeshUtils.AddP2PTrisToMesh(0, false);
	                break;
	            case RenderShape.Triangle:
	            default:
	                vectors = RenderShapeMeshUtils.AddTriVectorsToMesh(m_ParticleSize, m_RenderDirection, Vector3.zero, Vector3.zero);
	                tris = RenderShapeMeshUtils.AddTriTrisToMesh(0);
	                break;
	        }

	        // write
	        m_Mesh.Clear();
	        m_Mesh.vertices = vectors;
	        m_Mesh.SetTriangles(tris, 0);
	    }

	    public void RenderHeatmap()
	    {
	        //Update();
	    }
	    
	    public void Update()
	    {
	        if (allowRender)
	        {
	            if(m_RenderState == k_BeginRenderer)
	            {
	//                m_RenderCursor = 0;
	                var map = FilterPoints();
	                PopulateMatrices(map);
	                m_RenderState = k_RenderInProgress;
	            }
	            UpdateRender();
	        }
	    }

	//    int m_RenderCursor = 0;

	    void OnGUI()
	    {
	        //GUI.Label(new Rect(0,0,100,20f), "State: " + m_RenderState);
	        //UpdateRender();
	    }

	    void UpdateRender()
	    {
	        if (m_Matrices != null && m_Material != null)
	        {
	            for (int a = 0; a < m_Matrices.Count; a++)
	            {
	                Matrix4x4[] matrixList = m_Matrices[a];
	                MaterialPropertyBlock prop = m_Properties[a];
	                #if UNITY_5_5
	                Graphics.DrawMeshInstanced(m_Mesh, 0, m_Material,
	                    matrixList, matrixList.Length, prop,
	                    UnityEngine.Rendering.ShadowCastingMode.Off, false);
	                #endif
	            }
	        }
	    }
	    
	    List<HeatPoint> FilterPoints()
	    {
	        if (hasData())
	        {
	            totalPoints = m_Data.Length;
	            currentPoints = 0;

	            var map = new List<HeatPoint>();
	            for (int a = 0; a < m_Data.Length; a++)
	            {
	                // FILTER FOR TIME & POSITION
	                var pt = m_Data[a];
	                if (pt.time >= m_StartTime && pt.time <= m_EndTime &&
	                    pt.position.x >= m_LowX && pt.position.x <= m_HighX &&
	                    pt.position.y >= m_LowY && pt.position.y <= m_HighY &&
	                    pt.position.z >= m_LowZ && pt.position.z <= m_HighZ)
	                {
	                    currentPoints++;
	                    map.Add(pt);
	                }
	            }
	            return map;
	        }
	        return null;
	    }

	    void PopulateMatrices(List<HeatPoint> map)
	    {
	        if (map == null)
	            return;

	        m_Properties = new List<MaterialPropertyBlock>();
	        m_Matrices = new List<Matrix4x4[]>();
	        Vector3 scale = Vector3.one;
	        List<Vector4[]> colors = new List<Vector4[]>();

	        int index = 0;
	        for (int a = 0; a < map.Count; a++)
	        {
	            int mod = a % k_ShapesPerDraw;
	            if (mod == 0)
	            {
	                m_Properties.Add(new MaterialPropertyBlock());
	                m_Matrices.Add(new Matrix4x4[k_ShapesPerDraw]);
	                colors.Add(new Vector4[k_ShapesPerDraw]);
	            }

	            Matrix4x4 matrix = Matrix4x4.identity;
	            HeatPoint pt = map[a];
	            Quaternion quaternion = Quaternion.Euler(pt.rotation);
	            matrix.SetTRS(pt.position, quaternion, scale);
	            m_Matrices[index][mod] = matrix;

	            float pct = (pt.density/m_MaxDensity);
	            colors[index][mod] = GradientUtils.PickGradientColor(m_Gradient, pct);

	            if (a+1 % k_ShapesPerDraw == 0)
	            {
	                index++;
	            }
	        }

	        for(int a = 0; a < m_Properties.Count; a++)
	        {
	            if (colors[a] != null && colors[a].Length > 0)
	            {
	                m_Properties[a].SetVectorArray(m_ColorID, colors[a]);
	            }
	            index++;
	        }

	//        gameObject.GetComponent<Renderer>().materials = materials;
	//        gameObject.GetComponent<HeatmapSubmap>().m_PointData = map;
	//        gameObject.GetComponent<HeatmapSubmap>().m_TrianglesPerShape = RenderShapeUtils.GetTrianglesForShape(m_RenderStyle);
	//        
	//        if (m_Tips)
	//        {
	//            if (gameObject.GetComponent<MeshCollider>() == null)
	//            {
	//                gameObject.AddComponent<MeshCollider>();
	//            }
	//            
	//            go.GetComponent<MeshCollider>().sharedMesh = mesh;
	//        }
	    }
	    

	    
	    bool hasData()
	    {
	        return m_Data != null && m_Data.Length > 0;
	    }
	}
}
