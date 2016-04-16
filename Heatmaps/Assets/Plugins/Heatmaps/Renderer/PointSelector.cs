
using System;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(HeatmapSubmap))]
public class PointSelector : Editor
{
    public void OnSceneGUI()
    {
        if (Event.current != null && Event.current.type == EventType.mouseMove)
        {
            RaycastHit hit;
            HeatmapSubmap myTarget = (HeatmapSubmap)target;

            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);


//            if (!Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit))
//                return;


            if (myTarget.GetComponent<MeshCollider>().Raycast(ray, out hit, float.MaxValue))
            {
                Debug.Log(hit.triangleIndex);
            }

            return;



            
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (hit.triangleIndex == -1 || meshCollider == null || meshCollider.sharedMesh == null)
                return;


            Debug.Log(target);
            
            if (hit.collider.gameObject != myTarget.gameObject)
                return;
            
            Mesh mesh = meshCollider.sharedMesh;






            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3 p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
            Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
            Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];
            Transform hitTransform = hit.collider.transform;
            p0 = hitTransform.TransformPoint(p0);
            p1 = hitTransform.TransformPoint(p1);
            p2 = hitTransform.TransformPoint(p2);
            Debug.DrawLine(p0, p1);
            Debug.DrawLine(p1, p2);
            Debug.DrawLine(p2, p0);

           
        }
    }
}

