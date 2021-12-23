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
        [Header("At any given point in time, there should only be 1 of these!")]
        [SerializeField] private Vector3 _startPos = new Vector3(0f, 0f ,0f);
        [SerializeField] private Vector3 _startScale = new Vector3(1f, 1f ,1f);

        [Header("Collider that acts as starting area.")]
        /// <summary>This collider's information will be used to create the first node's collider. It will be disabled on start</summary>
        [SerializeField] private Collider _guideCollider;

        [Header("Detection Parameters")]
        [SerializeField] private int _maxSteps = 10;
        private static int _maxStepsStatic;
        static public int MaxSteps
        { 
            get { return _maxStepsStatic; } 
        }

        private static List<GeometryNode> _bottomNodes = new List<GeometryNode>();
        public static List<GeometryNode> BottomNodes
        {
            get { return _bottomNodes; }
        }

        private Octree<GeometryNode> _nodeTree = new Octree<GeometryNode>();
        public Octree<GeometryNode> NodeTree
        {
            get { return _nodeTree; }
        }

        // -------- Initial Generation Variables --------
        private static Progress _currentProgress = Progress.Generating;
        public static Progress CurrentProgress
        {
            get { return _currentProgress; }
            set { _currentProgress = value; }
        }

        public UnityEvent GenerationFinished;
        // ----------------------------------------------

        void Start()
        {
            _maxStepsStatic = _maxSteps;

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
                    GenerationFinished.Invoke();
                    break;
                case Progress.Finished:
                default:
                    break;
            }
        }
    }

}