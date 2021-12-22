using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryDetection
{
    public class HoleDetector : MonoBehaviour
    {
        [SerializeField] private Vector3 _startPos = new Vector3(0f, 0f ,0f);
        [SerializeField] private Vector3 _startScale = new Vector3(1f, 1f ,1f);

        /// <summary>This collider's information will be used to create the first node's collider. It will be disabled on start</summary>
        [SerializeField] private Collider _guideCollider;

        private Octree<GeometryNode> _nodeTree = new Octree<GeometryNode>();
        public Octree<GeometryNode> NodeTree
        {
            get { return _nodeTree; }
        }

        void Start()
        {
            GameObject obj = new GameObject("BaseNode");
            obj.transform.parent = transform;

            GeometryNode comp = obj.AddComponent<GeometryNode>();
            comp.DetectionCollider.size = _guideCollider ? _guideCollider.bounds.size : _startScale;

            obj.transform.position = _guideCollider ? _guideCollider.transform.position : _startPos;
            NodeTree.BaseNode = obj.GetComponent<GeometryNode>();
            NodeTree.BaseNode.Partition();
            NodeTree.BaseNode.DetectGeometry();

            if (_guideCollider)
                _guideCollider.enabled = false;
                //Destroy(_guideCollider);
        }
    }

}