using System.ComponentModel.DataAnnotations;

namespace GrowQuest.Models
{
    public class Mission
    {
        public int MissionId { get; set; }


        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;


        [StringLength(500)]
        public string? Description { get; set; }


        [Required]
        public string Difficulty { get; set; } = "Easy";


        public bool IsCompleted { get; set; } = false;


        [DataType(DataType.Date)]
        public DateTime? MissionDate { get; set; }


        public DateTime CreatedDate { get; set; }
            = DateTime.Now;


        public DateTime? CompletedDate { get; set; }


        // User who owns this mission
        public string? UserId { get; set; }


        public ApplicationUser? User { get; set; }
    }
}