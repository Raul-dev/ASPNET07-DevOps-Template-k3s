using ShopManager.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using ShopManager.Extensions;

namespace ShopManager.Controllers
{
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            var roles = _db.Roles.ToList();
            return View(roles);
        }

        [HttpGet]
        public IActionResult Upsert(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return View();
            }
            else
            {
                // Update
                var objFromDb = _db.Roles.FirstOrDefault(u => u.Id == id);
                return View(objFromDb);
            }
        }

        [HttpPost]
        [Authorize(Policy = "OnlySuperAdminChecker")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upsert(IdentityRole roleObj)
        {
            if (await _roleManager.RoleExistsAsync(roleObj.Name))
            {
                // error
                TempData[DataState.Error] = "Role already exists.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(roleObj.Id))
            {
                // create
                await _roleManager.CreateAsync(new IdentityRole() { Name = roleObj.Name });
                TempData[DataState.Success] = "Role created successfully";
            }
            else
            {
                // update
                var objRoleFromDb = _db.Roles.FirstOrDefault(u => u.Id == roleObj.Id);

                if (objRoleFromDb == null)
                {
                    TempData[DataState.Error] = "Role not found.";
                    return RedirectToAction(nameof(Index));
                }

                objRoleFromDb.Name = roleObj.Name;
                objRoleFromDb.NormalizedName = roleObj.Name.ToUpper();
                var result = await _roleManager.UpdateAsync(objRoleFromDb);
                TempData[DataState.Success] = "Role updated successfully";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Policy = "OnlySuperAdminChecker")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var objFromDb = _db.Roles.FirstOrDefault(u => u.Id == id);

            if (objFromDb == null)
            {
                TempData[DataState.Error] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            var userRolesForThisRole = _db.UserRoles.Where(u => u.RoleId == id).Count();

            if (userRolesForThisRole > 0)
            {
                TempData[DataState.Error] = "Cannot delete this role, since there are users assigned to this role.";
                return RedirectToAction(nameof(Index));
            }

            await _roleManager.DeleteAsync(objFromDb);
            TempData[DataState.Success] = "Role deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
