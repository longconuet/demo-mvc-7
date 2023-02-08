using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication7.Models
{
    [Table("Courses")]
    public class Course : BaseModel
    {
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public int MaxStudentNum { get; set; }
        public virtual ICollection<CourseStudent> CourseStudents { get; set; }
    }
}
