using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryDetection
{
    public class Octree<T> where T : OctreeNode
    {
        private T _baseNode;
        public T BaseNode
        {
            get { return _baseNode; }
            set 
            { 
                _baseNode = value;
            }
        }

        //private void GetNode(byte[] path)
        //{
        //    foreach(byte step in path)
        //    {
        //        if ((byte)(step & (step - 1)) != 0)
        //        {

        //        }
        //    }
        //}
    }
}