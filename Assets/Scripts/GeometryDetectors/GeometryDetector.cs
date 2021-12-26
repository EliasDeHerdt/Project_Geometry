using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GeometryDetection
{
    public class GeometryDetector : MonoBehaviour
    {
        public enum Progress
        {
            Generating,
            Processing,
            Finished
        }

        [Header("Data to spawn when no collider is given.")]
        [Header("There should only be 1 instance of this component!")]
        [SerializeField] private Vector3 _startPos = new Vector3(0f, 0f ,0f);
        [SerializeField] private Vector3 _startScale = new Vector3(1f, 1f ,1f);

        [Header("Collider that acts as starting area.")]
        // This collider's information will be used to create the first node's collider. It will be disabled on start!
        [SerializeField] private Collider _guideCollider;

        [Header("Detection Parameters")]
        [SerializeField] private int _maxSteps = 5;
        public int MaxSteps
        { 
            get { return _maxSteps; } 
        }

        [Header("Visualization Parameters")]
        [SerializeField] private Mesh _nodePreviewMesh;
        public Mesh NodePreviewMesh
        {
            get { return _nodePreviewMesh; }
        }

        [SerializeField] private Material _nodePreviewMaterial;
        public Material NodePreviewMaterial
        {
            get { return _nodePreviewMaterial; }
        }

        [Header("Events")]
        public UnityEvent GenerationFinished;

        // ---------- Possible Output Variables ----------
        private Octree<GeometryNode> _nodeTree = new Octree<GeometryNode>();
        public Octree<GeometryNode> NodeTree
        {
            get { return _nodeTree; }
        }

        private List<GeometryNode> _emptyNodes = new List<GeometryNode>();
        public List<GeometryNode> EmptyNodes
        {
            get { return _emptyNodes; }
        }

        // -------- Initial Generation Variables --------
        private Progress _currentProgress = Progress.Generating;
        public Progress CurrentProgress
        {
            get { return _currentProgress; }
            set { _currentProgress = value; }
        }

        // ----------------------------------------------

        void Start()
        {
            GameObject obj = new GameObject("BaseNode");
            obj.transform.parent = transform;

            GeometryNode comp = obj.AddComponent<GeometryNode>();
            comp.DetectionCollider.size = _guideCollider ? _guideCollider.bounds.size : _startScale;

            obj.transform.position = _guideCollider ? _guideCollider.transform.position : _startPos;
            NodeTree.BaseNode = obj.GetComponent<GeometryNode>();

            if (_guideCollider)
                _guideCollider.enabled = false;
        }

        private void Update()
        {
            switch (CurrentProgress)
            {
                case Progress.Generating:
                    CurrentProgress = Progress.Processing;
                    break;
                case Progress.Processing:
                    CurrentProgress = Progress.Finished;
                    Debug.Log("Generation of octree has finished.");

                    GenerationFinished.Invoke();
                    break;
                case Progress.Finished:
                default:
                    break;
            }
        }
    }
}