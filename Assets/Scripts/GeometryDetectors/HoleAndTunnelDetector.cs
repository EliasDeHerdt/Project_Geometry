using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

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

        [SerializeField] private bool _visualizeExits = true;
        public bool VisualizeExits
        {
            get { return _visualizeExits; }
            set { _visualizeExits = value; }
        }

        [SerializeField] private Material _tunnelMaterial = null;
        public Material TunnelMaterial
        {
            get { return _tunnelMaterial; }
        }

        [SerializeField] private Material _exitMaterial = null;
        public Material ExitMaterial
        {
            get { return _exitMaterial; }
        }

        private GeometryDetector _geometryDetector;
        public GeometryDetector GeometryDetector 
        { 
            get { return _geometryDetector; } 
        }

        private List<DetectedGeometry> _detectGeometry = new List<DetectedGeometry>();
        public List<DetectedGeometry> DetectedGeometry
        {
            get { return _detectGeometry; }
            private set { _detectGeometry = value; }
        }

        public UnityEvent DetectionFinished;
        #endregion

        private void Awake()
        {
            _geometryDetector = GetComponent<GeometryDetector>();
            if (_geometryDetector)
            {
                _geometryDetector.GenerationFinished.AddListener(DetectHoles);
            }
            else
            {
                Debug.LogError("HoleDetector: No GeometryDetector has been found on this object!");
            }
        }

        public void DetectHoles()
        {
            // Create new wrapper class that stores Nodes and extra information unique to the hole detector
            List<GeometryNode> geometry = GeometryDetector.EmptyNodes.OrderBy(g => g.Depth).ToList();
            Debug.Log("Pulled in data.");

            // Start detecting the holes.
            foreach (GeometryNode foundNode in geometry)
            {
                DetectedGeometry detectedGeometry = new DetectedGeometry(new List<GeometryNode>());
                List<GeometryNode> potentialExits = new List<GeometryNode>();

                List<GeometryNode> priorityQueue = new List<GeometryNode>();
                priorityQueue.Add(foundNode);
                while(priorityQueue.Count != 0)
                {
                    GeometryNode node = priorityQueue[0];

                    priorityQueue.AddRange(DetectTunnelOrHole(node, ref detectedGeometry, potentialExits));
                    priorityQueue.RemoveAt(0);
                }

                if (detectedGeometry.Nodes.Count > 0)
                {
                    // Add the exit nodes to the detected geometry
                    foreach (GeometryNode exit in potentialExits)
                    {
                        List<GeometryNode> exits = new List<GeometryNode>();
                        DetectIfPartOfExit(exit, exits);

                        if (exits.Count > 0)
                        {
                            detectedGeometry.Exits.Add(new Exit(exits, detectedGeometry));
                        }
                    }

                    if (detectedGeometry.MarkedGeometry.Contains(GeometryType.Hole))
                    {
                        if (detectedGeometry.Exits.Count > 1)
                        {
                            detectedGeometry.MarkedGeometry.Remove(GeometryType.Hole);
                            detectedGeometry.MarkedGeometry.Add(GeometryType.Tunnel);
                        }
                        DetectedGeometry.Add(detectedGeometry);
                    }
                }
            }

            VisualizeData();
            Debug.Log("Finished Tunnel Detection with " + DetectedGeometry.Count + " Tunnel(s) or Hole(s).");

            DetectionFinished.Invoke();
        }

        #region DirectionalChecks
        private List<GeometryNode> DetectTunnelOrHole(GeometryNode node, ref DetectedGeometry detectedGeometry, List<GeometryNode> potentialExits)
        {
            List<GeometryNode> output = new List<GeometryNode>();
            if (node.ContainsGeometry
                || node.MarkedAs.Contains(GeometryType.Hole)
                || node.MarkedAs.Contains(GeometryType.Tunnel))
            {
                return output;
            }

            bool isNeighbor = false;
            int neighboringHits = 0;
            DetectionInfo TerrainChecks = new DetectionInfo();
            float distance = GetSizeInDirection(node, NeighborDirection.Up);

            // Check Top for terrain.
            TerrainChecks.UpHit = CheckDirectionForTerrain(NeighborDirection.Up, node, node, ref distance, out isNeighbor);
            neighboringHits += isNeighbor ? 1 : 0;

            // Check Bottom for terrain.
            TerrainChecks.DownHit = CheckDirectionForTerrain(NeighborDirection.Down, node, node, ref distance, out isNeighbor);
            neighboringHits += isNeighbor ? 1 : 0;

            if (distance > MaxDiameter)
            {
                return output;
            }
            distance = GetSizeInDirection(node, NeighborDirection.Left);

            // Check Left for terrain.
            TerrainChecks.LeftHit = CheckDirectionForTerrain(NeighborDirection.Left, node, node, ref distance, out isNeighbor);
            neighboringHits += isNeighbor ? 1 : 0;

            // Check Right for terrain.
            TerrainChecks.RightHit = CheckDirectionForTerrain(NeighborDirection.Right, node, node, ref distance, out isNeighbor);
            neighboringHits += isNeighbor ? 1 : 0;

            if (distance > MaxDiameter)
            {
                return output;
            }
            distance = GetSizeInDirection(node, NeighborDirection.Front);

            // Check Front for terrain.
            TerrainChecks.FrontHit = CheckDirectionForTerrain(NeighborDirection.Front, node, node, ref distance, out isNeighbor);
            neighboringHits += isNeighbor ? 1 : 0;

            // Check Back for terrain.
            TerrainChecks.BackHit = CheckDirectionForTerrain(NeighborDirection.Back, node, node, ref distance, out isNeighbor);
            neighboringHits += isNeighbor ? 1 : 0;

            if (distance > MaxDiameter)
            {
                return output;
            }

            // To speed up the checks for the exits, we save the nodes that directly neighbor geometry on 2 sides.
            if (neighboringHits == 2)
            {
                potentialExits.Add(node);
            }

            if (CheckGeometry(ref detectedGeometry, TerrainChecks))
            {
                detectedGeometry.Nodes.Add(node);
                foreach (GeometryType GeoType in detectedGeometry.MarkedGeometry)
                {
                    node.MarkedAs.Add(GeoType);
                }

                List<NeighborInfo> neighbors = node.GetNeighbors();
                foreach (NeighborInfo neighbor in neighbors)
                {
                    output.Add(neighbor.Neighbor);
                }
            }

            return output;
        }
        
        private bool CheckDirectionForTerrain(NeighborDirection direction, GeometryNode startNode, GeometryNode currentNode, ref float distanceChecked, out bool foundOnFirstIteration)
        {
            if (startNode != currentNode
                && startNode.Depth <= currentNode.Depth)
            {
                GeometryNode nodeToCheck = currentNode;
                while(nodeToCheck.Depth != startNode.Depth)
                {
                    nodeToCheck = nodeToCheck.ParentNode;
                }

                Vector3 dirAsVec = DirectionToVector(direction);
                if ((nodeToCheck.transform.position - startNode.transform.position).normalized != dirAsVec)
                {
                    return foundOnFirstIteration = false;
                }
            }

            List<NeighborInfo> neighbors = currentNode.GetNeighbors(direction);
            foreach (NeighborInfo neighbor in neighbors)
            {
                if (currentNode.Depth > neighbor.Neighbor.Depth
                    && !neighbor.Neighbor.MarkedAs.Contains(GeometryType.Hole)
                    && !neighbor.Neighbor.MarkedAs.Contains(GeometryType.Tunnel))
                {
                    continue;
                }

                // Only after we checked the depth of our neighbor, we check the other requirements.
                // This is due to one of these conditions triggering recursion!
                if (neighbor.Neighbor.ContainsGeometry
                    || CheckDirectionForTerrain(direction, startNode, neighbor.Neighbor, ref distanceChecked, out foundOnFirstIteration))
                {
                    // If the first iteration instantly finds geometry, it will return true.
                    // It doesn't matter if geometry is found in the recursion as it will always bubble back up to the first iteration.
                    foundOnFirstIteration = neighbor.Neighbor.ContainsGeometry;

                    if (startNode != currentNode)
                    {
                        distanceChecked += GetSizeInDirection(currentNode, direction);
                    }

                    return true;
                }
            }

            return foundOnFirstIteration = false;
        }

        private void DetectIfPartOfExit(GeometryNode node, List<GeometryNode> exits, bool failedBefore = false)
        {
            if ((!node.MarkedAs.Contains(GeometryType.Hole) && !node.MarkedAs.Contains(GeometryType.Tunnel))
                || node.MarkedAs.Contains(GeometryType.Exit))
                return;

            DetectionInfo exitChecks = new DetectionInfo();
            List<NeighborInfo> neighborsToCheck = new List<NeighborInfo>();

            exitChecks.UpHit = CheckNeighborsForTunnelOrHole(NeighborDirection.Up, node, neighborsToCheck);
            exitChecks.DownHit = CheckNeighborsForTunnelOrHole(NeighborDirection.Down, node, neighborsToCheck);
            exitChecks.LeftHit = CheckNeighborsForTunnelOrHole(NeighborDirection.Left, node, neighborsToCheck);
            exitChecks.RightHit = CheckNeighborsForTunnelOrHole(NeighborDirection.Right, node, neighborsToCheck);
            exitChecks.FrontHit = CheckNeighborsForTunnelOrHole(NeighborDirection.Front, node, neighborsToCheck);
            exitChecks.BackHit = CheckNeighborsForTunnelOrHole(NeighborDirection.Back, node, neighborsToCheck);

            // When we find Geometry or a Node with the Tunnel or Hole mark in all but one direction, we have an exit.
            if (exitChecks.SuccesfullChecks == 4
                || exitChecks.SuccesfullChecks == 5)
            {
                node.MarkedAs.Add(GeometryType.Exit);
                exits.Add(node);

                foreach (NeighborInfo neighbor in neighborsToCheck)
                {
                    DetectIfPartOfExit(neighbor.Neighbor, exits);
                }
            }
            else if (!failedBefore)
            {
                foreach (NeighborInfo neighbor in neighborsToCheck)
                {
                    DetectIfPartOfExit(neighbor.Neighbor, exits, true);
                }
            }
        }

        private bool CheckNeighborsForTunnelOrHole(NeighborDirection direction, GeometryNode node, List<NeighborInfo> neighborsToCheck)
        {
            List<NeighborInfo> neighbors = node.GetNeighbors(direction);

            foreach (NeighborInfo neighbor in neighbors)
            {
                // If we find Geometry or a Node with the Hole or Tunnel mark, we need to continue looking in this direction later on.
                if (neighbor.Neighbor.ContainsGeometry
                    || neighbor.Neighbor.MarkedAs.Contains(GeometryType.Hole)
                    || neighbor.Neighbor.MarkedAs.Contains(GeometryType.Tunnel))
                {
                    neighborsToCheck.AddRange(neighbors);
                    return true;
                }
            }

            return false;
        }
        #endregion
        #region GeometryChecks
        private bool CheckGeometry(ref DetectedGeometry detectedGeometry, DetectionInfo detectedTerrain)
        {
            if (detectedTerrain.SuccesfullChecks < MinimumSuccesfullChecks)
                return false;

            return HoleCheck(ref detectedGeometry, detectedTerrain);
        }

        private bool HoleCheck(ref DetectedGeometry detectedGeometry, DetectionInfo detectedTerrain)
        {
            int doubleHitCounter = 0;
            doubleHitCounter += (detectedTerrain.UpHit && detectedTerrain.DownHit) ? 1 : 0;
            doubleHitCounter += (detectedTerrain.LeftHit && detectedTerrain.RightHit) ? 1 : 0;
            doubleHitCounter += (detectedTerrain.FrontHit && detectedTerrain.BackHit) ? 1 : 0;

            if (detectedTerrain.SuccesfullChecks >= 4
                && doubleHitCounter >= 2)
            {
                detectedGeometry.MarkedGeometry.Add(GeometryType.Hole);
                return true;
            }

            return false;
        }
        #endregion
        #region Other
        private void VisualizeData()
        {
            foreach (DetectedGeometry geo in DetectedGeometry)
            {
                foreach (GeometryNode node in geo.Nodes)
                {
                    if (VisualizeExits
                && node.MarkedAs.Contains(GeometryType.Exit))
                    {
                        node.NodeRenderer.sharedMaterial = ExitMaterial;
                    }
                    else if (VisualizeTunnels
                        && (node.MarkedAs.Contains(GeometryType.Hole) || node.MarkedAs.Contains(GeometryType.Tunnel)))
                    {
                        node.NodeRenderer.sharedMaterial = TunnelMaterial;
                    }
                }
            }
        }

        private Vector3 DirectionToVector(NeighborDirection direction)
        {
            switch (direction)
            {
                case NeighborDirection.Up: return new Vector3(0, 1, 0);
                case NeighborDirection.Down: return new Vector3(0, -1, 0);
                case NeighborDirection.Left: return new Vector3(-1, 0, 0);
                case NeighborDirection.Right: return new Vector3(1, 0, 0);
                case NeighborDirection.Front: return new Vector3(0, 0, 1);
                case NeighborDirection.Back: return new Vector3(0, 0, -1);
            }

            return new Vector3();
        }

        private float GetSizeInDirection(GeometryNode node, NeighborDirection direction)
        {
            switch (direction)
            {
                case NeighborDirection.Up: 
                case NeighborDirection.Down: return node.DetectionCollider.size.y;
                case NeighborDirection.Left: 
                case NeighborDirection.Right: return node.DetectionCollider.size.x;
                case NeighborDirection.Front: 
                case NeighborDirection.Back: return node.DetectionCollider.size.z;
            }

            return 0f;
        }
        #endregion
    }
}