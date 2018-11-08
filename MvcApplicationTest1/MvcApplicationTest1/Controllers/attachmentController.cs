using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplicationTest1.DAL;
using System.IO;

namespace MvcApplicationTest1.Controllers
{
    public class attachmentController : Controller
    {
        private ftestEntities db = new ftestEntities();

        //
        // GET: /attachment/
          [ChildActionOnly]
        public ActionResult Index(int pid = 0 , int isid = -1)
        {

            ViewBag.pid = pid;
            ViewBag.isid = isid;
              // for the issue attachment
            if (pid != 0 && isid != -1)
            {
                var xx = db.attachments.Select(x => x).Where(x => x.projectid == pid && x.issueid == isid).ToList();
                return PartialView(xx);
            }
              //for the project attachment
            if (pid != 0)
            {
                var xx = db.attachments.Select(x => x).Where(x => x.projectid == pid).ToList();
                return PartialView(xx);
            }

            var zz = db.attachments.Select(x => x).ToList();
            return View(zz); 
            //var attachments = db.attachments.Include(a => a.issue).Include(a => a.project);
          
        }

        //
        // GET: /attachment/Details/5

        public ActionResult Details(int id = 0)
        {
            attachment attachment = db.attachments.Find(id);
            if (attachment == null)
            {
                return HttpNotFound();
            }
            return View(attachment);
        }

        //
        // GET: /attachment/Create
          [Authorize(Roles = "admin, developer,projectowner")]
        public ActionResult Create(int pid = 0, int isid = -1)
        {
              //for checking if the user is allowed
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

           // ViewBag.issueid = new SelectList(db.issues, "id", "keyname");
          //  ViewBag.projectid = new SelectList(db.projects, "id", "projectkey");
            ViewBag.pid = pid;
            ViewBag.isid = isid;
            return View();
        }

        //
        // POST: /attachment/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(attachment attachment, HttpPostedFileBase fileToUpload)
        {
            if (ModelState.IsValid)
            {
                var filevalid = fileToUpload as HttpPostedFileBase;
                if (filevalid == null) {
                    ViewBag.pid = attachment.projectid;
                    ViewBag.isid = attachment.issueid;
                    ViewBag.error = "File is Required";
                    return View(attachment);
                
                }
                //save the uploaded file in a variable
                var file = Request.Files[0];
             
                if (file != null && file.ContentLength > 0)
                {
                    //for the file name
                    var fileName = Path.GetFileName(file.FileName);
                    //for not doubling
                    string path2 = Path.GetRandomFileName();
                    fileName = path2 + fileName;
                    //for saving file path
                    var path = Path.Combine(Server.MapPath("~/Upload/"), fileName);
                    //for saving file
                    attachment.attachmentdest = fileName;

                    file.SaveAs(path);//saved the file in path
                }


                db.attachments.Add(attachment);
                db.SaveChanges();


                String username = Convert.ToString(Session["UserName"]);

                db.SaveChanges();
                var ii = db.issues.Select(x => x).Where(x => x.id == attachment.issueid).FirstOrDefault();
                //for adding assign action to activity stream
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "Add Attachment";
                activitystr.description = username + " Added Attachment " + attachment.Name + " To Issue " + ii.keyname;
                activitystr.issueid = attachment.issueid;
                activitystr.issuekey = ii.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = attachment.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();


                return RedirectToAction("Details", "issues", new {id = attachment.issueid });
            }

            ViewBag.pid = attachment.projectid;
            ViewBag.isid = attachment.issueid;
            return View(attachment);
        }


        [Authorize]
        public FileResult Download(string attachName , int pid = -1)
        {   // for checking if the user is allowed
            if (User.IsInRole("developer"))
            {
                var adminproj = db.pojectdevs.Select(x => x).Where(x => x.projectid == pid && x.devname == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return null;
                }

            }

            if (User.IsInRole("projectowner"))
            {

                var adminproj = db.projects.Select(x => x).Where(x => x.id == pid && x.projectowner == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return null;
                }
            }

            if (User.IsInRole("teamleader"))
            {
                var adminproj = db.projects.Select(x => x).Where(x => x.id ==pid && x.projectleader == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return null;
                }

            }


            var FileVirtualPath = "~/Upload/" + attachName;
            return File(FileVirtualPath, "application/force-download", Path.GetFileName(FileVirtualPath));
        }  

        //
        // GET: /attachment/Edit/5
         [Authorize(Roles = "admin, developer")]
        public ActionResult Edit(int id = 0)
        {
            

            attachment attachment = db.attachments.Find(id);
            // for checking if the user is allowed
            var adminproj = db.pojectdevs.Select(x => x).Where(x => x.projectid == attachment.projectid && x.devname == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
          
            ViewBag.pid = attachment.projectid;
            ViewBag.isid = attachment.issueid;
            return View(attachment);
        }

        //
        // POST: /attachment/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(attachment attachment)
        {
            if (ModelState.IsValid)
            {
                attachment oldattachment = db.attachments.Find(attachment.id);
                // for the activity stream 
                String old = oldattachment.Name;
                oldattachment.Name = attachment.Name;
               
                db.SaveChanges();



                String username = Convert.ToString(Session["UserName"]);

                db.SaveChanges();
                var ii = db.issues.Select(x => x).Where(x => x.id == attachment.issueid).FirstOrDefault();
                //for adding assign action to activity stream
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "Edit Attachment";
                activitystr.description = username + " Edited Attachment " + old + " In Issue " + ii.keyname + " To "+ attachment.Name;
                activitystr.issueid = attachment.issueid;
                activitystr.issuekey = ii.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = attachment.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();





                return RedirectToAction("Details", "issues", new { id = attachment.issueid });
            }
            ViewBag.issueid = new SelectList(db.issues, "id", "keyname", attachment.issueid);
            ViewBag.projectid = new SelectList(db.projects, "id", "projectkey", attachment.projectid);
            return View(attachment);
        }

        //
        // GET: /attachment/Delete/5
        [Authorize(Roles = "admin, developer")]
        public ActionResult Delete(int id = 0)
        {
           
            attachment attachment = db.attachments.Find(id);
            // for checking if the user is allowed
            var adminproj = db.pojectdevs.Select(x => x).Where(x => x.projectid == attachment.projectid && x.devname == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            db.attachments.Remove(attachment);
            db.SaveChanges();


            String username = Convert.ToString(Session["UserName"]);

            db.SaveChanges();
            var ii = db.issues.Select(x => x).Where(x => x.id == attachment.issueid).FirstOrDefault();
            //for adding assign action to activity stream
            MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
            activitystr.actiontype = "Delete Attachment";
            activitystr.description = username + " Deleted Attachment " + attachment.Name + " In Issue " + ii.keyname;
            activitystr.issueid = attachment.issueid;
            activitystr.issuekey = ii.keyname;
            activitystr.actiondate = DateTime.Now.ToString();
            activitystr.projectid = attachment.projectid;
            int uid = int.Parse(Session["UserId"] + "");
            activitystr.userid = uid;
            db.activitystreams.Add(activitystr);
            db.SaveChanges();



            return RedirectToAction("Details", "issues", new { id = attachment.issueid });
        }

        //
        // POST: /attachment/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            attachment attachment = db.attachments.Find(id);
            db.attachments.Remove(attachment);
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