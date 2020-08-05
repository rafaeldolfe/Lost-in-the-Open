using UnityEngine;
using System.Collections.Generic;
using System.Text;


namespace Utils
{
    public class PlainMesh
    {
        public List<Vector3> vertices;
        public List<Vector2> uv;
        public List<int> triangles;

        public PlainMesh(List<Vector3> vertices, List<Vector2> uv, List<int> triangles)
        {
            this.vertices = vertices;
            this.uv = uv;
            this.triangles = triangles;
        }

        /// <summary>
        /// Converts this PlainMesh class into its Unity Mesh equivalent
        /// </summary>
        /// <returns></returns>
        public Mesh GetUnityClass()
        {
            return new Mesh
            {
                vertices = vertices.ToArray(),
                uv = uv.ToArray(),
                triangles = triangles.ToArray()
            };
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("vertices: ");
            sb.Append(vertices);
            sb.Append("uv: ");
            sb.Append(uv);
            sb.Append("triangles: ");
            sb.Append(triangles);

            return sb.ToString();
        }
    }
}