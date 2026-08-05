namespace GrowQuest.Models
{
    public class GrowthItem
    {
        public int GrowthItemId { get; set; }

        public string Name { get; set; } = "GrowQuest Plant";

        public int CurrentStage { get; set; } = 1;

        public int ProgressPoints { get; set; } = 0;
    }
}