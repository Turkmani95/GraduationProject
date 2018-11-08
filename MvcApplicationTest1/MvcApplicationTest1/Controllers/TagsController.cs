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
    public class TagsController : Controller
    {
        private ftestEntities db = new ftestEntities();

        //
        // GET: /Tags/
         [ChildActionOnly]
        public ActionResult Index(int pid = 0 , int isid = -1)
        {
            ViewBag.pid = pid;
            ViewBag.isid = isid; 
             // to display the issues tag 
            if (pid != 0 && isid != -1) 
            {
                var xx = db.Tags.Select(x => x).Where(x => x.projectid == pid && x.issueid == isid).ToList();
                return PartialView(xx);            
            }
             // to delete
             // to display the project tags
            if (pid != 0)
            {
                var xx = db.Tags.Select(x => x).Where(x => x.projectid == pid).ToList();
                return PartialView(xx);             
            }
           
            var zz = db.Tags.Select(x => x).ToList();
            return PartialView(zz); 
            
        }

        [Authorize]
        public ActionResult issuesintag (int pid = 0, int tagid = -1)
        {
            // for checking if the user is allowed
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
                var adminproj = db.projects.Select(x => x).Where(x => x.id ==pid && x.projectleader == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return RedirectToAction("Index", "project");
                }

            }

            // to show the issues in the tag
            // for the name of tag
            var xx = db.Tags.Select(x => x).Where(x => x.id == tagid ).FirstOrDefault();            
            ViewBag.tag = xx.Tag1;
            String tagname = xx.Tag1.ToString();
            List<issue> q = new List<issue>();
            // for selecting the tags with the same tag name and in the same project 
            var zz = db.Tags.Select(x => x).Where(x => x.Tag1 == tagname && x.projectid == pid).ToList();
            foreach( var m in zz){
                // for selecting the issues which have the same tag 
                var tt = db.issues.Select(x => x).Where(x => x.id == m.issueid).FirstOrDefault();
                // for the sprint issue copying 
                var yy = db.issues.Select(x => x).Where(x => x.keyname == tt.keyname).OrderByDescending(x => x.sprintid).FirstOrDefault();
                q.Add(yy);
            
            
            }
            ViewBag.search = "Issues in Tag " + tagname;
            
            ViewBag.pid = pid;
            return View("~/Views/issues/Index.cshtml", q);

        }

        //
        // GET: /Tags/Details/5

        public ActionResult Details(int id = 0)
        {
            Tag tag = db.Tags.Find(id);
            if (tag == null)
            {
                return HttpNotFound();
            }
            return View(tag);
        }

        //
        // GET: /Tags/Create
           [Authorize(Roles = "admin, developer")]
        public ActionResult Create(int pid = 0 , int isid = 0)
        {
            // for checking if the user is allowed 
            var adminproj = db.pojectdevs.Select(x => x).Where(x => x.projectid == pid && x.devname == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
           // ViewBag.issueid = new SelectList(db.issues, "id", "keyname");
           //ViewBag.projectid = new SelectList(db.projects, "id", "projectkey");
            ViewBag.pid = pid;
            ViewBag.isid = isid;
            return View();
        }

        //
        // POST: /Tags/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Tag tag)
        {
            if (ModelState.IsValid)
            {
                // for checking if the tag is already in use
                var zz = db.Tags.Select(x => x).Where(x => x.Tag1 == tag.Tag1 && x.projectid == tag.projectid && x.issueid == tag.issueid).FirstOrDefault();
                if (zz != null) {
                    ViewBag.errormsg = "Tag is already Used";
                    ViewBag.pid = tag.projectid;
                    ViewBag.isid = tag.issueid;
                    return View(tag);
                }
                db.Tags.Add(tag);
                db.SaveChanges();

                String username = Convert.ToString(Session["UserName"]);
               
                db.SaveChanges();
                var ii = db.issues.Select(x => x).Where(x => x.id == tag.issueid).FirstOrDefault();
                //for adding assign action to activity stream
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "Create Tag";
                activitystr.description = username + " Created Tag " + tag.Tag1 + " For Issue "+ ii.keyname ;
                activitystr.issueid = tag.issueid;
                activitystr.issuekey = ii.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = tag.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();

                return RedirectToAction("Details", "issues", new { id = tag.issueid });
            }

            ViewBag.pid = tag.projectid;
            ViewBag.isid = tag.issueid;
            return View(tag);
        }

        //
        // GET: /Tags/Edit/5
         [Authorize(Roles = "admin, developer")]
        public ActionResult Edit(int id = 0)
        {
            Tag tag = db.Tags.Find(id);
             // for checking if the user allowed 
            var adminproj = db.pojectdevs.Select(x => x).Where(x => x.projectid == tag.projectid && x.devname == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
           
            if (tag == null)
            {
                return HttpNotFound();
            }
          
            ViewBag.pid = tag.projectid;
            ViewBag.isid = tag.issueid;
            return View(tag);
        }

        //
        // POST: /Tags/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Tag tag)
        {
            if (ModelState.IsValid)
            {    // for checking if the tag is already in use
                var zz = db.Tags.Select(x => x).Where(x => x.Tag1 == tag.Tag1 && x.projectid == tag.projectid && x.issueid == tag.issueid).FirstOrDefault();
                if (zz != null)
                {
                    ViewBag.errormsg = "Tag is already Used";
                    ViewBag.pid = tag.projectid;
                    ViewBag.isid = tag.issueid;
                    return View(tag);
                }
                Tag oldtag = db.Tags.Find(tag.id);
                // for the old tag name for the activity stream
                String old = oldtag.Tag1; 
                oldtag.Tag1 = tag.Tag1;
                
                db.SaveChanges();



                String username = Convert.ToString(Session["UserName"]);

               
                var ii = db.issues.Select(x => x).Where(x => x.id == tag.issueid).FirstOrDefault();
                //for adding assign action to activity stream
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "Edit Tag";
                activitystr.description = username + " Edited Tag " + old + " In Issue " + ii.keyname + " To "+tag.Tag1;
                activitystr.issueid = tag.issueid;
                activitystr.issuekey = ii.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = tag.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();




                return RedirectToAction("Details", "issues", new { id = tag.issueid });
            }
            ViewBag.pid = tag.projectid;
            ViewBag.isid = tag.issueid;
            return View(tag);
        }

        //
        // GET: /Tags/Delete/5
         [Authorize(Roles = "admin, developer")]
        public ActionResult Delete(int id = 0)
        {
            Tag tag = db.Tags.Find(id);
             // for checking if the user allowed 
            var adminproj = db.pojectdevs.Select(x => x).Where(x => x.projectid == tag.projectid && x.devname == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            db.Tags.Remove(tag);


            String username = Convert.ToString(Session["UserName"]);


            var ii = db.issues.Select(x => x).Where(x => x.id == tag.issueid).FirstOrDefault();
            //for adding assign action to activity stream
            MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
            activitystr.actiontype = "Delete Tag";
            activitystr.description = username + " Deleted Tag " + tag.Tag1 + " In Issue " + ii.keyname;
            activitystr.issueid = tag.issueid;
            activitystr.issuekey = ii.keyname;
            activitystr.actiondate = DateTime.Now.ToString();
            activitystr.projectid = tag.projectid;
            int uid = int.Parse(Session["UserId"] + "");
            activitystr.userid = uid;
            db.activitystreams.Add(activitystr);
            db.SaveChanges();



            db.SaveChanges();
            return RedirectToAction("Details", "issues", new { id = tag.issueid });
        }

        //
        // POST: /Tags/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tag tag = db.Tags.Find(id);
            db.Tags.Remove(tag);
            db.SaveChanges();
            return RedirectToAction("Details", "issues", new { id = tag.issueid });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}