using Microsoft.AspNetCore.Mvc;
using WorkoutPlanner.Models;
using WorkoutPlanner.Repositories;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Claims;
using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.AspNetCore.Authorization;


namespace WorkoutPlanner.Controllers
{
    [Authorize]
    public class WorkoutController : Controller
    {
        private readonly WorkoutRepository _workoutRepository;

        /// <summary>
        /// Constructor for WorkoutController, initializes workout repository
        /// </summary>
        /// <param name="workoutRepository">Repository for handling workout data operations</param>

        public WorkoutController(WorkoutRepository workoutRepository)
        {
            _workoutRepository = workoutRepository;
        }


        /// <summary>
        /// Displays List of workouts with optional search functionality
        /// </summary>
        /// <param name="searchString">Optional search term to filter workouts</param>
        /// <returns>View containing filtered or all workouts</returns>
        public async Task<IActionResult> Index(string? searchString = null)
        {
            ViewData["CurrentFilter"] = searchString;

            var workouts = string.IsNullOrEmpty(searchString)
                ? await _workoutRepository.GetAllWorkoutsAsync()
                : await _workoutRepository.SearchWorkoutAsync(searchString);

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

        /// <summary>
        /// Displays edit form for workout with option to return to previous page
        /// </summary>
        /// <param name="id">ID of workout to edit</param>
        /// <param name="returnUrl">URL to return to after edit</param>
        /// <returns>Edit view for specified workout</returns>
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

        /// <summary>
        /// Processes the workout edit form submission
        /// </summary>
        /// <param name="id">ID of workout to edit</param>
        /// <param name="workout">Updated workout data</param>
        /// <param name="returnUrl">URL to return to after edit</param>
        /// <returns>Redirects to return URL or Today's Workouts if successful</returns>
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Workout workout, string returnUrl)
        {
            // Verify the ID in route matches the workout object
            if (id != workout.Id)
            {
                return BadRequest("Workout ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                // Log validation errors for debugging
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }

                // Return to edit form with current values if validation fails
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
            var result = await _workoutRepository.DeleteWorkoutAsync(id);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Shows today's selected workouts for logged in user
        /// </summary>
        /// <returns>View containing user's workouts for today</returns>
        public async Task<IActionResult> TodaysWorkouts()
        {
            var userId = GetLoggedInUserId();
            var workouts = await _workoutRepository.GetTodaysWorkoutsAsync(userId);
            return View(workouts);
        }


        /// <summary>
        /// Adds a workout to user's today's workout list
        /// </summary>
        /// <param name="workoutId">ID of workout to add</param>
        /// <returns>Redirects to Today's Workouts page</returns>
        [HttpPost]
        public async Task<IActionResult> AddToTodaysWorkouts(int workoutId)
        {
            // Get user ID from session, redirect to login if not found
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            // Verify workout exists before adding
            var workout = await _workoutRepository.GetWorkoutByIdAsync(workoutId);

            if (workout != null)
            {
                // Add to database instead of session
                await _workoutRepository.AddWorkoutAsync(workout, int.Parse(userId));
                return RedirectToAction("TodaysWorkouts");
            }

            return NotFound(); // Return 404 if workout doesn't exist
        }


        /// <summary>
        /// Removes a workout from user's today's workout list without deleting it from the database
        /// </summary>
        /// <param name="id">ID of workout to remove</param>
        /// <returns>Redirects to Today's Workouts page</returns>
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


        /// <summary>
        /// Saves current workout routine to workout history
        /// </summary>
        /// <returns>Redirects to History page after saving</returns>
        [HttpPost]
        public async Task<IActionResult> SaveWorkoutHistory()
        {
            try
            {
                var userId = GetLoggedInUserId();
                //Get all workouts currently in user's today's workout list
                var todaysWorkouts = await _workoutRepository.GetTodaysWorkoutsAsync(userId);
                var today = DateTime.Now;

                // Delete any existing history entries for today to prevent duplicates
                await _workoutRepository.DeleteWorkoutHistoryForDateAsync(userId, today);

                // Create history entries for each workout in today's list
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
                // Log error and redirect to prevent user from seeing error in details
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

