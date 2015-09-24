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

public class HeatmapMeshRenderer : MonoBehaviour, IHeatmapRenderer
{

	private const int NOT_RENDERING = 0;
	private const int BEGIN_RENDER = 1;
	private const int RENDER_IN_PROGRESS = 2;
	private const int UPDATE_MATERIALS = 4;

	//Unity limit of vectors per mesh
	int VERTICES_PER_MESH = 65000;

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

	//private Material[] renderMaterials;
	//private int renderMeshIndex = 0;

	private GameObject[] gameObjects;

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

	public int currentPoints { get; private set; }
	public int totalPoints { get; private set; }

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
			renderStyle = style;
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
					//UpdateRenderCycle (0, data.Length, renderMaterials);
				}
				break;
			case UPDATE_MATERIALS:
				int pt = 0;
				int indexPt = 0;
				int currentSubmap = 0;
				int oldSubmap = -1;
				int verticesPerShape = GetVecticesForShape ();
				GameObject go = null;
				Material[] materials = null;
				for (int a = 0; a < data.Length; a++) {
					if (data [a].time >= StartTime && data [a].time <= EndTime) {

						currentSubmap = (pt * verticesPerShape) / VERTICES_PER_MESH;

						if (currentSubmap != oldSubmap) {
							if (go != null && materials != null) {
								go.GetComponent<Renderer> ().materials = materials;
							}

							indexPt = 0;
							go = gameObjects [currentSubmap];
							materials = go.GetComponent<Renderer> ().sharedMaterials;
						}


						materials [indexPt] = PickMaterial (data [a].density / maxDensity);

						oldSubmap = currentSubmap;
						pt++;
						indexPt++;
					}
				}
				if (go != null && materials != null) {
					go.GetComponent<Renderer> ().materials = materials;
				}

				renderState = NOT_RENDERING;
				break;
			}
		}
	}

	private void CreatePoints()
	{
		if (hasData ()) {
			totalPoints = data.Length;
			currentPoints = 0;

			List<List<HeatPoint>> submaps = new List<List<HeatPoint>> ();
			int currentSubmap = 0;
			int verticesPerShape = GetVecticesForShape ();


			for (int a = 0; a < data.Length; a++) {
				//FILTER FOR TIME
				if (data [a].time >= StartTime && data [a].time <= EndTime) {
					currentPoints++;
					if (submaps.Count <= currentSubmap) {
						submaps.Add(new List<HeatPoint>());
					}
					submaps [currentSubmap].Add (data [a]);
					currentSubmap = (currentPoints * verticesPerShape) / VERTICES_PER_MESH;
				}
			}

			// Remove existing GOs
			// FIXME: optimize since most of the time we won't need to destroy and create GOs

			int c = 0;
			int bailout = 99;
			while (gameObject.transform.childCount > 0 && c < bailout) {
				Transform trans = gameObject.transform.FindChild ("Submap" + c);
				if (trans != null) {
					trans.parent = null;
					GameObject.DestroyImmediate (trans.gameObject);
				}
				c++;
			}

			if (currentPoints == 0) {
				renderState = NOT_RENDERING;
				return;
			}

			gameObjects = new GameObject[submaps.Count];

			for (int b = 0; b < gameObjects.Length; b++) {
				GameObject go = new GameObject ("Submap" + b);

				go.AddComponent<MeshFilter> ();
				go.AddComponent<MeshRenderer> ();

				Mesh renderMesh = new Mesh ();
				renderMesh.Clear ();
				renderMesh.subMeshCount = submaps[b].Count;

				go.GetComponent<MeshFilter> ().mesh = renderMesh;
				go.GetComponent<MeshRenderer> ().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
				go.GetComponent<MeshRenderer> ().receiveShadows = false;
				go.GetComponent<MeshRenderer> ().useLightProbes = false;
				go.GetComponent<MeshRenderer> ().reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;


				gameObjects [b] = go;
				go.transform.parent = gameObject.transform;
				RenderSubmap (go, submaps[b]);
			}
			renderState = NOT_RENDERING;
		}
	}

	private void RenderSubmap(GameObject go, List<HeatPoint> submap) {
		List<int[]> allTris = new List<int[]> ();
		List<Vector3[]> allVectors = new List<Vector3[]> ();
		Vector3[] vector3;
		Material[] materials = new Material[submap.Count];

		for (int a = 0; a < submap.Count; a++) {
			materials [a] = PickMaterial (submap [a].density / maxDensity);
			Vector3 position = data [a].position;
			switch (renderStyle) {
			case RenderShape.CUBE:
				vector3 = AddCubeVectorsToMesh (position.x, position.y, position.z);
				allVectors.Add (vector3);
				allTris.Add (AddCubeTrisToMesh (a * vector3.Length));
				break;
			case RenderShape.PYRAMID:
				vector3 = AddPyramidVectorsToMesh (position.x, position.y, position.z);
				allVectors.Add (vector3);
				allTris.Add (AddPyramidTrisToMesh (a * vector3.Length));
				break;
			case RenderShape.SQUARE:
				vector3 = AddSquareVectorsToMesh (position.x, position.y, position.z);
				allVectors.Add (vector3);
				allTris.Add (AddSquareTrisToMesh (a * vector3.Length));
				break;
			case RenderShape.TRI:
				vector3 = AddTriVectorsToMesh (position.x, position.y, position.z);
				allVectors.Add (vector3);
				allTris.Add (AddTriTrisToMesh (a * vector3.Length));
				break;
			}
		}

		Vector3[] combinedVertices = allVectors.SelectMany (x => x).ToArray<Vector3> ();
		Mesh mesh = go.GetComponent<MeshFilter> ().sharedMesh;
		mesh.vertices = combinedVertices;
		for (int j = 0; j < allTris.Count; j++) {
			int[] t = allTris [j];
			mesh.SetTriangles (t, j);
		}
		go.GetComponent<Renderer> ().materials = materials;
		mesh.Optimize ();

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

	private Vector3[] AddPyramidVectorsToMesh(float x, float y, float z) {
		float halfP = particleSize / 2;

		Vector3 p0 = new Vector3 (x-halfP, y-halfP, z-halfP);
		Vector3 p1 = new Vector3 (x+halfP, y-halfP, z-halfP);
		Vector3 p2 = new Vector3 (x, y-halfP, z+halfP);
		Vector3 p3 = new Vector3 (x, y+halfP, z);

		return new Vector3[] { p0, p1, p2, p3 };
	}

	//Generate a pyramid mesh procedurally
	private int[] AddPyramidTrisToMesh(int offset) {
		var tris = new int[] {
			0, 1, 2,	//bottom
			0, 3, 1,	//front
			1, 3, 2,	//right-b
			2, 3, 1,	//left-b
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

	private int GetVecticesForShape() {
		// Verts is the number of UNIQUE vertices in each shape
		int verts = 0;
		switch (renderStyle) {
		case RenderShape.CUBE:
			verts = 8;
			break;
		case RenderShape.PYRAMID:
			verts = 4;
			break;
		case RenderShape.SQUARE:
			verts = 4;
			break;
		case RenderShape.TRI:
			verts = 3;
			break;
		}
		return verts;
	}



	private int GetTrisForShape() {
		int tris = 0;
		switch (renderStyle) {
		case RenderShape.CUBE:
			tris = 32;
			break;
		case RenderShape.PYRAMID:
			tris = 4;
			break;
		case RenderShape.SQUARE:
			tris = 6;
			break;
		case RenderShape.TRI:
			tris = 3;
			break;
		}
		return tris;
	}

	private Material PickMaterial(float value) {
		int i = 1;
		if (materials == null)
			return null;
		if (value > HighThreshold) {
			i = 2;
		} else if (value < LowThreshold) {
			i = 0;
		}
		return materials[i];
	}
}
