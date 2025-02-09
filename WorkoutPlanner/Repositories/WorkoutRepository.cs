using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkoutPlanner.Models;


namespace WorkoutPlanner.Repositories
{
    /// <summary>
    /// Repository handling all database operations for workouts, including tracking, history, and user assistance
    /// </summary>
    public class WorkoutRepository
    {
        private readonly MySqlConnection _connection;

        /// <summary>
        /// Initializes repository with database connection
        /// </summary>
        /// <param name="connection">MySQL database connection</param>
        /// <exception cref="ArgumentNullException">Thrown when connection is null</exception>
        public WorkoutRepository(MySqlConnection connection)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        // Method to get all workouts from the database
        public async Task<IEnumerable<Workout>> GetAllWorkoutsAsync()
        {
            string query = "SELECT * FROM Workouts";
            return await _connection.QueryAsync<Workout>(query);
        }

        // Get a specific workout by Id
        public async Task<Workout> GetWorkoutByIdAsync(int id)
        {
            string query = "SELECT * FROM Workouts WHERE Id = @Id";
            return await _connection.QuerySingleOrDefaultAsync<Workout>(query, new { id = id });
        }

        // Update workout details
        public async Task UpdateWorkoutAsync(Workout workout)
        {
            string query = @"UPDATE Workouts 
                             SET Name = @Name, Day = @Day, Sets = @Sets, Reps = @Reps, Weight = @Weight 
                             WHERE Id = @Id";
            await _connection.ExecuteAsync(query, workout);
        }

        // Log a new workout
        public async Task<int> LogWorkoutAsync(Workout workout)
        {
            string query = @"
                            INSERT INTO Workouts (Name, Sets, Reps, Weight, Day)
                            VALUES (@Name, @Sets, @Reps, @Weight, @Day);
                            SELECT LAST_INSERT_ID();";
            return await _connection.QuerySingleAsync<int>(query, workout);
        }


        // Delete a workout by Id
        public async Task DeleteWorkoutAsync(int id)
        {
            string query = "DELETE FROM Workouts WHERE Id = @Id";
            await _connection.ExecuteAsync(query, new { Id = id });
        }

        // Get workouts by a specific date
        public async Task<IEnumerable<Workout>> GetWorkoutByDayAsync(DateTime day)
        {
            string query = "SELECT * FROM Workouts WHERE Day = @Day";
            return await _connection.QueryAsync<Workout>(query, new { Day = day });
        }

        // Add a workout and associate it with a user
        public async Task AddWorkoutAsync(Workout workout, int userId)
        {
            string query = @"INSERT INTO UserWorkouts (WorkoutId, UserId) 
                     VALUES (@WorkoutId, @UserId)
                     ON DUPLICATE KEY UPDATE WorkoutId = @WorkoutId";
            await _connection.ExecuteAsync(query, new { WorkoutId = workout.Id, UserId = userId });
        }


        public async Task<IEnumerable<Workout>> GetTodaysWorkoutsAsync(int userId)
        {
            string query = @"
                            SELECT w.* 
                            FROM Workouts w
                            JOIN UserWorkouts uw ON w.Id = uw.WorkoutId
                            WHERE uw.UserId = @UserId";
            return await _connection.QueryAsync<Workout>(query, new { UserId = userId });
        }


        // Get all workouts by a specific user
        public async Task<IEnumerable<Workout>> GetWorkoutsByUserIdAsync(int userId)
        {
            string query = "SELECT * FROM Workouts WHERE UserId = @UserId";
            return await _connection.QueryAsync<Workout>(query, new { UserId = userId });
        }

        public async Task RemoveFromTodaysWorkoutsAsync(int userId, int workoutId)
        {
            string query = "DELETE FROM UserWorkouts WHERE UserId = @UserId AND WorkoutId = @WorkoutId";
            await _connection.ExecuteAsync(query, new { UserId = userId, WorkoutId = workoutId });
        }

        public async Task LogWorkoutHistoryAsync(WorkoutHistory history)
        {
            string query = @"
                            INSERT INTO WorkoutHistory (WorkoutId, UserId, Date, SetsCompleted, RepsCompleted, WeightUsed, Notes)
                            VALUES (@WorkoutId, @UserId, @Date, @SetsCompleted, @RepsCompleted, @WeightUsed, @Notes)";
            await _connection.ExecuteAsync(query, history);
        }

        public async Task<IEnumerable<WorkoutHistory>> GetWorkoutHistoryAsync(int userId)
        {
            string query = @"
                            SELECT h.*, w.Name as WorkoutName 
                            FROM WorkoutHistory h
                            JOIN Workouts w ON h.WorkoutId = w.Id
                            WHERE h.UserId = @UserId
                            ORDER BY h.Date DESC";
            return await _connection.QueryAsync<WorkoutHistory>(query, new { UserId = userId });
        }

        /// <summary>
        /// Retrieves workout history for a specific date
        /// </summary>
        /// <param name="userId">ID of the user</param>
        /// <param name="date">Date to retieve history for</param>
        /// <returns>Collection of workout history entries with associated workout details</returns>
        public async Task<IEnumerable<WorkoutHistory>> GetWorkoutHistoryByDateAsync(int userId, DateTime date)
        {
            string query = @"
                            SELECT h.*, w.*
                            FROM WorkoutHistory h
                            JOIN Workouts w ON h.WorkoutId = w.Id
                            WHERE h.UserId = @UserId 
                            AND DATE(h.Date) = DATE(@Date)
                            ORDER BY h.Date";

            //Dictionary to prevent duplicate entries
            var workoutDictionary = new Dictionary<int, WorkoutHistory>();

            var result = await _connection.QueryAsync<WorkoutHistory, Workout, WorkoutHistory>(
                query,
                (history, workout) =>
                {
                    if (!workoutDictionary.TryGetValue(history.Id, out WorkoutHistory workoutEntry))
                    {
                        workoutEntry = history;
                        workoutEntry.Workout = workout;
                        workoutDictionary.Add(history.Id, workoutEntry);
                    }
                    return workoutEntry;
                },
                splitOn: "Id",
                param: new { UserId = userId, Date = date }
            );

            return workoutDictionary.Values;
        }

        public async Task DeleteWorkoutHistoryForDateAsync(int userId, DateTime date)
        {
            string query = @"
        DELETE FROM WorkoutHistory 
        WHERE UserId = @UserId 
        AND DATE(Date) = DATE(@Date)";

            await _connection.ExecuteAsync(query, new { UserId = userId, Date = date });
        }

        public async Task<IEnumerable<Workout>> SearchWorkoutAsync(string searchString)
        {
            string query = @"
                SELECT *
                FROM Workouts
                WHERE Name LIKE CONCAT('%', @SearchString, '%')";

            return await _connection.QueryAsync<Workout>(
                query,
                new { SearchString = searchString ?? "" }
            );
        }

    }
}
