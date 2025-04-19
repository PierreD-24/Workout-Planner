using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;
using WorkoutPlanner.Interfaces;
using WorkoutPlanner.Models;

namespace WorkoutPlanner.Repositories
{
    public class ProgressRepository : IProgressRepository
    {
        private readonly MySqlConnection _connection;

        public ProgressRepository(MySqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<IEnumerable<Workout>> GetUserWorkoutHistoryListAsync(int userId)
        {
            string query = @"
                SELECT DISTINCT w.*
                FROM Workouts w
                JOIN WorkoutHistory h ON w.Id = h.WorkoutId
                WHERE h.UserId = @UserId
                ORDER BY w.Name";

            return await _connection.QueryAsync<Workout>(query, new { UserId = userId });
        }

        public async Task<IEnumerable<WorkoutHistory>> GetWorkoutProgressAsync(int userId, int workoutId)
        {
            string query = @"
                SELECT h.*, w.Name as WorkoutName 
                FROM WorkoutHistory h
                JOIN Workouts w ON h.WorkoutId = w.Id
                WHERE h.UserId = @UserId 
                AND h.WorkoutId = @WorkoutId
                ORDER BY h.Date ASC";

            return await _connection.QueryAsync<WorkoutHistory>(query, new { UserId = userId, WorkoutId = workoutId });
        }
    }
}