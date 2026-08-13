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
        // DASHBOARD - TODAY
        // =========================================

        public async Task<IActionResult> Index()
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            var missions = await _context.Missions
                .Where(m =>
                    (
                        m.MissionDate.HasValue &&
                        m.MissionDate.Value >= today &&
                        m.MissionDate.Value < tomorrow
                    )
                    ||
                    (
                        !m.MissionDate.HasValue &&
                        m.CreatedDate >= today &&
                        m.CreatedDate < tomorrow
                    )
                )
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
                // Recalculate stage from XP.
                // This also upgrades existing users to the
                // new 10-level system automatically.
                UpdateGrowthStage(growthItem);

                await _context.SaveChangesAsync();

                ViewBag.ProgressPoints =
                    growthItem.ProgressPoints;

                ViewBag.CurrentStage =
                    growthItem.CurrentStage;

                ViewBag.GrowthName =
                    growthItem.Name;
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
        // UPCOMING + OVERDUE
        // =========================================

        public async Task<IActionResult> Upcoming()
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            var allMissions = await _context.Missions
                .Where(m => !m.IsCompleted)
                .ToListAsync();

            var upcomingMissions = allMissions
                .Where(m =>
                {
                    DateTime missionDate =
                        (m.MissionDate ?? m.CreatedDate).Date;

                    return missionDate >= tomorrow;
                })
                .OrderBy(m =>
                    (m.MissionDate ?? m.CreatedDate).Date)
                .ThenBy(m => m.CreatedDate)
                .ToList();

            var overdueMissions = allMissions
                .Where(m =>
                {
                    DateTime missionDate =
                        (m.MissionDate ?? m.CreatedDate).Date;

                    return missionDate < today;
                })
                .OrderBy(m =>
                    (m.MissionDate ?? m.CreatedDate).Date)
                .ThenBy(m => m.CreatedDate)
                .ToList();

            var viewModel = new MissionPlanningViewModel
            {
                UpcomingMissions = upcomingMissions,
                OverdueMissions = overdueMissions
            };

            return View(viewModel);
        }


        // =========================================
        // HISTORY
        // =========================================

        public async Task<IActionResult> History()
        {
            DateTime today = DateTime.Today;

            var allCompletedMissions = await _context.Missions
                .Where(m => m.IsCompleted)
                .ToListAsync();

            var missions = allCompletedMissions
                .Where(m =>
                {
                    DateTime missionDate =
                        (m.MissionDate ?? m.CreatedDate).Date;

                    return missionDate < today;
                })
                .OrderByDescending(m =>
                    (m.MissionDate ?? m.CreatedDate).Date)
                .ThenByDescending(m => m.CompletedDate)
                .ToList();

            ViewBag.TotalMissions =
                missions.Count;

            ViewBag.CompletedMissions =
                missions.Count;

            return View(missions);
        }


        // =========================================
        // DETAILS
        // =========================================

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

        public IActionResult Create()
        {
            var mission = new Mission
            {
                MissionDate = DateTime.Today
            };

            return View(mission);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Title,Description,Difficulty,MissionDate")]
            Mission mission)
        {
            if (!mission.MissionDate.HasValue)
            {
                mission.MissionDate =
                    DateTime.Today;
            }

            if (ModelState.IsValid)
            {
                mission.MissionDate =
                    mission.MissionDate.Value.Date;

                mission.CreatedDate =
                    DateTime.Now;

                mission.IsCompleted =
                    false;

                mission.CompletedDate =
                    null;

                _context.Missions.Add(mission);

                await _context.SaveChangesAsync();

                DateTime scheduledDate =
                    mission.MissionDate.Value.Date;

                if (scheduledDate > DateTime.Today)
                {
                    TempData["SuccessMessage"] =
                        "Future mission created successfully.";

                    return RedirectToAction(
                        nameof(Upcoming));
                }

                if (scheduledDate < DateTime.Today)
                {
                    TempData["SuccessMessage"] =
                        "Past-dated mission created successfully.";

                    return RedirectToAction(
                        nameof(Upcoming));
                }

                TempData["SuccessMessage"] =
                    "Mission created successfully.";

                return RedirectToAction(
                    nameof(Index));
            }

            return View(mission);
        }


        // =========================================
        // EDIT
        // =========================================

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
                return RedirectToAction(
                    nameof(Index));
            }

            if (!mission.MissionDate.HasValue)
            {
                mission.MissionDate =
                    mission.CreatedDate.Date;
            }

            return View(mission);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MissionId,Title,Description,Difficulty,MissionDate")]
            Mission editedMission)
        {
            if (id != editedMission.MissionId)
            {
                return NotFound();
            }

            if (!editedMission.MissionDate.HasValue)
            {
                editedMission.MissionDate =
                    DateTime.Today;
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
                        return RedirectToAction(
                            nameof(Index));
                    }

                    existingMission.Title =
                        editedMission.Title;

                    existingMission.Description =
                        editedMission.Description;

                    existingMission.Difficulty =
                        editedMission.Difficulty;

                    existingMission.MissionDate =
                        editedMission.MissionDate.Value.Date;

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

                DateTime savedDate =
                    editedMission.MissionDate.Value.Date;

                if (savedDate != DateTime.Today)
                {
                    return RedirectToAction(
                        nameof(Upcoming));
                }

                return RedirectToAction(
                    nameof(Index));
            }

            return View(editedMission);
        }


        // =========================================
        // DELETE
        // =========================================

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


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var mission = await _context.Missions
                .FindAsync(id);

            if (mission == null)
            {
                return NotFound();
            }

            // XP is lifetime earned progress.
            // Deleting a mission does not remove XP.
            _context.Missions.Remove(mission);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // COMPLETE MISSION
        // =========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(
            int id)
        {
            var mission = await _context.Missions
                .FindAsync(id);

            if (mission == null)
            {
                return NotFound();
            }

            // Prevent duplicate XP
            if (!mission.IsCompleted)
            {
                mission.IsCompleted = true;

                mission.CompletedDate =
                    DateTime.Now;

                int pointsEarned =
                    GetMissionPoints(
                        mission.Difficulty);

                var growthItem =
                    await _context.GrowthItems
                        .FirstOrDefaultAsync();

                if (growthItem == null)
                {
                    growthItem =
                        new GrowthItem
                        {
                            Name =
                                "GrowQuest Plant",

                            CurrentStage =
                                1,

                            ProgressPoints =
                                0
                        };

                    _context.GrowthItems.Add(
                        growthItem);
                }

                growthItem.ProgressPoints +=
                    pointsEarned;

                UpdateGrowthStage(
                    growthItem);

                await _context.SaveChangesAsync();
            }

            DateTime effectiveDate =
                (mission.MissionDate ??
                 mission.CreatedDate).Date;

            if (effectiveDate < DateTime.Today)
            {
                return RedirectToAction(
                    nameof(Upcoming));
            }

            return RedirectToAction(
                nameof(Index));
        }


        // =========================================
        // XP REWARD
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


        // =========================================
        // 10 LEVEL GROWTH SYSTEM
        // =========================================

        private void UpdateGrowthStage(
            GrowthItem growthItem)
        {
            int xp =
                growthItem.ProgressPoints;

            if (xp >= 1200)
            {
                growthItem.CurrentStage = 10;
            }
            else if (xp >= 900)
            {
                growthItem.CurrentStage = 9;
            }
            else if (xp >= 650)
            {
                growthItem.CurrentStage = 8;
            }
            else if (xp >= 450)
            {
                growthItem.CurrentStage = 7;
            }
            else if (xp >= 300)
            {
                growthItem.CurrentStage = 6;
            }
            else if (xp >= 200)
            {
                growthItem.CurrentStage = 5;
            }
            else if (xp >= 120)
            {
                growthItem.CurrentStage = 4;
            }
            else if (xp >= 70)
            {
                growthItem.CurrentStage = 3;
            }
            else if (xp >= 30)
            {
                growthItem.CurrentStage = 2;
            }
            else
            {
                growthItem.CurrentStage = 1;
            }
        }


        // =========================================
        // CHECK
        // =========================================

        private bool MissionExists(int id)
        {
            return _context.Missions
                .Any(e => e.MissionId == id);
        }
    }
}