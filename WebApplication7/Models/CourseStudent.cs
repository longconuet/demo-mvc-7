using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication7.Models
{
    [Table("CourseStudents")]
    public class CourseStudent : BaseModel
    {
        public int CourseId { get; set; }
        public int StudentId { get; set; }
        public Course Course { get; set; }
        public Student Student { get; set; }
    }
}
