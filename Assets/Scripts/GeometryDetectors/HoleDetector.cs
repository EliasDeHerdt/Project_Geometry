using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryDetection
{
    // Utilizes the already detected geometry and only tries to find holes or tunnels
    public class HoleDetector : MonoBehaviour
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
            List<GeometryNode> geometry = GeometryDetector.EmptyNodes;
            Debug.Log("Pulled in data.");

            // Start detecting the holes.
            foreach(GeometryNode node in geometry)
            {
                DetectionInfo TerrainChecks = new DetectionInfo();
                List<GeometryNode> geometryNodes = new List<GeometryNode>();

                // Check Top for terrain.
                TerrainChecks.UpHit = CheckDirectionForTerrain(NeighborDirection.Up, node, geometryNodes);

                // Check Bottom for terrain.
                TerrainChecks.DownHit = CheckDirectionForTerrain(NeighborDirection.Down, node, geometryNodes);

                // Check Left for terrain.
                TerrainChecks.LeftHit = CheckDirectionForTerrain(NeighborDirection.Left, node, geometryNodes);

                // Check Right for terrain.
                TerrainChecks.RightHit = CheckDirectionForTerrain(NeighborDirection.Right, node, geometryNodes);

                // Check Front for terrain.
                TerrainChecks.FrontHit = CheckDirectionForTerrain(NeighborDirection.Front, node, geometryNodes);

                // Check Back for terrain.
                TerrainChecks.BackHit = CheckDirectionForTerrain(NeighborDirection.Back, node, geometryNodes);

                DetectedGeometry detectedGeometry = new DetectedGeometry(geometryNodes);
                CheckGeometry(TerrainChecks, ref detectedGeometry);

                if (detectedGeometry.GeometryType != GeometryType.None)
                {
                    TunnelNodes.Add(detectedGeometry);
                    if (VisualizeTunnels)
                    {
                        node.NodeRenderer.sharedMaterial = TunnelMaterial;
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
        private void CheckGeometry(DetectionInfo detectedTerrain, ref DetectedGeometry detectedGeometry)
        {
            if (detectedTerrain.SuccesfullChecks < MinimumSuccesfullChecks)
                return;

            detectedGeometry.GeometryType = HoleCheck(detectedTerrain);
            detectedGeometry.GeometryType = TunnelCheck(detectedTerrain);
        }

        private GeometryType TunnelCheck(DetectionInfo detectedTerrain)
        {
            return GeometryType.Tunnel;
        }

        private GeometryType HoleCheck(DetectionInfo detectedTerrain)
        {
            return GeometryType.None;
        }
        #endregion
    }
}