using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication7.Models
{
    [Table("Students")]
    public class Student : BaseModel
    {
        public string FullName { get; set; } = "";
        public int Age { get; set; }
        public string Code { get; set; } = "";
        public string? Address { get; set; }
        public virtual ICollection<CourseStudent> CourseStudents { get; set; }
    }
}
