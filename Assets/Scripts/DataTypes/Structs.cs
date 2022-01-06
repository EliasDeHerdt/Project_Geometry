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

    [System.Serializable]
    public struct DetectionInfo
    {
        public int SuccesfullChecks;

        private bool _topHit;
        public bool TopHit 
        { 
            get { return _topHit; } 
            set { if (_topHit != value) SuccesfullChecks += value ? 1 : -1; _topHit = value; } 
        }

        private bool _leftHit;
        public bool LeftHit 
        { 
            get { return _leftHit; }
            set { if (_leftHit != value) SuccesfullChecks += value ? 1 : -1; _leftHit = value; }
        }

        private bool _backHit;
        public bool BackHit 
        { 
            get { return _backHit; }
            set { if (_backHit != value) SuccesfullChecks += value ? 1 : -1; _backHit = value; }
        }

        private bool _rightHit;
        public bool RightHit 
        {
            get { return _rightHit; }
            set { if (_rightHit != value) SuccesfullChecks += value ? 1 : -1; _rightHit = value; }
        }

        private bool _frontHit;
        public bool FrontHit 
        { 
            get { return _frontHit; }
            set { if (_frontHit != value) SuccesfullChecks += value ? 1 : -1; _frontHit = value; }
        }

        private bool _bottomHit;
        public bool BottomHit 
        { 
            get { return _bottomHit; }
            set { if (_bottomHit != value) SuccesfullChecks += value ? 1 : -1; _bottomHit = value; }
        }
    }
}
