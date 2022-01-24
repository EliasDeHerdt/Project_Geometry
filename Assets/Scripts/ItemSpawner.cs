using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeometryDetection;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _itemToSpawn;
    public GameObject ItemToSpawn
    {
        get { return _itemToSpawn; }
        set { _itemToSpawn = value; }
    }

    [SerializeField] private Transform _itemParent;
    public Transform ItemParent
    {
        get { return _itemParent; }
        set { _itemParent = value; }
    }

    [SerializeField] private HoleAndTunnelDetector _detector;
    public GeometryDetection.HoleAndTunnelDetector Detector
    {
        get { return _detector; }
        set { _detector = value; }
    }

    void Start()
    {
        if (Detector)
        {
            Detector.DetectionFinished.AddListener(SpawnItems);
        }
    }

    private void SpawnItems()
    {
        List<DetectedGeometry> detectedGeo = Detector.DetectedGeometry;

        foreach (DetectedGeometry geometry in detectedGeo)
        {
            if (geometry.MarkedGeometry.Contains(GeometryType.Hole))
            {
                Debug.Log("Spawning item in Hole.");
            }

            if (geometry.MarkedGeometry.Contains(GeometryType.Tunnel))
            {
                Debug.Log("Spawning item in Tunnel.");
            }

            foreach (Exit exit in geometry.Exits)
            {
                Vector3 center = new Vector3();

                foreach(GeometryNode node in exit.Nodes)
                {
                    center += node.transform.position;
                }

                center /= exit.Nodes.Count;
                Instantiate(ItemToSpawn, center, Quaternion.identity, ItemParent);
            }
        }
    }
}
