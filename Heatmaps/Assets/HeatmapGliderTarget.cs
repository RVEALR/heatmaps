using UnityEngine;
using System.Collections;

public class HeatmapGliderTarget : MonoBehaviour
{

    float m_Area = 100f;
    float m_ChangeFreq = 3f;

    public HeatmapGlider target;


    // Use this for initialization
    void Start()
    {
        UpdatePosition();
        StartCoroutine(DoChangePosition());
    }
	
    // Update is called once per frame
    void Update()
    {
	
    }

    void UpdatePosition()
    {
		
        float xOff = UnityEngine.Random.Range(-m_Area, m_Area);
        float yOff = UnityEngine.Random.Range(-m_Area, m_Area);
        float zOff = UnityEngine.Random.Range(-m_Area, m_Area);

        transform.position = new Vector3(xOff, yOff, zOff);
    }

    IEnumerator DoChangePosition()
    {
        yield return new WaitForSeconds(m_ChangeFreq);
        UpdatePosition();
        target.ResetTurn();
        StartCoroutine(DoChangePosition());
    }
}
