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


        // =========================================
        // DASHBOARD
        // =========================================

        // GET: Missions
        public async Task<IActionResult> Index()
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            var missions = await _context.Missions
                .Where(m =>
                    m.CreatedDate >= today &&
                    m.CreatedDate < tomorrow)
                .OrderBy(m => m.IsCompleted)
                .ThenByDescending(m => m.CreatedDate)
                .ToListAsync();

            var growthItem = await _context.GrowthItems
                .FirstOrDefaultAsync();

            if (growthItem == null)
            {
                ViewBag.ProgressPoints = 0;
                ViewBag.CurrentStage = 1;
                ViewBag.GrowthName = "GrowQuest Plant";
            }
            else
            {
                ViewBag.ProgressPoints = growthItem.ProgressPoints;
                ViewBag.CurrentStage = growthItem.CurrentStage;
                ViewBag.GrowthName = growthItem.Name;
            }

            ViewBag.TotalToday =
                missions.Count;

            ViewBag.CompletedToday =
                missions.Count(m => m.IsCompleted);

            ViewBag.RemainingToday =
                missions.Count(m => !m.IsCompleted);

            return View(missions);
        }


        // =========================================
        // MISSION HISTORY
        // =========================================

        // GET: Missions/History
        public async Task<IActionResult> History()
        {
            var missions = await _context.Missions
                .OrderByDescending(m => m.CreatedDate)
                .ToListAsync();

            ViewBag.TotalMissions =
                missions.Count;

            ViewBag.CompletedMissions =
                missions.Count(m => m.IsCompleted);

            ViewBag.IncompleteMissions =
                missions.Count(m => !m.IsCompleted);

            return View(missions);
        }


        // =========================================
        // DETAILS
        // =========================================

        // GET: Missions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mission = await _context.Missions
                .FirstOrDefaultAsync(
                    m => m.MissionId == id);

            if (mission == null)
            {
                return NotFound();
            }

            return View(mission);
        }


        // =========================================
        // CREATE
        // =========================================

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

                _context.Missions.Add(mission);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(mission);
        }


        // =========================================
        // EDIT
        // =========================================

        // GET: Missions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mission = await _context.Missions
                .FindAsync(id);

            if (mission == null)
            {
                return NotFound();
            }

            if (mission.IsCompleted)
            {
                return RedirectToAction(nameof(Index));
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
                    var existingMission =
                        await _context.Missions
                            .FirstOrDefaultAsync(
                                m => m.MissionId == id);

                    if (existingMission == null)
                    {
                        return NotFound();
                    }

                    if (existingMission.IsCompleted)
                    {
                        return RedirectToAction(nameof(Index));
                    }

                    existingMission.Title =
                        editedMission.Title;

                    existingMission.Description =
                        editedMission.Description;

                    existingMission.Difficulty =
                        editedMission.Difficulty;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MissionExists(
                        editedMission.MissionId))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(editedMission);
        }


        // =========================================
        // DELETE
        // =========================================

        // GET: Missions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var mission = await _context.Missions
                .FirstOrDefaultAsync(
                    m => m.MissionId == id);

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
            var mission = await _context.Missions
                .FindAsync(id);

            if (mission == null)
            {
                return NotFound();
            }

            // XP represents lifetime earned progress.
            // Deleting a mission does not remove XP.
            _context.Missions.Remove(mission);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // COMPLETE MISSION
        // =========================================

        // POST: Missions/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var mission = await _context.Missions
                .FindAsync(id);

            if (mission == null)
            {
                return NotFound();
            }

            // Only award XP once
            if (!mission.IsCompleted)
            {
                mission.IsCompleted = true;
                mission.CompletedDate = DateTime.Now;

                int pointsEarned =
                    GetMissionPoints(
                        mission.Difficulty);

                var growthItem =
                    await _context.GrowthItems
                        .FirstOrDefaultAsync();

                if (growthItem == null)
                {
                    growthItem = new GrowthItem
                    {
                        Name = "GrowQuest Plant",
                        CurrentStage = 1,
                        ProgressPoints = 0
                    };

                    _context.GrowthItems.Add(
                        growthItem);
                }

                growthItem.ProgressPoints +=
                    pointsEarned;

                UpdateGrowthStage(growthItem);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // XP HELPERS
        // =========================================

        private int GetMissionPoints(
            string difficulty)
        {
            return difficulty switch
            {
                "Easy" => 10,
                "Medium" => 20,
                "Hard" => 30,
                _ => 10
            };
        }


        private void UpdateGrowthStage(
            GrowthItem growthItem)
        {
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
        }


        // =========================================
        // CHECK MISSION
        // =========================================

        private bool MissionExists(int id)
        {
            return _context.Missions
                .Any(
                    e => e.MissionId == id);
        }
    }
}