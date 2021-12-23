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

        private BoxCollider _detectionCollider;
        public BoxCollider DetectionCollider
        {
            get { return _detectionCollider; }
            set { _detectionCollider = value; }
        }

        protected override void Awake()
        {
            base.Awake();

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
            int newDepth = Depth + 1;
            GeometryDetector.CurrentProgress = GeometryDetector.Progress.Generating;
            if (newDepth > GeometryDetector.MaxSteps)
            {
                GeometryDetector.BottomNodes.Add(this);
                return;
            }

            foreach (OctreeNode child in Children)
            {
                if (child)
                    Destroy(child);
            }

            for (int i = 0; i < Children.Capacity; i++)
            {
                GameObject obj = new GameObject("Layer" + newDepth + "Node" + (i + 1));
                obj.transform.parent = transform;
                obj.transform.position = transform.position + IteratePositions(i);

                GeometryNode comp = obj.AddComponent<GeometryNode>();
                comp.Depth = newDepth;
                comp.DetectionCollider.size = DetectionCollider.size / 2f;

                Children[i] = comp;
            }

            if (HasChildren)
                DetectionCollider.enabled = false;
        }

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