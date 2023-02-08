﻿using System.ComponentModel.DataAnnotations;

namespace WebApplication7.Requests
{
    public class EnrollStudentToCourseRequest
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public List<int> StudentIds { get; set; } = new List<int>();
    }
}
