using GrowQuest.Data;
using GrowQuest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrowQuest.Controllers
{
    public class MissionsController : Controller
    {
        private readonly GrowQuestDbContext _context;

        public MissionsController(GrowQuestDbContext context)
        {
            _context = context;
        }

        // GET: Missions
        public async Task<IActionResult> Index()
        {
            var missions = await _context.Missions
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();

            return View(missions);
        }

        // GET: Missions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mission = await _context.Missions
                .FirstOrDefaultAsync(m => m.MissionId == id);

            if (mission == null)
            {
                return NotFound();
            }

            return View(mission);
        }

        // GET: Missions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Missions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Description,Difficulty")] Mission mission)
        {
            if (ModelState.IsValid)
            {
                mission.CreatedDate = DateTime.Now;
                mission.IsCompleted = false;
                mission.CompletedDate = null;

                _context.Add(mission);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(mission);
        }

        // GET: Missions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mission = await _context.Missions.FindAsync(id);

            if (mission == null)
            {
                return NotFound();
            }

            return View(mission);
        }

        // POST: Missions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MissionId,Title,Description,Difficulty")] Mission editedMission)
        {
            if (id != editedMission.MissionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMission = await _context.Missions
                        .FirstOrDefaultAsync(m => m.MissionId == id);

                    if (existingMission == null)
                    {
                        return NotFound();
                    }

                    existingMission.Title = editedMission.Title;
                    existingMission.Description = editedMission.Description;
                    existingMission.Difficulty = editedMission.Difficulty;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MissionExists(editedMission.MissionId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(editedMission);
        }

        // GET: Missions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mission = await _context.Missions
                .FirstOrDefaultAsync(m => m.MissionId == id);

            if (mission == null)
            {
                return NotFound();
            }

            return View(mission);
        }

        // POST: Missions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var mission = await _context.Missions.FindAsync(id);

            if (mission != null)
            {
                _context.Missions.Remove(mission);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Missions/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var mission = await _context.Missions.FindAsync(id);

            if (mission == null)
            {
                return NotFound();
            }

            // Prevent the same mission from giving points more than once
            if (!mission.IsCompleted)
            {
                mission.IsCompleted = true;
                mission.CompletedDate = DateTime.Now;

                int pointsEarned = mission.Difficulty switch
                {
                    "Easy" => 10,
                    "Medium" => 20,
                    "Hard" => 30,
                    _ => 10
                };

                var growthItem = await _context.GrowthItems
                    .FirstOrDefaultAsync();

                if (growthItem == null)
                {
                    growthItem = new GrowthItem
                    {
                        Name = "GrowQuest Plant",
                        CurrentStage = 1,
                        ProgressPoints = 0
                    };

                    _context.GrowthItems.Add(growthItem);
                }

                growthItem.ProgressPoints += pointsEarned;

                if (growthItem.ProgressPoints >= 120)
                {
                    growthItem.CurrentStage = 4;
                }
                else if (growthItem.ProgressPoints >= 70)
                {
                    growthItem.CurrentStage = 3;
                }
                else if (growthItem.ProgressPoints >= 30)
                {
                    growthItem.CurrentStage = 2;
                }
                else
                {
                    growthItem.CurrentStage = 1;
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MissionExists(int id)
        {
            return _context.Missions.Any(e => e.MissionId == id);
        }
    }
}