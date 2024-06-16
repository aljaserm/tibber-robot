namespace Application.DTOs
{
    /// <summary>
    /// Data transfer object for execution results.
    /// </summary>
    public class ExecutionResultDto
    {
        /// <summary>
        /// Gets or sets the ID of the execution.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of the execution.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the number of commands executed.
        /// </summary>
        public int Commands { get; set; }

        /// <summary>
        /// Gets or sets the number of unique positions visited.
        /// </summary>
        public int Result { get; set; }

        /// <summary>
        /// Gets or sets the duration of the execution.
        /// </summary>
        public double Duration { get; set; }
    }
}