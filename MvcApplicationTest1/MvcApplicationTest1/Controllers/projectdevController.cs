using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplicationTest1.DAL;
using System.Web.Security;

namespace MvcApplicationTest1.Controllers
{
    public class projectdevController : Controller
    {
        private ftestEntities db = new ftestEntities();

        //
        // GET: /projectdev/
        [Authorize]
        public ActionResult Index(int pid = -1)
        {   //for checking if the user allowed
            if (User.IsInRole("developer"))
            {
                var adminproj = db.pojectdevs.Select(x => x).Where(x => x.projectid == pid && x.devname == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return RedirectToAction("Index", "project");
                }

            }

            if (User.IsInRole("projectowner"))
            {

                var adminproj = db.projects.Select(x => x).Where(x => x.id == pid && x.projectowner == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return RedirectToAction("Index", "project");
                }
            }

            if (User.IsInRole("teamleader"))
            {
                var adminproj = db.projects.Select(x => x).Where(x => x.id == pid && x.projectleader == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return RedirectToAction("Index", "project");
                }

            }

            var pojectdevs = db.pojectdevs.Include(p => p.project);
            ViewBag.projid = pid;
            if (pid == -1) { return View(pojectdevs.ToList()); }
            // to display the developers in the project
            return View(pojectdevs.Select(x =>x).Where(x => x.projectid == pid).ToList());
           
        }

        //
        // GET: /projectdev/Details/5

        public ActionResult Details(int id = 0)
        {
            pojectdev pojectdev = db.pojectdevs.Find(id);
            if (pojectdev == null)
            {
                return HttpNotFound();
            }
            return View(pojectdev);
        }

        public int numberofassignedissues (String Name,int id=0)
        {   
            // to display the number of assigned issues to the user 
          
            var xx = db.issues.Select(x => x).Where(x =>x.assignee == Name && x.projectid == id).GroupBy(y => y.keyname).Select(z => z.OrderByDescending(q => q.sprintid).FirstOrDefault());

            return xx.Count();
        }

        //
        // GET: /projectdev/Create
        [Authorize(Roles = "admin, teamleader")]
        public ActionResult Create(int pid = 0)
        {
            //for checking if the user allowed
            var adminproj = db.projects.Select(x => x).Where(x => x.id == pid && x.projectleader == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            ViewBag.id = new SelectList(db.projects, "id", "projectkey");
            ViewBag.pid = pid;

            var users2 = Roles.GetUsersInRole("developer").ToList();
            var devsinpr = db.pojectdevs.Where(x => x.projectid == pid).Select(x => x.devname).ToList();
            // to make a list of developers who arent in the project to add them to it 
            foreach (var x in devsinpr){users2.Remove(x);}
            SelectList list2 = new SelectList(users2);
            ViewBag.Usersqq = list2;



            return View();
        }

        //
        // POST: /projectdev/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(pojectdev pojectdev)
        {
            if (ModelState.IsValid)
            {
                db.pojectdevs.Add(pojectdev);
                db.SaveChanges();

                //for adding ADD Developer action to activity stream
                String username = Convert.ToString(Session["UserName"]);
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "ADD DEVELOPER";
                activitystr.description = username + " Added Developer " + pojectdev.devname;
                activitystr.issueid =4;
                activitystr.issuekey = "-";
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = pojectdev.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();

                return RedirectToAction("Create", new { pid = pojectdev.projectid});
            }

            ViewBag.id = new SelectList(db.projects, "id", "projectkey", pojectdev.id);
            return View(pojectdev);
        }

        //
        // GET: /projectdev/Edit/5

        public ActionResult Edit(int id = 0)
        {
            pojectdev pojectdev = db.pojectdevs.Find(id);
            if (pojectdev == null)
            {
                return HttpNotFound();
            }
            ViewBag.id = new SelectList(db.projects, "id", "projectkey", pojectdev.id);
            return View(pojectdev);
        }

        //
        // POST: /projectdev/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(pojectdev pojectdev)
        {
            if (ModelState.IsValid)
            {
                db.Entry(pojectdev).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.id = new SelectList(db.projects, "id", "projectkey", pojectdev.id);
            return View(pojectdev);
        }

        //
        // GET: /projectdev/Delete/5

        public ActionResult Delete(int id = 0)
        {
            pojectdev pojectdev = db.pojectdevs.Find(id);
            if (pojectdev == null)
            {
                return HttpNotFound();
            }
            return View(pojectdev);
        }

        //
        // POST: /projectdev/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            pojectdev pojectdev = db.pojectdevs.Find(id);
            db.pojectdevs.Remove(pojectdev);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}