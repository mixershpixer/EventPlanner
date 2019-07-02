using EventPlanner.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EventPlanner.Controllers
{
    public class EventController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ApplicationDbContext db = new ApplicationDbContext();
        public ApplicationUser CurrentUser { get; set; }

        public EventController()
        {
        }

        public EventController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Event
        public ActionResult Index()
        {
            return View();
        }

        // GET: Event
        [HttpGet]
        public ActionResult NewEvent()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Account");
            else
            {
                List<UserEvent> EventUser = new List<UserEvent>();
                EventUser.AddRange(db.UserEvents);
                ViewBag.EventUser = EventUser;
                List<Field> fields = new List<Field>();
                fields.AddRange(db.Fields);
                ViewBag.Fields = fields;
                List<Event> events = new List<Event>();
                ApplicationUser cur_user = db.Users.Find(User.Identity.GetUserId());
                foreach (Event e in db.Events)
                {
                    if (cur_user.CreatedEvents.Contains(e))
                    {
                        events.Add(e);
                    }
                }
                ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
                ViewBag.UserId = User.Identity.GetUserId();
                ViewBag.Events = events;
                return View();
            }
        }

        [HttpGet]
        public ActionResult AllEvents()
        {
            List<UserEvent> EventUser = new List<UserEvent>();
            EventUser.AddRange(db.UserEvents);
            ViewBag.EventUser = EventUser;
            List<Field> fields = new List<Field>();
            fields.AddRange(db.Fields);
            ViewBag.Fields = fields;
            List<Event> events = new List<Event>();
            foreach (Event ev in db.Events)
            {
                events.Add(ev);
            }
            ViewBag.IsAuthenticated = User.Identity.IsAuthenticated;
            ViewBag.UserId = User.Identity.GetUserId();
            ViewBag.Events = events;
            return View();
        }

        [HttpPost]
        public ActionResult CreateNewEvent(Event model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Event new_event = new Event { Title = model.Title, MaxUsers = model.MaxUsers, SignedUsersCount = model.SignedUsersCount, Fields = model.Fields };
                    List<Field> fields = model.Fields.ToList();
                    foreach (Field item in fields)
                    {

                        item.Event = new_event;
                        new_event.Fields.Add(item);
                    }

                    new_event.UserId = this.User.Identity.GetUserId();
                    this.db.Events.Add(new_event);
                    ApplicationUser cur_user = this.db.Users.Find(this.User.Identity.GetUserId());
                    cur_user.CreatedEvents.Add(new_event);
                    this.db.SaveChanges();
                }
                return RedirectToAction("NewEvent");
            }
            catch (Exception ex)
            {
                ViewBag.Title = ex.Message;
                return RedirectToAction("NewEvent");
            }
        }

        [HttpPost]
        public ActionResult SignProjectAsync(string id)
        {
            try
            {
                Event current_event = this.db.Events.Find(Convert.ToInt32(Request.Params[0]));
                var Sign = new UserEvent { EventId = current_event.Id, UserId = this.User.Identity.GetUserId() };
                if (this.db.UserEvents.Any(e => e.EventId == Sign.EventId && e.UserId == Sign.UserId))
                {
                    this.db.UserEvents.Remove(this.db.UserEvents.Find(Sign.EventId, Sign.UserId));
                    if (current_event.SignedUsersCount > 0)
                    {
                        this.db.Events.Find(current_event.Id).SignedUsersCount -= 1;
                        this.db.Events.Find(current_event.Id).EventUsersSigned.Remove(Sign);
                        this.db.SaveChanges();
                    }
                }
                else
                {
                    this.db.UserEvents.Add(Sign);
                    this.db.SaveChanges();
                    if (this.db.Events.Any(p => p.EventUsersSigned.Any(f => f.EventId == current_event.Id)))
                    {
                        if (current_event.MaxUsers != current_event.SignedUsersCount)
                        {
                            this.db.Events.Find(current_event.Id).SignedUsersCount += 1;
                            this.db.Events.Find(current_event.Id).EventUsersSigned.Add(Sign);
                            this.db.SaveChanges();
                        }
                    }
                }
                this.ViewBag.IsAuthenticated = this.User.Identity.IsAuthenticated;
                this.ViewBag.UserId = this.User.Identity.GetUserId();
                List<UserEvent> EventUser = new List<UserEvent>();
                EventUser.AddRange(this.db.UserEvents);
                this.ViewBag.EventUser = EventUser;
                this.ViewBag.Fields = this.db.Fields;

                return PartialView("EventViewItem", current_event);
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return PartialView();
            }
        }
    }
}