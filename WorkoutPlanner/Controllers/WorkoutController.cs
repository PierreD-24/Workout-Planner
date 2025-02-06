using Microsoft.AspNetCore.Mvc;
using WorkoutPlanner.Models;
using WorkoutPlanner.Repositories;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Claims;
using MySql.Data.MySqlClient;
using System.Data;


namespace WorkoutPlanner.Controllers
{
    public class WorkoutController : Controller
    {
        private readonly WorkoutRepository _workoutRepository;

        // Constructor to inject the workout repository
        public WorkoutController(WorkoutRepository workoutRepository)
        {
            _workoutRepository = workoutRepository;
        }

        // Action to display all workouts
        public async Task<IActionResult> Index()
        {
            var workouts = await _workoutRepository.GetAllWorkoutsAsync();
            return View(workouts);
        }

        // Action to show form for adding a new workout
        public IActionResult Create()
        {
            return View(new Workout()); // Returns empty form
        }

        [HttpPost]
        public async Task<IActionResult> Create(Workout workout)
        {
            if (workout == null)
            {
                return View("Error");
            }

            var userId = GetLoggedInUserId();
            workout.UserId = userId;

            if (ModelState.IsValid)
            {

                int workoutId = await _workoutRepository.LogWorkoutAsync(workout);

                await _workoutRepository.AddWorkoutAsync(new Workout { Id = workoutId }, userId);

                return RedirectToAction("Index");
            }

            return View(workout);
        }

        // GET workout
        public async Task<IActionResult> Edit(int id, string returnUrl)
        {
            var workout = await _workoutRepository.GetWorkoutByIdAsync(id);
            if (workout == null)
            {
                return NotFound();
            }

            ViewData["ReturnUrl"] = returnUrl ?? Url.Action("Index");
            return View(workout);
        }

        // POST Workout
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Workout workout, string returnUrl)
        {
            if (id != workout.Id)
            {
                return BadRequest("Workout ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }

                ViewData["ReturnUrl"] = returnUrl ?? Url.Action("Index");
                return View(workout);
            }

            Console.WriteLine($"Updating workout: {workout.Id}, {workout.Name}");
            await _workoutRepository.UpdateWorkoutAsync(workout);

            // Redirect to the appropriate page
            return Redirect(returnUrl ?? Url.Action("TodaysWorkouts"));
        }


        public async Task<IActionResult> Log()
        {
            var workouts = await _workoutRepository.GetAllWorkoutsAsync();
            // Get only the Name properties from the workouts
            ViewBag.WorkoutNames = workouts.Select(w => w.Name).ToList();
            return View(new Workout());  // Pass an empty workout object to the view
        }

        [HttpPost]
        public async Task<IActionResult> LogWorkout(Workout workout)
        {
            if (ModelState.IsValid)
            {
                await _workoutRepository.LogWorkoutAsync(workout);
                return RedirectToAction("Index");
            }
            return View(workout);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var workout = await _workoutRepository.GetWorkoutByIdAsync(id);
            if (workout == null)
            {
                return NotFound();
            }
            return View(workout);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _workoutRepository.DeleteWorkoutAsync(id);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> TodaysWorkouts()
        {
            var userId = GetLoggedInUserId();
            var workouts = await _workoutRepository.GetTodaysWorkoutsAsync(userId);
            return View(workouts);
        }


        [HttpPost]
        public async Task<IActionResult> AddToTodaysWorkouts(int workoutId)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            var workout = await _workoutRepository.GetWorkoutByIdAsync(workoutId);

            if (workout != null)
            {
                // Add to database instead of session
                await _workoutRepository.AddWorkoutAsync(workout, int.Parse(userId));
                return RedirectToAction("TodaysWorkouts");
            }

            return NotFound();
        }


        [HttpPost]
        public async Task<IActionResult> RemoveFromTodaysWorkouts(int id)
        {
            try
            {
                var userId = GetLoggedInUserId();
                await _workoutRepository.RemoveFromTodaysWorkoutsAsync(userId, id);
                return RedirectToAction("TodaysWorkouts");
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error removing workout: {ex.Message}");
                return RedirectToAction("TodaysWorkouts");
            }
        }

        private int GetLoggedInUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                userId = HttpContext.Session.GetString("UserId");
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User not logged in.");
            }
            return int.Parse(userId);
        }

        public async Task<IActionResult> History()
        {
            var userId = GetLoggedInUserId();
            var history = await _workoutRepository.GetWorkoutHistoryAsync(userId);
            return View(history);
        }

        [HttpPost]
        public async Task<IActionResult> LogHistory(WorkoutHistory history)
        {
            history.UserId = GetLoggedInUserId();
            history.Date = DateTime.Now;
            await _workoutRepository.LogWorkoutHistoryAsync(history);
            return RedirectToAction("History");
        }

        [HttpPost]
        public async Task<IActionResult> SaveWorkoutHistory()
        {
            try
            {
                var userId = GetLoggedInUserId();
                var todaysWorkouts = await _workoutRepository.GetTodaysWorkoutsAsync(userId);
                var today = DateTime.Now;

                await _workoutRepository.DeleteWorkoutHistoryForDateAsync(userId, today);

                foreach (var workout in todaysWorkouts)
                {
                    var history = new WorkoutHistory
                    {
                        WorkoutId = workout.Id,
                        UserId = userId,
                        Date = today,
                        SetsCompleted = workout.Sets,
                        RepsCompleted = workout.Reps,
                        WeightUsed = workout.Weight
                    };

                    await _workoutRepository.LogWorkoutHistoryAsync(history);
                }

                return RedirectToAction("History");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving workout history: {ex.Message}");
                TempData["Error"] = "There was an error saving your workout history.";
                return RedirectToAction("History");
            }
        }

        public async Task<IActionResult> HistoryDetail(DateTime date)
        {
            var userId = GetLoggedInUserId();
            var workouts = await _workoutRepository.GetWorkoutHistoryByDateAsync(userId, date);
            return View(workouts);
        }

    }
}

