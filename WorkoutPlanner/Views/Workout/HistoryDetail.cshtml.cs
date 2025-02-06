using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WorkoutPlanner.Views.Workout
{
    public class HistoryDetail : PageModel
    {
        private readonly ILogger<HistoryDetail> _logger;

        public HistoryDetail(ILogger<HistoryDetail> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }
    }
}