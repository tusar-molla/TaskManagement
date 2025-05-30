using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Models
{
        public class TaskItem
        {
            public int Id { get; set; }
            public string? UserId { get; set; }
            [Required(ErrorMessage = "Title is required.")]
            [StringLength(100, ErrorMessage = "Title cannot be longer than 100 characters.")]
            public string Title { get; set; }
            [StringLength(500, ErrorMessage = "Description cannot be longer than 500 characters.")]
            public string? Description { get; set; }
            public bool IsCompleted { get; set; }
            [DataType(DataType.Date)]
            public DateTime? DueDate { get; set; }
        }
    }
