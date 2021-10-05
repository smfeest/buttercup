using Buttercup.Models;
using Microsoft.AspNetCore.Http;

namespace Buttercup.Web.Authentication
{
    /// <summary>
    /// Provides extension methods for <see cref="HttpContext" />.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the current user for a request.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context for the request.
        /// </param>
        /// <returns>
        /// The current user.
        /// </returns>
        public static User? GetCurrentUser(this HttpContext httpContext) =>
            httpContext.Items.TryGetValue(typeof(User), out var user) ? (User?)user : null;

        /// <summary>
        /// Sets the current user for a request.
        /// </summary>
        /// <param name="httpContext">
        /// The HTTP context for the request.
        /// </param>
        /// <param name="user">
        /// The current user.
        /// </param>
        public static void SetCurrentUser(this HttpContext httpContext, User user) =>
            httpContext.Items[typeof(User)] = user;
    }
}
