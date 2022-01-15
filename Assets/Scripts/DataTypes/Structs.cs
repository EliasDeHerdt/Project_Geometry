using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
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

        private bool _upHit;
        public bool UpHit 
        { 
            get { return _upHit; } 
            set { if (_upHit != value) SuccesfullChecks += value ? 1 : -1; _upHit = value; } 
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

        private bool _downHit;
        public bool DownHit 
        { 
            get { return _downHit; }
            set { if (_downHit != value) SuccesfullChecks += value ? 1 : -1; _downHit = value; }
        }
    }

    [System.Serializable]
    public struct DetectedGeometry
    {
        public DetectedGeometry(List<GeometryNode> nodes)
        {
            Nodes = nodes;
            Exits = new List<Exit>();
            MarkedGeometry = new HashSet<GeometryType>();
        }

        public List<GeometryNode> Nodes;
        public List<Exit> Exits;
        public HashSet<GeometryType> MarkedGeometry;
    }

    public struct Exit
    {
        public Exit(List<GeometryNode> nodes, DetectedGeometry parent)
        {
            Nodes = nodes;
            ParentGeometry = parent;
        }

        public List<GeometryNode> Nodes;
        DetectedGeometry ParentGeometry;
    }
}
