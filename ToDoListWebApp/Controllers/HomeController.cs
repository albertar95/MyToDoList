using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using ToDoListWebApp.Models;
using ToDoListWebApp.ViewModels;

namespace ToDoListWebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static string BaseAddress = "http://192.168.1.100:8061/api/todolist";//local service
        //private static string BaseAddress = "https://todolistwebapi.albertar95.ir/api/todolist"; // host service
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        //user section
        public async Task<IActionResult> Users()
        {
            using (HttpClient client = new HttpClient())
            {
                List<User> users = new List<User>();
                var response = await client.GetAsync($"{BaseAddress}/getusers");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    users = JsonConvert.DeserializeObject<List<User>>(stringResponse) ?? new List<User>();
                }
                return View(users);
            }
        }
        [AllowAnonymous]
        [Route("Login")]
        public IActionResult Login(string ReturnUrl = "")
        {
            return View("Login", ReturnUrl);
        }
        public IActionResult Profile()
        {
            return View();
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
        public async Task<IActionResult> AddUser(IFormFile profile, User user)
        {
            if (profile != null)
            {
                BinaryReader br = new BinaryReader(profile.OpenReadStream());
                user.ProfilePic = br.ReadBytes(Convert.ToInt32(profile.Length));
            }
            user.NidUser = Guid.NewGuid();
            user.CreateDate = DateTime.Now;
            user.Password = EncryptString(user.Password);
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/adduser", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["UserSuccess"] = $"{user.Username} created successfully";
                else
                    TempData["UserError"] = $"an error occured while creating user!";
            }
            return RedirectToAction("Users");
        }
        [Route("[controller]/[action]/{NidUser}")]
        public async Task<IActionResult> DeleteUser(string NidUser)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.DeleteAsync($"{BaseAddress}/deleteuser/{NidUser}");
                if (response.IsSuccessStatusCode)
                    TempData["UserSuccess"] = "user deleted successfully";
                else
                    TempData["UserError"] = "an error occured while deleting user!";
                return RedirectToAction("Users");
            }
        }
        [AllowAnonymous]
        public async Task<IActionResult> SubmitLogin(string Username, string Password, string returnUrl = "")
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync($"{BaseAddress}/loginuser?Username={Username}&Password={Password}");
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        User user = JsonConvert.DeserializeObject<User>(result) ?? new User();
                        List<Claim> claims = null;
                        if (user.ProfilePic != null)
                        {
                            claims = new List<Claim>
                    {
                        new Claim("Username",user.Username),
                        new Claim("LastLoginDate",user.LastLoginDate.ToString()),
                        new Claim("Profile",$"data:image/jpg;base64,{Convert.ToBase64String(user.ProfilePic)}"),
                        new Claim("NidUser",user.NidUser.ToString()),
                        new Claim("Role", "Admin"),
                     };
                        }
                        else
                        {
                            claims = new List<Claim>
                    {
                        new Claim("Username",user.Username),
                        new Claim("LastLoginDate",user.LastLoginDate.ToString()),
                        new Claim("Profile",""),
                        new Claim("NidUser",user.NidUser.ToString()),
                        new Claim("Role", "Admin"),
                     };
                        }

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var authProperties = new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.Now.AddHours(8) };

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);
                        if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return Redirect(returnUrl);
                        else
                            return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        TempData["LoginError"] = "error occured in login.try again";
                        return RedirectToAction("Login");
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public IActionResult UploadProfile()
        {
            var file = Request.Form.Files[0];
            BinaryReader br = new BinaryReader(file.OpenReadStream());
            byte[] s = br.ReadBytes(Convert.ToInt32(file.Length));
            //byte[] img = br.ReadString();
            return Json(new JsonResults() { HasValue = true, Message = br.ReadString() });
        }

        //goal section
        public async Task<IActionResult> Goals()
        {
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Goal> goals = new List<Goal>();
                List<Models.Task> tasks = new List<Models.Task>();
                var response = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    goals = JsonConvert.DeserializeObject<List<Goal>>(stringResponse) ?? new List<Goal>();
                }
                foreach (var g in goals)
                {
                    var response2 = await client.GetAsync($"{BaseAddress}/GetTasksByGoalId/{g.NidGoal}");
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse2 = await response2.Content.ReadAsStringAsync();
                        var tmpTasks = JsonConvert.DeserializeObject<List<Models.Task>>(stringResponse2) ?? new List<Models.Task>();
                        foreach (var t in tmpTasks)
                        {
                            tasks.Add(t);
                        }
                    }
                }
                return View(new ToDoListWebApp.ViewModels.GoalViewModel() {  Goals = goals, Tasks = tasks });
            }
        }
        public IActionResult AddGoal() 
        {
            return View();
        }
        public async Task<IActionResult> SubmitAddGoal(Goal goal) 
        {
            string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
            goal.NidGoal = Guid.NewGuid();
            goal.CreateDate = DateTime.Now;
            goal.UserId = Guid.Parse(NidUser);
            goal.GoalStatus = 0;
            goal.LastModifiedDate = DateTime.Now;
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(goal), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddGoal", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["GoalSuccess"] = $"{goal.Title} created successfully";
                else
                    TempData["GoalError"] = $"an error occured while creating goal!";
            }
            return RedirectToAction("Goals");
        }
        public async Task<IActionResult> Goal(Guid NidGoal) 
        {
            using (HttpClient client = new HttpClient())
            {
                Goal goal = new Goal();
                List<Models.Task> tasks = new List<Models.Task>();
                List<Models.Progress> progresses = new List<Models.Progress>();
                var Goalresponse = await client.GetAsync($"{BaseAddress}/GetGoalById/{NidGoal}");
                if (Goalresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await Goalresponse.Content.ReadAsStringAsync();
                    goal = JsonConvert.DeserializeObject<Goal>(stringResponse) ?? new Goal();
                }
                var TasksResponse = await client.GetAsync($"{BaseAddress}/GetTasksByGoalId/{NidGoal}");
                if (TasksResponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse2 = await TasksResponse.Content.ReadAsStringAsync();
                    tasks = JsonConvert.DeserializeObject<List<Models.Task>>(stringResponse2) ?? new List<Models.Task>();
                }
                var ProgressResponse = await client.GetAsync($"{BaseAddress}/GetProgressesByGoalId/{NidGoal}");
                if (ProgressResponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse3 = await ProgressResponse.Content.ReadAsStringAsync();
                    progresses = JsonConvert.DeserializeObject<List<Models.Progress>>(stringResponse3) ?? new List<Models.Progress>();
                }
                return View(new ToDoListWebApp.ViewModels.GoalPageViewModel() { Goal = goal, Tasks = tasks, Progresses = progresses });
            }
        }
        public async Task<IActionResult> SubmitAddTask(string NidGoal,string Title,string TWeight, string Description = "")
        {
            string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
            Models.Task newtask = new Models.Task() {  CreateDate = DateTime.Now, Description = Description, GoalId = Guid.Parse(NidGoal), LastModifiedDate = DateTime.Now, NidTask = Guid.NewGuid(), TaskStatus = false, Title = Title, UserId = Guid.Parse(NidUser), TaskWeight = byte.Parse(TWeight) };
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(newtask), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddTask", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["GoalPageSuccess"] = $"{newtask.Title} created successfully";
                else
                    TempData["GoalPageError"] = $"an error occured while creating task!";
            }
            return RedirectToAction("Goal",new { NidGoal = NidGoal});
        }
        public async Task<IActionResult> DeleteTask(Guid NidTask,Guid NidGoal)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.DeleteAsync($"{BaseAddress}/DeleteTask/{NidTask}");
                if (response.IsSuccessStatusCode)
                    TempData["GoalPageSuccess"] = $"task deleted successfully";
                else
                    TempData["GoalPageError"] = $"an error occured while deleting task!";
            }
            return RedirectToAction("Goal", new { NidGoal = NidGoal });
        }
        public async Task<IActionResult> DeleteProgress(Guid NidProgress, Guid NidGoal)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.DeleteAsync($"{BaseAddress}/DeleteProgress/{NidProgress}");
                if (response.IsSuccessStatusCode)
                    TempData["GoalPageSuccess"] = $"progress deleted successfully";
                else
                    TempData["GoalPageError"] = $"an error occured while deleting task!";
            }
            return RedirectToAction("Goal", new { NidGoal = NidGoal });
        }
        public async Task<IActionResult> DoneTask(Guid NidTask, Guid NidGoal)
        {
            using (HttpClient client = new HttpClient())
            {
                var getTaskResponse = await client.GetAsync($"{BaseAddress}/GetTaskById/{NidTask}");
                if (getTaskResponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await getTaskResponse.Content.ReadAsStringAsync();
                    Models.Task task = JsonConvert.DeserializeObject<Models.Task>(stringResponse) ?? new Models.Task();
                    task.TaskStatus = true;
                    task.ClosureDate = DateTime.Now;
                    task.LastModifiedDate = DateTime.Now;
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(task), Encoding.UTF8, "application/json");
                    var putTaskResponse = await client.PatchAsync($"{BaseAddress}/EditTask", stringContent);
                    if (putTaskResponse.IsSuccessStatusCode)
                        TempData["GoalPageSuccess"] = $"task edited successfully";
                    else
                        TempData["GoalPageError"] = $"an error occured while editing task!";
                }
            }
            return RedirectToAction("Goal", new { NidGoal = NidGoal });
        }
        public async Task<IActionResult> UndoTask(Guid NidTask, Guid NidGoal)
        {
            using (HttpClient client = new HttpClient())
            {
                var getTaskResponse = await client.GetAsync($"{BaseAddress}/GetTaskById/{NidTask}");
                if (getTaskResponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await getTaskResponse.Content.ReadAsStringAsync();
                    Models.Task task = JsonConvert.DeserializeObject<Models.Task>(stringResponse) ?? new Models.Task();
                    task.TaskStatus = false;
                    task.ClosureDate = null;
                    task.LastModifiedDate = DateTime.Now;
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(task), Encoding.UTF8, "application/json");
                    var putTaskResponse = await client.PatchAsync($"{BaseAddress}/EditTask", stringContent);
                    if (putTaskResponse.IsSuccessStatusCode)
                        TempData["GoalPageSuccess"] = $"task edited successfully";
                    else
                        TempData["GoalPageError"] = $"an error occured while editing task!";
                }
            }
            return RedirectToAction("Goal", new { NidGoal = NidGoal });
        }
        public async Task<IActionResult> SubmitEditTask(string NidTask, Guid NidGoal,string Title,string TWeight)
        {
            using (HttpClient client = new HttpClient())
            {
                var getTaskResponse = await client.GetAsync($"{BaseAddress}/GetTaskById/{NidTask.Replace("'","")}");
                if (getTaskResponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await getTaskResponse.Content.ReadAsStringAsync();
                    Models.Task task = JsonConvert.DeserializeObject<Models.Task>(stringResponse) ?? new Models.Task();
                    task.Title = Title;
                    task.LastModifiedDate = DateTime.Now;
                    task.TaskWeight = byte.Parse(TWeight);
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(task), Encoding.UTF8, "application/json");
                    var putTaskResponse = await client.PatchAsync($"{BaseAddress}/EditTask", stringContent);
                    if (putTaskResponse.IsSuccessStatusCode)
                        TempData["GoalPageSuccess"] = $"task edited successfully";
                    else
                        TempData["GoalPageError"] = $"an error occured while editing task!";
                }
            }
            return RedirectToAction("Goal", new { NidGoal = NidGoal });
        }
        public async Task<IActionResult> SubmitEditTaskDescription(string NidTask, Guid NidGoal, string Description)
        {
            using (HttpClient client = new HttpClient())
            {
                var getTaskResponse = await client.GetAsync($"{BaseAddress}/GetTaskById/{NidTask.Replace("'", "")}");
                if (getTaskResponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await getTaskResponse.Content.ReadAsStringAsync();
                    Models.Task task = JsonConvert.DeserializeObject<Models.Task>(stringResponse) ?? new Models.Task();
                    task.Description = Description;
                    task.LastModifiedDate = DateTime.Now;
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(task), Encoding.UTF8, "application/json");
                    var putTaskResponse = await client.PatchAsync($"{BaseAddress}/EditTask", stringContent);
                    if (putTaskResponse.IsSuccessStatusCode)
                        TempData["GoalPageSuccess"] = $"task edited successfully";
                    else
                        TempData["GoalPageError"] = $"an error occured while editing task!";
                }
            }
            return Json(new JsonResults() {  HasValue = true});
        }
        public async Task<IActionResult> EditGoal(Guid NidGoal) 
        {
            using (HttpClient client = new HttpClient())
            {
                Goal goal = new Goal();
                var Goalresponse = await client.GetAsync($"{BaseAddress}/GetGoalById/{NidGoal}");
                if (Goalresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await Goalresponse.Content.ReadAsStringAsync();
                    goal = JsonConvert.DeserializeObject<Goal>(stringResponse) ?? new Goal();
                }
                return View(goal);
            }
        }
        public async Task<IActionResult> SubmitEditGoal(Goal goal)
        {
            goal.LastModifiedDate = DateTime.Now;
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(goal), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PatchAsync($"{BaseAddress}/EditGoal", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["GoalPageSuccess"] = $"goal edited successfully";
                else
                    TempData["GoalPageError"] = $"an error occured while editing goal!";
            }
            return RedirectToAction("Goal", new { NidGoal = goal.NidGoal });
        }
        public async Task<IActionResult> CloseGoal(Guid NidGoal)
        {
            Goal goal = new Goal();
            using (HttpClient client = new HttpClient())
            {
                var Goalresponse = await client.GetAsync($"{BaseAddress}/GetGoalById/{NidGoal}");
                if (Goalresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await Goalresponse.Content.ReadAsStringAsync();
                    goal = JsonConvert.DeserializeObject<Goal>(stringResponse) ?? new Goal();
                    goal.LastModifiedDate = DateTime.Now;
                    goal.GoalStatus = 1;
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(goal), Encoding.UTF8, "application/json");
                    var response = await client.PatchAsync($"{BaseAddress}/EditGoal", stringContent);
                    if (response.IsSuccessStatusCode)
                        TempData["GoalSuccess"] = $"goal edited successfully";
                    else
                        TempData["GoalError"] = $"an error occured while editing goal!";
                }
            }
            return RedirectToAction("Goals");
        }
        public async Task<IActionResult> SubmitDeleteGoal(Guid NidGoal)
        {
            Goal goal = new Goal();
            using (HttpClient client = new HttpClient())
            {
                var Goalresponse = await client.GetAsync($"{BaseAddress}/GetGoalById/{NidGoal}");
                if (Goalresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await Goalresponse.Content.ReadAsStringAsync();
                    goal = JsonConvert.DeserializeObject<Goal>(stringResponse) ?? new Goal();
                }
                var response = await client.DeleteAsync($"{BaseAddress}/DeleteGoal/{NidGoal}");
                if (response.IsSuccessStatusCode)
                    TempData["GoalSuccess"] = $"goal deleted successfully";
                else
                    TempData["GoalError"] = $"an error occured while deleting goal!";
            }
            return RedirectToAction("Goals");
        }
        public async Task<IActionResult> OpenGoal(Guid NidGoal)
        {
            Goal goal = new Goal();
            using (HttpClient client = new HttpClient())
            {
                var Goalresponse = await client.GetAsync($"{BaseAddress}/GetGoalById/{NidGoal}");
                if (Goalresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await Goalresponse.Content.ReadAsStringAsync();
                    goal = JsonConvert.DeserializeObject<Goal>(stringResponse) ?? new Goal();
                    goal.LastModifiedDate = DateTime.Now;
                    goal.GoalStatus = 0;
                    StringContent stringContent = new StringContent(JsonConvert.SerializeObject(goal), Encoding.UTF8, "application/json");
                    var response = await client.PatchAsync($"{BaseAddress}/EditGoal", stringContent);
                    if (response.IsSuccessStatusCode)
                        TempData["GoalSuccess"] = $"goal edited successfully";
                    else
                        TempData["GoalError"] = $"an error occured while editing goal!";
                }
            }
            return RedirectToAction("Goals");
        }
        //generals
        public async Task<IActionResult> Index()
        {
            string[] DatePeriod = new string[3];
            var weekDates = Helpers.Dates.GetWeekPeriod();
            DatePeriod[0] = $"{weekDates.Item1.ToString("dd/MM/yyyy")} - {weekDates.Item2.ToString("dd/MM/yyyy")}";
            DatePeriod[1] = "-1";
            DatePeriod[2] = "1";
            var persianDates = Helpers.Dates.ToPersianDate(weekDates);
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Goal> goals = new List<Goal>();
                List<Goal> allGoals = new List<Goal>();
                List<Models.Task> tasks = new List<Models.Task>();
                List<Models.Task> allTasks = new List<Models.Task>();
                List<Models.Schedule> schedules = new List<Models.Schedule>();
                List<Models.Progress> progress = new List<Models.Progress>();
                var response = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    allGoals = JsonConvert.DeserializeObject<List<Goal>>(stringResponse);
                    goals = allGoals.Where(p => p.GoalStatus == 0).ToList() ?? new List<Goal>();
                }
                foreach (var goal in allGoals)
                {
                    var response2 = await client.GetAsync($"{BaseAddress}/GetTasksByGoalId/{goal.NidGoal}");
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse2 = await response2.Content.ReadAsStringAsync();
                        allTasks.AddRange(JsonConvert.DeserializeObject<List<Models.Task>>(stringResponse2));
                        tasks = allTasks.Where(p => p.TaskStatus == false && goals.GroupBy(x => x.NidGoal).Select(q => q.Key).ToList()
                        .Contains(p.GoalId)).ToList() ?? new List<Models.Task>();
                    }
                }
                foreach (var task in allTasks)
                {
                    var response3 = await client.GetAsync($"{BaseAddress}/GetScheduleByTaskId/{task.NidTask}");
                    if (response3.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse3 = await response3.Content.ReadAsStringAsync();
                        schedules.AddRange(JsonConvert.DeserializeObject<List<Models.Schedule>>(stringResponse3).Where(p => p.ScheduleDate >= weekDates.Item1 && p.ScheduleDate <= weekDates.Item2) ?? new List<Models.Schedule>());
                    }
                }
                foreach (var schedule in schedules)
                {
                    var ProgressResponse = await client.GetAsync($"{BaseAddress}/GetProgressesByScheduleId/{schedule.NidSchedule}");
                    if (ProgressResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse3 = await ProgressResponse.Content.ReadAsStringAsync();
                        progress.AddRange(JsonConvert.DeserializeObject<List<Models.Progress>>(stringResponse3) ?? new List<Models.Progress>());
                    }
                }
                return View(new ViewModels.IndexViewModel() 
                {  DatePeriodInfo = DatePeriod, PersianDatePeriodInfo = new string[] { persianDates.Item1,persianDates.Item2 }, 
                    Goals = goals, Progresses = progress, Tasks = tasks, Schedules = schedules, StartDate = weekDates.Item1, EndDate = weekDates.Item2,
                 AllTasks = allTasks, AllGoals = allGoals });
            }
        }
        public async Task<IActionResult> IndexPagination(int Direction) 
        {
            string[] DatePeriod = new string[3];
            var weekDates = Helpers.Dates.GetWeekPeriod(Direction);
            DatePeriod[0] = $"{weekDates.Item1.ToString("dd/MM/yyyy")} - {weekDates.Item2.ToString("dd/MM/yyyy")}";
            DatePeriod[1] = $"{Direction - 1}";
            DatePeriod[2] = $"{Direction + 1}";
            var persianDates = Helpers.Dates.ToPersianDate(weekDates);
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Goal> goals = new List<Goal>();
                List<Goal> allGoals = new List<Goal>();
                List<Models.Task> tasks = new List<Models.Task>();
                List<Models.Task> allTasks = new List<Models.Task>();
                List<Models.Schedule> schedules = new List<Models.Schedule>();
                List<Models.Progress> progress = new List<Models.Progress>();
                var response = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    allGoals = JsonConvert.DeserializeObject<List<Goal>>(stringResponse);
                    goals = allGoals.Where(p => p.GoalStatus == 0).ToList() ?? new List<Goal>();
                }
                foreach (var goal in allGoals)
                {
                    var response2 = await client.GetAsync($"{BaseAddress}/GetTasksByGoalId/{goal.NidGoal}");
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse2 = await response2.Content.ReadAsStringAsync();
                        allTasks.AddRange(JsonConvert.DeserializeObject<List<Models.Task>>(stringResponse2));
                        tasks = allTasks.Where(p => p.TaskStatus == false && goals.GroupBy(x => x.NidGoal).Select(q => q.Key).ToList()
                        .Contains(p.GoalId)).ToList() ?? new List<Models.Task>();
                    }
                }
                foreach (var task in allTasks)
                {
                    var response3 = await client.GetAsync($"{BaseAddress}/GetScheduleByTaskId/{task.NidTask}");
                    if (response3.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse3 = await response3.Content.ReadAsStringAsync();
                        schedules.AddRange(JsonConvert.DeserializeObject<List<Models.Schedule>>(stringResponse3).Where(p => p.ScheduleDate >= weekDates.Item1 && p.ScheduleDate <= weekDates.Item2) ?? new List<Models.Schedule>());
                    }
                }
                foreach (var schedule in schedules)
                {
                    var ProgressResponse = await client.GetAsync($"{BaseAddress}/GetProgressesByScheduleId/{schedule.NidSchedule}");
                    if (ProgressResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse3 = await ProgressResponse.Content.ReadAsStringAsync();
                        progress.AddRange(JsonConvert.DeserializeObject<List<Models.Progress>>(stringResponse3) ?? new List<Models.Progress>());
                    }
                }
                return Json(new JsonResults() { HasValue = true, Html = await Helpers.RenderViewToString.RenderViewAsync(this, "_IndexPartialView", 
                    new ViewModels.IndexViewModel() { DatePeriodInfo = DatePeriod, PersianDatePeriodInfo = new string[] { persianDates.Item1, persianDates.Item2 }, 
                        Goals = goals, Progresses = progress, Tasks = tasks, Schedules = schedules, StartDate = weekDates.Item1, EndDate = weekDates.Item2,
                     AllGoals = allGoals, AllTasks = allTasks }, true) });

            }
        }
        public async Task<IActionResult> IndexPaginationView(int Direction)
        {
            string[] DatePeriod = new string[3];
            var weekDates = Helpers.Dates.GetWeekPeriod(Direction);
            DatePeriod[0] = $"{weekDates.Item1.ToString("dd/MM/yyyy")} - {weekDates.Item2.ToString("dd/MM/yyyy")}";
            DatePeriod[1] = $"{Direction - 1}";
            DatePeriod[2] = $"{Direction + 1}";
            var persianDates = Helpers.Dates.ToPersianDate(weekDates);
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Goal> goals = new List<Goal>();
                List<Goal> allGoals = new List<Goal>();
                List<Models.Task> tasks = new List<Models.Task>();
                List<Models.Task> allTasks = new List<Models.Task>();
                List<Models.Schedule> schedules = new List<Models.Schedule>();
                List<Models.Progress> progress = new List<Models.Progress>();
                var response = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    allGoals = JsonConvert.DeserializeObject<List<Goal>>(stringResponse);
                    goals = allGoals.Where(p => p.GoalStatus == 0).ToList() ?? new List<Goal>();
                }
                foreach (var goal in allGoals)
                {
                    var response2 = await client.GetAsync($"{BaseAddress}/GetTasksByGoalId/{goal.NidGoal}");
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse2 = await response2.Content.ReadAsStringAsync();
                        allTasks.AddRange(JsonConvert.DeserializeObject<List<Models.Task>>(stringResponse2));
                        tasks = allTasks.Where(p => p.TaskStatus == false && goals.GroupBy(x => x.NidGoal).Select(q => q.Key).ToList()
                        .Contains(p.GoalId)).ToList() ?? new List<Models.Task>();
                    }
                }
                foreach (var task in allTasks)
                {
                    var response3 = await client.GetAsync($"{BaseAddress}/GetScheduleByTaskId/{task.NidTask}");
                    if (response3.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse3 = await response3.Content.ReadAsStringAsync();
                        schedules.AddRange(JsonConvert.DeserializeObject<List<Models.Schedule>>(stringResponse3).Where(p => p.ScheduleDate >= weekDates.Item1 && p.ScheduleDate <= weekDates.Item2) ?? new List<Models.Schedule>());
                    }
                }
                foreach (var schedule in schedules)
                {
                    var ProgressResponse = await client.GetAsync($"{BaseAddress}/GetProgressesByScheduleId/{schedule.NidSchedule}");
                    if (ProgressResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse3 = await ProgressResponse.Content.ReadAsStringAsync();
                        progress.AddRange(JsonConvert.DeserializeObject<List<Models.Progress>>(stringResponse3) ?? new List<Models.Progress>());
                    }
                }
                return View("Index", new ViewModels.IndexViewModel() { DatePeriodInfo = DatePeriod, PersianDatePeriodInfo = new string[] { persianDates.Item1, persianDates.Item2 }, 
                    Goals = goals, Progresses = progress, Tasks = tasks, Schedules = schedules, StartDate = weekDates.Item1, EndDate = weekDates.Item2,
                 AllGoals = allGoals, AllTasks = allTasks });

            }
        }
        public async Task<IActionResult> SubmitAddSchedule(Models.Schedule schedule,int Direction = 0)
        {
            schedule.NidSchedule = Guid.NewGuid();
            schedule.CreateDate = DateTime.Now;
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(schedule), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddSchedule", stringContent);
            }
            return RedirectToAction("IndexPaginationView",new { Direction = Direction });
        }
        public async Task<IActionResult> SubmitAddProgress(Models.Progress progress, int Direction = 0)
        {
            string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
            progress.NidProgress = Guid.NewGuid();
            progress.CreateDate = DateTime.Now;
            progress.UserId = Guid.Parse(NidUser);
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(progress), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddProgress", stringContent);
            }
            return RedirectToAction("IndexPaginationView", new { Direction = Direction });
        }
        public async Task<IActionResult> SubmitEditProgress(Models.Progress progress, int Direction = 0)
        {
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(progress), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PatchAsync($"{BaseAddress}/EditProgress", stringContent);
                return RedirectToAction("IndexPaginationView", new { Direction = Direction });
            }
        }
        public async Task<IActionResult> SubmitDeleteProgress(string NidProgress, int Direction = 0)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.DeleteAsync($"{BaseAddress}/DeleteProgress/{NidProgress}");
                return RedirectToAction("IndexPaginationView", new { Direction = Direction });
            }
        }
        public async Task<IActionResult> SubmitDeleteSchedule(string NidSchedule, int Direction = 0)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.DeleteAsync($"{BaseAddress}/DeleteSchedule/{NidSchedule}");
                return RedirectToAction("IndexPaginationView", new { Direction = Direction });
            }
        }
        public static string EncryptString(string text)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes("B@8CCto%YgfBF8OP1!Con007W"));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateEncryptor())
                    {
                        byte[] textBytes = UTF8Encoding.UTF8.GetBytes(text);
                        byte[] bytes = transform.TransformFinalBlock(textBytes, 0, textBytes.Length);
                        return Convert.ToBase64String(bytes, 0, bytes.Length);
                    }
                }
            }
        }
        //routine section
        public async Task<IActionResult> Routines()
        {
            RoutineViewModel model = new RoutineViewModel();
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Routine> routines = new List<Routine>();
                List<RoutineProgress> routineProgresses = new List<RoutineProgress>();
                var response = await client.GetAsync($"{BaseAddress}/GetRoutinesByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    routines = JsonConvert.DeserializeObject<List<Routine>>(stringResponse) ?? new List<Routine>();
                }
                foreach (var rt in routines)
                {
                    var response2 = await client.GetAsync($"{BaseAddress}/GetProgressesByRoutineId/{rt.NidRoutine}");
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse2 = await response2.Content.ReadAsStringAsync();
                        routineProgresses.AddRange(JsonConvert.DeserializeObject<List<RoutineProgress>>(stringResponse2));
                    }
                }
                model.Routines = routines;
                model.RoutineProgresses = routineProgresses;
                return View(model);
            }
        }
        [HttpPost]
        public async Task<IActionResult> SubmitAddRoutine(Routine routine) 
        {
            routine.NidRoutine = Guid.NewGuid();
            string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
            routine.UserId = Guid.Parse(NidUser);
            routine.CreateDate = DateTime.Now;
            routine.Status = false;
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(routine), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddRoutine", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["RoutineSuccess"] = $"{routine.Title} created successfully";
                else
                    TempData["RoutineError"] = $"an error occured while creating routine!";
            }
            return RedirectToAction("Routines");
        }
        public async Task<IActionResult> SubmitDeleteRoutine(Guid NidRoutine) 
        {
            Routine routine = new Routine();
            using (HttpClient client = new HttpClient())
            {
                var routineresponse = await client.GetAsync($"{BaseAddress}/GetRoutineById/{NidRoutine}");
                if (routineresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await routineresponse.Content.ReadAsStringAsync();
                    routine = JsonConvert.DeserializeObject<Routine>(stringResponse) ?? new Routine();
                }
                var response = await client.DeleteAsync($"{BaseAddress}/DeleteRoutine/{NidRoutine}");
                if (response.IsSuccessStatusCode)
                    TempData["RoutineSuccess"] = $"routine deleted successfully";
                else
                    TempData["RoutineError"] = $"an error occured while deleting routine!";
            }
            return RedirectToAction("Routines");
        }
        public async Task<IActionResult> SubmitDeleteRoutine2(Guid NidRoutine, int Direction = 0)
        {
            Routine routine = new Routine();
            using (HttpClient client = new HttpClient())
            {
                var routineresponse = await client.GetAsync($"{BaseAddress}/GetRoutineById/{NidRoutine}");
                if (routineresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await routineresponse.Content.ReadAsStringAsync();
                    routine = JsonConvert.DeserializeObject<Routine>(stringResponse) ?? new Routine();
                }
                var response = await client.DeleteAsync($"{BaseAddress}/DeleteRoutine/{NidRoutine}");
                if (response.IsSuccessStatusCode)
                    TempData["RoutineSuccess"] = $"routine deleted successfully";
                else
                    TempData["RoutineError"] = $"an error occured while deleting routine!";
            }
            return RedirectToAction("IndexPaginationView2", new { Direction = Direction });
        }
        public async Task<IActionResult> SubmitDoneRoutine(RoutineProgress Progress)
        {
            Progress.NidRoutineProgress = Guid.NewGuid();
            Progress.CreateDate = DateTime.Now;
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(Progress), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddRoutineProgress", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["RoutineSuccess"] = $"routine done successfully";
                else
                    TempData["RoutineError"] = $"an error occured while doning routine!";
            }
            return RedirectToAction("Routines");
        }
        public async Task<IActionResult> SubmitUnDoneRoutine(RoutineProgress Progress)
        {
            List<RoutineProgress> routine = new List<RoutineProgress>();
            using (HttpClient client = new HttpClient())
            {
                var routineresponse = await client.GetAsync($"{BaseAddress}/GetProgressesByRoutineId/{Progress.RoutineId}");
                if (routineresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await routineresponse.Content.ReadAsStringAsync();
                    routine = JsonConvert.DeserializeObject<List<RoutineProgress>>(stringResponse) ?? new List<RoutineProgress>();
                    foreach (var rp in routine)
                    {
                        if(rp.ProgressDate == Progress.ProgressDate.Date)
                            await client.DeleteAsync($"{BaseAddress}/DeleteRoutineProgress/{rp.NidRoutineProgress}");
                    }
                }
            }
            return RedirectToAction("Routines");
        }
        public async Task<IActionResult> RoutineCalendar() 
        {
            RoutineViewModel model = new RoutineViewModel();
            string[] DatePeriod = new string[3];
            var weekDates = Helpers.Dates.GetWeekPeriod();
            DatePeriod[0] = $"{weekDates.Item1.ToString("dd/MM/yyyy")} - {weekDates.Item2.ToString("dd/MM/yyyy")}";
            DatePeriod[1] = "-1";
            DatePeriod[2] = "1";
            var persianDates = Helpers.Dates.ToPersianDate(weekDates);
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Routine> routines = new List<Routine>();
                List<RoutineProgress> routineProgresses = new List<RoutineProgress>();
                var response = await client.GetAsync($"{BaseAddress}/GetRoutinesByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    routines = JsonConvert.DeserializeObject<List<Routine>>(stringResponse) ?? new List<Routine>();
                }
                foreach (var rt in routines)
                {
                    var response2 = await client.GetAsync($"{BaseAddress}/GetProgressesByRoutineId/{rt.NidRoutine}");
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse2 = await response2.Content.ReadAsStringAsync();
                        routineProgresses.AddRange(JsonConvert.DeserializeObject<List<RoutineProgress>>(stringResponse2));
                    }
                }
                model.Routines = routines;
                model.RoutineProgresses = routineProgresses;
                model.PersianDatePeriodInfo = new string[] { persianDates.Item1, persianDates.Item2 };
                model.DatePeriodInfo = DatePeriod;
                model.StartDate = weekDates.Item1;
                model.EndDate = weekDates.Item2;
                return View(model);
            }
        }
        public async Task<IActionResult> IndexPagination2(int Direction)
        {
            RoutineViewModel model = new RoutineViewModel();
            string[] DatePeriod = new string[3];
            var weekDates = Helpers.Dates.GetWeekPeriod(Direction);
            DatePeriod[0] = $"{weekDates.Item1.ToString("dd/MM/yyyy")} - {weekDates.Item2.ToString("dd/MM/yyyy")}";
            DatePeriod[1] = $"{Direction - 1}";
            DatePeriod[2] = $"{Direction + 1}";
            var persianDates = Helpers.Dates.ToPersianDate(weekDates);
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Routine> routines = new List<Routine>();
                List<RoutineProgress> routineProgresses = new List<RoutineProgress>();
                var response = await client.GetAsync($"{BaseAddress}/GetRoutinesByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    routines = JsonConvert.DeserializeObject<List<Routine>>(stringResponse) ?? new List<Routine>();
                }
                foreach (var rt in routines)
                {
                    var response2 = await client.GetAsync($"{BaseAddress}/GetProgressesByRoutineId/{rt.NidRoutine}");
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse2 = await response2.Content.ReadAsStringAsync();
                        routineProgresses.AddRange(JsonConvert.DeserializeObject<List<RoutineProgress>>(stringResponse2));
                    }
                }
                model.Routines = routines;
                model.RoutineProgresses = routineProgresses;
                model.PersianDatePeriodInfo = new string[] { persianDates.Item1, persianDates.Item2 };
                model.DatePeriodInfo = DatePeriod;
                model.StartDate = weekDates.Item1;
                model.EndDate = weekDates.Item2;
                return Json(new JsonResults() { HasValue = true, Html = await Helpers.RenderViewToString.RenderViewAsync(this, "_RoutineCalendarPartialView", model, true) });
            }
        }
        public async Task<IActionResult> IndexPaginationView2(int Direction)
        {
            RoutineViewModel model = new RoutineViewModel();
            string[] DatePeriod = new string[3];
            var weekDates = Helpers.Dates.GetWeekPeriod(Direction);
            DatePeriod[0] = $"{weekDates.Item1.ToString("dd/MM/yyyy")} - {weekDates.Item2.ToString("dd/MM/yyyy")}";
            DatePeriod[1] = $"{Direction - 1}";
            DatePeriod[2] = $"{Direction + 1}";
            var persianDates = Helpers.Dates.ToPersianDate(weekDates);
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Routine> routines = new List<Routine>();
                List<RoutineProgress> routineProgresses = new List<RoutineProgress>();
                var response = await client.GetAsync($"{BaseAddress}/GetRoutinesByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    routines = JsonConvert.DeserializeObject<List<Routine>>(stringResponse) ?? new List<Routine>();
                }
                foreach (var rt in routines)
                {
                    var response2 = await client.GetAsync($"{BaseAddress}/GetProgressesByRoutineId/{rt.NidRoutine}");
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse2 = await response2.Content.ReadAsStringAsync();
                        routineProgresses.AddRange(JsonConvert.DeserializeObject<List<RoutineProgress>>(stringResponse2));
                    }
                }
                model.Routines = routines;
                model.RoutineProgresses = routineProgresses;
                model.PersianDatePeriodInfo = new string[] { persianDates.Item1, persianDates.Item2 };
                model.DatePeriodInfo = DatePeriod;
                model.StartDate = weekDates.Item1;
                model.EndDate = weekDates.Item2;
                return View("RoutineCalendar", model);
            }
        }
        public async Task<IActionResult> SubmitEditRoutine(Routine Routine, int Direction = 0)
        {
            using (HttpClient client = new HttpClient())
            {
                Routine r = new Routine();
                var response = await client.GetAsync($"{BaseAddress}/GetRoutineById/{Routine.NidRoutine}");
                if (response.IsSuccessStatusCode)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    r = JsonConvert.DeserializeObject<Routine>(stringResponse);
                    r.Todate = Routine.Todate;
                    r.FromDate = Routine.FromDate;
                    r.Title = Routine.Title;
                    r.ModifiedDate = DateTime.Now;
                }
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(r), Encoding.UTF8, "application/json");
                var response2 = await client.PatchAsync($"{BaseAddress}/EditRoutine", stringContent);
                if(response2.IsSuccessStatusCode)
                    TempData["RoutineSuccess"] = $"routine edited successfully";
                else
                    TempData["RoutineError"] = $"an error occured while editing routine!";
            }
            return RedirectToAction("IndexPaginationView2", new { Direction = Direction });
        }
        public async Task<IActionResult> SubmitDeleteRoutineProgress(Guid NidRoutineProgress, int Direction = 0)
        {
            List<RoutineProgress> routine = new List<RoutineProgress>();
            using (HttpClient client = new HttpClient())
            {
                var routineresponse = await client.DeleteAsync($"{BaseAddress}/DeleteRoutineProgress/{NidRoutineProgress}");
            }
            return RedirectToAction("IndexPaginationView2", new { Direction = Direction });
        }
        public async Task<IActionResult> SubmitAddRoutineProgress(RoutineProgress Progress, int Direction = 0)
        {
            Progress.NidRoutineProgress = Guid.NewGuid();
            Progress.CreateDate = DateTime.Now;
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(Progress), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddRoutineProgress", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["RoutineSuccess"] = $"routine done successfully";
                else
                    TempData["RoutineError"] = $"an error occured while doning routine!";
            }
            return RedirectToAction("IndexPaginationView2", new { Direction = Direction });
        }
        //notes section
        public async Task<IActionResult> NoteGroups()
        {
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<NoteGroup> groups = new List<NoteGroup>();
                var response = await client.GetAsync($"{BaseAddress}/GetNoteGroupsByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    groups = JsonConvert.DeserializeObject<List<NoteGroup>>(stringResponse) ?? new List<NoteGroup>();
                }
                return View(groups);
            }
        }
        public IActionResult AddNoteGroup()
        {
            return View();
        }
        public async Task<IActionResult> SubmitAddNoteGroup(NoteGroup group) 
        {
            string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
            group.NidGroup = Guid.NewGuid();
            group.CreateDate = DateTime.Now;
            group.UserId = Guid.Parse(NidUser);
            group.ModifiedDate = DateTime.Now;
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(group), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddNoteGroup", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["GroupSuccess"] = $"{group.Title} created successfully";
                else
                    TempData["GroupError"] = $"an error occured while creating group!";
            }
            return RedirectToAction("NoteGroups");
        }
        public async Task<IActionResult> NoteGroup(Guid NidGroup)
        {
            using (HttpClient client = new HttpClient())
            {
                NoteGroup group = new NoteGroup();
                List<Models.Note> notes = new List<Models.Note>();
                var Groupresponse = await client.GetAsync($"{BaseAddress}/GetNoteGroupById/{NidGroup}");
                if (Groupresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await Groupresponse.Content.ReadAsStringAsync();
                    group = JsonConvert.DeserializeObject<NoteGroup>(stringResponse) ?? new NoteGroup();
                }
                var NotesResponse = await client.GetAsync($"{BaseAddress}/GetNotesByGroupId/{NidGroup}");
                if (NotesResponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse2 = await NotesResponse.Content.ReadAsStringAsync();
                    notes = JsonConvert.DeserializeObject<List<Models.Note>>(stringResponse2) ?? new List<Models.Note>();
                }
                return View(new ToDoListWebApp.ViewModels.NotesViewModel() { Group = group, Notes = notes });
            }
        }
        public async Task<IActionResult> EditNoteGroup(Guid NidGroup)
        {
            using (HttpClient client = new HttpClient())
            {
                NoteGroup note = new NoteGroup();
                var Noteresponse = await client.GetAsync($"{BaseAddress}/GetNoteGroupById/{NidGroup}");
                if (Noteresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await Noteresponse.Content.ReadAsStringAsync();
                    note = JsonConvert.DeserializeObject<NoteGroup>(stringResponse) ?? new NoteGroup();
                }
                return View(note);
            }
        }
        public async Task<IActionResult> SubmitEditNoteGroup(NoteGroup group)
        {
            group.ModifiedDate = DateTime.Now;
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(group), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PatchAsync($"{BaseAddress}/EditNoteGroup", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["NoteSuccess"] = $"group edited successfully";
                else
                    TempData["NoteError"] = $"an error occured while editing group!";
            }
            return RedirectToAction("NoteGroup", new { NidGroup = group.NidGroup });
        }
        public async Task<IActionResult> SubmitDeleteNoteGroup(Guid NidGroup)
        {
            NoteGroup group = new NoteGroup();
            using (HttpClient client = new HttpClient())
            {
                var groupresponse = await client.GetAsync($"{BaseAddress}/GetNoteGroupById/{NidGroup}");
                if (groupresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await groupresponse.Content.ReadAsStringAsync();
                    group = JsonConvert.DeserializeObject<NoteGroup>(stringResponse) ?? new NoteGroup();
                }
                var response = await client.DeleteAsync($"{BaseAddress}/DeleteNoteGroup/{NidGroup}");
                if (response.IsSuccessStatusCode)
                    TempData["GroupSuccess"] = $"group deleted successfully";
                else
                    TempData["GroupError"] = $"an error occured while deleting group!";
            }
            return RedirectToAction("NoteGroups");
        }
        public IActionResult AddNote(Guid NidGroup)
        {
            var note = new Note() { GroupId = NidGroup };
            return View(note);
        }
        public async Task<IActionResult> SubmitAddNote(Note note)
        {
            note.NidNote = Guid.NewGuid();
            note.CreateDate = DateTime.Now;
            note.ModifiedDate = DateTime.Now;
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(note), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddNote", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["NoteSuccess"] = $"{note.Title} created successfully";
                else
                    TempData["NoteError"] = $"an error occured while creating note!";
            }
            return RedirectToAction("NoteGroup",new { NidGroup = note.GroupId });
        }
        public async Task<IActionResult> Note(Guid NidNote)
        {
            using (HttpClient client = new HttpClient())
            {
                Note note = new Note();
                var Noteresponse = await client.GetAsync($"{BaseAddress}/GetNoteById/{NidNote}");
                if (Noteresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await Noteresponse.Content.ReadAsStringAsync();
                    note = JsonConvert.DeserializeObject<Note>(stringResponse) ?? new Note();
                }
                return View(note);
            }
        }
        public async Task<IActionResult> SubmitEditNote(Note note)
        {
            note.ModifiedDate = DateTime.Now;
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(note), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PatchAsync($"{BaseAddress}/EditNote", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["NoteSuccess"] = $"note edited successfully";
                else
                    TempData["NoteError"] = $"an error occured while editing note!";
            }
            return RedirectToAction("NoteGroup", new { NidGroup = note.GroupId });
        }
        public async Task<IActionResult> SubmitDeleteNote(Guid NidNote)
        {
            Note note = new Note();
            using (HttpClient client = new HttpClient())
            {
                var Noteresponse = await client.GetAsync($"{BaseAddress}/GetNoteById/{NidNote}");
                if (Noteresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await Noteresponse.Content.ReadAsStringAsync();
                    note = JsonConvert.DeserializeObject<Note>(stringResponse) ?? new Note();
                }
                var response = await client.DeleteAsync($"{BaseAddress}/DeleteNote/{NidNote}");
                if (response.IsSuccessStatusCode)
                    TempData["NoteSuccess"] = $"note deleted successfully";
                else
                    TempData["NoteError"] = $"an error occured while deleting note!";
            }
            return RedirectToAction("NoteGroup",new { NidGroup = note.GroupId });
        }
        //account section
        public async Task<IActionResult> FinancialRecords(bool IncludeAll = false)
        {
            FinanceViewModel model = new FinanceViewModel();
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Account> accounts = new List<Account>();
                List<Transaction> transactions = new List<Transaction>();
                var response = await client.GetAsync($"{BaseAddress}/GetAccountsByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    accounts = JsonConvert.DeserializeObject<List<Account>>(stringResponse) ?? new List<Account>();
                }
                if(!IncludeAll)
                {
                    var response2 = await client.GetAsync($"{BaseAddress}/GetTransactionsByUserId/{NidUser}");
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse2 = await response2.Content.ReadAsStringAsync();
                        transactions = JsonConvert.DeserializeObject<List<Transaction>>(stringResponse2) ?? new List<Transaction>();
                    }
                }
                else
                {
                    var response2 = await client.GetAsync($"{BaseAddress}/GetTransactionsByUserId/{NidUser}?IncludeAll=true");
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse2 = await response2.Content.ReadAsStringAsync();
                        transactions = JsonConvert.DeserializeObject<List<Transaction>>(stringResponse2) ?? new List<Transaction>();
                    }
                }
                model.Accounts = accounts;
                model.Transactions = transactions;
                model.AllTransactions = IncludeAll;
                return View(model);
            }
        }
        public async Task<IActionResult> SubmitAddAccount(string Title,decimal Amount,bool IsActive) 
        {
            Guid NidUser = Guid.Parse(User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value);
            Account account = new Account() { Amount = Amount, CreateDate = DateTime.Now, IsActive = IsActive, LastModified = DateTime.Now,
            LendAmount = 0, NidAccount = Guid.NewGuid(), Title = Title, UserId = NidUser };
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(account), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddAccount", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["FinanceSuccess"] = $"{account.Title} created successfully";
                else
                    TempData["FinanceError"] = $"an error occured while creating account!";
            }
            return RedirectToAction("FinancialRecords");
        }
        public async Task<IActionResult> SubmitAddTransaction(byte TrType, Guid PayerAccount, Guid RecieverAccount,decimal Amount,string Reason)
        {
            Guid NidUser = Guid.Parse(User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value);
            Transaction tr = new Transaction()
            {
                Amount = Amount,
                CreateDate = DateTime.Now,
                TransactionType = TrType,
                PayerAccount = PayerAccount,
                RecieverAccount = RecieverAccount,
                TransactionReason = Reason,
                NidTransaction = Guid.NewGuid(),
                UserId = NidUser
            };
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(tr), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddTransaction", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["FinanceSuccess"] = $"transaction created successfully";
                else
                    TempData["FinanceError"] = $"an error occured while creating transaction!";
            }
            return RedirectToAction("FinancialRecords");
        }
        public async Task<IActionResult> SubmitEditTransaction(Guid NidTr, byte TrType, Guid PayerAccount, Guid RecieverAccount, decimal Amount, string Reason)
        {
            using (HttpClient client = new HttpClient())
            {
                Transaction tr = new Transaction();
                var trresponse = await client.GetAsync($"{BaseAddress}/GetTransactionById/{NidTr}");
                if (trresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await trresponse.Content.ReadAsStringAsync();
                    tr = JsonConvert.DeserializeObject<Transaction>(stringResponse) ?? new Transaction();
                }
                tr.TransactionType = TrType;
                tr.PayerAccount = PayerAccount;
                tr.RecieverAccount = RecieverAccount;
                tr.Amount = Amount;
                tr.TransactionReason = Reason;
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(tr), Encoding.UTF8, "application/json");
                var response = await client.PatchAsync($"{BaseAddress}/EditTransaction", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["FinanceSuccess"] = $"transaction edited successfully";
                else
                    TempData["FinanceError"] = $"an error occured while editing transaction!";
            }
            return RedirectToAction("FinancialRecords");
        }
        public async Task<IActionResult> SubmitDeleteTransaction(Guid NidTr)
        {
            using (HttpClient client = new HttpClient())
            {
                Transaction tr = new Transaction();
                var trresponse = await client.GetAsync($"{BaseAddress}/GetTransactionById/{NidTr}");
                if (trresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await trresponse.Content.ReadAsStringAsync();
                    tr = JsonConvert.DeserializeObject<Transaction>(stringResponse) ?? new Transaction();
                }
                if(tr.NidTransaction != Guid.Empty)
                {
                    var response = await client.DeleteAsync($"{BaseAddress}/DeleteTransaction/{NidTr}");
                    if (response.IsSuccessStatusCode)
                        TempData["FinanceSuccess"] = $"transaction deleted successfully";
                    else
                        TempData["FinanceError"] = $"an error occured while deleting transaction!";
                }
                else
                    TempData["FinanceError"] = $"an error occured while deleting transaction!";
            }
            return RedirectToAction("FinancialRecords");
        }
        public async Task<IActionResult> GetTrById(Guid NidTransaction)
        {
            using (HttpClient client = new HttpClient())
            {
                Transaction tr = new Transaction();
                var trresponse = await client.GetAsync($"{BaseAddress}/GetTransactionById/{NidTransaction}");
                if (trresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await trresponse.Content.ReadAsStringAsync();
                    tr = JsonConvert.DeserializeObject<Transaction>(stringResponse) ?? new Transaction();
                }
                if(tr.NidTransaction == Guid.Empty)
                    return Json(new { HasValue = false });
                else
                    return Json(new { HasValue = true, NidTr = tr.NidTransaction.ToString(), TrType = tr.TransactionType.ToString(),
                        PAccount = tr.PayerAccount.ToString(),RAccount = tr.RecieverAccount.ToString(),Reason = tr.TransactionReason.ToString(),
                        Amount = ((int)(tr.Amount)).ToString()
                    });
            }
        }
        public async Task<IActionResult> Account(Guid NidAccount)
        {
            FinanceViewModel model = new FinanceViewModel();
            using (HttpClient client = new HttpClient())
            {
                Account account = new Account();
                var response = await client.GetAsync($"{BaseAddress}/GetAccountById/{NidAccount}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    account = JsonConvert.DeserializeObject<Account>(stringResponse) ?? new Account();
                }
                return View(account);
            }
        }
        public async Task<IActionResult> SubmitEditAccount(Account account)
        {
            using (HttpClient client = new HttpClient())
            {
                account.LastModified = DateTime.Now;
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(account), Encoding.UTF8, "application/json");
                var response = await client.PatchAsync($"{BaseAddress}/EditAccount", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["AccountSuccess"] = $"account edited successfully";
                else
                    TempData["AccountError"] = $"an error occured while editing account!";
            }
            return RedirectToAction("Account",new { NidAccount = account.NidAccount });
        }
        public async Task<IActionResult> SubmitDeleteAccount(Guid NidAccount)
        {
            using (HttpClient client = new HttpClient())
            {
                Account acc = new Account();
                var accresponse = await client.GetAsync($"{BaseAddress}/GetAccountById/{NidAccount}");
                if (accresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await accresponse.Content.ReadAsStringAsync();
                    acc = JsonConvert.DeserializeObject<Account>(stringResponse) ?? new Account();
                }
                if (acc.NidAccount != Guid.Empty)
                {
                    var response = await client.DeleteAsync($"{BaseAddress}/DeleteAccount/{NidAccount}");
                    if (response.IsSuccessStatusCode)
                        TempData["FinanceSuccess"] = $"account deleted successfully";
                    else
                        TempData["FinanceError"] = $"an error occured while deleting account!";
                }
                else
                    TempData["FinanceError"] = $"an error occured while deleting account!";
            }
            return RedirectToAction("FinancialRecords");
        }
        //shield section
        public async Task<IActionResult> Shields(bool IncludeAll = false)
        {
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Shield> shields = new List<Shield>();
                var response = await client.GetAsync($"{BaseAddress}/GetShieldsByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    shields = JsonConvert.DeserializeObject<List<Shield>>(stringResponse) ?? new List<Shield>();
                }
                return View(shields);
            }
        }
        public IActionResult AddShield()
        {
            return View();
        }
        public async Task<IActionResult> SubmitAddShield(Shield shield)
        {
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                shield.UserId = Guid.Parse(NidUser);
                shield.CreateDate = DateTime.Now;
                shield.Id = Guid.NewGuid();
                shield.Password = Helpers.Encryption.EncryptString(shield.Password.Trim());
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(shield), Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{BaseAddress}/AddShield",stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["ShieldSuccess"] = $"{shield.Title} created successfully";
                else
                    TempData["ShieldError"] = $"an error occured while creating Shield!";
            }
            return RedirectToAction("Shields");
        }
        public async Task<IActionResult> SubmitEditShield(Shield shield)
        {
            using (HttpClient client = new HttpClient())
            {
                shield.LastModified = DateTime.Now;
                shield.Password = Helpers.Encryption.EncryptString(shield.Password.Trim());
                StringContent stringContent = new StringContent(JsonConvert.SerializeObject(shield), Encoding.UTF8, "application/json");
                var response = await client.PatchAsync($"{BaseAddress}/EditShield", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["ShieldSuccess"] = $"{shield.Title} edited successfully";
                else
                    TempData["ShieldError"] = $"an error occured while editing Shield!";
            }
            return RedirectToAction("Shields");
        }
        public async Task<IActionResult> SubmitDeleteShield(Guid NidShield)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.DeleteAsync($"{BaseAddress}/DeleteShield/{NidShield}");
                if (response.IsSuccessStatusCode)
                    TempData["ShieldSuccess"] = $"Shield deleted successfully";
                else
                    TempData["ShieldError"] = $"an error occured while deleting Shield!";
            }
            return RedirectToAction("Shields");
        }
        public async Task<IActionResult> EditShield(Guid NidShield)
        {
            var shield = new Shield();
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync($"{BaseAddress}/GetShieldById/{NidShield}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    shield = JsonConvert.DeserializeObject<Shield>(stringResponse) ?? new Shield();
                }
            }
            return View(shield);
        }
        public async Task<IActionResult> ShieldDetail(Guid NidShield)
        {
            var shield = new Shield();
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync($"{BaseAddress}/GetShieldById/{NidShield}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    shield = JsonConvert.DeserializeObject<Shield>(stringResponse) ?? new Shield();
                }
            }
            return View(shield);
        }
    }
    public class JsonResults 
    {
        public string? Message { get; set; }
        public string? Html { get; set; }
        public bool HasValue { get; set; }
    }
}