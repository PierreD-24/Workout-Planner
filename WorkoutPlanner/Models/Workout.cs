namespace WorkoutPlanner.Models
{
    public class Workout
    {
        /// <summary>
        /// Unique identifier for the workout
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Date the workout is scheduled
        /// </summary>
        public DateTime Day { get; set; }

        /// <summary>
        /// Name of the workout
        /// </summary>
        public string? Name { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int Weight { get; set; }

         /// <summary>
         /// New field to track the user who created the workout
         /// </summary>
        public int UserId { get; set; }
        
        /// <summary>
        /// Navigation property to link workout to the user
        /// </summary>
        public User? User { get; set; }
    }
}