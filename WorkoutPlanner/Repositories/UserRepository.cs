using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using WorkoutPlanner.Models;
using Dapper;
using WorkoutPlanner.Interfaces;


namespace WorkoutPlanner.Repositories
{
    /// <summary>
    /// Repository for handling all user related database operations including authentication and user management
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly MySqlConnection _connection;

        /// <summary>
        /// Initializes user repository with database connection
        /// </summary>
        /// <param name="userRepository">MySQL database connection</param>
        public UserRepository(MySqlConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));;
        }

        /// <summary>
        /// Creates a new user in the database
        /// </summary>
        /// <param name="user">User object containing registration information</param>
        /// <returns>The created user with assigned ID</returns>
        public async Task<User> CreateUserAsync(User user)
        {
            string query = @"INSERT INTO Users (Username, PasswordHash, CreatedAt) 
                             VALUES (@Username, @PasswordHash, @CreatedAt)";
            await _connection.ExecuteAsync(query, user);
            return user;
        }

        /// <summary>
        /// Retrieves a user by their username
        /// </summary>
        /// <param name="username">Username to search for</param>
        /// <returns>User if found, null if not found</returns>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            string query = "SELECT * FROM Users WHERE Username = @Username";
            return await _connection.QuerySingleOrDefaultAsync<User>(query, new { Username = username });
        }

    }
}