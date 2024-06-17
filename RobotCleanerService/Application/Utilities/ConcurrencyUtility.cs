using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Utilities
{
    public static class ConcurrencyUtility
    {
        /// <summary>
        /// Handles the concurrency conflicts and retries the save operation.
        /// </summary>
        /// <typeparam name="T">The type of the entity being saved.</typeparam>
        /// <param name="context">The database context.</param>
        /// <param name="entity">The entity to save.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="maxRetries">The maximum number of retries.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task representing the asynchronous operation.</returns>
        public static async Task<bool> SaveWithRetriesAsync<T>(ApplicationDbContext context, T entity, ILogger logger, int maxRetries, CancellationToken cancellationToken) where T : class
        {
            int retryCount = 0;
            bool saved = false;

            while (!saved)
            {
                try
                {
                    context.Set<T>().Add(entity);
                    await context.SaveChangesAsync(cancellationToken);
                    saved = true;
                    logger.LogInformation("Entity saved successfully.");
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    logger.LogWarning("Concurrency conflict detected: {Message}", ex.Message);
                    retryCount++;

                    if (retryCount >= maxRetries)
                    {
                        logger.LogError("Max retry attempts reached. Could not save entity.");
                        throw new InvalidOperationException("Concurrency conflict occurred. Please try again later.", ex);
                    }

                    foreach (var entry in ex.Entries)
                    {
                        entry.OriginalValues.SetValues((await entry.GetDatabaseValuesAsync(cancellationToken))!);
                    }
                }
            }

            return saved;
        }
    }
}
