using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GeometryDetection
{
    [CustomEditor(typeof(GeometryDetector))]
    public class GeometryDetectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            //GeometryDetector geometryDetector = (GeometryDetector)target;

            //if (GUILayout.Button("Partition Space"))
            //{
            //    geometryDetector.StartDetection();
            //}

            DrawDefaultInspector();
        }
    }
}