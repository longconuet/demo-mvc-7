﻿namespace WebApplication7.ModelViews
{
    public class StudentModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public int Age { get; set; }
        public string Code { get; set; } = "";
        public string? Address { get; set; }
        public List<SimpleCourseModel> Courses { get; set; } = new List<SimpleCourseModel>();
    }
}
