using System.Threading.Tasks;
using Buttercup.Models;
using Microsoft.AspNetCore.Http;

namespace Buttercup.Web.Authentication
{
    /// <summary>
    /// Defines the contract for the authentication manager.
    /// </summary>
    public interface IAuthenticationManager
    {
        /// <summary>
        /// Authenticate a user with an email address and password.
        /// </summary>
        /// <param name="email">
        /// The email address.
        /// </param>
        /// <param name="password">
        /// The password.
        /// </param>
        /// <returns>
        /// A task for the operation. The result is the user if successfully authenticated, or a
        /// null reference otherwise.
        /// </returns>
        Task<User> Authenticate(string email, string password);

        /// <summary>
        /// Validates a password reset token.
        /// </summary>
        /// <param name="token">
        /// The password reset token.
        /// </param>
        /// <returns>
        /// A task for the operation. The result is <b>true</b> if the token is valid, <b>false</b>
        /// if it isn't.
        /// </returns>
        Task<bool> PasswordResetTokenIsValid(string token);

        /// <summary>
        /// Sends a password reset link to the user with a given email address.
        /// </summary>
        /// <remarks>
        /// <para>
        /// No email is sent if there is no user with the specified email address.
        /// </para>
        /// <para>
        /// To reduce the risk of revealing the existence of a matching user, any exception raised
        /// while sending the email is caught, logged and not rethrown.
        /// </para>
        /// </remarks>
        /// <param name="email">
        /// The email address.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        Task SendPasswordResetLink(string email);

        /// <summary>
        /// Signs in a user.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context for the current request.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        Task SignIn(HttpContext httpContext, User user);

        /// <summary>
        /// Signs out the current user.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context for the current request.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        Task SignOut(HttpContext httpContext);
    }
}
