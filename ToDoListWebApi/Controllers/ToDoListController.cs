using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ToDoListWebApi.Models;

namespace ToDoListWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ToDoListController : ControllerBase
    {
        IWebHostEnvironment _webHost;
        public ToDoListController(IWebHostEnvironment webHost)
        {
            _webHost = webHost;
        }
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
            try
            {
                using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
                {
                    return Ok(await toDoListDbContext.Users.Take(PageSize).ToListAsync());
                }
            }
            catch (Exception ex)
            {
                return Ok(ex.Message);
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
                    {
                        user.LastLoginDate = DateTime.Now;
                        try
                        {
                            db.Entry(user).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                        }
                        return Ok(user);
                    }
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
                    var tasks = toDoListDbContext.Tasks.Where(p => p.GoalId == goal.NidGoal).GroupBy(q => q.NidTask).Select(w => w.Key).ToList();
                    var Schedules = toDoListDbContext.Schedules.Where(w => tasks.Contains(w.TaskId)).GroupBy(p => p.NidSchedule).Select(q => q.Key).ToList();
                    if(toDoListDbContext.Progresses.Where(p => Schedules.Contains(p.ScheduleId)).ToList().OrderBy(p => p.CreateDate).FirstOrDefault() != null)
                    {
                        if(toDoListDbContext.Progresses.Any(p => Schedules.Contains(p.ScheduleId)))
                        {
                            var minProgressDate = toDoListDbContext.Progresses.Where(p => Schedules.Contains(p.ScheduleId)).ToList().OrderBy(p => p.CreateDate).FirstOrDefault().CreateDate;
                            if (goal.FromDate.Date <= minProgressDate.Date)
                            {
                                toDoListDbContext.Entry(goal).State = EntityState.Modified;
                                await toDoListDbContext.SaveChangesAsync();
                                return Ok();
                            }
                            else
                                return BadRequest();
                        }
                        else
                        {
                            toDoListDbContext.Entry(goal).State = EntityState.Modified;
                            await toDoListDbContext.SaveChangesAsync();
                            return Ok();
                        }
                    }
                    else
                    {
                        toDoListDbContext.Entry(goal).State = EntityState.Modified;
                        await toDoListDbContext.SaveChangesAsync();
                        return Ok();
                    }
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
            Models.Task task = null;
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                task = toDoListDbContext.Tasks.Where(p => p.NidTask == NidTask).FirstOrDefault();
            }
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
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

        //routine apis
        [HttpGet, Route("GetRoutinesByUserId/{NidUser}")]
        public async Task<IActionResult> Routines([FromRoute] Guid NidUser)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Routines.Where(p => p.UserId == NidUser).ToListAsync());
            }
        }
        [HttpGet, Route("GetRoutinesByDate/{NidUser}")]
        public async Task<IActionResult> RoutinesByDate([FromRoute] Guid NidUser, [FromQuery]DateTime Date)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Routines.Where(p => p.UserId == NidUser && p.FromDate < Date.Date && p.Todate >= Date.Date).ToListAsync());
            }
        }
        [HttpGet, Route("GetRoutineById/{NidRoutine}")]
        public async Task<IActionResult> GetRoutine([FromRoute] Guid NidRoutine)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Routines.Where(p => p.NidRoutine == NidRoutine).FirstOrDefaultAsync());
            }
        }
        [HttpPost, Route("AddRoutine")]
        public async Task<IActionResult> AddRoutine([FromBody] Routine Routine)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                toDoListDbContext.Routines.Add(Routine);
                await toDoListDbContext.SaveChangesAsync();
                return Ok();
            }
        }
        [HttpPatch, Route("EditRoutine")]
        public async Task<IActionResult> EditRoutine([FromBody] Routine Routine)
        {
            using (var toDoListDbContext = new ToDoListDbContext())
            {
                try
                {
                    if(toDoListDbContext.RoutineProgresses.Where(p => p.RoutineId == Routine.NidRoutine).OrderBy(q => q.ProgressDate).FirstOrDefault() != null)
                    {
                        var minProgressDate = toDoListDbContext.RoutineProgresses.Where(p => p.RoutineId == Routine.NidRoutine).OrderBy(q => q.ProgressDate).FirstOrDefault().ProgressDate;
                        if (Routine.FromDate.Date <= minProgressDate.Date)
                        {
                            toDoListDbContext.Entry(Routine).State = EntityState.Modified;
                            await toDoListDbContext.SaveChangesAsync();
                            return Ok();
                        }
                        else
                            return BadRequest();
                    }
                    else
                    {
                        toDoListDbContext.Entry(Routine).State = EntityState.Modified;
                        await toDoListDbContext.SaveChangesAsync();
                        return Ok();
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpDelete, Route("DeleteRoutine/{NidRoutine}")]
        public async Task<IActionResult> DeleteRoutine([FromRoute] Guid NidRoutine)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var Routine = toDoListDbContext.Routines.Where(p => p.NidRoutine == NidRoutine).FirstOrDefault();
                if (Routine == null)
                {
                    return BadRequest();
                }
                else
                {
                    try
                    {
                        toDoListDbContext.Entry(Routine).State = EntityState.Deleted;
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

        //routine progresses apis

        [HttpGet, Route("GetRoutineProgressesByDate")]
        public async Task<IActionResult> RoutineProgresses([FromQuery] DateTime FromDate, [FromQuery] DateTime ToDate)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.RoutineProgresses.Where(p => p.ProgressDate >= FromDate && p.ProgressDate <= ToDate).ToListAsync());
            }
        }
        [HttpGet, Route("GetRoutineProgressById/{NidRoutineProgress}")]
        public async Task<IActionResult> RoutineProgressById([FromRoute] Guid NidRoutineProgress)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.RoutineProgresses.Where(p => p.NidRoutineProgress == NidRoutineProgress).FirstOrDefaultAsync());
            }
        }
        [HttpGet, Route("GetProgressesByRoutineId/{NidRoutine}")]
        public async Task<IActionResult> GetRoutineProgressesByRoutineId([FromRoute] Guid NidRoutine)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.RoutineProgresses.Where(p => p.RoutineId == NidRoutine).ToListAsync());
            }
        }
        [HttpPost, Route("AddRoutineProgress")]
        public async Task<IActionResult> AddRoutineProgress([FromBody] RoutineProgress Routineprogress)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                toDoListDbContext.RoutineProgresses.Add(Routineprogress);
                await toDoListDbContext.SaveChangesAsync();
                return Ok();
            }
        }
        [HttpDelete, Route("DeleteRoutineProgress/{NidRoutineProgress}")]
        public async Task<IActionResult> DeleteRoutineProgress([FromRoute] Guid NidRoutineProgress)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var progress = toDoListDbContext.RoutineProgresses.Where(p => p.NidRoutineProgress == NidRoutineProgress).FirstOrDefault();
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

        //note apis
        [HttpGet, Route("GetNoteGroupsByUserId/{NidUser}")]
        public async Task<IActionResult> NoteGroups([FromRoute] Guid NidUser)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.NoteGroups.Where(p => p.UserId == NidUser).ToListAsync());
            }
        }
        [HttpGet, Route("GetNoteGroupById/{NidGroup}")]
        public async Task<IActionResult> GetNoteGroup([FromRoute] Guid NidGroup)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.NoteGroups.Where(p => p.NidGroup == NidGroup).FirstOrDefaultAsync());
            }
        }
        [HttpPost, Route("AddNoteGroup")]
        public async Task<IActionResult> AddNoteGroup([FromBody] NoteGroup NoteGroup)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                toDoListDbContext.NoteGroups.Add(NoteGroup);
                await toDoListDbContext.SaveChangesAsync();
                return Ok();
            }
        }
        [HttpPatch, Route("EditNoteGroup")]
        public async Task<IActionResult> EditNoteGroup([FromBody] NoteGroup NoteGroup)
        {
            using (var toDoListDbContext = new ToDoListDbContext())
            {
                try
                {
                    toDoListDbContext.Entry(NoteGroup).State = EntityState.Modified;
                    await toDoListDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpDelete, Route("DeleteNoteGroup/{NidGroup}")]
        public async Task<IActionResult> DeleteNoteGroup([FromRoute] Guid NidGroup)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var NoteGroup = toDoListDbContext.NoteGroups.Where(p => p.NidGroup == NidGroup).FirstOrDefault();
                if (NoteGroup == null)
                {
                    return BadRequest();
                }
                else
                {
                    try
                    {
                        toDoListDbContext.Entry(NoteGroup).State = EntityState.Deleted;
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

        [HttpGet, Route("GetNotesByGroupId/{NidGroup}")]
        public async Task<IActionResult> Notes([FromRoute] Guid NidGroup)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Notes.Where(p => p.GroupId == NidGroup).ToListAsync());
            }
        }
        [HttpGet, Route("GetNoteById/{NidNote}")]
        public async Task<IActionResult> GetNote([FromRoute] Guid NidNote)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Notes.Where(p => p.NidNote == NidNote).FirstOrDefaultAsync());
            }
        }
        [HttpPost, Route("AddNote")]
        public async Task<IActionResult> AddNote([FromBody] Note Note)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                toDoListDbContext.Notes.Add(Note);
                await toDoListDbContext.SaveChangesAsync();
                return Ok();
            }
        }
        [HttpPatch, Route("EditNote")]
        public async Task<IActionResult> EditNote([FromBody] Note Note)
        {
            using (var toDoListDbContext = new ToDoListDbContext())
            {
                try
                {
                    toDoListDbContext.Entry(Note).State = EntityState.Modified;
                    await toDoListDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpDelete, Route("DeleteNote/{NidNote}")]
        public async Task<IActionResult> DeleteNote([FromRoute] Guid NidNote)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var Note = toDoListDbContext.Notes.Where(p => p.NidNote == NidNote).FirstOrDefault();
                if (Note == null)
                {
                    return BadRequest();
                }
                else
                {
                    try
                    {
                        toDoListDbContext.Entry(Note).State = EntityState.Deleted;
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

        //account apis
        [HttpGet, Route("GetAccountsByUserId/{NidUser}")]
        public async Task<IActionResult> Accounts([FromRoute] Guid NidUser)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Accounts.Where(p => p.UserId == NidUser).ToListAsync());
            }
        }
        [HttpGet, Route("GetAccountById/{NidAccount}")]
        public async Task<IActionResult> GetAccount([FromRoute] Guid NidAccount)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Accounts.Where(p => p.NidAccount == NidAccount).FirstOrDefaultAsync());
            }
        }
        [HttpPost, Route("AddAccount")]
        public async Task<IActionResult> AddAccount([FromBody] Account account)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                toDoListDbContext.Accounts.Add(account);
                await toDoListDbContext.SaveChangesAsync();
                return Ok();
            }
        }
        [HttpPatch, Route("EditAccount")]
        public async Task<IActionResult> EditAccount([FromBody] Account account)
        {
            using (var toDoListDbContext = new ToDoListDbContext())
            {
                try
                {
                    toDoListDbContext.Entry(account).State = EntityState.Modified;
                    await toDoListDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpDelete, Route("DeleteAccount/{NidAccount}")]
        public async Task<IActionResult> DeleteAccount([FromRoute] Guid NidAccount)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                if (toDoListDbContext.Transactions.Any(p => p.PayerAccount == NidAccount || p.RecieverAccount == NidAccount))
                    return BadRequest();
                else
                {
                    var account = toDoListDbContext.Accounts.Where(p => p.NidAccount == NidAccount).FirstOrDefault();
                    if (account == null)
                    {
                        return BadRequest();
                    }
                    else
                    {
                        try
                        {
                            toDoListDbContext.Entry(account).State = EntityState.Deleted;
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
        }

        //transaction apis
        [HttpGet, Route("GetTransactionsByUserId/{NidUser}")]//95b1684c-e19e-49a9-9806-f165658d103c
        public async Task<IActionResult> Transactions([FromRoute] Guid NidUser, [FromQuery] bool IncludeAll = false)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                if(IncludeAll)
                    return Ok(await db.Transactions.Where(p => p.UserId == NidUser).ToListAsync());
                else
                {
                    PersianCalendar pc = new PersianCalendar();
                    var StartOfMonth = pc.ToDateTime(pc.GetYear(DateTime.Now),pc.GetMonth(DateTime.Now), 1, 0, 0, 0, 0);
                    var EndOfMonth = pc.ToDateTime(pc.GetYear(StartOfMonth.AddMonths(1).AddDays(3)), pc.GetMonth(StartOfMonth.AddMonths(1).AddDays(3)), 1, 0, 0, 0, 0);
                    return Ok(await db.Transactions.Where(p => p.UserId == NidUser && p.CreateDate.Date >= StartOfMonth && p.CreateDate < EndOfMonth).ToListAsync());
                }
            }
        }
        [HttpGet, Route("GetTransactionById/{NidTransaction}")]
        public async Task<IActionResult> GetTransaction([FromRoute] Guid NidTransaction)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Transactions.Where(p => p.NidTransaction == NidTransaction).FirstOrDefaultAsync());
            }
        }
        [HttpPost, Route("AddTransaction")]
        public async Task<IActionResult> AddTransaction([FromBody] Transaction Transaction)
        {
            bool IsAccUpdate = false;
            bool IsConditionOk = false;
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var payer = toDoListDbContext.Accounts.Where(p => p.NidAccount == Transaction.PayerAccount).ToList().FirstOrDefault();
                var reciever = toDoListDbContext.Accounts.Where(p => p.NidAccount == Transaction.RecieverAccount).ToList().FirstOrDefault();
                if (payer != null)
                {
                    if (Transaction.Amount <= payer.Amount)
                    {
                        payer.Amount = payer.Amount - Transaction.Amount;
                        payer.LastModified = DateTime.Now;
                        if (Transaction.TransactionType == 2)
                            payer.LendAmount = payer.LendAmount + Transaction.Amount;
                        if (reciever != null)
                        {
                            reciever.Amount = reciever.Amount + Transaction.Amount;
                            if (Transaction.TransactionType == 3)
                            {
                                if (Transaction.Amount <= reciever.LendAmount)
                                {
                                    reciever.LendAmount = reciever.LendAmount - Transaction.Amount;
                                    IsConditionOk = true;
                                }
                            }
                            else
                                IsConditionOk = true;
                        }
                        if(IsConditionOk)
                        {
                            toDoListDbContext.Entry(payer).State = EntityState.Modified;
                            if (await toDoListDbContext.SaveChangesAsync() >= 1)
                            {
                                reciever.LastModified = DateTime.Now;
                                toDoListDbContext.Entry(reciever).State = EntityState.Modified;
                                if(await toDoListDbContext.SaveChangesAsync() >= 1)
                                    IsAccUpdate = true;
                            }
                        }
                    }
                }
                if (IsAccUpdate)
                {
                    toDoListDbContext.Transactions.Add(Transaction);
                    await toDoListDbContext.SaveChangesAsync();
                    return Ok();
                }
                else
                    return BadRequest();
            }
        }
        [HttpPatch, Route("EditTransaction")]
        public async Task<IActionResult> EditTransaction([FromBody] Transaction Transaction)
        {
            using (var toDoListDbContext = new ToDoListDbContext())
            {
                try
                {
                    toDoListDbContext.Entry(Transaction).State = EntityState.Modified;
                    await toDoListDbContext.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }
        [HttpDelete, Route("DeleteTransaction/{NidTransaction}")]
        public async Task<IActionResult> DeleteTransaction([FromRoute] Guid NidTransaction)
        {
            bool IsAccountOk = false;
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var Transaction = toDoListDbContext.Transactions.Where(p => p.NidTransaction == NidTransaction).FirstOrDefault();
                if (Transaction == null)
                {
                    return BadRequest();
                }
                else
                {
                    try
                    {
                        var payer = toDoListDbContext.Accounts.FirstOrDefault(p => p.NidAccount == Transaction.PayerAccount);
                        var reciever = toDoListDbContext.Accounts.FirstOrDefault(p => p.NidAccount == Transaction.RecieverAccount);
                        if(payer != null && reciever != null) 
                        {
                            switch (Transaction.TransactionType)
                            {
                                case 1://pay
                                    if(reciever.Amount >= Transaction.Amount)
                                    {
                                        reciever.Amount = reciever.Amount - Transaction.Amount;
                                        payer.Amount = payer.Amount + Transaction.Amount;
                                        toDoListDbContext.Entry(reciever).State = EntityState.Modified;
                                        if(await toDoListDbContext.SaveChangesAsync() >= 1)
                                        {
                                            toDoListDbContext.Entry(payer).State = EntityState.Modified;
                                            if (await toDoListDbContext.SaveChangesAsync() >= 1)
                                                IsAccountOk = true;
                                            else
                                            {
                                                reciever.Amount = reciever.Amount + Transaction.Amount;
                                                toDoListDbContext.Entry(reciever).State = EntityState.Modified;
                                                await toDoListDbContext.SaveChangesAsync();
                                            }
                                        }
                                    }
                                    break;
                                case 2://lend
                                    if (reciever.Amount >= Transaction.Amount)
                                    {
                                        reciever.Amount = reciever.Amount - Transaction.Amount;
                                        payer.Amount = payer.Amount + Transaction.Amount;
                                        if (payer.LendAmount >= Transaction.Amount)
                                        {
                                            payer.LendAmount = payer.LendAmount - Transaction.Amount;
                                            toDoListDbContext.Entry(reciever).State = EntityState.Modified;
                                            if (await toDoListDbContext.SaveChangesAsync() >= 1)
                                            {
                                                toDoListDbContext.Entry(payer).State = EntityState.Modified;
                                                if (await toDoListDbContext.SaveChangesAsync() >= 1)
                                                    IsAccountOk = true;
                                                else
                                                {
                                                    reciever.Amount = reciever.Amount + Transaction.Amount;
                                                    toDoListDbContext.Entry(reciever).State = EntityState.Modified;
                                                    await toDoListDbContext.SaveChangesAsync();
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case 3://lend back
                                    if (reciever.Amount >= Transaction.Amount)
                                    {
                                        reciever.Amount = reciever.Amount - Transaction.Amount;
                                        reciever.LendAmount = reciever.LendAmount + Transaction.Amount;
                                        payer.Amount = payer.Amount + Transaction.Amount;
                                        toDoListDbContext.Entry(reciever).State = EntityState.Modified;
                                        if (await toDoListDbContext.SaveChangesAsync() >= 1)
                                        {
                                            toDoListDbContext.Entry(payer).State = EntityState.Modified;
                                            if (await toDoListDbContext.SaveChangesAsync() >= 1)
                                                IsAccountOk = true;
                                            else
                                            {
                                                reciever.Amount = reciever.Amount + Transaction.Amount;
                                                toDoListDbContext.Entry(reciever).State = EntityState.Modified;
                                                await toDoListDbContext.SaveChangesAsync();
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                        if (IsAccountOk)
                        {
                            toDoListDbContext.Entry(Transaction).State = EntityState.Deleted;
                            await toDoListDbContext.SaveChangesAsync();
                            return Ok();
                        }
                        else
                            return BadRequest();
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex);
                    }
                }
            }
        }

        //shields apis
        [HttpGet, Route("GetShieldById/{NidShield}")]
        public async Task<IActionResult> ShieldById([FromRoute] Guid NidShield)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                var shields = await db.Shields.ToListAsync();
                return Ok(shields.FirstOrDefault(p => p.Id == NidShield));
            }
        }
        [HttpGet, Route("GetShieldsByUserId/{NidUser}")]
        public async Task<IActionResult> GetShields([FromRoute] Guid NidUser)
        {
            using (ToDoListDbContext db = new ToDoListDbContext())
            {
                return Ok(await db.Shields.Where(p => p.UserId == NidUser).ToListAsync());
            }
        }
        [HttpPost, Route("AddShield")]
        public async Task<IActionResult> AddShield([FromBody] Models.Shield shield)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                toDoListDbContext.Shields.Add(shield);
                await toDoListDbContext.SaveChangesAsync();
                return Ok();
            }
        }
        [HttpPatch, Route("EditShield")]
        public async Task<IActionResult> EditShield([FromBody] Models.Shield shield)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var shields = await toDoListDbContext.Shields.ToListAsync();
                var shield2 = shields.FirstOrDefault(p => p.Id == shield.Id);
                if (shield == null)
                {
                    return BadRequest();
                }
                else
                {
                    try
                    {
                        toDoListDbContext.Entry(shield2).State = EntityState.Deleted;
                        await toDoListDbContext.SaveChangesAsync();
                        shield.Id = Guid.NewGuid();
                        toDoListDbContext.Add(shield);
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
        [HttpDelete, Route("DeleteShield/{NidShield}")]
        public async Task<IActionResult> DeleteShield([FromRoute] Guid NidShield)
        {
            using (ToDoListDbContext toDoListDbContext = new ToDoListDbContext())
            {
                var shields = await toDoListDbContext.Shields.ToListAsync();
                var shield = shields.FirstOrDefault(p => p.Id == NidShield);
                if (shield == null)
                {
                    return BadRequest();
                }
                else
                {
                    try
                    {
                        toDoListDbContext.Entry(shield).State = EntityState.Deleted;
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
