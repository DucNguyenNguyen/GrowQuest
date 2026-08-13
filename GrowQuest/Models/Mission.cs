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

        // The day this mission is scheduled for.
        // Nullable so old database records still work.
        [DataType(DataType.Date)]
        public DateTime? MissionDate { get; set; }

        // Exact time the mission record was created.
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? CompletedDate { get; set; }
    }
}