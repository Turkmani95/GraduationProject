using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplicationTest1.DAL;
using System.Web.Security;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MvcApplicationTest1.Controllers
{
    public class issuesController : Controller
    {
        private ftestEntities db = new ftestEntities();

        //
        // GET: /issues/

        public ActionResult Index()
        {
            return View(db.issues.ToList());
        }

        [Authorize]
        public ActionResult ViewIssues(String name = "")
        {   
            //for displaying the user assigned issues
            if (name != User.Identity.Name) {

                return RedirectToAction("ViewIssues", "issues", new { name = User.Identity.Name });
            }
            var xx = db.issues.Select(zz => zz).Where(zz => zz.assignee == name).GroupBy(y => y.keyname).Select(z => z.OrderByDescending(q => q.sprintid).FirstOrDefault());

            return View(xx);
        }


        //
        // GET: /issues/Details/5
         [Authorize]
        public ActionResult Details(int id = 0)
        {
             //for displaying the issue details


            issue issue = db.issues.Find(id);
            if (issue == null)
            {
                return HttpNotFound();
            }
             // for checking if the user allowed 
            if (User.IsInRole("developer"))
            {
                var adminproj = db.pojectdevs.Select(x => x).Where(x => x.projectid == issue.projectid && x.devname == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return RedirectToAction("Index", "project");
                }

            }

            if (User.IsInRole("projectowner"))
            {

                var adminproj = db.projects.Select(x => x).Where(x => x.id == issue.projectid && x.projectowner == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return RedirectToAction("Index", "project");
                }
            }

            if (User.IsInRole("teamleader"))
            {
                var adminproj = db.projects.Select(x => x).Where(x => x.id == issue.projectid && x.projectleader == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return RedirectToAction("Index", "project");
                }

            }

        
            return View(issue);
        }

         [Authorize]
        public ActionResult Searchissues(int pid =0 ,String Search="")
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

            // searching for issues that its keyname or description contains the search string passed to this action and grouping the results
            // for selecting the issues without its copies in sprints
            var xx = db.issues.Select(zz => zz).Where(zz =>(zz.keyname.Contains(Search) || zz.descreption.Contains(Search) ) && zz.projectid == pid).GroupBy(y => y.keyname ).Select(z => z.OrderByDescending(q=>q.sprintid).FirstOrDefault());
            ViewBag.pid = pid;
            ViewBag.search ="Issues that Conatins ("+ Search+")";
            return View("Index",xx);
        }

        public ActionResult findnewissue(int id = 0)
        {   // to find the Original issue which is in the latest sprint 
            var xx = db.issues.Select(zz => zz).Where(zz => zz.id == id).FirstOrDefault();
            var yy = db.issues.Select(x => x).Where(x => x.keyname == xx.keyname).OrderByDescending(x => x.sprintid).FirstOrDefault();

            return RedirectToAction("details", "issues", new { id = yy.id});
        }
        public String priorityName(int id = 0)
        {
            if (id == 1) return "High";
            if (id == 2) return "Medium";
            return "Low";
        }

        public Boolean isAssignee(String name)
        {   // to check if the user is the assignee 
            if (name == Convert.ToString(Session["UserName"])) return true;
            return false; 
        }
        public ActionResult Viewtag(int id = 0)
        {   // to view issues in the tag in the same project 
            var zz = db.issues.Select(x => x).Where(x => x.id == id).FirstOrDefault();

            var xx = db.issues.Select(x => x).Where(x => x.tags == zz.tags && x.projectid == zz.projectid).ToList();
            ViewBag.title = zz.tags;
            return View(xx);
        }
        [Authorize(Roles = "admin, developer")]
        public ActionResult assigneissue(int id = 0)
        {

          // to assign issue to developer 
            issue issue = db.issues.Find(id);
            // for checking if the user allowed 
            var adminproj = db.pojectdevs.Select(x => x).Where(x => x.projectid == issue.projectid && x.devname == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            //int uid = int.Parse(Session["UserId"] + "");
            if (issue == null)
            {
                return HttpNotFound();
            }
            // for checking if the issue has already an assignee 
            if (issue.assignee == null)
            {
                String username = Convert.ToString(Session["UserName"]);
                issue.assignee = username;
                db.SaveChanges();
                //for adding assign action to activity stream
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "Assign";
                activitystr.description = username + " Assgined issue " + issue.keyname;
                activitystr.issueid = issue.id;
                activitystr.issuekey = issue.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = issue.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();
                return View("Details",issue);
            }
            ViewBag.errormsg = "already assigned";
            //html condition using viewbag if the viewbag contains some value
            return View("Details",issue);
         
        }

        [Authorize(Roles = "admin, developer")]
        public ActionResult unassigneissue(int id = 0)
        {   // for unassign the issue from the developer 
            issue issue = db.issues.Find(id);
            var adminproj = db.pojectdevs.Select(x => x).Where(x => x.projectid == issue.projectid && x.devname == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            //int uid = int.Parse(Session["UserId"] + "");
            String username = Convert.ToString(Session["UserName"]);
            if (issue == null)
            {
                return HttpNotFound();
            }
            if (issue.assignee != null && issue.assignee == User.Identity.Name)
            {
                
                issue.assignee = null;
                db.SaveChanges();
                //for adding assign action to activity stream
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "UnAssign";
                activitystr.description =  username + " UnAssgined issue " + issue.keyname;
                activitystr.issueid = issue.id;
                activitystr.issuekey = issue.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = issue.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();
                return View("Details", issue);
            }
            ViewBag.errormsg = "Not assigned";
            //html condition using viewbag if the viewbag contains some value
            return View("Details", issue);

        }


        [Authorize(Roles = "admin, teamleader")]
        public ActionResult assignissuemaster(int id = 0)
        {

            // for assign the issue to a developer by the scrum master
            issue issue = db.issues.Find(id);
            var adminproj = db.projects.Select(x => x).Where(x => x.id == issue.projectid && x.projectleader == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            //int uid = int.Parse(Session["UserId"] + "");
            if (issue == null)
            {
                return HttpNotFound();
            }
            if (issue.assignee == null)
            {
                var devsinpr = db.pojectdevs.Where(x => x.projectid == issue.projectid).OrderBy(x => x.devname).Select(x => x.devname).ToList();
               
                //for passing the list to the view 
                IEnumerable<SelectListItem> basetypes = db.pojectdevs.Where(x => x.projectid == issue.projectid).Select(
               b => new SelectListItem { Value = b.devname, Text = b.devname });
              
                ViewBag.IssueName = issue.keyname;
               
                ViewBag.Usersqq = basetypes;
                
                return View(issue);

              
            }
            ViewBag.errormsg = "already assigned";
            //html condition using viewbag if the viewbag contains some value
            return View("Details", issue);

        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult assignissuemaster(issue issue)
        {

                issue issue1 = db.issues.Find(issue.id);
                issue1.assignee = issue.assignee;
                db.SaveChanges();
                //for adding edit action to activity stream
                String username = Convert.ToString(Session["UserName"]);
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "Assign Issue To DEV";
                activitystr.description = username + " Assigned Issue " + issue.keyname + " To Developer "+issue.assignee;
                activitystr.issueid = issue.id;
                activitystr.issuekey = issue.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = issue.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();

                return RedirectToAction("Details", new { id = issue.id });
            
            
        }



        [Authorize(Roles = "admin, teamleader")]
        public ActionResult unassignissuemaster(int id = 0)
        {   // for unassign the issue from the developer by the scrum master
            issue issue = db.issues.Find(id);
            var adminproj = db.projects.Select(x => x).Where(x => x.id == issue.projectid && x.projectleader == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            //int uid = int.Parse(Session["UserId"] + "");
            if (issue == null)
            {
                return HttpNotFound();
            }
            if (issue.assignee != null)
            {
                String username = Convert.ToString(Session["UserName"]);
                var oldassignne = issue.assignee;
                issue.assignee = null;
                db.SaveChanges();
                //for adding assign action to activity stream
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "UnAssign Issue From DEV";
                activitystr.description = username + " UnAssigned Issue " + issue.keyname + " From Developer " + oldassignne;
                activitystr.issueid = issue.id;
                activitystr.issuekey = issue.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = issue.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();
                return View("Details", issue);
            }
            ViewBag.errormsg = "Not assigned";
            //html condition using viewbag if the viewbag contains some value
            return View("Details", issue);

        }





        //
        // GET: /issues/Create
          [Authorize]
        public ActionResult Create(int id=0)
        {
            ViewBag.projid = id;

            //for checking if the user allowed to show this action
          
            if (User.IsInRole("developer"))
            {
                var adminproj = db.pojectdevs.Select(x => x).Where(x => x.projectid == id && x.devname == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return RedirectToAction("Index", "project");
                }

            }

            if (User.IsInRole("projectowner"))
            {

                var adminproj = db.projects.Select(x => x).Where(x => x.id ==id && x.projectowner == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return RedirectToAction("Index", "project");
                }
            }

            if (User.IsInRole("teamleader"))
            {
                var adminproj = db.projects.Select(x => x).Where(x => x.id == id && x.projectleader == User.Identity.Name).FirstOrDefault();
                if (adminproj == null)
                {
                    return RedirectToAction("Index", "project");
                }

            }





            // if there wasnt any column in the agile board 
            var zz = db.columns.Where(x => x.projectid == id).Select(x => x.name ).ToList();
            
            if (zz.Count == 0) 
            {
                return RedirectToAction("indexproject", "Column", new { id = ViewBag.projid,err = "You Can't Add an Issue Without A Column in the Agile Board" });
            
            }
            SelectList list = new SelectList(zz);
            ViewBag.colnames = list;

            //for selecting developers in the project 
            var devsinproj = db.pojectdevs.Where(x => x.projectid == id).Select(x => x.devname).ToList();
              // for selecting all the users in the developer role
            var users = Roles.GetUsersInRole("developer");
              //for selecting the developers in the project from the users list 
            foreach (var user in users) {
                if (!devsinproj.Contains(user)) { users = users.Select(x => x).Where(x => x != user).ToArray(); }

            
            }
              //for passing the list to the view 
            SelectList slist = new SelectList(users);
            
            ViewBag.Usersq = slist;
            return View();
        }


        public ActionResult status(int id = 0)
        {
            ViewBag.projid = id;

            var zz = db.columns.Where(x => x.projectid == id).Select(x => x).ToList();
            //var zz2 = db.columns.Select(item => new column { name = item.name}).Where(x => x.projectid == id).ToList();
            
            return PartialView(zz);
        }

        //
        // POST: /issues/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(issue issue)
        {
            if (ModelState.IsValid)
            {   // for the keyname error message 
                ViewBag.errormsg = "";
             
                var zzz = db.projects.Select(x => x).Where(x => x.id == issue.projectid).FirstOrDefault();
                // for compining the keyname of the project with the keyname of the issue
                String kname = zzz.projectkey + "_" + issue.keyname;
                // for checking if the keyname is already in use 
                var mm = db.issues.Select(x => x).Where(x => x.keyname == kname).FirstOrDefault();
                if (mm != null)
                {
                    ViewBag.errormsg = "Key Name is in Use";
                    // for passing the list of the developers in the project to the view 
                    var devsinproj = db.pojectdevs.Where(x => x.projectid == issue.projectid).Select(x => x.devname).ToList();
                    var users = Roles.GetUsersInRole("developer");
                    foreach (var user in users)
                    {
                        if (!devsinproj.Contains(user)) { users = users.Select(x => x).Where(x => x != user).ToArray(); }


                    }
                    SelectList slist = new SelectList(users);

                    ViewBag.Usersq = slist;
                    ViewBag.projid = issue.projectid;


                    return View(issue);
                }
                issue.keyname = kname;
                //for getting the first column id in the agileboard and save it in the status field , when we add it to the agileboard it 
                // takes place in the first column
                var zq = db.columns.Select(x => x).Where(x => x.projectid == issue.projectid ).FirstOrDefault();
                issue.status = zq.colid;
                if (issue.priority == "High") { issue.priority = "1"; }
                if (issue.priority == "Medium") { issue.priority = "2"; }
                if (issue.priority == "Low") { issue.priority = "3"; }


                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri("http://localhost:4419");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = client.PostAsJsonAsync("api/Backlogapi", issue).Result;
                String username = Convert.ToString(Session["UserName"]);
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                int uid = int.Parse(Session["UserId"] + "");
                // if the response successeded
                if (response.IsSuccessStatusCode)
                {
                    //for adding create action to activity stream
                    var issueaftersave = db.issues.Select(z => z).Where(z => z.keyname == issue.keyname).FirstOrDefault();
                    activitystr.actiontype = "Create Issue";
                    activitystr.description = username + " Created issue " + issue.keyname;
                    activitystr.issueid = issueaftersave.id;
                    activitystr.issuekey = issue.keyname;
                    activitystr.actiondate = DateTime.Now.ToString();
                    activitystr.projectid = issue.projectid;
                    //int uid = int.Parse(Session["UserId"] + "");
                    activitystr.userid = uid;
                    db.activitystreams.Add(activitystr);
                    db.SaveChanges();

                    return RedirectToAction("indexproject", "Column", new { id = issue.projectid });



                }
                else
                {
                    return View(issue);
                }



                //db.issues.Add(issue);
                //db.SaveChanges();

               
              //  String username = Convert.ToString(Session["UserName"]);
               // MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                //activitystr.actiontype = "Create Issue";
                //activitystr.description = username + " Created issue " + issue.keyname;
                //activitystr.issueid = issue.id ;
                //activitystr.issuekey = issue.keyname ;
                //activitystr.actiondate = DateTime.Now.ToString(); 
                //activitystr.projectid = issue.projectid ;
               
                //activitystr.userid = uid;
                //db.activitystreams.Add(activitystr);
                //db.SaveChanges();
                
              
            }
          
            return View(issue);
        }

        //
        // GET: /issues/Edit/5

        //public ActionResult SortIssuespriorityHightoLow(int id = 0)
        //{
        //    var xx = db.issues.Select(x => x).Where(x => x.projectid == id).OrderBy(x => x.priority);
        //    if (issue == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    var users = Roles.GetUsersInRole("developer");
        //    SelectList slist = new SelectList(users);
        //    ViewBag.Usersq = slist;
        //    return View(issue);
        //}
         [Authorize(Roles = "admin, developer, teamleader")]
        public ActionResult Edit(int id = 0)
        {
             // for checking if the user allowed 
            var adminproj = db.issues.Select(x => x).Where(x => x.id == id && x.assignee == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            issue issue = db.issues.Find(id);
            if (issue == null)
            {
                return HttpNotFound();
            }
             // for passing the developers list to the view 
            var devsinproj = db.pojectdevs.Where(x => x.projectid == issue.projectid).Select(x => x.devname).ToList();
            var users = Roles.GetUsersInRole("developer");
            foreach (var user in users)
            {
                if (!devsinproj.Contains(user)) { users = users.Select(x => x).Where(x => x != user).ToArray(); }


            }
            SelectList slist = new SelectList(users);

            ViewBag.Usersq = slist;
            return View(issue);
        }



        //
        // POST: /issues/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(issue issue)
        {
            if (ModelState.IsValid)
            {
                db.Entry(issue).State = EntityState.Modified;
                db.SaveChanges();
                //for adding edit action to activity stream
                String username = Convert.ToString(Session["UserName"]);
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "Edit Issue";
                activitystr.description = username + " Edited issue " + issue.keyname;
                activitystr.issueid = issue.id;
                activitystr.issuekey = issue.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = issue.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();




                return RedirectToAction("Details", "issues", new { id = issue.id });
            }
            return View(issue);
        }

        //
        // GET: /issues/Delete/5
        [Authorize(Roles = "teamleader")]
        public ActionResult Delete(int id = 0)
        {
            var issueids = db.issues.Select(x => x).Where(x => x.id == id).FirstOrDefault();
            var adminproj = db.projects.Select(x => x).Where(x => x.id == issueids.projectid && x.projectleader == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            issue issue = db.issues.Find(id);
            if (issue == null)
            {
                return HttpNotFound();
            }
            //for adding delete action to activity stream
            MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
            String username = Convert.ToString(Session["UserName"]);
            activitystr.actiontype = "Delete Issue";
            activitystr.description = username + " Deleted issue " + issue.keyname;
            activitystr.issueid = issue.id;
            activitystr.issuekey = issue.keyname;
            activitystr.actiondate = DateTime.Now.ToString();
            activitystr.projectid = issue.projectid;
            int uid = int.Parse(Session["UserId"] + "");
            activitystr.userid = uid;
            db.activitystreams.Add(activitystr);
            db.SaveChanges();


            db.issues.Remove(issue);
            db.SaveChanges();
            return RedirectToAction("indexproject", "Column", new { id = issue.projectid });
        }

        //
        // POST: /issues/Delete/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            issue issue = db.issues.Find(id);
            //for adding delete action to activity stream
            MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
            activitystr.actiontype = "Delete";
            activitystr.description = "Deleted an issue";
            activitystr.issueid = issue.id;
            activitystr.issuekey = issue.keyname;
            activitystr.actiondate = DateTime.Now.ToString();
            activitystr.projectid = issue.projectid;
            int uid = int.Parse(Session["UserId"] + "");
            activitystr.userid = uid;
            db.activitystreams.Add(activitystr);
            db.SaveChanges();


            db.issues.Remove(issue);
            db.SaveChanges();
            return RedirectToAction("indexproject", "Column", new { id = issue.projectid });
        }



        public ActionResult DeleteComplete(int id)
        {
            issue issue = db.issues.Find(id);
            //for adding delete action to activity stream
            MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
            activitystr.actiontype = "Delete Issue";
            activitystr.description = "Deleted an issue";
            activitystr.issueid = issue.id;
            activitystr.issuekey = issue.keyname;
            activitystr.actiondate = DateTime.Now.ToString();
            activitystr.projectid = issue.projectid;
            int uid = int.Parse(Session["UserId"] + "");
            activitystr.userid = uid;
            db.activitystreams.Add(activitystr);
            db.SaveChanges();


            db.issues.Remove(issue);
            db.SaveChanges();
            return RedirectToAction("indexproject", "Column", new { id = issue.projectid });
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}