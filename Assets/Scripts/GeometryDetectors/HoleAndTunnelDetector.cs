using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryDetection
{
    // Utilizes the already detected geometry and only tries to find holes or tunnels
    public class HoleAndTunnelDetector : MonoBehaviour
    {
        #region Variables
        [Header("Detection Parameters")]
        [SerializeField] private float _maxDiameter = 10;
        public float MaxDiameter 
        { 
            get { return _maxDiameter; } 
            set { _maxDiameter = value; }
        }

        [SerializeField] private int _minimumSuccesfullChecks = 4;
        public int MinimumSuccesfullChecks
        {
            get { return _minimumSuccesfullChecks; }
            set { _minimumSuccesfullChecks = value; }
        }

        [Header("Visualization Parameters")]
        [SerializeField] private bool _visualizeTunnels = true;
        public bool VisualizeTunnels
        {
            get { return _visualizeTunnels; }
            set { _visualizeTunnels = value;}
        }

        [SerializeField] private Material _tunnelMaterial = null;
        public Material TunnelMaterial
        {
            get { return _tunnelMaterial; }
        }

        private GeometryDetector _geometryDetector;
        public GeometryDetector GeometryDetector 
        { 
            get { return _geometryDetector; } 
        }

        private List<DetectedGeometry> _holeNodes = new List<DetectedGeometry>();
        public List<DetectedGeometry> HolesNodes
        {
            get { return _holeNodes; }
            private set { _holeNodes = value; }
        }

        private List<DetectedGeometry> _tunnelNodes = new List<DetectedGeometry>();
        public List<DetectedGeometry> TunnelNodes
        {
            get { return _tunnelNodes; }
            private set { _tunnelNodes = value; }
        }
        #endregion

        private void Start()
        {
            _geometryDetector = GetComponent<GeometryDetector>();
            if (!_geometryDetector)
                Debug.LogError("HoleDetector: No GeometryDetector has been found on this object!");
        }

        public void DetectHoles()
        {
            // Create new wrapper class that stores Nodes and extra information unique to the hole detector
            List<GeometryNode> geometry = GeometryDetector.EmptyNodes;
            Debug.Log("Pulled in data.");

            // Start detecting the holes.
            foreach(GeometryNode node in geometry)
            {
                DetectionInfo TerrainChecks = new DetectionInfo();
                List<GeometryNode> usedNodes = new List<GeometryNode>();

                // Check Top for terrain.
                TerrainChecks.UpHit = CheckDirectionForTerrain(NeighborDirection.Up, node, usedNodes);

                // Check Bottom for terrain.
                TerrainChecks.DownHit = CheckDirectionForTerrain(NeighborDirection.Down, node, usedNodes);

                // Check Left for terrain.
                TerrainChecks.LeftHit = CheckDirectionForTerrain(NeighborDirection.Left, node, usedNodes);

                // Check Right for terrain.
                TerrainChecks.RightHit = CheckDirectionForTerrain(NeighborDirection.Right, node, usedNodes);

                // Check Front for terrain.
                TerrainChecks.FrontHit = CheckDirectionForTerrain(NeighborDirection.Front, node, usedNodes);

                // Check Back for terrain.
                TerrainChecks.BackHit = CheckDirectionForTerrain(NeighborDirection.Back, node, usedNodes);

                DetectedGeometry detectedGeometry = new DetectedGeometry(usedNodes);
                if (CheckGeometry(TerrainChecks, ref detectedGeometry))
                {
                    foreach(GeometryType GeoType in detectedGeometry.MarkedGeometry)
                    {
                        switch (GeoType)
                        {
                            case GeometryType.Hole:
                                HolesNodes.Add(detectedGeometry);
                                break;
                            case GeometryType.Tunnel:
                                TunnelNodes.Add(detectedGeometry);
                                break;
                            default: break;
                        }
                    }

                    if (VisualizeTunnels)
                    {
                        foreach (GeometryNode usedNode in usedNodes)
                        {
                            usedNode.NodeRenderer.sharedMaterial = TunnelMaterial;
                        }
                    }
                }
            }

            Debug.Log("Finished Tunnel Detection.");
        }

        #region DirectionalChecks
        private bool CheckDirectionForTerrain(NeighborDirection direction, GeometryNode node, List<GeometryNode> usedNodes)
        {
            List<NeighborInfo> neighbors = node.GetNeighbors(direction);

            foreach (NeighborInfo neighbor in neighbors)
            {
                if (neighbor.Neighbor.ContainsGeometry
                    || node.MarkedAs.Contains(GeometryType.Hole)
                    || node.MarkedAs.Contains(GeometryType.Tunnel)
                    || CheckDirectionForTerrain(direction, neighbor.Neighbor, usedNodes))
                {
                    usedNodes.Add(node);
                    return true;
                }
            }

            return false;
        }
        #endregion
        #region GeometryChecks
        private bool CheckGeometry(DetectionInfo detectedTerrain, ref DetectedGeometry detectedGeometry)
        {
            if (detectedTerrain.SuccesfullChecks < MinimumSuccesfullChecks)
                return false;

            bool succesfull = false;

            DetectExits(ref detectedGeometry);
            succesfull = HoleCheck(detectedTerrain, ref detectedGeometry) || succesfull;
            succesfull = TunnelCheck(detectedTerrain, ref detectedGeometry) || succesfull;

            foreach (GeometryNode node in detectedGeometry.Nodes)
            {
                foreach(GeometryType GeoType in detectedGeometry.MarkedGeometry)
                {
                    node.MarkedAs.Add(GeoType);
                }
            }

            return succesfull;
        }

        private void DetectExits(ref DetectedGeometry detectedGeometry)
        {

        }

        private bool TunnelCheck(DetectionInfo detectedTerrain, ref DetectedGeometry detectedGeometry)
        {
            int succesfullHitCount = 0;
            succesfullHitCount += (detectedTerrain.UpHit && detectedTerrain.DownHit) ? 1 : 0;
            succesfullHitCount += (detectedTerrain.RightHit && detectedTerrain.LeftHit) ? 1 : 0;
            succesfullHitCount += (detectedTerrain.FrontHit && detectedTerrain.BackHit) ? 1 : 0;

            if (succesfullHitCount == 2)
            {
                detectedGeometry.MarkedGeometry.Add(GeometryType.Tunnel);
                return true;
            }

            return false;
        }


        private bool HoleCheck(DetectionInfo detectedTerrain, ref DetectedGeometry detectedGeometry)
        {
            if (detectedTerrain.SuccesfullChecks > 4)
            {
                detectedGeometry.MarkedGeometry.Add(GeometryType.Hole);
                return true;
            }

            return false;
        }
        #endregion
    }
}