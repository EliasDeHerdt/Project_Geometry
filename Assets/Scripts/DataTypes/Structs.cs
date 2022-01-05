using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryDetection
{
    [System.Serializable]
    public struct NeighborInfo
    {
        public NeighborInfo(GeometryNode neighbor, NeighborDirection direction)
        {
            Neighbor = neighbor;
            Direction = direction;
        }

        public static NeighborInfo FindNeighbor(GeometryNode neighbor, List<NeighborInfo> list)
        {
            foreach (NeighborInfo info in list)
            {
                if (info.Neighbor == neighbor)
                    return info;
            }

            return new NeighborInfo();
        }

        public GeometryNode Neighbor;
        public NeighborDirection Direction;
    }
}
