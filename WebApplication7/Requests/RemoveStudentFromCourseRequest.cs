using System.ComponentModel.DataAnnotations;

namespace WebApplication7.Requests
{
    public class RemoveStudentFromCourseRequest
    {
        [Required]
        public int CourseId { get; set; }

        [Required]
        public int StudentId { get; set; }
    }
}
