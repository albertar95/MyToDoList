using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;
using ToDoListWebApi.Models;

namespace ToDoListWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoListController : ControllerBase
    {

        //user apis

        [HttpPost,Route("AddUser")]
        public async Task<IActionResult> AddUser([FromBody] User user) 
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                toDoListDbContext.Users.Add(user);
                await toDoListDbContext.SaveChangesAsync();
                return CreatedAtAction("Users", new { NidUser = user.NidUser });
            }
        }
        [HttpGet,Route("GetUsers")]
        public async Task<IActionResult> Users([FromQuery] int PageSize = 100) 
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                return Ok(await toDoListDbContext.Users.Take(PageSize).ToListAsync());
            }
        }
        [HttpPut, Route("EditUser")]
        public async Task<IActionResult> EditUser([FromBody] User user) 
        {
            using (var toDoListDbContext = new ToDoListDbContext())
            {
                try
                {
                    toDoListDbContext.Entry(user).State = EntityState.Modified;
                    await toDoListDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            } 
        }
        [HttpDelete,Route("DeleteUser/{NidUser}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid NidUser) 
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var user = toDoListDbContext.Users.Where(p => p.NidUser == NidUser).FirstOrDefault();
                if (user == null)
                {
                    return BadRequest("user not found");
                }
                else
                {
                    try
                    {
                        toDoListDbContext.Entry(user).State = EntityState.Deleted;
                        await toDoListDbContext.SaveChangesAsync();
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex);
                    }
                }
            }
        }
        [HttpGet, Route("LoginUser")]
        public IActionResult LoginUser([FromQuery] string Username, [FromQuery] string Password) 
        {
            using (ToDoListDbContext db = new ToDoListDbContext()) 
            {
                var user = db.Users.FirstOrDefault(p => p.Username == Username);
                if (user == null)
                    return BadRequest();
                else 
                {
                    if (user.Password == EncryptString(Password))
                        return Ok(user);
                    else
                        return BadRequest();
                }
            }
        }

        //goal apis

        [HttpGet, Route("GetGoalsByUserId/{NidUser}")]
        public async Task<IActionResult> Goals([FromRoute] Guid NidUser) 
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Goals.Where(p => p.UserId == NidUser).ToListAsync());
            }
        }
        [HttpGet, Route("GetGoalById/{NidGoal}")]
        public async Task<IActionResult> GetGoal([FromRoute] Guid NidGoal)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Goals.Where(p => p.NidGoal == NidGoal).FirstOrDefaultAsync());
            }
        }
        [HttpPost, Route("AddGoal")]
        public async Task<IActionResult> AddGoal([FromBody] Goal goal)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                toDoListDbContext.Goals.Add(goal);
                await toDoListDbContext.SaveChangesAsync();
                return Ok();
            } 
        }
        [HttpPatch, Route("EditGoal")]
        public async Task<IActionResult> EditGoal([FromBody] Goal goal)
        {
            using (var toDoListDbContext = new ToDoListDbContext())
            {
                try
                {
                    toDoListDbContext.Entry(goal).State = EntityState.Modified;
                    await toDoListDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpDelete, Route("DeleteGoal/{NidGoal}")]
        public async Task<IActionResult> DeleteGoal([FromRoute] Guid NidGoal)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var goal = toDoListDbContext.Goals.Where(p => p.NidGoal == NidGoal).FirstOrDefault();
                if (goal == null)
                {
                    return BadRequest();
                }
                else
                {
                    try
                    {
                        toDoListDbContext.Entry(goal).State = EntityState.Deleted;
                        await toDoListDbContext.SaveChangesAsync();
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex);
                    }
                }
            }
        }

        //tasks apis

        [HttpGet, Route("GetTasksByUserId/{NidUser}")]
        public async Task<IActionResult> Tasks([FromRoute] Guid NidUser)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Tasks.Where(p => p.UserId == NidUser).ToListAsync());
            }
        }
        [HttpGet, Route("GetTaskById/{NidTask}")]
        public async Task<IActionResult> TaskById([FromRoute] Guid NidTask)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Tasks.Where(p => p.NidTask == NidTask).FirstOrDefaultAsync());
            }
        }
        [HttpGet, Route("GetTasksByGoalId/{NidGoal}")]
        public async Task<IActionResult> GetTasks([FromRoute] Guid NidGoal)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Tasks.Where(p => p.GoalId == NidGoal).ToListAsync());
            }
        }
        [HttpPost, Route("AddTask")]
        public async Task<IActionResult> AddTask([FromBody] Models.Task task)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                toDoListDbContext.Tasks.Add(task);
                await toDoListDbContext.SaveChangesAsync();
                return Ok();
            }
        }
        [HttpPatch, Route("EditTask")]
        public async Task<IActionResult> EditTask([FromBody] Models.Task task)
        {
            using (var toDoListDbContext = new ToDoListDbContext())
            {
                try
                {
                    toDoListDbContext.Entry(task).State = EntityState.Modified;
                    await toDoListDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpDelete, Route("DeleteTask/{NidTask}")]
        public async Task<IActionResult> DeleteTask([FromRoute] Guid NidTask)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var task = toDoListDbContext.Tasks.Where(p => p.NidTask == NidTask).FirstOrDefault();
                if (task == null)
                {
                    return BadRequest();
                }
                else
                {
                    try
                    {
                        toDoListDbContext.Entry(task).State = EntityState.Deleted;
                        await toDoListDbContext.SaveChangesAsync();
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex);
                    }
                }
            }
        }

        //schedules apis
        [HttpGet, Route("GetScheduleById/{NidSchedule}")]
        public async Task<IActionResult> ScheduleById([FromRoute] Guid NidSchedule)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Schedules.Where(p => p.NidSchedule == NidSchedule).FirstOrDefaultAsync());
            }
        }
        [HttpGet, Route("GetScheduleByTaskId/{NidTask}")]
        public async Task<IActionResult> GetSchedules([FromRoute] Guid NidTask)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Schedules.Where(p => p.TaskId == NidTask).ToListAsync());
            }
        }
        [HttpPost, Route("AddSchedule")]
        public async Task<IActionResult> AddSchedule([FromBody] Models.Schedule schedule)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                toDoListDbContext.Schedules.Add(schedule);
                await toDoListDbContext.SaveChangesAsync();
                return Ok();
            }
        }
        [HttpPatch, Route("EditSchedule")]
        public async Task<IActionResult> EditSchedule([FromBody] Models.Schedule schedule)
        {
            using (var toDoListDbContext = new ToDoListDbContext())
            {
                try
                {
                    toDoListDbContext.Entry(schedule).State = EntityState.Modified;
                    await toDoListDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpDelete, Route("DeleteSchedule/{NidSchedule}")]
        public async Task<IActionResult> DeleteSchedule([FromRoute] Guid NidSchedule)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var schedule = toDoListDbContext.Schedules.Where(p => p.NidSchedule == NidSchedule).FirstOrDefault();
                if (schedule == null)
                {
                    return BadRequest();
                }
                else
                {
                    try
                    {
                        toDoListDbContext.Entry(schedule).State = EntityState.Deleted;
                        await toDoListDbContext.SaveChangesAsync();
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex);
                    }
                }
            }
        }
        //progresses apis

        [HttpGet, Route("GetProgressesByUserId/{NidUser}")]
        public async Task<IActionResult> Progresses([FromRoute] Guid NidUser)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Progresses.Where(p => p.UserId == NidUser).ToListAsync());
            }
        }
        [HttpGet, Route("GetProgressById/{NidProgress}")]
        public async Task<IActionResult> ProgressById([FromRoute] Guid NidProgress)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Progresses.Where(p => p.NidProgress == NidProgress).FirstOrDefaultAsync());
            }
        }
        [HttpGet, Route("GetProgressesByScheduleId/{NidSchedule}")]
        public async Task<IActionResult> GetProgressesByScheduleId([FromRoute] Guid NidSchedule)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Progresses.Where(p => p.ScheduleId == NidSchedule).ToListAsync());
            }
        }
        [HttpPost, Route("AddProgress")]
        public async Task<IActionResult> AddProgress([FromBody] Progress progress)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                toDoListDbContext.Progresses.Add(progress);
                await toDoListDbContext.SaveChangesAsync();
                return Ok();
            }
        }
        [HttpPatch, Route("EditProgress")]
        public async Task<IActionResult> EditProgress([FromBody] Progress progress)
        {
            using (var toDoListDbContext = new ToDoListDbContext())
            {
                try
                {
                    toDoListDbContext.Entry(progress).State = EntityState.Modified;
                    await toDoListDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpDelete, Route("DeleteProgress/{NidProgress}")]
        public async Task<IActionResult> DeleteProgress([FromRoute] Guid NidProgress)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var progress = toDoListDbContext.Progresses.Where(p => p.NidProgress == NidProgress).FirstOrDefault();
                if (progress == null)
                {
                    return BadRequest("user not found");
                }
                else
                {
                    try
                    {
                        toDoListDbContext.Entry(progress).State = EntityState.Deleted;
                        await toDoListDbContext.SaveChangesAsync();
                        return Ok();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex);
                    }
                }
            }
        }

        //generals

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
}
