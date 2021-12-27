using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryDetection
{
    // Utilizes the already detected geometry and only tries to find holes or tunnels
    public class HoleDetector : MonoBehaviour
    {
        #region Variables
        private GeometryDetector _geometryDetector;
        public GeometryDetector GeometryDetector 
        { 
            get { return _geometryDetector; } 
        }
        #endregion

        private void Start()
        {
            _geometryDetector = GetComponent<GeometryDetector>();
            if (!_geometryDetector)
                Debug.LogError("HoleDetector: No GeometryDetector has been found on this object!");
        }

        public void DetectHoles()
        {
            List<GeometryNode> geometry = GeometryDetector.EmptyNodes;
            Debug.Log("Pulled in data.");

            // Start detecting the holes.
        }
    }
}