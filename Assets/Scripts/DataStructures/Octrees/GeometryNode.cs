using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GeometryDetection
{
    public class GeometryNode : OctreeNode
    {
        #region Variables
        private bool _containsGeometry = false;
        public bool ContainsGeometry
        {
            get { return _containsGeometry; }
            private set { _containsGeometry = value;}
        }

        private GeometryDetector _geometryDetector;
        public GeometryDetector GeometryDetector
        {
            get { return _geometryDetector; }
        }

        private BoxCollider _detectionCollider;
        public BoxCollider DetectionCollider
        {
            get { return _detectionCollider; }
            private set { _detectionCollider = value; }
        }

        private Rigidbody _detectionRigidBody;
        public Rigidbody DetectionRigidBody
        {
            get { return _detectionRigidBody; }
            private set { _detectionRigidBody = value; }
        }

        private GeometryNode _parentNode;
        public GeometryNode ParentNode
        {
            get { return _parentNode;}
            private set { _parentNode = value; }
        }

        private List<NeighborInfo> _neighbors = new List<NeighborInfo>();
        public List<NeighborInfo> Neighbors
        {
            get { return _neighbors; }
        }

        private MeshFilter _meshFilter;
        private Renderer _nodeRenderer;
        public Renderer NodeRenderer
        {
            get 
            { 
                if (_nodeRenderer == null)
                {
                    _meshFilter = gameObject.AddComponent<MeshFilter>();
                    _meshFilter.mesh = GeometryDetector.NodePreviewMesh;
                    ScaleMeshToNode(_meshFilter.mesh);

                    _nodeRenderer = gameObject.AddComponent<MeshRenderer>();
                }

                return _nodeRenderer; 
            }
        }
        #endregion

        protected override void Awake()
        {
            base.Awake();

            _geometryDetector = GetComponentInParent<GeometryDetector>();

            DetectionCollider = gameObject.AddComponent<BoxCollider>();
            DetectionCollider.isTrigger = true;

            DetectionRigidBody = gameObject.AddComponent<Rigidbody>();
            DetectionRigidBody.useGravity = false;
            DetectionRigidBody.isKinematic = true;
        }

        protected override void Start()
        {
            base.Start();

            // Detect if we overlap with anything
            DetectGeometryAndNeighbors();
            ProcessNodeState();

            // Notify detector we are finished
            GeometryDetector.NodesCompleted++;
        }

        private void DetectGeometryAndNeighbors()
        {
            // Get all colliders that overlap with our box
            Collider[] overlaps = Physics.OverlapBox(transform.position, DetectionCollider.bounds.extents, transform.rotation, LayerMask.GetMask(new string[] { "Geometry", "GeoDetection" }));

            // Loop over the found colliders and perform certain checks
            foreach (Collider collider in overlaps)
            {
                GeometryNode otherNode;
                if (collider.gameObject.layer == LayerMask.NameToLayer("Geometry"))
                {
                    ContainsGeometry = true;

                    List<Vector3> colliderBounds = new List<Vector3>();
                    colliderBounds.Add(transform.position + new Vector3(DetectionCollider.bounds.extents.x, DetectionCollider.bounds.extents.y, DetectionCollider.bounds.extents.z));
                    colliderBounds.Add(transform.position + new Vector3(-DetectionCollider.bounds.extents.x, DetectionCollider.bounds.extents.y, DetectionCollider.bounds.extents.z));
                    colliderBounds.Add(transform.position + new Vector3(-DetectionCollider.bounds.extents.x, -DetectionCollider.bounds.extents.y, DetectionCollider.bounds.extents.z));
                    colliderBounds.Add(transform.position + new Vector3(DetectionCollider.bounds.extents.x, -DetectionCollider.bounds.extents.y, DetectionCollider.bounds.extents.z));
                    colliderBounds.Add(transform.position + new Vector3(DetectionCollider.bounds.extents.x, DetectionCollider.bounds.extents.y, -DetectionCollider.bounds.extents.z));
                    colliderBounds.Add(transform.position + new Vector3(-DetectionCollider.bounds.extents.x, DetectionCollider.bounds.extents.y, -DetectionCollider.bounds.extents.z));
                    colliderBounds.Add(transform.position + new Vector3(-DetectionCollider.bounds.extents.x, -DetectionCollider.bounds.extents.y, -DetectionCollider.bounds.extents.z));
                    colliderBounds.Add(transform.position + new Vector3(DetectionCollider.bounds.extents.x, -DetectionCollider.bounds.extents.y, -DetectionCollider.bounds.extents.z));

                    // Check if every corner of our cube is inside of our hit mesh.
                    // If this is the case, there is no need to partition any further as our entire collider will be terrain.
                    foreach (Vector3 point in colliderBounds)
                    {
                        if (!ColliderContainsPoint(DetectionCollider.transform, point, DetectionCollider.enabled))
                        {
                            Partition();
                            break;
                        }
                    }
                }
                else if (collider.gameObject.layer == LayerMask.NameToLayer("GeoDetection")
                    && (otherNode = collider.gameObject.GetComponent<GeometryNode>()))
                {
                    StoreNeighbor(otherNode);
                }
            }
        }

        // Code taken from UnityForums: https://forum.unity.com/threads/bounds-contains-is-not-working-at-all-as-expected.483628/
        private bool ColliderContainsPoint(Transform ColliderTransform, Vector3 Point, bool Enabled)
        {
            Vector3 localPos = ColliderTransform.InverseTransformPoint(Point);
            if (Enabled 
                && Mathf.Abs(localPos.x) < 0.5f 
                && Mathf.Abs(localPos.y) < 0.5f 
                && Mathf.Abs(localPos.z) < 0.5f)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region Partitioning
        protected override void Partition()
        {
            if (!GeometryDetector)
            {
                Debug.LogError("GeometryNode: No GeometryDetector is present on this GameObject!");
                return;
            }

            // Check if it is allowed to go any deeper in our tree.
            int newDepth = Depth + 1;
            if (newDepth > GeometryDetector.MaxSteps
                || HasChildren)
            {
                return;
            }

            // Clear our neighbors as this node is no longer used, we are more interested in its children (also for clarity in the editor)
            Neighbors.Clear();

            // Disable our Rigidbody to prevent faulty neighbor detection.
            DetectionRigidBody.detectCollisions = false;

            // Create a new Node for every child and set its parameters.
            for (int i = 0; i < Children.Capacity; i++)
            {
                // Create our new object with the correct name, parent, and position.
                GameObject obj = new GameObject("Layer" + newDepth + "Node" + ParentTree.NodeCount);
                obj.transform.parent = transform;
                obj.transform.position = transform.position + IteratePositions(i);
                obj.layer = LayerMask.NameToLayer("GeoDetection");

                // Create and add our Node component to the object.
                // Also set its correct Depth and Collider-Size.
                GeometryNode node = obj.AddComponent<GeometryNode>();
                node.Depth = newDepth;
                node.ParentNode = this;
                node.ParentTree = ParentTree;
                node.DetectionCollider.size = DetectionCollider.size / 2f;
                node.DetectionCollider.enabled = true;

                Children[i] = node;
            }

            // Disable our collider as it is no longer used (the children fill up the same space)
            DetectionCollider.enabled = false;
        }

        // This function returns the correct position for the child nodes based on which of the 8 nodes is being spawned.
        private Vector3 IteratePositions(int step)
        {
            int moveSide = step % 2;
            int moveForward = (step / 2) % 2;
            bool moveDown = (-Children.Capacity / 2 + step) >= 0;

            float x = (DetectionCollider.bounds.extents.x / 2) * (1 + -moveSide * 2);
            float y = (DetectionCollider.bounds.extents.y / 2);
            float z = (DetectionCollider.bounds.extents.z / 2) * (1 + -moveForward * 2);

            if (moveDown)
            {
                y *= -1;
            }

            return new Vector3(x, y, z);
        }
#endregion
        #region Neighbors
        private void StoreNeighbor(GeometryNode foundNeighbor)
        {
            // Check if neighbor isn't already present in our list.
            NeighborInfo alreadyPresentCheck = NeighborInfo.FindNeighbor(foundNeighbor, Neighbors);
            if (alreadyPresentCheck.Neighbor)
                return;

            // Check if neighbors parent is already in our neighbor list.
            // If so, remove it first.
            Neighbors.Remove(NeighborInfo.FindNeighbor(foundNeighbor.ParentNode, Neighbors));

            // Check where the neigbor is located compared to this node
            NeighborDirection direction = GetDirectionToNode(foundNeighbor);
            if (direction == NeighborDirection.Invalid)
                return;

            // Set up the Neighbor Info
            NeighborInfo info = new NeighborInfo(foundNeighbor, direction);

            // Add the neighbor to our list.
            Neighbors.Add(info);

            // Add ourselves to the neighbor.
            foundNeighbor.StoreNeighbor(this);
        }

        private NeighborDirection GetDirectionToNode(GeometryNode node)
        {
            GeometryNode thisCorrectParent = this;
            GeometryNode otherCorrectParent = node;

            // If the other node is smaller, go up until their parent is the same size.
            while (otherCorrectParent != null
                && thisCorrectParent.Depth < otherCorrectParent.Depth)
            {
                otherCorrectParent = otherCorrectParent.ParentNode;
            }

            // If we are smaller, go up so our parent is the same size.
            while (thisCorrectParent != null
                && otherCorrectParent.Depth < thisCorrectParent.Depth)
            {
                thisCorrectParent = thisCorrectParent.ParentNode;
            }

            Vector3 ourPosition = thisCorrectParent.transform.position;
            Vector3 otherPosition = otherCorrectParent.gameObject.transform.position;

            #region Default Checks
            if (otherPosition.x == ourPosition.x
                && otherPosition.y > ourPosition.y
                && otherPosition.z == ourPosition.z)
            {
                return NeighborDirection.Up;
            }
            else if (otherPosition.x < ourPosition.x
                && otherPosition.y == ourPosition.y
                && otherPosition.z == ourPosition.z)
            {
                return NeighborDirection.Left;
            }
            else if (otherPosition.x == ourPosition.x
                && otherPosition.y < ourPosition.y
                && otherPosition.z == ourPosition.z)
            {
                return NeighborDirection.Down;
            }
            else if (otherPosition.x > ourPosition.x
                && otherPosition.y == ourPosition.y
                && otherPosition.z == ourPosition.z)
            {
                return NeighborDirection.Right;
            }
            else if (otherPosition.x == ourPosition.x
                && otherPosition.y == ourPosition.y
                && otherPosition.z < ourPosition.z)
            {
                return NeighborDirection.Back;
            }
            else if (otherPosition.x == ourPosition.x
                && otherPosition.y == ourPosition.y
                && otherPosition.z > ourPosition.z)
            {
                return NeighborDirection.Front;
            }
            #endregion
            #region Extra Checks
            //else if(otherPosition.x < ourPosition.x
            //    && otherPosition.y > ourPosition.y
            //    && otherPosition.z == ourPosition.z)
            //{
            //    return NeighborDirection.UpLeft;
            //}
            //else if (otherPosition.x < ourPosition.x
            //    && otherPosition.y < ourPosition.y
            //    && otherPosition.z == ourPosition.z)
            //{
            //    return NeighborDirection.DownLeft;
            //}
            //else if (otherPosition.x > ourPosition.x
            //    && otherPosition.y < ourPosition.y
            //    && otherPosition.z == ourPosition.z)
            //{
            //    return NeighborDirection.DownRight;
            //}
            //else if (otherPosition.x > ourPosition.x
            //    && otherPosition.y > ourPosition.y
            //    && otherPosition.z == ourPosition.z)
            //{
            //    return NeighborDirection.UpRight;
            //}
            #endregion

            return NeighborDirection.Invalid;
        }

        public List<NeighborInfo> GetNeighbors(NeighborDirection direction = NeighborDirection.None)
        {
            if (direction == NeighborDirection.None)
            {
                return Neighbors;
            }

            List<NeighborInfo> output = new List<NeighborInfo>();
            foreach (NeighborInfo neighbor in Neighbors)
            {
                if (neighbor.Direction == direction)
                {
                    output.Add(neighbor);
                }
            }

            return output;
        }
        #endregion
        #region VisualizationAndProcessing
        private void ProcessNodeState()
        {
            if (!ContainsGeometry)
            {
                GeometryDetector.EmptyNodes.Add(this);

                if (GeometryDetector.VisualizeAir)
                {
                    NodeRenderer.sharedMaterial = GeometryDetector.AirMaterial;
                }
            }
            else if (GeometryDetector.VisualizeGeometry
                && !HasChildren)
            {
                NodeRenderer.sharedMaterial = GeometryDetector.GeometryMaterial;
            }
        }

        private void ScaleMeshToNode(Mesh mesh)
        {
            Vector3[] baseVertices = mesh.vertices;
            Vector3[] newVertices = new Vector3[baseVertices.Length];
            for (int i = 0; i < baseVertices.Length; ++i)
            {
                Vector3 currVertex = baseVertices[i];

                currVertex.x *= DetectionCollider.size.x;
                currVertex.y *= DetectionCollider.size.y;
                currVertex.z *= DetectionCollider.size.z;

                newVertices[i] = currVertex;
            }
            mesh.vertices = newVertices;
        }
        #endregion
        #region CleanUp
        public void CleanUpColliders()
        {
            Destroy(DetectionCollider);
            Destroy(DetectionRigidBody);

            foreach(GeometryNode child in Children)
            {
                if (child)
                    child.CleanUpColliders();
            }
        }
        #endregion
    }
}