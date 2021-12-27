using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeometryDetection
{
    public enum Progress
    {
        Generating,
        Processing,
        Finished
    }

    public enum NodeSetUp
    {
        SkipFrame,
        SetUpData,
        Waiting,
        Finished
    }

    public enum Detection
    {
        Detecting,
        Empty,
        ContainsGeometry
    }

    // Direction is based on world(X, Y ,Z) where:
    // X = Left - Right
    // Y = Up - Down
    // Z = Front - Back
    public enum NeighborDirection
    {
        Up,
        UpLeft,
        Left,
        DownLeft,
        Down,
        DownRight,
        Right,
        UpRight,
        Front,
        FrontUp,
        FrontUpLeft,
        FrontLeft,
        FrontDownLeft,
        FrontDown,
        FrontDownRight,
        FrontRight,
        FrontUpRight,
        Back,
        BackUp,
        BackUpLeft,
        BackLeft,
        BackDownLeft,
        BackDown,
        BackDownRight,
        BackRight,
        BackUpRight,
        Invalid
    }
}
