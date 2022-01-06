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

        private List<GeometryNode> _tunnelNodes = new List<GeometryNode>();
        public List<GeometryNode> TunnelNodes
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
                TerrainChecks.TopHit = CheckTopForTerrain(node);

                // Check Bottom for terrain.
                TerrainChecks.BottomHit = CheckBottomForTerrain(node);

                // Check Left for terrain.
                TerrainChecks.LeftHit = CheckLeftForTerrain(node);

                // Check Right for terrain.
                TerrainChecks.RightHit = CheckRightForTerrain(node);

                // Check Front for terrain.
                TerrainChecks.FrontHit = CheckFrontForTerrain(node);

                // Check Back for terrain.
                TerrainChecks.BackHit = CheckBackForTerrain(node);

                if (TerrainChecks.SuccesfullChecks >= MinimumSuccesfullChecks)
                {
                    TunnelNodes.Add(node);

                    if (VisualizeTunnels)
                    {
                        node.NodeRenderer.sharedMaterial = TunnelMaterial;
                    }
                }
            }

            Debug.Log("Finished Tunnel Detection.");
        }

        #region DirectionalChecks
        private bool CheckTopForTerrain(GeometryNode node)
        {
            List<NeighborInfo> neighbors = node.GetNeighbors(NeighborDirection.Up);

            foreach (NeighborInfo neighbor in neighbors)
            {
                if (neighbor.Neighbor.ContainsGeometry
                    || CheckTopForTerrain(neighbor.Neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckBottomForTerrain(GeometryNode node)
        {
            List<NeighborInfo> neighbors = node.GetNeighbors(NeighborDirection.Down);

            foreach (NeighborInfo neighbor in neighbors)
            {
                if (neighbor.Neighbor.ContainsGeometry
                    || CheckBottomForTerrain(neighbor.Neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckLeftForTerrain(GeometryNode node)
        {
            List<NeighborInfo> neighbors = node.GetNeighbors(NeighborDirection.Left);

            foreach (NeighborInfo neighbor in neighbors)
            {
                if (neighbor.Neighbor.ContainsGeometry
                    || CheckLeftForTerrain(neighbor.Neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckRightForTerrain(GeometryNode node)
        {
            List<NeighborInfo> neighbors = node.GetNeighbors(NeighborDirection.Right);

            foreach (NeighborInfo neighbor in neighbors)
            {
                if (neighbor.Neighbor.ContainsGeometry
                    || CheckRightForTerrain(neighbor.Neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckFrontForTerrain(GeometryNode node)
        {
            List<NeighborInfo> neighbors = node.GetNeighbors(NeighborDirection.Front);

            foreach (NeighborInfo neighbor in neighbors)
            {
                if (neighbor.Neighbor.ContainsGeometry
                    || CheckFrontForTerrain(neighbor.Neighbor))
                {
                    return true;
                }
            }

            return false;
        }

        private bool CheckBackForTerrain(GeometryNode node)
        {
            List<NeighborInfo> neighbors = node.GetNeighbors(NeighborDirection.Back);

            foreach (NeighborInfo neighbor in neighbors)
            {
                if (neighbor.Neighbor.ContainsGeometry
                    || CheckBackForTerrain(neighbor.Neighbor))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}