/// <summary>
/// Heat map mesh renderer.
/// </summary>
/// This is the default renderer that comes with the Heat Maps package.
/// It procedurally constructs a mesh to display Heat Map data. You
/// might consider writing your own renderer. If you do, we recommend
/// following the defined IHeatmapRenderer interface.

using System;
using UnityEngine;
using UnityAnalyticsHeatmap;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent (typeof (MeshCollider))]
[RequireComponent (typeof (MeshFilter))]
[RequireComponent (typeof (MeshRenderer))]
public class HeatmapMeshRenderer : MonoBehaviour, IHeatmapRenderer
{

	private const int NOT_RENDERING = 0;
	private const int BEGIN_RENDER = 1;
	private const int RENDER_IN_PROGRESS = 2;
	private const int UPDATE_MATERIALS = 4;

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
	private RenderDirection renderDirection = RenderDirection.YZ;

	private Shader shader;
	public Material[] materials;

	private int renderState = NOT_RENDERING;
	private Mesh renderMesh;
	private Material[] renderMaterials;
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

		if (materials == null || materials.Length == 0 ||
			materials[0].GetColor("_TintColor") != newLowColor || 
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

			renderState = UPDATE_MATERIALS;
		}
	}

	public void UpdateThresholds(float[] threshholds)
	{
		float newLowThreshold = threshholds [0];
		float newHighThreshold = threshholds [1];
		if (HighThreshold != newHighThreshold || LowThreshold != newLowThreshold) {
			HighThreshold = newHighThreshold;
			LowThreshold = newLowThreshold;
			renderState = UPDATE_MATERIALS;
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

	public void UpdateRenderStyle(RenderShape style, RenderDirection direction) {
		if (style != renderStyle || direction != renderDirection) {
			renderDirection = direction;
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

	public void RenderHeatmap() {
		if (allowRender) {
			switch (renderState) {
			case BEGIN_RENDER:
				renderState = RENDER_IN_PROGRESS;
				CreatePoints ();
				break;
			case RENDER_IN_PROGRESS:
				if (hasData ()) {
					UpdateRenderCycle (0, data.Length, renderMesh, renderMaterials);
				}
				break;
			case UPDATE_MATERIALS:
				renderMeshIndex = 0;
				for (int i = 0; i < data.Length; i++) {
					if (data [i].time >= StartTime && data [i].time <= EndTime) {
						renderMaterials [renderMeshIndex] = PickMaterial (data [i].density / maxDensity);
						renderMeshIndex ++;
					}
				}
				gameObject.GetComponent<Renderer> ().materials = renderMaterials;
				renderState = NOT_RENDERING;
				break;
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

			int count = 1;
			for (int i = 0; i < data.Length; i++) {
				//FILTER FOR TIME
				if (data [i].time >= StartTime && data [i].time <= EndTime) {
					count++;
				}
			}

			renderMaterials = new Material[count];

			renderMeshIndex = 0;
			UpdateRenderCycle (0, data.Length, renderMesh, renderMaterials);
		}
	}

	private void UpdateRenderCycle(int startPointsIndex, int endPointsIndex, Mesh mesh, Material[] materials) 
	{
		List<int[]> allTris = new List<int[]> ();
		List<Vector3[]> allVectors = new List<Vector3[]> ();
		Vector3[] vector3;

		for (int i = startPointsIndex; i < endPointsIndex; i++) {
			materials [renderMeshIndex] = PickMaterial (data [i].density / maxDensity);
			//FILTER FOR TIME
			if (data [i].time >= StartTime && data [i].time <= EndTime) {
				Vector3 position = data [i].position;
				switch (renderStyle) {
				case RenderShape.CUBE:
					vector3 = AddCubeVectorsToMesh (position.x, position.y, position.z);
					allVectors.Add (vector3);
					allTris.Add(AddCubeTrisToMesh (renderMeshIndex++ * vector3.Length));
					break;
				case RenderShape.SQUARE:
					vector3 = AddSquareVectorsToMesh (position.x, position.y, position.z);
					allVectors.Add (vector3);
					allTris.Add(AddSquareTrisToMesh (renderMeshIndex++ * vector3.Length));
					break;
				case RenderShape.TRI:
					vector3 = AddTriVectorsToMesh (position.x, position.y, position.z);
					allVectors.Add (vector3);
					allTris.Add(AddTriTrisToMesh (renderMeshIndex++ * vector3.Length));
					break;
				}
			}
		}

		int[] tris = allTris.SelectMany (x => x).ToArray<int> ();
		Vector3[] combinedVertices = allVectors.SelectMany (x => x).ToArray<Vector3> ();

		mesh.vertices = combinedVertices;
		mesh.SetTriangles (tris, 0);
		gameObject.GetComponent<Renderer> ().materials = materials;

		mesh.Optimize ();
		renderState = NOT_RENDERING;
	}

	private Vector3[] AddCubeVectorsToMesh(float x, float y, float z) {
		float halfP = particleSize / 2;

		Vector3 p0 = new Vector3 (x-halfP, y-halfP, z-halfP);
		Vector3 p1 = new Vector3 (x+halfP, y-halfP, z-halfP);
		Vector3 p2 = new Vector3 (x+halfP, y+halfP, z-halfP);
		Vector3 p3 = new Vector3 (x-halfP, y+halfP, z-halfP);

		Vector3 p4 = new Vector3 (x-halfP, y-halfP, z+halfP);
		Vector3 p5 = new Vector3 (x+halfP, y-halfP, z+halfP);
		Vector3 p6 = new Vector3 (x+halfP, y+halfP, z+halfP);
		Vector3 p7 = new Vector3 (x-halfP, y+halfP, z+halfP);

		return new Vector3[] { p0, p1, p2, p3, p4, p5, p6, p7 };
	}

	//Generate a cube mesh procedurally
	private int[] AddCubeTrisToMesh(int offset) {
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
		for (int a = 0; a < tris.Length; a++) {
			tris[a] += offset;
		}
		return tris;
	}

	private Vector3[] AddSquareVectorsToMesh(float x, float y, float z) {
		float halfP = particleSize / 2;

		Vector3 p0, p1, p2, p3;

		switch (renderDirection) {
		case RenderDirection.YZ:
			p0 = new Vector3 (x, y-halfP, z-halfP);
			p1 = new Vector3 (x, y+halfP, z-halfP);
			p2 = new Vector3 (x, y+halfP, z+halfP);
			p3 = new Vector3 (x, y-halfP, z+halfP);
			break;

		case RenderDirection.XZ:
			p0 = new Vector3 (x-halfP, y, z-halfP);
			p1 = new Vector3 (x+halfP, y, z-halfP);
			p2 = new Vector3 (x+halfP, y, z+halfP);
			p3 = new Vector3 (x-halfP, y, z+halfP);
			break;

		default:
			p0 = new Vector3 (x-halfP, y-halfP, z);
			p1 = new Vector3 (x+halfP, y-halfP, z);
			p2 = new Vector3 (x+halfP, y+halfP, z);
			p3 = new Vector3 (x-halfP, y+halfP, z);
			break;
		}

		return new Vector3[] { p0, p1, p2, p3 };
	}

	//Generate a procedural square
	private int[] AddSquareTrisToMesh(int offset) {
		var tris = new int[] {
			offset, offset+2, offset+1,	//top
			offset, offset+3, offset+2
		};
		return tris;
	}

	private Vector3[] AddTriVectorsToMesh(float x, float y, float z) {
		float halfP = particleSize / 2;

		Vector3 p0, p1, p2;

		switch (renderDirection) {
		case RenderDirection.YZ:
			p0 = new Vector3 (x, y-halfP, z-halfP);
			p1 = new Vector3 (x, y, z+halfP);
			p2 = new Vector3 (x, y+halfP, z-halfP);
			break;

		case RenderDirection.XZ:
			p0 = new Vector3 (x-halfP, y, z-halfP);
			p1 = new Vector3 (x, y, z+halfP);
			p2 = new Vector3 (x+halfP, y, z-halfP);
			break;

		default:
			p0 = new Vector3 (x-halfP, y-halfP, z);
			p1 = new Vector3 (x, y+halfP, z);
			p2 = new Vector3 (x+halfP, y-halfP, z);
			break;
		}

		return new Vector3[] { p0, p1, p2 };
	}

	//Generate a procedural tri
	private int[] AddTriTrisToMesh(int offset) {
		var tris = new int[] {
			offset, offset+2, offset+1	//top
		};
		return tris;
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
