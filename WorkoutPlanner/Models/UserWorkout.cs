using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkoutPlanner.Models
{
    /// <summary>
    /// Represents a completed workout in user's history
    /// </summary>
    public class UserWorkout
    {
        /// <summary>
        /// Unique identifier for the history entry
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User who completed the workout
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Reference to the workout that was completed
        /// </summary>
        public int WorkoutId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
        public Workout? Workout { get; set; }
    }
}