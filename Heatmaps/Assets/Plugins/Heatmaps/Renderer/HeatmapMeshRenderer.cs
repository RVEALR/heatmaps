/// <summary>
/// Heat map mesh renderer.
/// </summary>
/// This is the default renderer that comes with the Heat Maps package.
/// It procedurally constructs a mesh to display Heat Map data. You
/// might consider writing your own renderer. If you do, we recommend
/// following the defined IHeatmapRenderer interface.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityAnalyticsHeatmap;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(GradientContainer))]
public class HeatmapMeshRenderer : MonoBehaviour, IHeatmapRenderer
{

    const int k_NotRendering = 0;
    const int k_BeginRenderer = 1;
    const int k_RenderInProgress = 2;
    const int k_UpdateMaterials = 4;

    const int k_SliceMasking = 0;
    const int k_RadiusMasking = 1;

    // Unity limit of vectors per mesh
    const int k_VerticesPerMesh = 65000;

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

    int m_MaskOption = k_SliceMasking;
    float m_MaskRadius = 1.0f;
    Vector3 m_MaskSource = Vector3.zero;

    RenderProjection m_Projection = RenderProjection.FirstPerson;

    RenderShape m_RenderStyle = RenderShape.Cube;
    RenderDirection m_RenderDirection = RenderDirection.Billboard;

    Shader m_Shader;
    public Material[] m_Materials;
    Gradient m_Gradient;

    int m_RenderState = k_NotRendering;

    List<GameObject> m_GameObjects = new List<GameObject>();

    void Start()
    {
        if (m_Materials == null || m_Materials.Length == 0)
        {
            m_Shader = Shader.Find("Heatmaps/Particles/AlphaBlend");
            m_Materials = new Material[1];
            m_Materials[0] = new Material(m_Shader);
        }
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
        if (gradient == null || !GradientUtils.CompareGradients(gradient, m_Gradient))
        {
            m_Gradient = gradient;
            m_RenderState = k_UpdateMaterials;
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
        if (m_MaskOption != k_SliceMasking)
        {
            m_MaskOption = k_SliceMasking;
            m_RenderState = k_BeginRenderer;
        }
    }

    public void UpdateRenderMask(float radius)
    {
        if (m_MaskOption != k_RadiusMasking || radius != m_MaskRadius)
        {
            m_MaskRadius = radius;
            m_MaskOption = k_RadiusMasking;
            m_RenderState = k_BeginRenderer;
        }
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
        if (projection != m_Projection)
        {
            m_Projection = projection;
            m_RenderState = k_BeginRenderer;
        }
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

    public string remapLabel{ get; set; }

    public bool allowRender{ get; set; }

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
            m_RenderState = k_BeginRenderer;
        }
    }

    void Update()
    {
        RenderHeatmap();
    }

    public void RenderHeatmap()
    {
        if (allowRender)
        {
            switch (m_RenderState)
            {
                case k_BeginRenderer:
                    m_RenderState = k_RenderInProgress;
                    CreatePoints();
                    break;
                case k_RenderInProgress:
                    if (hasData())
                    {
                        // No-op for now.
                        //UpdateRenderCycle (0, data.Length, renderMaterials);
                    }
                    break;
                case k_UpdateMaterials:
                    if (hasData())
                    {
                        UpdateMaterials();
                        CreatePoints();
                    }
                    break;
            }
        }
    }

    void UpdateMaterials()
    {
        int pt = 0;         // cursor that increments each time we find a point in the time range
        int currentSubmap = 0;
        int oldSubmap = -1;
        int verticesPerShape = RenderShapeMeshUtils.GetVecticesForShape(m_RenderStyle, m_Projection);
        GameObject go = null;
        Material[] materials = null;

        for (int a = 0; a < m_Data.Length; a++)
        {
            if (m_Data[a].time >= m_StartTime && m_Data[a].time <= m_EndTime)
            {
                currentSubmap = (pt * verticesPerShape) / k_VerticesPerMesh;
                if (currentSubmap != oldSubmap)
                {
                    if (go != null && materials != null)
                    {
                        go.GetComponent<Renderer>().materials = materials;
                    }
                    go = m_GameObjects[currentSubmap];
                    materials = go.GetComponent<Renderer>().sharedMaterials;
                }
                materials = m_Materials;
                oldSubmap = currentSubmap;
                pt++;
            }
        }
        if (go != null && materials != null)
        {
            go.GetComponent<Renderer>().materials = materials;
        }
    }

    bool FilterPoint(HeatPoint pt)
    {
        if (pt.time < m_StartTime || pt.time > m_EndTime)
        {
            return false;
        }

        if (m_MaskOption == k_RadiusMasking)
        {
            return true;
        }

        return 
            pt.position.x >= m_LowX && pt.position.x <= m_HighX &&
            pt.position.y >= m_LowY && pt.position.y <= m_HighY &&
            pt.position.z >= m_LowZ && pt.position.z <= m_HighZ;
    }

    void CreatePoints()
    {
        if (hasData())
        {
            m_CollapseDensity = 0f;

            totalPoints = m_Data.Length;
            currentPoints = 0;

            var submaps = new List<List<HeatPoint>>();
            int currentSubmap = 0;
            int verticesPerShape = RenderShapeMeshUtils.GetVecticesForShape(m_RenderStyle, m_Projection);

            // Filter & Aggregate
            Dictionary<Vector3, HeatPoint> collapsePoints = new Dictionary<Vector3, HeatPoint>();
            List<HeatPoint> otherPoints = new List<HeatPoint>();

            for (int a = 0; a < m_Data.Length; a++)
            {
                // FILTER FOR TIME & POSITION
                var pt = m_Data[a];
                if (FilterPoint(pt))
                {
                    if (m_MaskOption == k_RadiusMasking && m_Projection == RenderProjection.FirstPerson && IsOutsideRadius(pt))
                    {
                        Aggregate(pt, collapsePoints);
                    }
                    else
                    {
                        otherPoints.Add(pt);
                    }
                }
            }
            HeatPoint[] dictData = collapsePoints.Values.ToArray();
            HeatPoint[] filteredData = new HeatPoint[dictData.Length + otherPoints.Count];

            dictData.CopyTo(filteredData, 0);
            otherPoints.CopyTo(filteredData, dictData.Length);


            // Arrange into submaps
            for (int a = 0; a < filteredData.Length; a++)
            {
                var pt = filteredData[a];
                currentPoints++;
                if (submaps.Count <= currentSubmap)
                {
                    submaps.Add(new List<HeatPoint>());
                }
                submaps[currentSubmap].Add(pt);
                currentSubmap = (currentPoints * verticesPerShape) / k_VerticesPerMesh;
            }

            int neededSubmaps = submaps.Count;
            int currentSubmaps = m_GameObjects.Count;
            int addCount = neededSubmaps - currentSubmaps;

            if (addCount > 0)
            {
                // Add submaps if we need more
                for (int a = 0; a < addCount; a++)
                {
                    int submapID = currentSubmaps + a;
                    var go = new GameObject("Submap" + submapID);
                    go.AddComponent<HeatmapSubmap>();
                    go.GetComponent<MeshFilter>().sharedMesh = new Mesh();

                    go.transform.parent = gameObject.transform;
                    m_GameObjects.Add(go);
                }
            }
            else if (addCount < 0)
            {
                // Dispose of excess submaps
                for (var a = neededSubmaps; a < currentSubmaps; a++)
                {
                    Transform trans = gameObject.transform.FindChild("Submap" + a);
                    if (trans != null)
                    {
                        trans.parent = null;
                        m_GameObjects.Remove(trans.gameObject);
                        GameObject.DestroyImmediate(trans.gameObject);
                    }
                }
            }
            //Render submaps
            for (var a = 0; a < m_GameObjects.Count; a++)
            {
                Mesh renderMesh = m_GameObjects[a].GetComponent<MeshFilter>().sharedMesh;
                renderMesh.Clear();
                renderMesh.subMeshCount = submaps[a].Count;
                RenderSubmap(m_GameObjects[a], submaps[a]);
            }
            m_RenderState = k_NotRendering;
        }
    }

    void RenderSubmap(GameObject go, List<HeatPoint> submap)
    {
        var allTris = new List<int[]>();
        var allVectors = new List<Vector3[]>();
        var allColors = new List<Color32[]>();
        Vector3[] vector3 = null;
        var materials = new Material[submap.Count];

        for (int a = 0; a < submap.Count; a++)
        {
            materials[a] = m_Materials[0];
            Vector3 position = submap[a].position;
            Vector3 rotation = submap[a].rotation;
            Vector3 destination = submap[a].destination;

            switch (m_RenderStyle)
            {
                case RenderShape.Cube:
                    vector3 = RenderShapeMeshUtils.AddCubeVectorsToMesh(m_ParticleSize, position.x, position.y, position.z);
                    allVectors.Add(vector3);
                    allTris.Add(RenderShapeMeshUtils.AddCubeTrisToMesh(a * vector3.Length));
                    break;
                case RenderShape.Arrow:
                    if (m_MaskOption == k_RadiusMasking && m_Projection == RenderProjection.FirstPerson && IsOutsideRadius(submap[a]))
                    {
                        vector3 = RenderShapeMeshUtils.AddDiamondVectorsToMesh(m_ParticleSize, RenderDirection.Billboard, position, m_MaskSource);
                        allVectors.Add(vector3);
                        allTris.Add(RenderShapeMeshUtils.AddArrowTrisToMesh(a * vector3.Length, m_Projection));
                    }
                    else
                    {
                        vector3 = RenderShapeMeshUtils.AddArrowVectorsToMesh(m_ParticleSize, position, rotation, m_Projection);
                        allVectors.Add(vector3);
                        allTris.Add(RenderShapeMeshUtils.AddArrowTrisToMesh(a * vector3.Length, m_Projection));
                    }
                    break;
                case RenderShape.Square:
                    vector3 = RenderShapeMeshUtils.AddSquareVectorsToMesh(m_ParticleSize, m_RenderDirection, position, m_MaskSource);
                    allVectors.Add(vector3);
                    allTris.Add(RenderShapeMeshUtils.AddSquareTrisToMesh(a * vector3.Length));
                    break;
                case RenderShape.Triangle:
                    vector3 = RenderShapeMeshUtils.AddTriVectorsToMesh(m_ParticleSize, m_RenderDirection, position, m_MaskSource);
                    allVectors.Add(vector3);
                    allTris.Add(RenderShapeMeshUtils.AddTriTrisToMesh(a * vector3.Length));
                    break;
                case RenderShape.PointToPoint:
                    var collapsed = m_MaskOption == k_RadiusMasking && m_Projection == RenderProjection.FirstPerson && IsOutsideRadius(submap[a]);
                    vector3 = RenderShapeMeshUtils.AddP2PVectorsToMesh(m_ParticleSize, position, destination, collapsed);
                    allVectors.Add(vector3);
                    allTris.Add(RenderShapeMeshUtils.AddP2PTrisToMesh(a * vector3.Length, collapsed));
                    break;
            }
            allColors.Add(AddColorsToMesh(vector3.Length, submap[a]));
        }
        Vector3[] combinedVertices = allVectors.SelectMany(x => x).ToArray<Vector3>();
        Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
        mesh.vertices = combinedVertices;
        mesh.colors32 = allColors.SelectMany(x => x).ToArray<Color32>();

        for (int j = 0; j < allTris.Count; j++)
        {
            int[] t = allTris[j];
            mesh.SetTriangles(t, j);
        }
        go.GetComponent<Renderer>().materials = materials;
        go.GetComponent<HeatmapSubmap>().m_PointData = submap;
        go.GetComponent<HeatmapSubmap>().m_TrianglesPerShape = RenderShapeMeshUtils.GetTrianglesForShape(m_RenderStyle, m_Projection);
        //mesh.Optimize();

        if (m_Tips)
        {
            if (go.GetComponent<MeshCollider>() == null)
            {
                go.AddComponent<MeshCollider>();
            }

            go.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }

    bool hasData()
    {
        return m_Data != null && m_Data.Length > 0;
    }

    float m_CollapseDensity = 0f;

    HeatPoint Aggregate(HeatPoint pt, Dictionary<Vector3, HeatPoint> collapsePoints)
    {
        HeatPoint retv = pt;
        if (collapsePoints.ContainsKey(pt.position))
        {
            retv = collapsePoints[pt.position];
            retv.density += pt.density;
        }
        m_CollapseDensity = Mathf.Max(retv.density, m_MaxDensity);
        collapsePoints[pt.position] = retv;
        return retv;
    }

    Color32[] AddColorsToMesh(int count, HeatPoint pt)
    {
        Color32[] colors = new Color32[count];
        float pct = (m_MaskOption == k_RadiusMasking && IsOutsideRadius(pt)) ? (pt.density/m_CollapseDensity) : (pt.density/m_MaxDensity);
        if (float.IsInfinity(pct)) {
            pct = 0f;
        }
        Color color = GradientUtils.PickGradientColor(m_Gradient, pct);
        for (int b = 0 ; b < count ; b++)
        {
            colors[b] = new Color (color.r, color.g, color.b, color.a) ;
        }
        return colors;
    }

    bool IsOutsideRadius(HeatPoint pt)
    {
        return  Vector3.Distance(m_MaskSource, pt.position) > m_MaskRadius;
    }
}
