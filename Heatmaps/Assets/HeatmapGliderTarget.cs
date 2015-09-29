using UnityEngine;
using System.Collections;

public class HeatmapGliderTarget : MonoBehaviour {

	float area = 100f;
	float changeFreq = 3f;

	public HeatmapGlider target;


	// Use this for initialization
	void Start () {
		UpdatePosition ();
		StartCoroutine (DoChangePosition());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void UpdatePosition() {
		
		float xOff = UnityEngine.Random.Range (-area, area);
		float yOff = UnityEngine.Random.Range (-area, area);
		float zOff = UnityEngine.Random.Range (-area, area);

		transform.position = new Vector3 (xOff, yOff, zOff);
	}

	IEnumerator DoChangePosition() {
		yield return new WaitForSeconds(changeFreq);
		UpdatePosition ();
		target.ResetTurn ();
		StartCoroutine (DoChangePosition());
	}
}
