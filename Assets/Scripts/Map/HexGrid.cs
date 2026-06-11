using System.Collections.Generic;
using UnityEngine;
using SettlersClone.Map;

namespace SettlersClone.Map
{
    public class HexGrid : MonoBehaviour
    {
        [Header("Dimensions")]
        [SerializeField] private int width  = 40;
        [SerializeField] private int height = 30;
        [SerializeField] private int seed   = 0;

        [Header("Prefabs")]
        [SerializeField] private HexCell cellPrefab;
        [SerializeField] private HexMesh hexMeshPrefab;

        private HexCell[] cells;
        private readonly Dictionary<HexCoordinates, HexCell> cellLookup = new();

        public int Width  => width;
        public int Height => height;

        private void Awake()
        {
            CreateGrid();
        }

        private void CreateGrid()
        {
            cells = new HexCell[width * height];
            HexMesh mesh = Instantiate(hexMeshPrefab, transform);

            for (int row = 0, i = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++, i++)
                {
                    CreateCell(col, row, i);
                }
            }

            TerrainGenerator.Generate(cells, width, height, seed);

            // Link neighbours after all cells are created
            for (int row = 0, i = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++, i++)
                {
                    WireNeighbours(cells[i], col, row);
                }
            }

            mesh.Triangulate(cells);

            foreach (var c in cells) cellLookup[c.Coordinates] = c;
        }

        private void CreateCell(int col, int row, int idx)
        {
            HexCoordinates coords = HexCoordinates.FromOffsetCoordinates(col, row);
            Vector3 pos = HexMetrics.Center(coords);
            HexCell cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
            cell.name = $"Cell ({col},{row})";
            cells[idx] = cell;
        }

        private void WireNeighbours(HexCell cell, int col, int row)
        {
            if (col > 0)
                cell.SetNeighbour(3, cells[row * width + col - 1]);
            if (row > 0)
            {
                if ((row & 1) == 0)
                {
                    cell.SetNeighbour(4, cells[(row - 1) * width + col]);
                    if (col > 0) cell.SetNeighbour(5, cells[(row - 1) * width + col - 1]);
                }
                else
                {
                    cell.SetNeighbour(5, cells[(row - 1) * width + col]);
                    if (col < width - 1) cell.SetNeighbour(4, cells[(row - 1) * width + col + 1]);
                }
            }
        }

        public HexCell GetCell(HexCoordinates coords) =>
            cellLookup.TryGetValue(coords, out var c) ? c : null;

        public HexCell GetCellAt(Vector3 worldPos) =>
            GetCell(HexCoordinates.FromWorldPosition(worldPos));

        public HexCell[] GetCellsInRadius(HexCoordinates center, int radius)
        {
            var result = new List<HexCell>();
            for (int dx = -radius; dx <= radius; dx++)
            for (int dz = Mathf.Max(-radius, -dx - radius); dz <= Mathf.Min(radius, -dx + radius); dz++)
            {
                var c = GetCell(new HexCoordinates(center.X + dx, center.Z + dz));
                if (c != null) result.Add(c);
            }
            return result.ToArray();
        }

        public HexCell[] AllCells => cells;
    }
}
