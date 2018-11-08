using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplicationTest1.DAL;
using MvcApplicationTest1.Models;

namespace MvcApplicationTest1.Controllers
{
    public class ActivityStreamController : Controller
    {
        private ftestEntities db = new ftestEntities();

        //
        // GET: /ActivityStream/
       
        [Authorize]
        public ActionResult Index(int isid = -1,int pid = -1,int usid = -1)
        {   //for user activity stream and project activity stream and issue activity stream
            var xx = db.projects.Select(x=>x).Where(x=>x.id == pid).FirstOrDefault();
            var zz = db.issues.Select(x=> x).Where(x=>x.id == isid).FirstOrDefault();
       
            UsersContext uc = new UsersContext();
            var yy = uc.UserProfiles.Where(x => x.UserId == usid).FirstOrDefault();
            if (isid == -1 && pid == -1 && usid == -1) { return View(db.activitystreams.ToList()); }
            
            //if this activity stream for an issue
            if (isid != -1) {

                //for checking if the user allowed to show this action
                var issueids = db.issues.Select(x => x).Where(x => x.id == isid).FirstOrDefault();

                if (User.IsInRole("developer"))
                {   
                    //for checking if the user in the project
                    var adminproj = db.pojectdevs.Select(x => x).Where(x => x.projectid == issueids.projectid && x.devname == User.Identity.Name).FirstOrDefault();
                    if (adminproj == null)
                    {
                        return RedirectToAction("Index", "project");
                    }

                }

                if (User.IsInRole("projectowner"))
                {
                    //for checking if the user in the project
                    var adminproj = db.projects.Select(x => x).Where(x => x.id == issueids.projectid && x.projectowner == User.Identity.Name).FirstOrDefault();
                    if (adminproj == null)
                    {
                        return RedirectToAction("Index", "project");
                    }
                }

                if (User.IsInRole("teamleader"))
                {
                    //for checking if the user in the project
                    var adminproj = db.projects.Select(x => x).Where(x => x.id == issueids.projectid && x.projectleader == User.Identity.Name).FirstOrDefault();
                    if (adminproj == null)
                    {
                        return RedirectToAction("Index", "project");
                    }

                }





                // for displaying the title
                ViewBag.pid = "Issue "+ zz.keyname ;

                // for the condition for display the type of the activity stream 
                ViewBag.type = "issue";

                ViewBag.id = isid;
                return View(db.activitystreams.Select(x => x).Where(x => x.issueid == isid).ToList()); }
            //if this activity stream for a project
            if (pid != -1) {

                //for checking if the user allowed to show this action
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

                  ViewBag.pid = "Project " + xx.projectname;
                ViewBag.type = "project";
                ViewBag.id = pid;
                return View(db.activitystreams.Select(x => x).Where(x => x.projectid == pid).ToList()); }

            //if this activity stream for a user
            //for checking if the user allowed to show this action
            int uid = int.Parse(Session["UserId"] + "");
            if (usid != uid) { return RedirectToAction("Index", "ActivityStream", new { usid = int.Parse(Session["UserId"] + "") }); }
              
            ViewBag.pid = "User " + yy.UserName;
            ViewBag.type = "user";
            ViewBag.isuser = "y";
            return View(db.activitystreams.Select(x => x).Where(x => x.userid == usid).ToList());
        }

        public String Showusername(int id = 0)
        {   
            UsersContext uc = new UsersContext();
            var yy = uc.UserProfiles.Where(x => x.UserId == id).FirstOrDefault();
            return yy.UserName;
        }

        //
        // GET: /ActivityStream/Details/5

        public ActionResult Details(int id = 0)
        {
            activitystream activitystream = db.activitystreams.Find(id);
            if (activitystream == null)
            {
                return HttpNotFound();
            }
            return View(activitystream);
        }

        //
        // GET: /ActivityStream/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ActivityStream/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(activitystream activitystream)
        {
            if (ModelState.IsValid)
            {
                db.activitystreams.Add(activitystream);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(activitystream);
        }

        //
        // GET: /ActivityStream/Edit/5

        public ActionResult Edit(int id = 0)
        {
            activitystream activitystream = db.activitystreams.Find(id);
            if (activitystream == null)
            {
                return HttpNotFound();
            }
            return View(activitystream);
        }

        //
        // POST: /ActivityStream/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(activitystream activitystream)
        {
            if (ModelState.IsValid)
            {
                db.Entry(activitystream).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(activitystream);
        }

        //
        // GET: /ActivityStream/Delete/5

        public ActionResult Delete(int id = 0)
        {
            activitystream activitystream = db.activitystreams.Find(id);
            if (activitystream == null)
            {
                return HttpNotFound();
            }
            return View(activitystream);
        }

        //
        // POST: /ActivityStream/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            activitystream activitystream = db.activitystreams.Find(id);
            db.activitystreams.Remove(activitystream);
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