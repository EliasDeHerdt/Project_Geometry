using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryDetection
{
    // Utilizes the already detected geometry and only tries to find holes or tunnels
    public class HoleDetector : MonoBehaviour
    {
        #region Variables
        [SerializeField] private float _maxDiameter = 10;
        public float MaxDiameter 
        { 
            get { return _maxDiameter; } 
            set { _maxDiameter = value; }
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
                bool TerrainAllDirections = true;
                List<GeometryNode> geometryNodes = new List<GeometryNode>();

                // Check Top for terrain.
                TerrainAllDirections = (TerrainAllDirections && CheckTopForTerrain(node));

                // Check Bottom for terrain.
                TerrainAllDirections = (TerrainAllDirections && CheckBottomForTerrain(node));

                // Check Left for terrain.
                TerrainAllDirections = (TerrainAllDirections && CheckLeftForTerrain(node));

                // Check Right for terrain.
                TerrainAllDirections = (TerrainAllDirections && CheckRightForTerrain(node));

                if (TerrainAllDirections)
                {
                    node.NodeRenderer.sharedMaterial = TunnelMaterial;
                }
            }
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
        #endregion
    }
}