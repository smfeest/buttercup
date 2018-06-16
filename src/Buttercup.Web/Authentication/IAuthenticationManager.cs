using System.Threading.Tasks;
using Buttercup.Models;

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
    }
}
