using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkoutPlanner.Models;

namespace WorkoutPlanner.Interfaces
{
    public interface IProgressRepository
    {
        Task<IEnumerable<WorkoutHistory>> GetWorkoutProgressAsync(int userId, int workoutId);
        Task <IEnumerable<Workout>> GetUserWorkoutHistoryListAsync(int userId);
    }
}