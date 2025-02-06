using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WorkoutPlanner.Views.Workout
{
    public class History : PageModel
    {
        private readonly ILogger<History> _logger;

        public History(ILogger<History> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}