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
    public class UserRepository : IUserRepository
    {
        private readonly MySqlConnection _connection;

        public UserRepository(MySqlConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            string query = @"INSERT INTO Users (Username, PasswordHash, CreatedAt) 
                             VALUES (@Username, @PasswordHash, @CreatedAt)";
            await _connection.ExecuteAsync(query, user);
            return user;
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            string query = "SELECT * FROM Users WHERE Username = @Username";
            return await _connection.QuerySingleOrDefaultAsync<User>(query, new { Username = username });
        }

    }
}