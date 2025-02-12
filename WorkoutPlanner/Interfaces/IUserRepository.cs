using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkoutPlanner.Models;

namespace WorkoutPlanner.Interfaces
{
    /// <summary>
    /// Defines contract for user data operations
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Creates a new user in the system
        /// </summary>
        /// <param name="user">User to create</param>
        /// <returns>Created user with assigned ID</returns>
        Task<User> CreateUserAsync(User user);

        /// <summary>
        /// Retieves user by their username
        /// </summary>
        /// <param name="username">Username to search for</param>
        /// <returns>User if found, null if not found</returns>
        Task<User> GetUserByUsernameAsync(string username);
    }
}