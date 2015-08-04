/// <summary>
/// Heat map mesh renderer.
/// </summary>
/// This is the default renderer that comes with the Heat Maps package.
/// It procedurally constructs a mesh to display Heat Map data. You
/// might consider writing your own renderer. If you do, we recommend
/// following the defined IHeatMapRenderer interface.

using System;
using UnityEngine;
using UnityAnalytics;
using System.Collections;

[RequireComponent (typeof (MeshCollider))]
[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class HeatMapMeshRenderer : MonoBehaviour, IHeatMapRenderer
{

	private const int NOT_RENDERING = 0;
	private const int BEGIN_RENDER = 1;
	private const int RENDER_IN_PROGRESS = 2;

	//Density Thresholds
	private float HighThreshold;
	private float LowThreshold;

	//Time limits
	private float StartTime = 0f;
	private float EndTime = 1.0f;

	[Range(0.1f, 100f)]
	private float particleSize = 1.0f;
	private int currentResolution;

	//Particle Rendering Data
	private HeatPoint[] data;
	private float maxDensity = 0f;

	private RenderShape renderStyle = RenderShape.CUBE;

	private Shader shader;
	public Material[] materials;

	private int renderState = NOT_RENDERING;
	private Mesh renderMesh;
	private Material[] renderMaterials;
	private int currentRenderIndex = 0;
	private int pointsPerCycle = 3000;
	private int renderMeshIndex = 0;

	void Start()
	{
		shader = Shader.Find("Heatmaps/Particles/AlphaBlend");
	}

	public void UpdatePointData(HeatPoint[] newData, float newMaxDensity)
	{
		data = newData;
		maxDensity = newMaxDensity;
		renderState = BEGIN_RENDER;
	}

	public void UpdateColors(Color[] colors)
	{
		Color newLowColor = colors [0];
		Color newMediumColor = colors [1];
		Color newHighColor = colors [2];

		if (materials == null || materials[0].GetColor("_TintColor") != newLowColor || 
			materials[1].GetColor("_TintColor") != newMediumColor || 
				materials[2].GetColor("_TintColor") != newHighColor) {

			shader = Shader.Find("Heatmaps/Particles/AlphaBlend");
			materials = new Material[3];
			materials [0] = new Material (shader);
			materials[0].SetColor("_TintColor", newLowColor);

			materials [1] = new Material (shader);
			materials[1].SetColor("_TintColor", newMediumColor);

			materials [2] = new Material (shader);
			materials[2].SetColor("_TintColor", newHighColor);

			renderState = BEGIN_RENDER;
		}
	}

	public void UpdateThresholds(float[] threshholds)
	{
		float newLowThreshold = threshholds [0];
		float newHighThreshold = threshholds [1];
		if (HighThreshold != newHighThreshold || LowThreshold != newLowThreshold) {
			HighThreshold = newHighThreshold;
			LowThreshold = newLowThreshold;
			renderState = BEGIN_RENDER;
		}
	}

	public float pointSize
	{
		get{
			return particleSize;
		}
		set{
			if (particleSize != value) {
				particleSize = value;
				renderState = BEGIN_RENDER;
			}
		}
	}

	public bool allowRender{ get; set; }

	public void UpdateTimeLimits(float startTime, float endTime) {
		if (StartTime != startTime || EndTime != endTime) {
			StartTime = startTime;
			EndTime = endTime;
			renderState = BEGIN_RENDER;
		}
	}

	public void UpdateRenderStyle(RenderShape style) {
		if (style != renderStyle) {
			switch (style) {
			case RenderShape.CUBE:
				renderStyle = style;
				break;
			case RenderShape.SQUARE:
				renderStyle = style;
				break;
			case RenderShape.TRI:
				renderStyle = style;
				break;
			}
			renderState = BEGIN_RENDER;
		}
	}

	public void RenderHeatMap() {
		if (allowRender) {
			if (renderState == BEGIN_RENDER) {
				renderState = RENDER_IN_PROGRESS;
				CreatePoints ();
			} else if (renderState == RENDER_IN_PROGRESS) {
				if (hasData ()) {
					UpdateRenderCycle (currentRenderIndex, data.Length, pointsPerCycle, renderMesh, renderMaterials);
				}
			}
		}
	}

	private void CreatePoints()
	{
		if (hasData ()) {
			renderMesh = new Mesh ();
			renderMesh.Clear ();
			renderMesh.subMeshCount = data.Length;
			gameObject.GetComponent<MeshFilter> ().mesh = renderMesh;

			renderMaterials = new Material[data.Length];

			renderMeshIndex = 0;
			UpdateRenderCycle (0, data.Length, pointsPerCycle, renderMesh, renderMaterials);
		}
	}

	private void UpdateRenderCycle(int startPointsIndex, int endPointsIndex, int pointsPerCycle, Mesh mesh, Material[] materials) 
	{
		currentRenderIndex = Math.Min (startPointsIndex + pointsPerCycle, endPointsIndex);
		for (int i = startPointsIndex; i < currentRenderIndex; i++) {
			materials [renderMeshIndex] = PickMaterial (data [i].density / maxDensity);
			//FILTER FOR TIME
			if (data [i].time >= StartTime && data [i].time <= EndTime) {
				switch (renderStyle) {
				case RenderShape.CUBE:
					AddCubeToMesh (mesh, renderMeshIndex++, data [i].position.x, data [i].position.y, data [i].position.z);
					break;
				case RenderShape.SQUARE:
					AddSquareToMesh (mesh, renderMeshIndex++, data [i].position.x, data [i].position.y, data [i].position.z);
					break;
				case RenderShape.TRI:
					AddTriToMesh (mesh, renderMeshIndex++, data [i].position.x, data [i].position.y, data [i].position.z);
					break;
				}
			}
		}
		gameObject.GetComponent<Renderer> ().materials = materials;
		mesh.RecalculateBounds ();
		mesh.Optimize ();
		if (currentRenderIndex >= endPointsIndex) {
			renderState = NOT_RENDERING;
		}
	}

	//Generate a cube mesh procedurally
	private Mesh AddCubeToMesh(Mesh mesh, int index, float x, float y, float z) {
		float halfP = particleSize / 2;

		Vector3 p0 = new Vector3 (x-halfP, y-halfP, z-halfP);
		Vector3 p1 = new Vector3 (x+halfP, y-halfP, z-halfP);
		Vector3 p2 = new Vector3 (x+halfP, y+halfP, z-halfP);
		Vector3 p3 = new Vector3 (x-halfP, y+halfP, z-halfP);

		Vector3 p4 = new Vector3 (x-halfP, y-halfP, z+halfP);
		Vector3 p5 = new Vector3 (x+halfP, y-halfP, z+halfP);
		Vector3 p6 = new Vector3 (x+halfP, y+halfP, z+halfP);
		Vector3 p7 = new Vector3 (x-halfP, y+halfP, z+halfP);

		var additionalVertices = new Vector3[] { p0, p1, p2, p3, p4, p5, p6, p7 };
		var combinedVertices = new Vector3[mesh.vertexCount + additionalVertices.Length];
		mesh.vertices.CopyTo (combinedVertices, 0);
		additionalVertices.CopyTo (combinedVertices, mesh.vertexCount);
		mesh.vertices = combinedVertices;

		var tris = new int[] {
			0, 1, 2,	//bottom
			0, 2, 3,
			4, 6, 5,	//top
			4, 7, 6,
			1, 6, 2,	//right
			1, 5, 6,

			3, 4, 0,	//left
			3, 7, 4,
			2, 7, 3,	//back
			2, 6, 7,
			0, 4, 5,	//front
			0, 5, 1
		};

		int offset = index * additionalVertices.Length;
		for (int a = 0; a < tris.Length; a++) {
			tris[a] += offset;
		}
		mesh.SetTriangles (tris, index);
		return mesh;
	}

	//Generate a procedural square
	private Mesh AddSquareToMesh(Mesh mesh, int index, float x, float y, float z) {
		float halfP = particleSize / 2;

		Vector3 p0 = new Vector3 (x-halfP, y, z-halfP);
		Vector3 p1 = new Vector3 (x+halfP, y, z-halfP);
		Vector3 p2 = new Vector3 (x+halfP, y, z+halfP);
		Vector3 p3 = new Vector3 (x-halfP, y, z+halfP);

		var additionalVertices = new Vector3[] { p0, p1, p2, p3 };
		var combinedVertices = new Vector3[mesh.vertexCount + additionalVertices.Length];
		mesh.vertices.CopyTo (combinedVertices, 0);
		additionalVertices.CopyTo (combinedVertices, mesh.vertexCount);
		mesh.vertices = combinedVertices;

		var tris = new int[] {
			0, 2, 1,	//top
			0, 3, 2
		};

		int offset = index * additionalVertices.Length;
		for (int a = 0; a < tris.Length; a++) {
			tris[a] += offset;
		}
		mesh.SetTriangles (tris, index);
		return mesh;
	}

	//Generate a procedural tri
	private Mesh AddTriToMesh(Mesh mesh, int index, float x, float y, float z) {
		float halfP = particleSize / 2;

		Vector3 p0 = new Vector3 (x-halfP, y, z-halfP);
		Vector3 p1 = new Vector3 (x, y, z+halfP);
		Vector3 p2 = new Vector3 (x+halfP, y, z-halfP);

		var additionalVertices = new Vector3[] { p0, p1, p2 };
		var combinedVertices = new Vector3[mesh.vertexCount + additionalVertices.Length];
		mesh.vertices.CopyTo (combinedVertices, 0);
		additionalVertices.CopyTo (combinedVertices, mesh.vertexCount);
		mesh.vertices = combinedVertices;

		var tris = new int[] {
			0, 2, 1	//top
		};

		int offset = index * additionalVertices.Length;
		for (int a = 0; a < tris.Length; a++) {
			tris[a] += offset;
		}
		mesh.SetTriangles (tris, index);
		return mesh;
	}

	private bool hasData() {
		return data != null && data.Length > 0;
	}

	private Material PickMaterial(float value) {
		if (materials == null)
			return null;
		Material material = materials[1];
		if (value > HighThreshold) {
			material = materials[2];
		} else if (value < LowThreshold) {
			material = materials[0];
		}
		return material;
	}
}
