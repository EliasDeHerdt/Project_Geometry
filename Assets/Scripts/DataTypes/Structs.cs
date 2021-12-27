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

        public GeometryNode Neighbor;
        public NeighborDirection Direction;
    }
}
