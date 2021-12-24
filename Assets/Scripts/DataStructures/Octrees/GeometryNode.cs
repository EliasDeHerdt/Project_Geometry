using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GeometryDetection
{
    public class GeometryNode : OctreeNode
    {
        [SerializeField] private bool _partitionNode = false;
        private bool _containsGeometry = false;
        public bool ContainsGeometry
        {
            get { return _containsGeometry; }
            private set 
            { 
                _containsGeometry = value;
            }
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
            set { _detectionCollider = value; }
        }

        protected override void Awake()
        {
            base.Awake();

            _geometryDetector = GetComponentInParent<GeometryDetector>();

            DetectionCollider = gameObject.AddComponent<BoxCollider>();
            DetectionCollider.isTrigger = true;
        }

        private void Update()
        {
            if (_partitionNode)
                Partition();

            _partitionNode = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Geometry"))
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
                foreach (var point in colliderBounds)
                {
                    if (!other.bounds.Contains(point))
                    {
                        Partition();
                        break;
                    }
                }
            }
        }

        protected override void Partition()
        {
            if (!GeometryDetector)
            {
                Debug.LogError("GeometryNode: No GeometryDetector is present on this GameObject!");
                return;
            }

            // Notify our GeometryDetector that we are still adding onto the octree.
            GeometryDetector.CurrentProgress = GeometryDetector.Progress.Generating;

            // Check if it is allowed to go any deeper in our tree.
            int newDepth = Depth + 1;
            if (newDepth > GeometryDetector.MaxSteps)
            {
                GeometryDetector.BottomNodes.Add(this);
                return;
            }

            // If any exist, destroy our previous children to re-partition.
            foreach (OctreeNode child in Children)
            {
                if (child)
                    Destroy(child);
            }

            // Create a new Node for every child and set its parameters.
            for (int i = 0; i < Children.Capacity; i++)
            {
                // Create our new object with the correct name, parent, and position.
                GameObject obj = new GameObject("Layer" + newDepth + "Node" + (i + 1));
                obj.transform.parent = transform;
                obj.transform.position = transform.position + IteratePositions(i);

                // Create and add our Node component to the object.
                // Also set its correct Depth and Collider-Size.
                GeometryNode comp = obj.AddComponent<GeometryNode>();
                comp.Depth = newDepth;
                comp.DetectionCollider.size = DetectionCollider.size / 2f;

                Children[i] = comp;
            }

            // If our children where succesfully made (which should always be the case), disable the parents collider.
            if (HasChildren)
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

            return new Vector3 (x, y, z);
        }
    }
}