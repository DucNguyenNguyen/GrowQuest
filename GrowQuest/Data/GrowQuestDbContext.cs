using GrowQuest.Models;
using Microsoft.EntityFrameworkCore;

namespace GrowQuest.Data
{
    public class GrowQuestDbContext : DbContext
    {
        public GrowQuestDbContext(DbContextOptions<GrowQuestDbContext> options)
            : base(options)
        {
        }

        public DbSet<Mission> Missions { get; set; }

        public DbSet<GrowthItem> GrowthItems { get; set; }
    }
}