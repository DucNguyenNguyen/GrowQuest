namespace GrowQuest.Models
{
    public class GrowthItem
    {
        public int GrowthItemId { get; set; }


        public string Name { get; set; }
            = "GrowQuest Plant";


        public int CurrentStage { get; set; }
            = 1;


        public int ProgressPoints { get; set; }
            = 0;


        // User who owns this plant
        public string? UserId { get; set; }


        public ApplicationUser? User { get; set; }
    }
}