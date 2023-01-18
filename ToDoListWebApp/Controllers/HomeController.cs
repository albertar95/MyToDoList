using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ToDoListWebApp.Models;

namespace ToDoListWebApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static string BaseAddress = "http://192.168.1.10:8061/api/todolist";
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
        public IActionResult Login()
        {
            return View("Login");
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
        public async Task<IActionResult> SubmitLogin(string Username, string Password)
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
                        new Claim("Profile",""),
                        new Claim("NidUser",user.NidUser.ToString()),
                        new Claim("Role", "Admin"),
                     };
                    }

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties { IsPersistent = true };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["LoginError"] = "error occured in login.try again";
                    return RedirectToAction("Login");
                }
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
        public async Task<IActionResult> Goals(byte goalType = 0)
        {
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Goal> goals = new List<Goal>();
                List<Models.Progress> progresses = new List<Models.Progress>();
                List<Models.Task> tasks = new List<Models.Task>();
                switch (goalType)
                {
                    case 0:
                        var response = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}");
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var stringResponse = await response.Content.ReadAsStringAsync();
                            goals = JsonConvert.DeserializeObject<List<Goal>>(stringResponse) ?? new List<Goal>();
                        }
                        break;
                        case 1:
                        var choreResponse = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}?GoalType=1");
                        if (choreResponse.StatusCode == HttpStatusCode.OK)
                        {
                            var choreStringResponse = await choreResponse.Content.ReadAsStringAsync();
                            goals = JsonConvert.DeserializeObject<List<Goal>>(choreStringResponse) ?? new List<Goal>();
                        }
                        break;
                    default:
                        break;
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
                    var ProgressResponse = await client.GetAsync($"{BaseAddress}/GetProgressesByGoalId/{g.NidGoal}");
                    if (ProgressResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse3 = await ProgressResponse.Content.ReadAsStringAsync();
                        var tmpProgresses = JsonConvert.DeserializeObject<List<Models.Progress>>(stringResponse3) ?? new List<Models.Progress>();
                        foreach (var p in tmpProgresses)
                        {
                            progresses.Add(p);
                        }
                    }
                }
                return View(new ToDoListWebApp.ViewModels.GoalViewModel() {  Goals = goals, Tasks = tasks, Progresses = progresses});
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
            if(goal.GoalType == 0)
            return RedirectToAction("Goals");
            else
                return RedirectToAction("Goals",new {goalType = 1});
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
        public async Task<IActionResult> SubmitAddTask(string NidGoal,string Title,string Estimate,string Description = "")
        {
            string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
            Models.Task newtask = new Models.Task() {  CreateDate = DateTime.Now, Description = Description, EstimateTime = Int16.Parse(Estimate), GoalId = Guid.Parse(NidGoal), LastModifiedDate = DateTime.Now, NidTask = Guid.NewGuid(), TaskStatus = false, Title = Title, UserId = Guid.Parse(NidUser)};
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
        public async Task<IActionResult> SubmitAddProgressFromGoal(string NidGoal, string NidTask, string ProgressTime, string CreateDate,string Description)
        {
            string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
            Models.Progress newProgress = new Models.Progress();
            newProgress.CreateDate = DateTime.Parse(CreateDate);
            newProgress.Description = Description;
            newProgress.GoalId = Guid.Parse(NidGoal);
            newProgress.NidProgress = Guid.NewGuid();
            newProgress.ProgressTime = Int16.Parse(ProgressTime);
            newProgress.TaskId = Guid.Parse(NidTask.Replace("'",""));
            newProgress.UserId = Guid.Parse(NidUser);
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(newProgress), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddProgress", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["GoalPageSuccess"] = $"progress created successfully";
                else
                    TempData["GoalPageError"] = $"an error occured while creating progress!";
            }
            return RedirectToAction("Goal", new { NidGoal = NidGoal });
        }
        public async Task<IActionResult> DeleteTask(Guid NidTask,Guid NidGoal)
        {
            using (HttpClient client = new HttpClient())
            {
                List<Models.Progress> progresses = new List<Models.Progress>();
                var ProgressResponse = await client.GetAsync($"{BaseAddress}/GetProgressesByTaskId/{NidTask}");
                if (ProgressResponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse3 = await ProgressResponse.Content.ReadAsStringAsync();
                    progresses = JsonConvert.DeserializeObject<List<Models.Progress>>(stringResponse3) ?? new List<Models.Progress>();
                }
                foreach (var prg in progresses)
                {
                    await client.DeleteAsync($"{BaseAddress}/DeleteProgress/{prg.NidProgress}");
                }
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
        public async Task<IActionResult> SubmitEditTask(string NidTask, Guid NidGoal,string Title,string Estimate)
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
                    task.EstimateTime = Int16.Parse(Estimate);
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
            if (goal.GoalType == 0)
                return RedirectToAction("Goals");
            else
                return RedirectToAction("Goals", new { goalType = 1 });
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
                    List<Models.Progress> progresses = new List<Models.Progress>();
                List<Models.Task> tasks = new List<Models.Task>();
                var ProgressResponse = await client.GetAsync($"{BaseAddress}/GetProgressesByGoalId/{NidGoal}");
                if (ProgressResponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse3 = await ProgressResponse.Content.ReadAsStringAsync();
                    progresses = JsonConvert.DeserializeObject<List<Models.Progress>>(stringResponse3) ?? new List<Models.Progress>();
                }
                foreach (var prg in progresses)
                {
                    await client.DeleteAsync($"{BaseAddress}/DeleteProgress/{prg.NidProgress}");
                }
                var TaskResponse = await client.GetAsync($"{BaseAddress}/GetTasksByGoalId/{NidGoal}");
                if (TaskResponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse2 = await TaskResponse.Content.ReadAsStringAsync();
                    tasks = JsonConvert.DeserializeObject<List<Models.Task>>(stringResponse2) ?? new List<Models.Task>();
                }
                foreach (var tsk in tasks)
                {
                    await client.DeleteAsync($"{BaseAddress}/DeleteTask/{tsk.NidTask}");
                }
                var response = await client.DeleteAsync($"{BaseAddress}/DeleteGoal/{NidGoal}");
                if (response.IsSuccessStatusCode)
                    TempData["GoalSuccess"] = $"goal deleted successfully";
                else
                    TempData["GoalError"] = $"an error occured while deleting goal!";
            }
            if (goal.GoalType == 0)
                return RedirectToAction("Goals");
            else
                return RedirectToAction("Goals", new { goalType = 1 });
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
            if (goal.GoalType == 0)
                return RedirectToAction("Goals");
            else
                return RedirectToAction("Goals", new { goalType = 1 });
        }
        //progress section
        public async Task<IActionResult> Progress()
        {
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Goal> goals = new List<Goal>();
                List<Models.Progress> progresses = new List<Models.Progress>();
                List<Models.Task> tasks = new List<Models.Task>();
                var response = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    var tmpGoals = JsonConvert.DeserializeObject<List<Goal>>(stringResponse) ?? new List<Goal>();
                    foreach (var g in tmpGoals)
                    {
                        goals.Add(g);
                    }
                }
                var gresponse = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}?GoalStatus=1");
                if (gresponse.StatusCode == HttpStatusCode.OK)
                {
                    var gstringResponse = await gresponse.Content.ReadAsStringAsync();
                    var tmpGoals = JsonConvert.DeserializeObject<List<Goal>>(gstringResponse) ?? new List<Goal>();
                    foreach (var g in tmpGoals)
                    {
                        goals.Add(g);
                    }
                }
                var gresponse2 = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}?GoalType=1");
                if (gresponse2.StatusCode == HttpStatusCode.OK)
                {
                    var gstringResponse2 = await gresponse2.Content.ReadAsStringAsync();
                    var tmpGoals = JsonConvert.DeserializeObject<List<Goal>>(gstringResponse2) ?? new List<Goal>();
                    foreach (var g in tmpGoals)
                    {
                        goals.Add(g);
                    }
                }
                var gresponse3 = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}?GoalType=1&GoalStatus=1");
                if (gresponse3.StatusCode == HttpStatusCode.OK)
                {
                    var gstringResponse3 = await gresponse3.Content.ReadAsStringAsync();
                    var tmpGoals = JsonConvert.DeserializeObject<List<Goal>>(gstringResponse3) ?? new List<Goal>();
                    foreach (var g in tmpGoals)
                    {
                        goals.Add(g);
                    }
                }
                var response2 = await client.GetAsync($"{BaseAddress}/GetTasksByUserId/{NidUser}");
                if (response2.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse2 = await response2.Content.ReadAsStringAsync();
                    tasks = JsonConvert.DeserializeObject<List<Models.Task>>(stringResponse2) ?? new List<Models.Task>();
                }
                var ProgressResponse = await client.GetAsync($"{BaseAddress}/GetProgressesByUserId/{NidUser}");
                if (ProgressResponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse3 = await ProgressResponse.Content.ReadAsStringAsync();
                    progresses = JsonConvert.DeserializeObject<List<Models.Progress>>(stringResponse3) ?? new List<Models.Progress>();
                }
                return View(new ToDoListWebApp.ViewModels.GoalViewModel() { Goals = goals, Tasks = tasks, Progresses = progresses });
            }
        }
        public async Task<IActionResult> AddProgress() 
        {
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Goal> goals = new List<Goal>();
                List<Models.Progress> progresses = new List<Models.Progress>();
                List<Models.Task> tasks = new List<Models.Task>();
                var response = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    goals.AddRange(JsonConvert.DeserializeObject<List<Goal>>(stringResponse) ?? new List<Goal>());
                }
                var choreresponse = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}?GoalType=1");
                if (choreresponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse3 = await choreresponse.Content.ReadAsStringAsync();
                    goals.AddRange(JsonConvert.DeserializeObject<List<Goal>>(stringResponse3) ?? new List<Goal>());
                }
                var response2 = await client.GetAsync($"{BaseAddress}/GetTasksByUserId/{NidUser}");
                if (response2.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse2 = await response2.Content.ReadAsStringAsync();
                    tasks = JsonConvert.DeserializeObject<List<Models.Task>>(stringResponse2) ?? new List<Models.Task>();
                }
                return View(new ToDoListWebApp.ViewModels.GoalViewModel() { Goals = goals, Tasks = tasks });
            }
        }
        public async Task<IActionResult> EditProgress(Guid NidProgress)
        {
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Goal> goals = new List<Goal>();
                Models.Progress progress = new Models.Progress();
                List<Models.Task> tasks = new List<Models.Task>();
                var response = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    goals = JsonConvert.DeserializeObject<List<Goal>>(stringResponse) ?? new List<Goal>();
                }
                var response2 = await client.GetAsync($"{BaseAddress}/GetTasksByUserId/{NidUser}");
                if (response2.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse2 = await response2.Content.ReadAsStringAsync();
                    tasks = JsonConvert.DeserializeObject<List<Models.Task>>(stringResponse2) ?? new List<Models.Task>();
                }
                var ProgressResponse = await client.GetAsync($"{BaseAddress}/GetProgressById/{NidProgress}");
                if (ProgressResponse.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse3 = await ProgressResponse.Content.ReadAsStringAsync();
                    progress = JsonConvert.DeserializeObject<Models.Progress>(stringResponse3) ?? new Models.Progress();
                }
                return View(new ToDoListWebApp.ViewModels.GoalViewModel() { Goals = goals, Tasks = tasks, Progress = progress });
            }
        }
        public async Task<IActionResult> SubmitAddProgress(Models.Progress progress) 
        {
            string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
            progress.NidProgress = Guid.NewGuid();
            progress.UserId = Guid.Parse(NidUser);
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(progress), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PostAsync($"{BaseAddress}/AddProgress", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["ProgressSuccess"] = $"progress created successfully";
                else
                    TempData["ProgressError"] = $"an error occured while creating progress!";
            }
            return RedirectToAction("Progress");
        }
        public async Task<IActionResult> SubmitEditProgress(Models.Progress progress) 
        {
            StringContent stringContent = new StringContent(JsonConvert.SerializeObject(progress), Encoding.UTF8, "application/json");
            using (HttpClient client = new HttpClient())
            {
                var response = await client.PatchAsync($"{BaseAddress}/EditProgress", stringContent);
                if (response.IsSuccessStatusCode)
                    TempData["ProgressSuccess"] = $"progress edited successfully";
                else
                    TempData["ProgressError"] = $"an error occured while editing progress!";
            }
            return RedirectToAction("Progress");
        }
        public async Task<IActionResult> SubmitDeleteProgress(string NidProgress)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                var response = await httpClient.DeleteAsync($"{BaseAddress}/DeleteProgress/{NidProgress}");
                if (response.IsSuccessStatusCode)
                    TempData["ProgressSuccess"] = "progress deleted successfully";
                else
                    TempData["ProgressError"] = "an error occured while deleting progress!";
                return RedirectToAction("Progress");
            }
        }

        //chores section

        //generals
        public async Task<IActionResult> Index()
        {
            string[] DatePeriod = new string[3];
            var weekDates = Helpers.Dates.GetWeekPeriod();
            DatePeriod[0] = $"{weekDates.Item1.ToString("dd/MM/yyyy")} - {weekDates.Item2.ToString("dd/MM/yyyy")}";
            DatePeriod[1] = "-1";
            DatePeriod[2] = "1";
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Goal> goals = new List<Goal>();
                List<Models.Progress> progress = new List<Models.Progress>();
                List<Models.Task> tasks = new List<Models.Task>();
                List<Models.Task> alltasks = new List<Models.Task>();
                var response = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    var TmpGoals = JsonConvert.DeserializeObject<List<Goal>>(stringResponse) ?? new List<Goal>();
                    goals = TmpGoals.Where(p => p.CreateDate <= weekDates.Item2).ToList();
                }
                foreach (var goal in goals)
                {
                    var response2 = await client.GetAsync($"{BaseAddress}/GetTasksByGoalId/{goal.NidGoal}");
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse2 = await response2.Content.ReadAsStringAsync();
                        var tmpTask = JsonConvert.DeserializeObject<List<Models.Task>>(stringResponse2) ?? new List<Models.Task>();
                        foreach (var task in tmpTask)
                        {
                            alltasks.Add(task);
                            if (!task.TaskStatus && goal.GoalStatus == 0)
                                tasks.Add(task);
                        }
                    }
                    var ProgressResponse = await client.GetAsync($"{BaseAddress}/GetProgressesByGoalId/{goal.NidGoal}");
                    if (ProgressResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse3 = await ProgressResponse.Content.ReadAsStringAsync();
                        var tmpProgress = JsonConvert.DeserializeObject<List<Models.Progress>>(stringResponse3) ?? new List<Models.Progress>();
                        foreach (var pr in tmpProgress)
                        {
                            progress.Add(pr);
                        }
                    }
                }
                return View(new ViewModels.IndexViewModel() {  DatePeriodInfo = DatePeriod, Goals = goals, Progresses = progress.Where(p => p.CreateDate >= weekDates.Item1 && p.CreateDate <= weekDates.Item2), Tasks = tasks, AllProgresses = progress, AllTasks = alltasks });
            }
        }
        public async Task<IActionResult> IndexPagination(int Direction) 
        {
            string[] DatePeriod = new string[3];
            var weekDates = Helpers.Dates.GetWeekPeriod(Direction);
            DatePeriod[0] = $"{weekDates.Item1.ToString("dd/MM/yyyy")} - {weekDates.Item2.ToString("dd/MM/yyyy")}";
            DatePeriod[1] = $"{Direction - 1}";
            DatePeriod[2] = $"{Direction + 1}";
            using (HttpClient client = new HttpClient())
            {
                string NidUser = User.Claims.Where(p => p.Type == "NidUser").FirstOrDefault().Value;
                List<Goal> goals = new List<Goal>();
                List<Models.Progress> progress = new List<Models.Progress>();
                List<Models.Task> tasks = new List<Models.Task>();
                List<Models.Task> alltasks = new List<Models.Task>();
                var response = await client.GetAsync($"{BaseAddress}/GetGoalsByUserId/{NidUser}");
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    var TmpGoals = JsonConvert.DeserializeObject<List<Goal>>(stringResponse) ?? new List<Goal>();
                    goals = TmpGoals.Where(p => p.CreateDate <= weekDates.Item2).ToList();
                }
                foreach (var goal in goals)
                {
                    var response2 = await client.GetAsync($"{BaseAddress}/GetTasksByGoalId/{goal.NidGoal}");
                    if (response2.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse2 = await response2.Content.ReadAsStringAsync();
                        var tmpTask = JsonConvert.DeserializeObject<List<Models.Task>>(stringResponse2) ?? new List<Models.Task>();
                        foreach (var task in tmpTask)
                        {
                            alltasks.Add(task);
                            if (!task.TaskStatus && goal.GoalStatus == 0)
                                tasks.Add(task);
                        }
                    }
                    var ProgressResponse = await client.GetAsync($"{BaseAddress}/GetProgressesByGoalId/{goal.NidGoal}");
                    if (ProgressResponse.StatusCode == HttpStatusCode.OK)
                    {
                        var stringResponse3 = await ProgressResponse.Content.ReadAsStringAsync();
                        var tmpProgress = JsonConvert.DeserializeObject<List<Models.Progress>>(stringResponse3) ?? new List<Models.Progress>();
                        foreach (var pr in tmpProgress)
                        {
                            progress.Add(pr);
                        }
                    }
                }
                return Json(new JsonResults() { HasValue = true, Html = await Helpers.RenderViewToString.RenderViewAsync(this, "_IndexPartialView", new ViewModels.IndexViewModel() { DatePeriodInfo = DatePeriod, Goals = goals, Progresses = progress.Where(p => p.CreateDate >= weekDates.Item1 && p.CreateDate <= weekDates.Item2), Tasks = tasks, AllProgresses = progress, AllTasks = alltasks }, true) });
            }
        }


        public IActionResult Charts()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
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
    }
    public class JsonResults 
    {
        public string? Message { get; set; }
        public string? Html { get; set; }
        public bool HasValue { get; set; }
    }
}