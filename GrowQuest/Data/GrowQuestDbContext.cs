using GrowQuest.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GrowQuest.Data
{
    public class GrowQuestDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public GrowQuestDbContext(
            DbContextOptions<GrowQuestDbContext> options)
            : base(options)
        {
        }


        public DbSet<Mission> Missions { get; set; }

        public DbSet<GrowthItem> GrowthItems { get; set; }
    }
}