using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkoutPlanner.Models
{
    public class User
    {
        /// <summary>
        /// Unique identifier for the user
        /// </summary>
        public int Id {get; set;}

        /// <summary>
        /// Users chosen username for login
        /// </summary>
        public string Username {get; set;} = string.Empty;
        
        /// <summary>
        /// Hashed version of user's password
        /// </summary>
        public string PasswordHash {get; set;} = string.Empty;

        /// <summary>
        /// Timestamp of when the user account was created
        /// </summary>
        public DateTime CreatedAt {get; set;}
    }
}