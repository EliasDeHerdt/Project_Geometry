using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryDetection
{
    public abstract class OctreeNode : MonoBehaviour
    {
        

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

        protected virtual void Awake()
        {
            for (int i = 0; i < Children.Capacity; i++)
            {
                Children.Add(null);
            }
        }

        //public abstract void Collapse();
        protected abstract void Partition();
    }
}