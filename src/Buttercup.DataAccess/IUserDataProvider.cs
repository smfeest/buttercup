using System;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.Models;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// Defines the contract for the user data provider.
    /// </summary>
    public interface IUserDataProvider
    {
        /// <summary>
        /// Tries to find a user by email address.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <param name="email">
        /// The email address.
        /// </param>
        /// <returns>
        /// A task for the operation. The result is the user, or a null reference if no matching
        /// user is found.
        /// </returns>
        Task<User?> FindUserByEmail(DbConnection connection, string email);

        /// <summary>
        /// Gets a user.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <param name="id">
        /// The user ID.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        /// <exception cref="NotFoundException">
        /// No matching user was found.
        /// </exception>
        Task<User> GetUser(DbConnection connection, long id);

        /// <summary>
        /// Updates a user's password.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <param name="userId">
        /// The user ID.
        /// </param>
        /// <param name="hashedPassword">
        /// The hashed password.
        /// </param>
        /// <param name="securityStamp">
        /// The new security stamp.
        /// </param>
        /// <param name="time">
        /// The date and time of the update.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        /// <exception cref="NotFoundException">
        /// No matching user was found.
        /// </exception>
        Task UpdatePassword(
            DbConnection connection,
            long userId,
            string hashedPassword,
            string securityStamp,
            DateTime time);

        /// <summary>
        /// Updates a user's preferences.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <param name="userId">
        /// The user ID.
        /// </param>
        /// <param name="timeZone">
        /// The TZ ID of the time zone.
        /// </param>
        /// <param name="time">
        /// The date and time of the update.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        /// <exception cref="NotFoundException">
        /// No matching user was found.
        /// </exception>
        Task UpdatePreferences(
            DbConnection connection, long userId, string timeZone, DateTime time);
    }
}
