using Application.Enums;
using System;
using System.Collections.Generic;

namespace Application.Utilities
{
    /// <summary>
    /// Utility class for handling movements.
    /// </summary>
    public static class MovementUtility
    {
        /// <summary>
        /// Moves the current position based on the direction.
        /// </summary>
        /// <param name="direction">The direction to move.</param>
        /// <param name="currentX">The current X coordinate (passed by reference).</param>
        /// <param name="currentY">The current Y coordinate (passed by reference).</param>
        /// <param name="uniquePositions">The set of unique positions visited.</param>
        public static void Move(DirectionEnum direction, ref int currentX, ref int currentY, HashSet<(int, int)> uniquePositions)
        {
            switch (direction)
            {
                case DirectionEnum.North:
                    currentY++;
                    break;
                case DirectionEnum.East:
                    currentX++;
                    break;
                case DirectionEnum.South:
                    currentY--;
                    break;
                case DirectionEnum.West:
                    currentX--;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported direction: {direction}");
            }
            uniquePositions.Add((currentX, currentY));
        }
    }
}