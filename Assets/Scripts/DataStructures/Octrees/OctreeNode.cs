using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryDetection
{
    public abstract class OctreeNode : MonoBehaviour
    {
        #region Variables
        private Octree _parentTree;
        public Octree ParentTree
        {
            get { return _parentTree; }
            set 
            {
                if (value == null
                    && _parentTree != null)
                {
                    _parentTree.NodeCount--;
                }

                _parentTree = value;

                if (_parentTree != null)
                {
                    _nodeID = _parentTree.NodeCount++;
                }
            }
        }

        private int _nodeID = 0;
        public int NodeID
        {
            get { return _nodeID; }
        }

        private int _depth = 0;
        public int Depth
        {
            get { return _depth; }
            set { _depth = value; }
        }

        public bool HasChildren
        {
            get
            {
                foreach (var child in Children)
                    if (child)
                        return true;

                return false;
            }
        }

        private List<OctreeNode> _children = new List<OctreeNode>(8);
        public List<OctreeNode> Children
        {
            get { return _children; }
        }
        #endregion

        protected virtual void Awake()
        {
            // Fill our list with empty values.
            // At start the capacity will be 8, but the list will still be empty.
            for (int i = 0; i < Children.Capacity; i++)
            {
                Children.Add(null);
            }
        }

        protected virtual void Start() { }

        //public abstract void Collapse();
        protected abstract void Partition();
    }
}