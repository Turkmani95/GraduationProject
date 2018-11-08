using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplicationTest1.DAL;
using System.Web.Security;
using MvcApplicationTest1.Filters;
using MvcApplicationTest1.Models;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

namespace MvcApplicationTest1.Controllers
{
    [InitializeSimpleMembership]
    public class projectController : Controller
    {
        private ftestEntities db = new ftestEntities();

        //
        // GET: /project/
        [Authorize]
        public ActionResult Index()
        {
            if (User.IsInRole("developer"))
            {
                //int usrid = int.Parse(Session["UserId"] + "");
                //var usrname = Membership.GetUser(usrid).UserName;
           

                String username = Convert.ToString(Session["UserName"]);
                var xx = db.pojectdevs.Where(x => x.devname == username).Select(x => x.projectid).ToList();
                // for returning projects the developer is in it 
                return View(db.projects.Select(x => x).Where(x => xx.Contains(x.id) ).ToList()); }

            if (User.IsInRole("projectowner"))
            {
              

                String username = Convert.ToString(Session["UserName"]);
                return View(db.projects.Select(x => x).Where(x => x.projectowner == username).ToList());
            }
            if (User.IsInRole("teamleader"))
            {
                
                String username = Convert.ToString(Session["UserName"]);
                return View(db.projects.Select(x => x).Where(x => x.projectleader == username).ToList());
            }

            return View(db.projects.ToList());
            
        }


        [Authorize]
        public ActionResult Viewprojects()
        {   // for displaying the projects the user is in it in the profile page
            if (User.IsInRole("developer"))
            {
                //int usrid = int.Parse(Session["UserId"] + "");
                //var usrname = Membership.GetUser(usrid).UserName;


                String username = Convert.ToString(Session["UserName"]);
                var xx = db.pojectdevs.Where(x => x.devname == username).Select(x => x.projectid).ToList();

                return View(db.projects.Select(x => x).Where(x => xx.Contains(x.id)).ToList());
            }

            if (User.IsInRole("projectowner"))
            {


                String username = Convert.ToString(Session["UserName"]);
                return View(db.projects.Select(x => x).Where(x => x.projectowner == username).ToList());
            }
            if (User.IsInRole("teamleader"))
            {

                String username = Convert.ToString(Session["UserName"]);
                return View(db.projects.Select(x => x).Where(x => x.projectleader == username).ToList());
            }

            return View(db.projects.ToList());

        }

        //
        // GET: /project/Details/5

        public ActionResult Details(int id = 0)
        {
            project project = db.projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        public String projectName(int id = 0)
        {   // for returning the project name 
            project project = db.projects.Find(id);
            
            return project.projectname;
        }

        //
        // GET: /project/Create
        [Authorize(Roles = "admin, teamleader")]
        public ActionResult Create()
        {
            // for displaying users in teamleader and projectowner roles
           //for delete
            var users = Roles.GetUsersInRole("teamleader");
            SelectList list = new SelectList(users);
            ViewBag.Usersq = list;

            var users2 = Roles.GetUsersInRole("projectowner");
            SelectList list2 = new SelectList(users2);
            ViewBag.Usersqq = list2;


            return View();
        }



        [ChildActionOnly]
        public ActionResult Backlog(int id = 0,String err = "")
        {
            
                //UsersContext uc = new UsersContext();
                //var user = uc.UserProfiles.Select(x => x).Where(x => x.UserName == project.projectleader).FirstOrDefault();
                //project.projectleader = user.UserName;
                //var user2 = uc.UserProfiles.Select(x => x).Where(x => x.UserName == project.projectowner).FirstOrDefault();
                //project.projectowner = user2.UserName;
                //var pro = db.projects.Select(x => x).Where(x => x.projectkey == project.projectkey).FirstOrDefault();

           
            // for displaying issues in the backlog of the project where we pass the project id to the api
            // and select all the issues with that project id and sprint is null 
            
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:4419");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = client.GetAsync("api/Backlogapi/"+id).Result;
            if (response.IsSuccessStatusCode)
            {
                ViewBag.rresult = response.Content.ReadAsAsync<IEnumerable<issue>>().Result;



            }
            else
            {
                return RedirectToAction("Home", "Index");
            }


            //var xx = db.issues.Select(x => x).Where(x => x.projectid == id && x.sprintid == null ).ToList();
            //return View();
            //ViewBag.SearchString = id;
            ViewBag.projiid = id;
            // if we created an issue and there wasnt any column in the agile board 
            ViewBag.errormsg = err;
            return PartialView("backlog2");

            
        }

         [Authorize(Roles = "admin, teamleader")]
        public ActionResult addtocurrentsprint(int issueid = 0 )
        {


            
            var xx = db.issues.Select(x => x).Where(x => x.id == issueid ).FirstOrDefault();
           //for checking if the user is allowed 
             var adminproj = db.projects.Select(x => x).Where(x => x.id == xx.projectid && x.projectleader == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            var projid = xx.projectid;
           //for checking if there are columns in the project
            var cc = db.columns.Select(x => x).Where(x => x.projectid == projid).First();
            if (cc != null) {
                var zz = db.projects.Select(x => x).Where(x => x.id == projid).FirstOrDefault();
                var currentsprintid = zz.currentsprint;
                // to make the issue in the current sprint 
                xx.sprintid = currentsprintid;
                db.SaveChanges();
                //for adding adding to the current sprint action to activity stream
                var yy = db.sprints.Select(x=>x).Where(x => x.sid == currentsprintid).FirstOrDefault();
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                String username = Convert.ToString(Session["UserName"]);
                activitystr.actiontype = "Add to Sprint";
                activitystr.description = username + " Added An Issue To Sprint Number " +yy.number ;
                activitystr.issueid = xx.id;
                activitystr.issuekey = xx.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = xx.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();
            





            }      



            return RedirectToAction("Indexproject", "column" , new { id = projid });


        }

        [Authorize]
        public ActionResult EndProject(int id = 0)
        {

            //UsersContext uc = new UsersContext();
            //var user = uc.UserProfiles.Select(x => x).Where(x => x.UserName == project.projectleader).FirstOrDefault();
            //project.projectleader = user.UserName;
            //var user2 = uc.UserProfiles.Select(x => x).Where(x => x.UserName == project.projectowner).FirstOrDefault();
            //project.projectowner = user2.UserName;
            //var pro = db.projects.Select(x => x).Where(x => x.projectkey == project.projectkey).FirstOrDefault();
           
            
            
            //for checking if the user allowed to show this action
            //var adminproj = db.projects.Select(x => x).Where(x => x.id == id && x.projectleader == User.Identity.Name).FirstOrDefault();
            //if (adminproj == null)
            //{
            //    return RedirectToAction("Index", "project");
            //}
            
            
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

                var adminproj = db.projects.Select(x => x).Where(x => x.id == id && x.projectowner == User.Identity.Name).FirstOrDefault();
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



          
            var xx = db.projects.Select(x => x).Where(x => x.id == id).FirstOrDefault();
            if (xx.status != "closed" && User.IsInRole("teamleader"))
            {
           xx.status = "closed";
           db.SaveChanges();

           Session["projectid"] = id;
           String username = Convert.ToString(Session["UserName"]);
           //for adding delete action to activity stream
           MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
           activitystr.actiontype = "End Project";
           activitystr.description = username + " Ended Project " + xx.projectname;
           activitystr.issueid = 4;
           activitystr.issuekey = "-";
           activitystr.actiondate = DateTime.Now.ToString();
           activitystr.projectid = id;
           int uid = int.Parse(Session["UserId"] + "");
           activitystr.userid = uid;
           db.activitystreams.Add(activitystr);
           db.SaveChanges();
           
           
           
           }
            //for checking if the project does closed or not 
            var tt = db.projects.Select(x => x).Where(x => x.id == id).FirstOrDefault();
            if (tt.status != "closed") {
                return RedirectToAction("Indexproject", "column", new { id = id });
            
            
            }
            var zz = db.issues.Select(x => x).Where(x => x.projectid == id).GroupBy(x => x.keyname).Select(z => z.OrderByDescending(q => q.sprintid).FirstOrDefault()).ToList();
            ViewBag.numofissues = zz.Count;
            var uu = db.sprints.Select(x => x).Where(x => x.projectid == id).ToList();
            ViewBag.numofsprints = uu.Count;

            //for counting unfinished issues
            // the last column id 
            var zz2 = db.columns.Select(x => x).Where(x => x.projectid == id).OrderByDescending(x => x.colid).FirstOrDefault();
            if (zz2 != null)
            {

                var lastcolid = zz2.colid;

                //count the list of unfinished issues in the last sprint 
                var zz3 = db.issues.Select(x => x).Where(x => x.projectid == id && x.status != lastcolid && x.sprintid == xx.currentsprint).ToList();
                ViewBag.numofunfinishedissues = zz3.Count;


            }else { ViewBag.numofunfinishedissues = 0; }
          
           

            return View(xx);


        }

        public ActionResult Columns(int id = 0)
        {

            //UsersContext uc = new UsersContext();
            //var user = uc.UserProfiles.Select(x => x).Where(x => x.UserName == project.projectleader).FirstOrDefault();
            //project.projectleader = user.UserName;
            //var user2 = uc.UserProfiles.Select(x => x).Where(x => x.UserName == project.projectowner).FirstOrDefault();
            //project.projectowner = user2.UserName;
            //var pro = db.projects.Select(x => x).Where(x => x.projectkey == project.projectkey).FirstOrDefault();

            var xx = db.issues.Select(x => x).Where(x => x.projectid == id).ToList();            
            return View(xx);


        }



        public String z(int id = 0)
        {


            return Convert.ToString(Session["UserName"]);

        }



        
        
        
        
        
        //
        // POST: /project/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(project project)
        {
            if (ModelState.IsValid)
            {
                UsersContext uc = new UsersContext();
                var user = uc.UserProfiles.Select(x => x).Where(x => x.UserName == project.projectleader).FirstOrDefault();
                project.projectleader = user.UserName;
                var user2 = uc.UserProfiles.Select(x => x).Where(x => x.UserName == project.projectowner).FirstOrDefault();
                project.projectowner = user2.UserName;
                // for checking if the keyname is in use 
                var pro = db.projects.Select(x => x).Where(x => x.projectkey == project.projectkey).FirstOrDefault();
               
                if (pro != null) { 
                     ViewBag.ErrorMessage = "Key is in use";

                     var users = Roles.GetUsersInRole("teamleader");
                     SelectList list = new SelectList(users);
                     ViewBag.Usersq = list;

                     var users2 = Roles.GetUsersInRole("projectowner");
                     SelectList list2 = new SelectList(users2);
                     ViewBag.Usersqq = list2;


                    return View(project); }
                project.date = DateTime.Now.ToString();
                db.projects.Add(project);
                db.SaveChanges();

                // for creating the first sprint 
                MvcApplicationTest1.DAL.sprint sp = new DAL.sprint();
                sp.date = DateTime.Now.ToString();
                sp.projectid = project.id;
                sp.number = 1;
                db.sprints.Add(sp);
                db.SaveChanges();
                // for making the new sprint the projects first sprint 
                project.currentsprint = sp.sid;

      
                
                
                db.SaveChanges();
                Session["projectid"] = project.id;
                String username = Convert.ToString(Session["UserName"]);
                //for adding delete action to activity stream
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "Create Project";
                activitystr.description = username + " Created Project "+ project.projectname;
                activitystr.issueid = 4;
                activitystr.issuekey = "-";
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = project.id;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(project);
        }

        //
        // GET: /project/Edit/5
        [Authorize(Roles = "admin, teamleader")]
        public ActionResult Edit(int id = 0)
        {
            // for checking if the user allowed 
            var adminproj = db.projects.Select(x => x).Where(x => x.id == id && x.projectleader == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            project project = db.projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            var users = Roles.GetUsersInRole("teamleader");
            SelectList list = new SelectList(users);
            ViewBag.Usersq = list;

            var users2 = Roles.GetUsersInRole("projectowner");
            SelectList list2 = new SelectList(users2);
            ViewBag.Usersqq = list2;

            return View(project);
        }

        //
        // POST: /project/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(project project)
        {
            if (ModelState.IsValid)
            {
                UsersContext uc = new UsersContext();
                var user = uc.UserProfiles.Select(x => x).Where(x => x.UserName == project.projectleader).FirstOrDefault();
                project.projectleader = user.UserName;
                var user2 = uc.UserProfiles.Select(x => x).Where(x => x.UserName == project.projectowner).FirstOrDefault();
                project.projectowner = user2.UserName;

                var pro = db.projects.Select(x => x).Where(x => x.id == project.id).FirstOrDefault();
                pro.date = project.date;
                pro.projectkey = project.projectkey;
                pro.projectleader = project.projectleader;
                pro.projectname = project.projectname;
                pro.projectowner = project.projectowner;
                pro.projecttype = project.projecttype;
              
                try
                {
                    db.Entry(pro).State = EntityState.Modified;
                    db.SaveChanges();
                    //for adding edit to activity stream
                    MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                    String username = Convert.ToString(Session["UserName"]);
                    activitystr.actiontype = "Edit project";
                    activitystr.description = username + " Edited project " + project.projectname;
                    activitystr.issueid = 4;
                    activitystr.issuekey = "-";
                    activitystr.actiondate = DateTime.Now.ToString();
                    activitystr.projectid = project.id;
                    int uid = int.Parse(Session["UserId"] + "");
                    activitystr.userid = uid;
                    db.activitystreams.Add(activitystr);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                catch (DbEntityValidationException ex)
                {
                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                        {
                            string errorMessages = string.Join("; ", ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.PropertyName + ": " + x.ErrorMessage));
                            throw new DbEntityValidationException(errorMessages);
                        }
                    }
                }
               
                

               
            }
            return View(project);
        }

        //
        // GET: /project/Delete/5


        [Authorize(Roles = "admin, teamleader")]
        public ActionResult EndSprint(int id = 0)
        {
            // for checking if the user allowed 
            var adminproj = db.projects.Select(x => x).Where(x => x.id == id && x.projectleader == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            // creating a new sprint 
            MvcApplicationTest1.DAL.sprint sp = new DAL.sprint();
            sp.date = DateTime.Now.ToString();
            sp.projectid = id;

            // for giving the new sprint its id 
            var zz = db.projects.Select(x => x).Where(x => x.id == id).FirstOrDefault();
            var xx = db.sprints.Select(x => x).Where(x => x.sid == zz.currentsprint).FirstOrDefault();
            sp.number = xx.number + 1;

            db.sprints.Add(sp);
            db.SaveChanges();
           
            //for moving not completed issues to the next sprint 
            var zz2 = db.columns.Select(x => x).Where(x => x.projectid == id).OrderByDescending(x => x.colid).FirstOrDefault();
            var lastcolid = zz2.colid;
           
            //list of unfinished issues 
            var zz3 = db.issues.Select(x => x).Where(x => x.projectid == id && x.status != lastcolid && x.sprintid == zz.currentsprint).ToList();
           
            // it copies the unfinshed issue to a new issue and add it to the current sprint
           // and moves the unfinished issues to the new sprint for saving the change log
            foreach (var item in zz3) 
            {
                MvcApplicationTest1.DAL.issue s = new MvcApplicationTest1.DAL.issue();
                s.keyname = item.keyname;
                s.status = item.status;
                s.descreption = item.descreption;
                s.type = item.type;
                s.priority = item.priority;
                s.tags = item.tags;
                s.estimate = item.estimate;
                s.assignee = item.assignee;
                s.rankid = item.rankid ;
                s.projectid = item.projectid;
                s.sprintid = zz.currentsprint;
                item.sprintid = sp.sid;
                db.issues.Add(s);
                db.SaveChanges();
            
            }
            // make the new sprint the current sprint 
            zz.currentsprint = sp.sid;
            db.SaveChanges();

            MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
            String username = Convert.ToString(Session["UserName"]);
            activitystr.actiontype = "End Sprint";
            activitystr.description = username + " Ended Sprint Number " +xx.number ;
            activitystr.issueid = 4;
            activitystr.issuekey = "-";
            activitystr.actiondate = DateTime.Now.ToString();
            activitystr.projectid = sp.projectid;
            int uid = int.Parse(Session["UserId"] + "");
            activitystr.userid = uid;
            db.activitystreams.Add(activitystr);
            db.SaveChanges();
            return RedirectToAction("Indexproject", "Column", new { id = zz.id }); ;
        }

        [Authorize]
        public ActionResult ViewSprints(int id = 0)
        {   //for checking if the user allowed
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

                var adminproj = db.projects.Select(x => x).Where(x => x.id == id && x.projectowner == User.Identity.Name).FirstOrDefault();
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
       
            var xx = db.sprints.Select(x => x).Where(x => x.projectid == id).ToList();
            ViewBag.projid = id;
            return View(xx);
        }

        public String ViewSprintNumber(int id = 0)
        {
            // for displaying the sprint number 
            var xx = db.sprints.Select(x => x).Where(x => x.sid == id).FirstOrDefault();
            if (xx != null) { return xx.number.ToString(); }
            return "-";
        }

        public int ViewSprintissuenumber(int id = 0)
        {
            // for displaying the number of issues in the sprint 
            var xx = db.sprints.Select(x => x).Where(x => x.sid == id).FirstOrDefault();
            var zz = db.issues.Select(x => x).Where(x => x.sprintid == xx.sid).ToList();
            int numofissues = zz.Count();
            return numofissues;
        }
        public int ViewSprintEstimated(int id = 0)
        {

            // for displaying the sum of issues estimates in the sprint 
            var zz = db.issues.Select(x => x).Where(x => x.sprintid == id).ToList();
            int estimated = 0;
            foreach(var x in zz){
                if(x.estimate.HasValue)
                estimated = estimated + x.estimate.Value;
            
            }
           
            return estimated;
        }

        public ActionResult Delete(int id = 0)
        {
            project project = db.projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        //
        // POST: /project/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            project project = db.projects.Find(id);
            db.projects.Remove(project);
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