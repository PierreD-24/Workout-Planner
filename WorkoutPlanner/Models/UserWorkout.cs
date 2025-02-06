using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkoutPlanner.Models
{
    public class UserWorkout
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int WorkoutId { get; set; }
        public DateTime CreatedAt { get; set; }

        public User? User { get; set; }
        public Workout? Workout { get; set; }
    }
}