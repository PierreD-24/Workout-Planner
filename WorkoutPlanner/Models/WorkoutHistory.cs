using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkoutPlanner.Models
{
    public class WorkoutHistory
    {
    public int Id { get; set; }
    public int WorkoutId { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public int SetsCompleted { get; set; }
    public int RepsCompleted { get; set; }
    public decimal WeightUsed { get; set; }
    public string Notes { get; set; }
    
    // Navigation property
    public Workout Workout { get; set; }
    }
}