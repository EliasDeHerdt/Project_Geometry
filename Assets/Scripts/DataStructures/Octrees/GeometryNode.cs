using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace GeometryDetection
{
    public class GeometryNode : OctreeNode
    {
        [SerializeField] private bool _partitionNode = false;

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
        }

        private void Update()
        {
            if (_partitionNode)
                Partition();

            _partitionNode = false;
        }

        public override void Partition()
        {
            foreach (OctreeNode child in Children)
            {
                if (child)
                    Destroy(child);
            }

            for (int i = 0; i < Children.Capacity; i++)
            {
                int newDepth = Depth + 1;
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

        public void DetectGeometry()
        {

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