namespace GrowQuest.Models
{
    public class MissionPlanningViewModel
    {
        public List<Mission> UpcomingMissions { get; set; }
            = new List<Mission>();

        public List<Mission> OverdueMissions { get; set; }
            = new List<Mission>();
    }
}