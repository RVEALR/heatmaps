using System;
using UnityEngine;
using System.Collections;
using UnityAnalyticsHeatmap;


public class HeatmapGlider : MonoBehaviour
{
	public Vector3 thrust = Vector3.forward;

	public Transform destination;
	public Vector3 offsetPosition = new Vector3(30f, 5f, 0f);

	float changeFreq = .5f;
	string currentResponse = "Not sent yet";

	float theTime = 0f;

	void Start() {
		StartCoroutine (MapEvent());
	}

	void FixedUpdate() {
		theTime += Time.deltaTime * 1f;
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(destination.position), Math.Min(1f, theTime));
		transform.Translate (-thrust);

		Camera.main.transform.position = transform.position - offsetPosition;
		Camera.main.transform.LookAt (transform);
	}

	void OnGUI() {
		GUI.TextArea (new Rect (10f, 10f, 250f, 20f), "Last Analytics result: " + currentResponse);
		GUI.TextArea (new Rect (10f, 30f, 250f, 20f), "destination: " + destination.position);
		GUI.TextArea (new Rect (10f, 50f, 250f, 20f), "position: " + transform.position);
	}

	public void ResetTurn() {
		theTime = 0f;
	}

	IEnumerator MapEvent() {
		yield return new WaitForSeconds(changeFreq);
		currentResponse  = HeatmapEvent.Send ("FlightCourse", transform, Time.fixedTime).ToString();
		StartCoroutine (MapEvent());
	}
}