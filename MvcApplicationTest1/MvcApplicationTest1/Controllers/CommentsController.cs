using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplicationTest1.DAL;

namespace MvcApplicationTest1.Controllers
{
    public class CommentsController : Controller
    {
        private ftestEntities db = new ftestEntities();

        //
        // GET: /Default1/
         [ChildActionOnly]
        public ActionResult Index(int pid =0 , int isid = 4)
        {
            ViewBag.pid = pid;
            ViewBag.isid = isid;
             // for the issue comments
            if (pid != 0 && isid != 4)
            {
                var xx = db.Comments.Select(x => x).Where(x => x.projectid == pid && x.issueid == isid).ToList();
                return PartialView(xx);
            }
             // for the project comments
            if (pid != 0) {
                var xx = db.Comments.Select(x => x).Where(x => x.projectid == pid && x.issueid == 4).ToList();
                return PartialView(xx);            
            }
            var zz = db.Comments.Select(x => x).ToList();
            return PartialView(zz);
            
        }

        //
        // GET: /Default1/Details/5

        public ActionResult Details(int id = 0)
        {
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        //
        // GET: /Default1/Create
        [Authorize]
        public ActionResult Create(int pid = 0 ,int isid = 4)
        {
            // for checking if the user allowed
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
            ViewBag.projectid = pid;
            ViewBag.isid = isid;
            String username = Convert.ToString(Session["UserName"]);
            ViewBag.username = username;
            int uid = int.Parse(Session["UserId"] + "");
            ViewBag.userid =uid;
            return PartialView("Create2");
        }

        //
        // POST: /Default1/Createe
        
        [HttpPost]
       
        public ActionResult Createe(Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.date = DateTime.Now.ToString();
                db.Comments.Add(comment);
                db.SaveChanges();
                ViewBag.pid = comment.projectid;
                ViewBag.isid = comment.issueid;
                String username = Convert.ToString(Session["UserName"]);
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                int uid = int.Parse(Session["UserId"] + "");
                // if the comment was on the project 
                if (comment.issueid == 4) {
                    var xx = db.projects.Select(x => x).Where(x => x.id == comment.projectid).FirstOrDefault();
                    //for adding Comment action to activity stream
                   
                    activitystr.actiontype = "Comment";
                    activitystr.description = username + " Commented on Project "+xx.projectname+" : " + comment.comment1;
                    activitystr.issueid = 4;
                    activitystr.issuekey = "-";
                    activitystr.actiondate = DateTime.Now.ToString();
                    activitystr.projectid = xx.id;
                  
                    activitystr.userid = uid;
                    db.activitystreams.Add(activitystr);
                    db.SaveChanges();
                    return RedirectToAction("indexproject", "column", new { id = comment.projectid }); 
                }

                //if the comment was on the issue 

                var zz = db.issues.Select(x => x).Where(x => x.id == comment.issueid).FirstOrDefault();
                //for adding Comment action to activity stream
                
                activitystr.actiontype = "Comment";
                activitystr.description = username + " Commented on Issue " + zz.keyname+ " : " + comment.comment1;
                activitystr.issueid = zz.id;
                activitystr.issuekey = zz.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = zz.projectid;
               
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();

                return RedirectToAction("Details", "issues", new { id = comment.issueid});
            }
            if (comment.issueid != 4) { return RedirectToAction("Details", "issues", new { id = comment.issueid }); }
            else { return RedirectToAction("indexproject", "column", new { id = comment.projectid }); }
           
        }

        //
        // GET: /Default1/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        //
        // POST: /Default1/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(comment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(comment);
        }

        //
        // GET: /Default1/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        //
        // POST: /Default1/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comment comment = db.Comments.Find(id);
            db.Comments.Remove(comment);
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