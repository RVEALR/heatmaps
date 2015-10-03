using System;
using UnityEngine;
using System.Collections;
using UnityAnalyticsHeatmap;


public class FlyingSphere : MonoBehaviour
{
    public float xOffset = 0f;
    public float yOffset = 0f;
    public float zOffset = 0f;

    float m_MaxVec = 5f;
    float m_RandomSize = 1f;
    float m_Limit = 100f;

    float m_ChangeFreq = 2f;
    string m_CurrentResponse = "Not sent yet";

    void Start()
    {
        ChangeCourse();
        StartCoroutine(ChangeAgain());
    }

    void FixedUpdate()
    {
        Vector3 pos = transform.position;
        pos.x += xOffset;
        pos.y += yOffset;
        pos.z += zOffset;
        transform.position = pos;

        Camera.main.transform.LookAt(transform);
    }

    void OnGUI()
    {
        GUI.TextArea(new Rect(10f, 10f, 250f, 20f), "Last Analytics result: " + m_CurrentResponse);
    }

    void ChangeCourse()
    {
        Vector3 pos = transform.position;

        //Reverse!
        if (xOffset > m_MaxVec || xOffset < m_MaxVec)
        {
            xOffset *= -.5f;
        }
        if (yOffset > m_MaxVec || yOffset < m_MaxVec)
        {
            yOffset *= -.5f;
        }
        if (zOffset > m_MaxVec || zOffset < m_MaxVec)
        {
            zOffset *= -.5f;
        }

        if (Mathf.Abs(pos.x) > m_Limit || Mathf.Abs(pos.y) > m_Limit || Mathf.Abs(pos.z) > m_Limit)
        {
            Vector3 v = Vector3.Normalize(pos) * -1f;

            xOffset = v.x;
            yOffset = v.y;
            zOffset = v.z;
        }
        else
        {
            xOffset += UnityEngine.Random.Range(-m_RandomSize, m_RandomSize);
            yOffset += UnityEngine.Random.Range(-m_RandomSize, m_RandomSize);
            zOffset += UnityEngine.Random.Range(-m_RandomSize, m_RandomSize);
        }
    }

    IEnumerator ChangeAgain()
    {
        yield return new WaitForSeconds(m_ChangeFreq);
        m_CurrentResponse = HeatmapEvent.Send("ChangeCourse", transform.position, Time.fixedTime).ToString();
        ChangeCourse();
        StartCoroutine(ChangeAgain());
    }
}
