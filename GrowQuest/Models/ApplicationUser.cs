using Microsoft.AspNetCore.Identity;

namespace GrowQuest.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; } = string.Empty;

        public ICollection<Mission> Missions { get; set; }
            = new List<Mission>();

        public ICollection<GrowthItem> GrowthItems { get; set; }
            = new List<GrowthItem>();
    }
}