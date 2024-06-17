using Application.DTOs;
using Application.Enums;
using System;

namespace Application.Utilities
{
    /// <summary>
    /// Utility class for handling movements.
    /// </summary>
    public static class MovementUtility
    {
        /// <summary>
        /// Updates the boundaries based on the command.
        /// </summary>
        /// <param name="command">The movement command.</param>
        /// <param name="minX">The minimum X boundary.</param>
        /// <param name="maxX">The maximum X boundary.</param>
        /// <param name="minY">The minimum Y boundary.</param>
        /// <param name="maxY">The maximum Y boundary.</param>
        /// <param name="currentX">The current X coordinate.</param>
        /// <param name="currentY">The current Y coordinate.</param>
        public static void UpdateBoundaries(MovementCommandDto command, ref int minX, ref int maxX, ref int minY, ref int maxY, ref int currentX, ref int currentY)
        {
            switch (command.Direction)
            {
                case DirectionEnum.North:
                    currentY += command.Steps;
                    if (currentY > maxY) maxY = currentY;
                    break;
                case DirectionEnum.East:
                    currentX += command.Steps;
                    if (currentX > maxX) maxX = currentX;
                    break;
                case DirectionEnum.South:
                    currentY -= command.Steps;
                    if (currentY < minY) minY = currentY;
                    break;
                case DirectionEnum.West:
                    currentX -= command.Steps;
                    if (currentX < minX) minX = currentX;
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported direction: {command.Direction}");
            }
        }
    }
}