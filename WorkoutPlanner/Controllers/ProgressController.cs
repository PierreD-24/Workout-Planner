using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WorkoutPlanner.Repositories;
using WorkoutPlanner.Models;
using System.Security.Claims;

namespace WorkoutPlanner.Controllers
{
    public class ProgressController : Controller
    {
        private readonly ProgressRepository _progressRepository;

        public ProgressController(ProgressRepository progressRepository)
        {
            _progressRepository = progressRepository;
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

        public async Task<IActionResult> Index(int? workoutId)
        {
            var userId = GetLoggedInUserId();

            var workouts = await _progressRepository.GetUserWorkoutHistoryListAsync(userId);

            if (workoutId.HasValue)
            {
                var progressData = await _progressRepository.GetWorkoutProgressAsync(userId, workoutId.Value);
                ViewData["ProgressData"] = progressData;

                var selectedWorkout = workouts.FirstOrDefault(w => w.Id == workoutId.Value);
                ViewData["SelectedWorkoutName"] = selectedWorkout?.Name;
            }

            return View(workouts);
        }
    }
}