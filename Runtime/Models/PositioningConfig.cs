using System;
using UnityEngine;

namespace UGC.Dropview
{
    public enum Direction
    {
        Down,
        Up,
        Right,
        Left
    }

    [Serializable]
    public class PositioningConfig
    {
        public Vector2 edgePadding = new Vector2(8, 8);
        public bool flipOnOverflow = true;
        public bool matchTriggerWidth = true;
        public Direction[] preferredDirections = new Direction[] { Direction.Down, Direction.Up, Direction.Right, Direction.Left };
    }
}
