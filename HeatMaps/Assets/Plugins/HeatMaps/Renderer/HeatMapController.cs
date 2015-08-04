/// <summary>
/// Heat map controller.
/// </summary>
/// This is an exceedingly simple example of a runtime controller
/// for the HeatMapMeshRenderer. It's not exceptionally flexible,
/// but it teaches you everything you need to know if you want to
/// use the HeatMapMeshRenderer as a runtime component. To use:
/// 
/// 1. If you don’t have a Resources folder, create one.
/// 2. In the Resources folder, place an output of heat_map_aggr.py (i.e., a JSON file).
/// 3. In your game, create an empty GameObject. Name it ‘MyRuntimeHeatMap’.
/// 4. Add the HeatMapController MonoBehaviour to ‘MyRuntimeHeatMap’
/// 5. Look at the Inspector for ‘MyRuntimeHeatMap’. Under Heat Map Controller, find the Data Path field.
/// 6. Type in the name of your JSON file.
/// 7. Hit Play! 


using System;
using UnityEngine;
using UnityAnalytics;
using System.Collections.Generic;


[RequireComponent (typeof (HeatMapMeshRenderer))]
public class HeatMapController : MonoBehaviour
{

	private HeatMapDataParser parser = new HeatMapDataParser();
	private Dictionary<string, HeatPoint[]> data; 

	public string dataPath = "";
	public string[] options;
	public int optionIndex = 0;
	public float pointSize = 10;

	private static Color HighDensityColor = new Color(1f, 0, 0, .1f);
	private static Color MediumDensityColor = new Color(1f, 1f, 0, .1f);
	private static Color LowDensityColor = new Color(0, 1f, 1f, .1f);

	private Color[] colors = new Color[]{LowDensityColor, MediumDensityColor, HighDensityColor};
	private float[] thresholds = new float[]{.1f, .9f};

	float maxDensity = 0;
	float maxTime = 0;

	void Start() {
		// If there's a path, load data
		if (!String.IsNullOrEmpty(dataPath)) {
			LoadData ();
		}
	}

	void LoadData() {
		// Use the parser to load data
		parser.LoadData (dataPath, parseHandler, true);
	}

	/// <summary>
	/// Once loaded, returns all the important info.
	/// </summary>
	/// <param name="heatData">A dictionary of all the heat data.</param>
	/// <param name="maxDensity">The maximum data density.</param>
	/// <param name="maxTime">The maximum time from the data.</param>
	/// <param name="options">The list of possible options (usually event names).</param>
	void parseHandler (Dictionary<string, HeatPoint[]> heatData, float maxDensity, float maxTime, string[] options) {
		data = heatData;
		this.options = options;
		this.maxDensity = maxDensity;
		this.maxTime = maxTime;
		Render ();
	}

	/// <summary>
	/// Renders the heatmap
	/// </summary>
	void Render() {
		var r = gameObject.GetComponent<IHeatMapRenderer> ();
		r.allowRender = true;
		r.pointSize = pointSize;
		r.UpdateColors (colors);
		r.UpdateThresholds (thresholds);
		r.UpdateTimeLimits (0, maxTime);
		r.UpdateRenderStyle (RenderShape.TRI);
		r.UpdatePointData (data[options[optionIndex]], maxDensity);
		r.RenderHeatMap ();
	}
}


