using Azure.Identity;
using Day11Identity.IdentityData;
using EventsApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Security.Claims;

namespace EventsApp.Controllers
{
    public class UsersController : Controller
    {
        private readonly RoleManager<IdentityRole> _rolemanager;
        private readonly EventIdentityDbContext _context;
        private readonly SignInManager<IdentityUser> _signinmanager;
        private readonly UserManager<IdentityUser> _usermanager;


        public UsersController(
            RoleManager<IdentityRole> roleManager,
            EventIdentityDbContext context,
            UserManager<IdentityUser> usermanager,
            SignInManager<IdentityUser> signInManager)
        {
            _rolemanager = roleManager;
            _context = context;
            _usermanager = usermanager;
            _signinmanager = signInManager;
        }

        

        [Authorize(Roles ="Admin")]
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        public IActionResult AccessDenied()
        {
            
            return View();       }
        [Authorize(Roles = "Admin")]
        public IActionResult Details(int? id)
        {
            try
            {
                var user = _context.Users.ToList().Where(m => m.Id == id).FirstOrDefault();
                if (user != null)
                    return View(user);
                else
                {
                    return RedirectToAction(nameof(UserNotFound));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return RedirectToAction(nameof(UserNotFound));

            }
            
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            var searchForUser = _context.Users.ToList().Where(m => m.Email == user.Email).FirstOrDefault();
            if (searchForUser != null)
            {
                TempData["AlertMessage"] = "This user already exists! Please login.";
                return RedirectToAction(nameof(Login));
            }

            var _user = new IdentityUser { UserName = user.UserName, Email = user.Email, PhoneNumber = user.PhoneNumber };
            var result = await _usermanager.CreateAsync(_user, user.Password);

            if (result.Succeeded)
            {
                // Assign default role "User" to the newly created user
                await _usermanager.AddToRoleAsync(_user, "User");

                user.Password = "";
                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }
            else
            {
                foreach (var r in result.Errors)
                {
                    ModelState.AddModelError("", r.Description);
                }
                return View(user);
            }
        }


        [HttpGet]
        public IActionResult Login()
        {
            if (TempData.TryGetValue("AlertMessage" , out var data))
            {
                ViewData["AlertMessage"] = data;
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel user , string returnurl)
        {

            var _user = await _usermanager.FindByNameAsync(user.UserName);
            Microsoft.AspNetCore.Identity.SignInResult result =
                await _signinmanager.PasswordSignInAsync(_user, user.Password, true , false);
            if (result.Succeeded)
            {
                if (returnurl.IsNullOrEmpty())
                    return RedirectToAction("Index", "Home");
                else
                    if (Url.IsLocalUrl(returnurl))
                    return Redirect(returnurl);
                else
                    return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(nameof(user.UserName), "Login Failed: Incorrect user or password");
            }
            return View(user);

        }


        public IActionResult UserNotFound()
        {
            return View();
        }

        public IActionResult ProfilePage()
        {
            var user = _context.Users.Where(d => d.UserName == User.Identity.Name).ToList().FirstOrDefault();
            if (user !=null)
            {
                var EventsRelations = _context.EventUserRelations.Where(m => m.UserId == user.Id).ToList();
                List<Event> events = new List<Event>();
                foreach (var item in EventsRelations)
                {
                    events.Add(_context.Events.FirstOrDefault(m => m.Id == item.EventId));
                }
                ViewBag.Events = events;
                ViewBag.Image = _context.Images.ToList();
                return View(events);
            }
            else
            {
                // Handle the case where the claim value is not a valid integer
                // You might want to redirect to an error page or perform appropriate actions
                return RedirectToAction("Error", "Home");
            }
        }


        [Authorize(Roles = "Admin")]
        public IActionResult MyPage()
        {
            var user = _context.Users.Where(d => d.UserName == User.Identity.Name).ToList().FirstOrDefault();
            return View(user);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult IndexOfRoles()
        {
            var rols = _rolemanager.Roles.ToList();
            List<RoleModel> rolesmodel = new List<RoleModel>();
            foreach (var t in rols)
            {
                var m = new RoleModel { RoleName = t.Name };
                rolesmodel.Add(m);
            }
            return View(rolesmodel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> IndexRoles(string username)
        {
            ViewBag.username = username;
            IdentityUser u = await _usermanager.FindByNameAsync(username);
            var userRoles = await _usermanager.GetRolesAsync(u);
            List<RoleUserModel> usersmodel = new List<RoleUserModel>();
            foreach (var t in userRoles)
            {
                var m = new RoleUserModel { UserName = u.UserName, RoleName = t };
                usersmodel.Add(m);
            }
            return View(usersmodel);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AddRole()
        {
            return View();
        }
        [HttpPost]

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRole(RoleModel role)
        {
            IdentityRole x = new IdentityRole { Name = role.RoleName };
            IdentityResult res = await _rolemanager.CreateAsync(x);
            if (res.Succeeded)
            {

                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var e in res.Errors)
                {
                    ModelState.AddModelError("", e.Description);
                }
                return View();
            }

        }

        [Authorize(Roles = "Admin")]
        public IActionResult DeleteRole(string rolename)
        {

            return View(new RoleModel { RoleName = rolename });
        }
        [HttpPost]

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRole(RoleModel role)
        {

            IdentityRole x = await _rolemanager.FindByNameAsync(role.RoleName);
            IdentityResult res = await _rolemanager.DeleteAsync(x);
            if (res.Succeeded)
            {

                return RedirectToAction(nameof(Index));
            }
            else
            {
                foreach (var e in res.Errors)
                {
                    ModelState.AddModelError("", e.Description);
                }
                return View();
            }

        }


        public IActionResult AddRoleToUser(string username)
        {

            createViewRoleList(username);
            return View();
        }

        [NonAction]
        public void createViewRoleList(string username)
        {
            ViewBag.username = username;
            var rols = _rolemanager.Roles.ToList();
            List<string> rolesmodel = new List<string>();
            foreach (var t in rols)
            {
                var m = t.Name;
                rolesmodel.Add(m);
            }

            ViewBag.RoleList = rolesmodel;
        }

        [HttpPost]
        public async Task<IActionResult> AddRoleToUser(RoleUserModel userrole)
        {
            //ViewBag.username = ;
            IdentityUser x = await _usermanager.FindByNameAsync(userrole.UserName);
            //FindByNameAsync(userrole.UserName);

            IdentityResult res = await _usermanager.AddToRoleAsync(x, userrole.RoleName);

            if (res.Succeeded)
            {

                return RedirectToAction(nameof(IndexRoles), new { username = userrole.UserName });
            }
            else
            {
                foreach (var e in res.Errors)
                {
                    ModelState.AddModelError("", e.Description);
                }
                createViewRoleList(userrole.UserName);
                return View();
            }

        }

        public IActionResult DeleteUserRole(string username, string rolename)
        {

            return View(new RoleUserModel { RoleName = rolename, UserName = username });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteUserRole(RoleUserModel userrole)
        {

            IdentityUser x = await _usermanager.FindByNameAsync(userrole.UserName);
            IdentityResult res = await _usermanager.RemoveFromRoleAsync(x, userrole.RoleName);
            if (res.Succeeded)
            {

                return RedirectToAction(nameof(IndexRoles), new { username = userrole.UserName });
            }
            else
            {
                foreach (var e in res.Errors)
                {
                    ModelState.AddModelError("", e.Description);
                }
                return View();
            }

        }
        public IActionResult IndexUsers()
        {

            var users = _usermanager.Users.ToList();
            List<RoleUserModel> usersmodel = new List<RoleUserModel>();
            foreach (var t in users)
            {
                var m = new RoleUserModel { UserName = t.UserName };
                usersmodel.Add(m);
            }
            return View(usersmodel);
        }

        public async Task<IActionResult> Logout()
        {
            await _signinmanager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }


    }
}
