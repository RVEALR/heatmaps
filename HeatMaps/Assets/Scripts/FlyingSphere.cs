using System;
using UnityEngine;
using System.Collections;
using UnityAnalytics;


public class FlyingSphere : MonoBehaviour
{
	public float xOff = 0f;
	public float yOff = 0f;
	public float zOff = 0f;

	float maxVec = 5f;
	float randSize = 1f;
	float limit = 100f;

	float changeFreq = 2f;
	string currentResponse = "Not sent yet";

	void Start() {

		Debug.Log (Application.persistentDataPath);

		ChangeCourse ();
		StartCoroutine (ChangeAgain());
	}

	void FixedUpdate() {
		Vector3 pos = transform.position;
		pos.x += xOff;
		pos.y += yOff;
		pos.z += zOff;
		transform.position = pos;

		Camera.main.transform.LookAt (transform);
	}

	void OnGUI() {
		GUI.TextArea (new Rect (10f, 10f, 250f, 20f), "Last Analytics result: " + currentResponse);
	}

	void ChangeCourse() {
		Vector3 pos = transform.position;

		//Reverse!
		if (xOff > maxVec || xOff < maxVec) {
			xOff *= -.5f;
		}
		if (yOff > maxVec || yOff < maxVec) {
			yOff *= -.5f;
		}
		if (zOff > maxVec || zOff < maxVec) {
			zOff *= -.5f;
		}

		if (Mathf.Abs (pos.x) > limit || Mathf.Abs (pos.y) > limit || Mathf.Abs (pos.z) > limit) {
			Vector3 v = Vector3.Normalize (pos) * -1f;

			xOff = v.x;
			yOff = v.y;
			zOff = v.z;
		} else {
			xOff += UnityEngine.Random.Range (-randSize, randSize);
			yOff += UnityEngine.Random.Range (-randSize, randSize);
			zOff += UnityEngine.Random.Range (-randSize, randSize);
		}
	}

	IEnumerator ChangeAgain() {
		yield return new WaitForSeconds(changeFreq);
		currentResponse  = HeatMapEvent.Send ("ChangeCourse", transform.position, Time.fixedTime).ToString();
		ChangeCourse ();
		StartCoroutine (ChangeAgain());
	}
}