namespace WorkoutPlanner.Models
{
    public class Workout
    {
        public int Id { get; set; }
        public DateTime Day { get; set; } // Date of the workout
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int Sets { get; set; }
        public int Reps { get; set; }
        public int Weight { get; set; }

         // New field to track the user who created the workout
        public int UserId { get; set; }
        
        // Navigation property to link workout to the user
        public User? User { get; set; }
    }
}