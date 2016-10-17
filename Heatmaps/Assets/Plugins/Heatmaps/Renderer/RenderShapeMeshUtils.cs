using System;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class RenderShapeMeshUtils
    {
        public static int GetTrianglesForShape(RenderShape renderShape)
        {
            // Verts is the number of UNIQUE vertices in each shape
            int verts = 0;
            switch (renderShape)
            {
                case RenderShape.Cube:
                    verts = 12;
                    break;
                case RenderShape.Arrow:
                    verts = 3;
                    break;
                case RenderShape.Square:
                    verts = 2;
                    break;
                case RenderShape.Triangle:
                    verts = 1;
                    break;
                case RenderShape.PointToPoint:
                    verts = 7;
                    break;
            }
            return verts;
        }

        public static int GetVecticesForShape(RenderShape renderShape)
        {
            // Verts is the number of UNIQUE vertices in each shape
            int verts = 0;
            switch (renderShape)
            {
                case RenderShape.Cube:
                    verts = 8;
                    break;
                case RenderShape.Arrow:
                    verts = 7;
                    break;
                case RenderShape.Square:
                    verts = 4;
                    break;
                case RenderShape.Triangle:
                    verts = 3;
                    break;
                case RenderShape.PointToPoint:
                    verts = 15;
                    break;
            }
            return verts;
        }

        public static Vector3[] AddCubeVectorsToMesh(float m_ParticleSize, float x, float y, float z)
        {
            float halfP = m_ParticleSize / 2;

            var p0 = new Vector3(x - halfP, y - halfP, z - halfP);
            var p1 = new Vector3(x + halfP, y - halfP, z - halfP);
            var p2 = new Vector3(x + halfP, y + halfP, z - halfP);
            var p3 = new Vector3(x - halfP, y + halfP, z - halfP);

            var p4 = new Vector3(x - halfP, y - halfP, z + halfP);
            var p5 = new Vector3(x + halfP, y - halfP, z + halfP);
            var p6 = new Vector3(x + halfP, y + halfP, z + halfP);
            var p7 = new Vector3(x - halfP, y + halfP, z + halfP);

            return new Vector3[] { p0, p1, p2, p3, p4, p5, p6, p7 };
        }

        // Generate a cube mesh procedurally
        public static int[] AddCubeTrisToMesh(int offset)
        {
            var tris = new int[]
                {
                    0, 1, 2,    // bottom
                    0, 2, 3,
                    4, 6, 5,    // top
                    4, 7, 6,
                    1, 6, 2,    // right
                    1, 5, 6,

                    3, 4, 0,    // left
                    3, 7, 4,
                    2, 7, 3,    // back
                    2, 6, 7,
                    0, 4, 5,    // front
                    0, 5, 1
                };
            for (int a = 0; a < tris.Length; a++)
            {
                tris[a] += offset;
            }
            return tris;
        }

        public static Vector3[] AddArrowVectorsToMesh(float m_ParticleSize, Vector3 position, Vector3 rotation)
        {
            float thirdP = m_ParticleSize / 3f;

            var p0 = new Vector3(-thirdP, 0f, 0f);
            var p1 = new Vector3(-thirdP, 0f, -m_ParticleSize * 2f);
            var p2 = new Vector3(-m_ParticleSize, 0f, -m_ParticleSize * 2f);
            var p3 = new Vector3(0f, 0f, -m_ParticleSize * 3f);
            var p4 = new Vector3(m_ParticleSize, 0f, -m_ParticleSize * 2f);
            var p5 = new Vector3(thirdP, 0f, -m_ParticleSize * 2f);
            var p6 = new Vector3(thirdP, 0f, 0f);

            var v = new Vector3[] { p0, p1, p2, p3, p4, p5, p6 };

            Quaternion q = Quaternion.Euler(rotation);
            Matrix4x4 m = Matrix4x4.TRS(position, q, Vector3.one);
            for (int a = 0; a < v.Length; a++)
            {
                v[a] = m.MultiplyPoint3x4(v[a]);
            }
            return v;
        }

        //Generate an arrow mesh procedurally
        public static int[] AddArrowTrisToMesh(int offset)
        {
            var tris = new int[]
                {
                    0, 1, 5,    // left
                    6, 0, 5,    //right
                    3, 4, 2     // head
                };
            for (int a = 0; a < tris.Length; a++)
            {
                tris[a] += offset;
            }
            return tris;
        }

        public static Vector3[] AddSquareVectorsToMesh(float m_ParticleSize, RenderDirection m_RenderDirection, Vector3 position, Vector3 source)
        {
            float halfP = m_ParticleSize / 2;
            float x = position.x;
            float y = position.y;
            float z = position.z;

            Vector3 p0, p1, p2, p3;

            switch (m_RenderDirection)
            {
                case RenderDirection.Billboard:
                    Quaternion q = Quaternion.LookRotation( source - position );
                    Matrix4x4 m = Matrix4x4.TRS(position, q, Vector3.one);
                    p0 = new Vector3(-halfP, -halfP);
                    p1 = new Vector3(halfP,  -halfP);
                    p2 = new Vector3(halfP,   halfP);
                    p3 = new Vector3(-halfP, halfP);
                    var v = new Vector3[] { p0, p1, p2, p3 };
                    for (int a = 0; a < v.Length; a++)
                    {
                        v[a] = m.MultiplyPoint3x4(v[a]);
                    }
                    return v;

                case RenderDirection.YZ:
                    p0 = new Vector3(x, y - halfP, z - halfP);
                    p1 = new Vector3(x, y + halfP, z - halfP);
                    p2 = new Vector3(x, y + halfP, z + halfP);
                    p3 = new Vector3(x, y - halfP, z + halfP);
                    break;

                case RenderDirection.XZ:
                    p0 = new Vector3(x - halfP, y, z - halfP);
                    p1 = new Vector3(x + halfP, y, z - halfP);
                    p2 = new Vector3(x + halfP, y, z + halfP);
                    p3 = new Vector3(x - halfP, y, z + halfP);
                    break;

                default:
                    p0 = new Vector3(x - halfP, y - halfP, z);
                    p1 = new Vector3(x + halfP, y - halfP, z);
                    p2 = new Vector3(x + halfP, y + halfP, z);
                    p3 = new Vector3(x - halfP, y + halfP, z);
                    break;
            }

            return new Vector3[] { p0, p1, p2, p3 };
        }

        //Generate a procedural square
        public static int[] AddSquareTrisToMesh(int offset)
        {
            var tris = new int[]
                {
                    offset, offset + 2, offset + 1, // top
                    offset, offset + 3, offset + 2
                };
            return tris;
        }

        public static Vector3[] AddTriVectorsToMesh(float m_ParticleSize, RenderDirection m_RenderDirection, Vector3 position, Vector3 source)
        {
            float halfP = m_ParticleSize / 2;
            float x = position.x;
            float y = position.y;
            float z = position.z;

            Vector3 p0, p1, p2;

            switch (m_RenderDirection)
            {
                case RenderDirection.Billboard:
                    Quaternion q = Quaternion.LookRotation( source - position );
                    Matrix4x4 m = Matrix4x4.TRS(position, q, Vector3.one);
                    p0 = new Vector3(-halfP, -halfP);
                    p1 = new Vector3(0f,  halfP);
                    p2 = new Vector3(halfP,  - halfP);
                    var v = new Vector3[] { p0, p1, p2 };
                    for (int a = 0; a < v.Length; a++)
                    {
                        v[a] = m.MultiplyPoint3x4(v[a]);
                    }
                    return v;

                case RenderDirection.YZ:
                    p0 = new Vector3(x, y - halfP, z - halfP);
                    p1 = new Vector3(x, y, z + halfP);
                    p2 = new Vector3(x, y + halfP, z - halfP);
                    break;

                case RenderDirection.XZ:
                    p0 = new Vector3(x - halfP, y, z - halfP);
                    p1 = new Vector3(x, y, z + halfP);
                    p2 = new Vector3(x + halfP, y, z - halfP);
                    break;

                default:
                    p0 = new Vector3(x - halfP, y - halfP, z);
                    p1 = new Vector3(x, y + halfP, z);
                    p2 = new Vector3(x + halfP, y - halfP, z);
                    break;
            }
            return new Vector3[] { p0, p1, p2 };
        }

        //Generate a procedural tri
        public static int[] AddTriTrisToMesh(int offset)
        {
            var tris = new int[]
                {
                    offset, offset + 1, offset + 2  // top
                };
            return tris;
        }

        public static Vector3[] AddP2PVectorsToMesh(float m_ParticleSize, Vector3 fromVector, Vector3 toVector)
        {
            Vector3 relativePos = toVector - fromVector;
            Quaternion q = (relativePos == Vector3.zero) ? Quaternion.identity : Quaternion.LookRotation(relativePos);
            float distance = Vector3.Distance(fromVector, toVector);

            float arrowBaseZ = distance - (m_ParticleSize * 1.5f);
            float halfP = m_ParticleSize * .5f;

            // base
            var p0 = new Vector3(-m_ParticleSize, 0f, -halfP);
            var p1 = new Vector3(-m_ParticleSize, 0f, halfP);
            var p2 = new Vector3(m_ParticleSize, 0f, halfP);
            var p3 = new Vector3(m_ParticleSize, 0f, -halfP);
            // stem
            var p4 = new Vector3(-.5f * m_ParticleSize, 0f, -halfP);
            var p5 = new Vector3(-.5f * m_ParticleSize, 0f, arrowBaseZ);
            var p6 = new Vector3(.5f * m_ParticleSize, 0f, arrowBaseZ);
            var p7 = new Vector3(.5f * m_ParticleSize, 0f, -halfP);
            // arrow
            var p8 = new Vector3(-m_ParticleSize, 0f, arrowBaseZ);
            var p9 = new Vector3(0f, 0f, distance);
            var p10 = new Vector3(m_ParticleSize, 0f, arrowBaseZ);
            // head
            var p11 = new Vector3(-m_ParticleSize, 0f, distance);
            var p12 = new Vector3(-m_ParticleSize, 0f, distance + halfP);
            var p13 = new Vector3(m_ParticleSize, 0f, distance + halfP);
            var p14 = new Vector3(m_ParticleSize, 0f, distance);

            var v = new Vector3[] { p0, p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14 };
            for (int a = 0; a < v.Length; a++)
            {
                Matrix4x4 m = Matrix4x4.TRS(fromVector, q, Vector3.one);
                v[a] = m.MultiplyPoint3x4(v[a]);
            }
            return v;
        }

        //Generate a procedural P2P
        public static int[] AddP2PTrisToMesh(int offset)
        {
            var tris = new int[]
                {
                    // base
                    offset, offset + 1, offset + 2,
                    offset, offset + 2, offset + 3,
                    // stem
                    offset + 4, offset + 5, offset + 6,
                    offset + 4, offset + 6, offset + 7,
                    // arrow
                    offset + 8, offset + 9, offset + 10,
                    // head
                    offset + 11, offset + 12, offset + 13,
                    offset + 11, offset + 13, offset + 14
                };
            return tris;
        }
    }
}

