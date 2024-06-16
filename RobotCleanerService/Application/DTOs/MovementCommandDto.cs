using System.Text.Json.Serialization;
using Application.Enums;

namespace Application.DTOs
{
    /// <summary>
    /// Data transfer object for movement commands.
    /// </summary>
    public class MovementCommandDto
    {
        /// <summary>
        /// Gets or sets the direction of the movement.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DirectionEnum Direction { get; set; }

        /// <summary>
        /// Gets or sets the number of steps to move.
        /// </summary>
        public int Steps { get; set; }
    }
}