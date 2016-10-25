using System;
using UnityEngine;

namespace UnityAnalyticsHeatmap
{
    public class RenderShapeMeshUtils
    {
        public static int GetTrianglesForShape(RenderShape renderShape, RenderProjection projection)
        {
            // Tris is the number of triangles in each shape
            int tris = 0;
            switch (renderShape)
            {
                case RenderShape.Cube:
                    tris = 12;
                    break;
                case RenderShape.Arrow:
                    tris = (projection == RenderProjection.FirstPerson) ? 2 : 3;
                    break;
                case RenderShape.Square:
                    tris = 2;
                    break;
                case RenderShape.Triangle:
                    tris = 1;
                    break;
                case RenderShape.PointToPoint:
                    tris = 4;
                    break;
            }
            return tris;
        }

        public static int GetVecticesForShape(RenderShape renderShape, RenderProjection projection)
        {
            // Verts is the number of UNIQUE vertices in each shape
            int verts = 0;
            switch (renderShape)
            {
                case RenderShape.Cube:
                    verts = 8;
                    break;
                case RenderShape.Arrow:
                    verts = (projection == RenderProjection.FirstPerson) ? 4 : 7;
                    break;
                case RenderShape.Square:
                    verts = 4;
                    break;
                case RenderShape.Triangle:
                    verts = 3;
                    break;
                case RenderShape.PointToPoint:
                    verts = 8;
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

        public static Vector3[] AddArrowVectorsToMesh(float m_ParticleSize, Vector3 position, Vector3 rotation, RenderProjection projection)
        {
            float stemLength = 5f * m_ParticleSize;

            if (projection == RenderProjection.FirstPerson)
            {
                Quaternion q1 = Quaternion.Euler(rotation);
                Matrix4x4 m1 = Matrix4x4.TRS(position, q1, Vector3.one);
                Vector3 projectedPosition = m1.MultiplyPoint3x4(new Vector3(0f, 0f, m_ParticleSize * (stemLength + 1f)));
                return AddDiamondVectorsToMesh(m_ParticleSize, RenderDirection.Billboard, projectedPosition, position);
            }

            float thirdP = m_ParticleSize / 3f;


            var p0 = new Vector3(-thirdP, 0f, 0f);
            var p1 = new Vector3(-thirdP, 0f, m_ParticleSize * stemLength);
            var p2 = new Vector3(-m_ParticleSize, 0f, m_ParticleSize * stemLength);
            var p3 = new Vector3(0f, 0f, m_ParticleSize * (stemLength + 1f));
            var p4 = new Vector3(m_ParticleSize, 0f, m_ParticleSize * stemLength);
            var p5 = new Vector3(thirdP, 0f, m_ParticleSize * stemLength);
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
        public static int[] AddArrowTrisToMesh(int offset, RenderProjection projection)
        {
            if (projection == RenderProjection.FirstPerson)
            {
                return AddSquareTrisToMesh(offset);
            }

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
                    p1 = new Vector3(-halfP,  halfP);
                    p2 = new Vector3(halfP,   halfP);
                    p3 = new Vector3(halfP,  -halfP);
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


        /// <summary>
        /// Use in place of AddSquareVectorsToMesh when you want to show squares as diamonds
        /// </summary>
        public static Vector3[] AddDiamondVectorsToMesh(float m_ParticleSize, RenderDirection m_RenderDirection, Vector3 position, Vector3 source)
        {
            float halfP = m_ParticleSize / 2;
            float x = position.x;
            float y = position.y;
            float z = position.z;

            Vector3 p0, p1, p2, p3;

            switch (m_RenderDirection)
            {
                case RenderDirection.Billboard:
                    Quaternion q = Quaternion.LookRotation(source - position);
                    Matrix4x4 m = Matrix4x4.TRS(position, q, Vector3.one);
                    p0 = new Vector3( 0,    -halfP);
                    p1 = new Vector3(-halfP, 0);
                    p2 = new Vector3( 0,     halfP);
                    p3 = new Vector3( halfP, 0);
                    var v = new Vector3[] { p0, p1, p2, p3 };
                    for (int a = 0; a < v.Length; a++)
                    {
                        v[a] = m.MultiplyPoint3x4(v[a]);
                    }
                    return v;

                case RenderDirection.YZ:
                    p0 = new Vector3(x, 0, z - halfP);
                    p1 = new Vector3(x, y + halfP, 0);
                    p2 = new Vector3(x, 0, z + halfP);
                    p3 = new Vector3(x, y - halfP, 0);
                    break;

                case RenderDirection.XZ:
                    p0 = new Vector3(0, y, z - halfP);
                    p1 = new Vector3(x + halfP, y, 0);
                    p2 = new Vector3(0, y, z + halfP);
                    p3 = new Vector3(x - halfP, y, 0);
                    break;

                default:
                    p0 = new Vector3(0, y - halfP, z);
                    p1 = new Vector3(x + halfP, 0, z);
                    p2 = new Vector3(0, y + halfP, z);
                    p3 = new Vector3(x - halfP, 0, z);
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
                    p0 = new Vector3(halfP,  - halfP);
                    p1 = new Vector3(0f,  halfP);
                    p2 = new Vector3(-halfP, -halfP);
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

        public static Vector3[] AddP2PVectorsToMesh(float m_ParticleSize, Vector3 fromVector, Vector3 toVector, bool collapsed)
        {
            Vector3 p0, p1, p2, p3, p4, p5, p6, p7;
            Vector3[] v = null;
            float halfP = m_ParticleSize * .5f;

            if (collapsed)
            {
                // Left half
                p0 = new Vector3(halfP, halfP, 0f);
                p1 = new Vector3(-halfP, 0f, -halfP);
                p2 = new Vector3(-halfP, 0f, halfP);
                p3 = new Vector3(halfP, -halfP, 0f);

                // Right half
                p4 = new Vector3(halfP, -halfP, 0f);
                p5 = new Vector3(halfP, halfP, 0f);
                p6 = new Vector3(-halfP, 0f, halfP);
                p7 = new Vector3(-halfP, 0f, -halfP);
            }
            else
            {
                float distance = Vector3.Distance(fromVector, toVector);
                float arrowBaseZ = distance * .75f;

                // base
                p0 = new Vector3(0f, 0f, arrowBaseZ * .05f);
                p1 = new Vector3(-halfP, 0f, arrowBaseZ * .5f);
                p2 = new Vector3(halfP, 0f, arrowBaseZ * .5f);

                // top of stem
                p3 = new Vector3(-halfP, 0f, arrowBaseZ);
                p4 = new Vector3(halfP, 0f, arrowBaseZ);

                // arrowhead
                p5 = new Vector3(-m_ParticleSize, 0f, arrowBaseZ);
                p6 = new Vector3(0f, 0f, distance);
                p7 = new Vector3(m_ParticleSize, 0f, arrowBaseZ);
            }

            v = new Vector3[] { p0, p1, p2, p3, p4, p5, p6, p7 };
            Vector3 relativePos = toVector - fromVector;
            Quaternion q = (relativePos == Vector3.zero) ? Quaternion.identity : Quaternion.LookRotation(relativePos);
            for (int a = 0; a < v.Length; a++)
            {
                Matrix4x4 m = Matrix4x4.TRS(fromVector, q, Vector3.one);
                v[a] = m.MultiplyPoint3x4(v[a]);
            }
            return v;
        }

        //Generate a procedural P2P
        public static int[] AddP2PTrisToMesh(int offset, bool collapsed)
        {
            int[] tris = null;
            if (collapsed)
            {
                tris = new int[]
                    {
                        // Left half
                        offset, offset + 1, offset + 2,
                        offset + 2, offset + 1, offset + 3,

                        // Right half
                        offset + 4, offset + 5, offset + 6,
                        offset + 4, offset + 7, offset + 5
                    };
            }
            else
            {
                tris = new int[]
                    {
                        // base
                        offset, offset + 1, offset + 2,

                        // stem
                        offset + 1, offset + 3, offset + 4,
                        offset + 2, offset + 1, offset + 4,

                        // arrowhead
                        offset + 5, offset + 6, offset + 7
                    };
            }
            return tris;
        }
    }
}

