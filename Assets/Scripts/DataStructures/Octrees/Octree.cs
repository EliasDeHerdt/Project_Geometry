using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryDetection
{
    // Not templated due to the limitations of a templated class being too big of a problem.
    // F.E. Octree<OctreeNode> cannot store a Octree<GeometryNode>
    // Also, we do not really care which nodes are inside of the tree, as long as the user is satisfied with it.
    public class Octree
    {
        #region Variables
        private int _nodeCount;
        public int NodeCount 
        { 
            get { return _nodeCount; } 
            internal set { _nodeCount = value; }
        }

        private OctreeNode _baseNode;
        public OctreeNode BaseNode
        {
            get { return _baseNode; }
            set 
            { 
                _baseNode = value;
                _baseNode.ParentTree = this;
            }
        }
        #endregion

    }
}