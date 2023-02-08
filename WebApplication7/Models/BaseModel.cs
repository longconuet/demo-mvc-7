using System.ComponentModel.DataAnnotations;

namespace WebApplication7.Models
{
    public class BaseModel
    {
        [Key]
        public int Id { get; set; }
        public int IsDeleted { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
