using UnityEngine;
using SettlersClone.Map;

namespace SettlersClone.Map
{
    // Procedurally assigns terrain types and elevation to hex cells using Perlin noise
    public static class TerrainGenerator
    {
        public static void Generate(HexCell[] cells, int width, int height, int seed = 0)
        {
            float noiseScale    = 0.08f;
            float elevScale     = 0.05f;
            float offsetX       = seed * 100f;
            float offsetZ       = seed * 73.1f;

            for (int i = 0; i < cells.Length; i++)
            {
                int row = i / width;
                int col = i % width;

                float nx = (col + offsetX) * noiseScale;
                float nz = (row + offsetZ) * noiseScale;

                float moisture = Mathf.PerlinNoise(nx,             nz);
                float altitude = Mathf.PerlinNoise(nx + 100, nz + 100);
                float ex       = (col + offsetX) * elevScale;
                float ez       = (row + offsetZ) * elevScale;
                int   elevation = Mathf.RoundToInt(Mathf.PerlinNoise(ex, ez) * 4f);

                TerrainType terrain = DetermineTerrainType(moisture, altitude);
                cells[i].Initialise(HexCoordinates.FromOffsetCoordinates(col, row), terrain, elevation);
            }
        }

        private static TerrainType DetermineTerrainType(float moisture, float altitude)
        {
            if (altitude < 0.25f)   return TerrainType.Water;
            if (altitude > 0.85f)   return altitude > 0.92f ? TerrainType.Snow : TerrainType.Mountain;
            if (moisture < 0.25f)   return TerrainType.Desert;
            if (moisture > 0.65f)   return TerrainType.Forest;
            return TerrainType.Grassland;
        }
    }
}
