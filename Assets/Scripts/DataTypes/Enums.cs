using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryDetection
{
    // Direction is based on world(X, Y ,Z) where:
    // X = Left - Right
    // Y = Up - Down
    // Z = Front - Back
    public enum NeighborDirection
    {
        None,
        Up,
        Down,
        Left,
        Right,
        Front,
        Back
    }

    public enum GeometryType
    {
        SpecialGeometry,
        Hole,
        Tunnel,
        Exit
    }
}
