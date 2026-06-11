using System;
using UnityEngine;

namespace SettlersClone.Map
{
    // Cube coordinate system for hex grids (x + y + z == 0 always)
    [Serializable]
    public struct HexCoordinates : IEquatable<HexCoordinates>
    {
        public readonly int X, Y, Z;

        public HexCoordinates(int x, int z)
        {
            X = x;
            Z = z;
            Y = -x - z;
        }

        public static HexCoordinates FromOffsetCoordinates(int col, int row)
        {
            int x = col - (row - (row & 1)) / 2;
            int z = row;
            return new HexCoordinates(x, z);
        }

        public static HexCoordinates FromWorldPosition(Vector3 position)
        {
            float x = position.x / (HexMetrics.InnerRadius * 2f);
            float y = -x;
            float offset = position.z / (HexMetrics.OuterRadius * 3f);
            x -= offset;
            y -= offset;
            int iX = Mathf.RoundToInt(x);
            int iY = Mathf.RoundToInt(y);
            int iZ = Mathf.RoundToInt(-x - y);
            if (iX + iY + iZ != 0)
            {
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x - y - iZ);
                if (dX > dY && dX > dZ) iX = -iY - iZ;
                else if (dZ > dY)       iZ = -iX - iY;
            }
            return new HexCoordinates(iX, iZ);
        }

        public static readonly HexCoordinates[] Directions =
        {
            new(1, 0), new(1, -1), new(0, -1),
            new(-1, 0), new(-1, 1), new(0, 1)
        };

        public HexCoordinates Neighbor(int direction) =>
            new(X + Directions[direction].X, Z + Directions[direction].Z);

        public int DistanceTo(HexCoordinates other) =>
            (Math.Abs(X - other.X) + Math.Abs(Y - other.Y) + Math.Abs(Z - other.Z)) / 2;

        public bool Equals(HexCoordinates other) => X == other.X && Z == other.Z;
        public override bool Equals(object obj) => obj is HexCoordinates h && Equals(h);
        public override int GetHashCode() => HashCode.Combine(X, Z);
        public static bool operator ==(HexCoordinates a, HexCoordinates b) => a.Equals(b);
        public static bool operator !=(HexCoordinates a, HexCoordinates b) => !a.Equals(b);
        public override string ToString() => $"({X}, {Y}, {Z})";
    }

    public static class HexMetrics
    {
        public const float OuterRadius = 5f;
        public const float InnerRadius = OuterRadius * 0.866025404f;

        public static readonly Vector3[] Corners =
        {
            new(0f,         0f, OuterRadius),
            new(InnerRadius, 0f, 0.5f * OuterRadius),
            new(InnerRadius, 0f, -0.5f * OuterRadius),
            new(0f,         0f, -OuterRadius),
            new(-InnerRadius, 0f, -0.5f * OuterRadius),
            new(-InnerRadius, 0f, 0.5f * OuterRadius)
        };

        public static Vector3 Center(HexCoordinates coords)
        {
            float x = (coords.X + coords.Z * 0.5f - coords.Z / 2) * (InnerRadius * 2f);
            float z = coords.Z * (OuterRadius * 1.5f);
            return new Vector3(x, 0f, z);
        }
    }
}
