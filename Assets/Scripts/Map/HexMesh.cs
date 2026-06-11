using System.Collections.Generic;
using UnityEngine;

namespace SettlersClone.Map
{
    // Generates procedural hex mesh geometry for terrain rendering
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class HexMesh : MonoBehaviour
    {
        private Mesh         hexMesh;
        private List<Vector3> vertices  = new();
        private List<int>     triangles = new();
        private List<Color>   colors    = new();
        private MeshCollider  col;

        private void Awake()
        {
            GetComponent<MeshFilter>().mesh = hexMesh = new Mesh { name = "HexMesh" };
            col = GetComponent<MeshCollider>();
        }

        public void Triangulate(HexCell[] cells)
        {
            hexMesh.Clear();
            vertices.Clear();
            triangles.Clear();
            colors.Clear();

            foreach (var cell in cells)
                TriangulateCell(cell);

            hexMesh.vertices  = vertices.ToArray();
            hexMesh.triangles = triangles.ToArray();
            hexMesh.colors    = colors.ToArray();
            hexMesh.RecalculateNormals();
            col.sharedMesh = hexMesh;
        }

        private void TriangulateCell(HexCell cell)
        {
            Vector3 center = cell.transform.localPosition;
            Color   colour = TerrainColour(cell.Terrain);
            float   elevY  = cell.Elevation * 1.5f;
            center.y = elevY;

            for (int d = 0; d < 6; d++)
            {
                AddTriangle(
                    center,
                    center + HexMetrics.Corners[d],
                    center + HexMetrics.Corners[(d + 1) % 6]
                );
                AddTriangleColor(colour, colour, colour);
            }
        }

        private static Color TerrainColour(TerrainType t) => t switch
        {
            TerrainType.Grassland => new Color(0.33f, 0.60f, 0.22f),
            TerrainType.Forest    => new Color(0.13f, 0.38f, 0.14f),
            TerrainType.Mountain  => new Color(0.55f, 0.55f, 0.55f),
            TerrainType.Water     => new Color(0.20f, 0.47f, 0.80f),
            TerrainType.Desert    => new Color(0.88f, 0.78f, 0.48f),
            TerrainType.Snow      => new Color(0.93f, 0.96f, 0.98f),
            _                     => Color.white
        };

        private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vi = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            triangles.Add(vi);
            triangles.Add(vi + 1);
            triangles.Add(vi + 2);
        }

        private void AddTriangleColor(Color c1, Color c2, Color c3)
        {
            colors.Add(c1);
            colors.Add(c2);
            colors.Add(c3);
        }
    }
}
