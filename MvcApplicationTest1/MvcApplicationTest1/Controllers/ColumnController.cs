using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplicationTest1.DAL;
using MvcApplicationTest1.Models;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Text;


namespace MvcApplicationTest1.Controllers
{   // for the exception message
    public class FormattedDbEntityValidationException : Exception
    {
        public FormattedDbEntityValidationException(DbEntityValidationException innerException) :
            base(null, innerException)
        {
        }

        public override string Message
        {
            get
            {
                var innerException = InnerException as DbEntityValidationException;
                if (innerException != null)
                {
                    StringBuilder sb = new StringBuilder();

                    sb.AppendLine();
                    sb.AppendLine();
                    foreach (var eve in innerException.EntityValidationErrors)
                    {
                        sb.AppendLine(string.Format("- Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().FullName, eve.Entry.State));
                        foreach (var ve in eve.ValidationErrors)
                        {
                            sb.AppendLine(string.Format("-- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                ve.PropertyName,
                                eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                ve.ErrorMessage));
                        }
                    }
                    sb.AppendLine();

                    return sb.ToString();
                }

                return base.Message;
            }
        }
    }
    public class ColumnController : Controller
    {
        private ftestEntities db = new ftestEntities();

        //
        // GET: /Column/

        public ActionResult Index()
        {
            var columns = db.columns.Include(c => c.project);
            return View(columns.ToList());
        }


        [Authorize]
        public ActionResult Indexproject(int id = 0,String err= "",int sort = 0,int filter = 0)
        {   
            //for checking if the user in the project and allowed
              if (User.IsInRole("developer")) { 
                var adminproj = db.pojectdevs.Select(x => x).Where(x =>x.projectid == id && x.devname == User.Identity.Name).FirstOrDefault();
               if (adminproj == null ){
                   return RedirectToAction("Index", "project");
               }
            
            }

            if (User.IsInRole("projectowner"))
            { 
                
                var adminproj = db.projects.Select(x => x).Where(x =>x.id == id && x.projectowner == User.Identity.Name).FirstOrDefault();
              if (adminproj == null ){
              return RedirectToAction("Index","project");
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


            ViewBag.projectid = id;
            var zz = db.projects.Select(x => x).Where(x => x.id == id).FirstOrDefault();
            // if the project is finished so display the finshed project page
            if (zz.status == "closed")
            {
                return RedirectToAction("endproject", "project", new { id = id });
            
            }

            ViewBag.currentsp = zz.currentsprint;
            // for the creation of the issue without columns
            ViewBag.errormsg = err;
            // for sorting issues in the agile board
            ViewBag.sort = sort;
            ViewBag.filter = filter;
            var columns = db.columns.Include(c => c.project);
            // for the columns of the project
            var xx = db.columns.Select(x => x).Where(x => x.projectid == id).ToList();
            ViewBag.hyyy = "s";
            ViewBag.indexpro = "y";
            return View("Indexproject",xx);
        }
        //
        // GET: /Column/Details/5

    


        public ActionResult RR(int id = 0)
        {
           return RedirectToAction("indexproject", "column", new { id = id});
        }

        // for delete
        public ActionResult colissues(int projid =0,int colid =0,int currsp = 0,int sort = 0,int filter = 0)
        {
            
            var columns = db.columns.Include(c => c.project);
            var cols = db.columns.Select(x => x).Where(x => x.projectid == projid).ToList();
            var xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).ToList();
       

            // 1: priority high to low , 2: priority low to high , 3: key name , 4: key name desc
            if (sort == 1) {  xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).OrderBy(x=>x.priority).ToList(); }
            if (sort == 2) {  xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).OrderByDescending(x => x.priority).ToList(); }
            if (sort == 3) {  xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).OrderBy(x => x.keyname).ToList(); }
            if (sort == 4) {  xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).OrderByDescending(x => x.keyname).ToList(); }
            if (sort == 5) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).OrderBy(x => x.descreption).ToList(); }
            if (sort == 6) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).OrderByDescending(x => x.descreption).ToList(); }
            // filter 1 : 
            if (filter == 1) { xx = xx.Where(x => x.type == "Epic").ToList(); }
            if (filter == 2) { xx = xx.Where(x => x.type == "User Story").ToList(); }
            if (filter == 3) { xx = xx.Where(x => x.type == "Bug").ToList(); }
            ViewBag.pid = projid;
            ViewBag.cid = colid;
            ViewBag.currentsprintid = currsp;

            return PartialView(xx); 
        }

         [ChildActionOnly]
        public ActionResult colissuesajax(int projid = 0, int colid = 0, int sort = 0, int filter = 0)
        {
             // for displaying the agile board 
            var columns = db.columns.Include(c => c.project);
            var pr = db.projects.Select(x => x).Where(x => x.id == projid).FirstOrDefault();
             //for the current sprint 
            int currsp = pr.currentsprint.Value;
            //for the issues in the current sprint 
            var xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.sprintid == currsp).ToList();
            //for the columns in the project 
            var cols = db.columns.Select(x => x).Where(x => x.projectid == projid).ToList();
             // agile class contains columns and its issues 
            AgileClass cc = new AgileClass();
            cc.Columns = cols;
           
            // for sorting 1: priority high to low , 2: priority low to high , 3: key name , 4: key name desc , 5 : name , 6 : name desc
            if (sort == 1) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.sprintid == currsp).OrderBy(x => x.priority).ToList(); }
            if (sort == 2) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid  && x.sprintid == currsp).OrderByDescending(x => x.priority).ToList(); }
            if (sort == 3) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid  && x.sprintid == currsp).OrderBy(x => x.keyname).ToList(); }
            if (sort == 4) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid  && x.sprintid == currsp).OrderByDescending(x => x.keyname).ToList(); }
            if (sort == 5) {xx= db.issues.Select(x => x).Where(x => x.projectid == projid  && x.sprintid == currsp).OrderBy(x => x.descreption.Substring(0,25)).ToList(); }
            if (sort == 6) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.sprintid == currsp).OrderByDescending(x => x.descreption.Substring(0, 25)).ToList(); }
            // filter 1 : User Story , 2 : bug  
           
            if (filter == 1) { xx = xx.Where(x => x.type == "User Story").ToList(); }
            if (filter == 2) { xx = xx.Where(x => x.type == "Bug").ToList(); }
            ViewBag.pid = projid;
            ViewBag.cid = colid;
            ViewBag.currentsprintid = currsp;
            cc.issues = xx;
            return PartialView("omardrag",cc);
        }

        [Authorize]
         public ActionResult Indexproject2(int id = 0, int sprintnum = 0, int sort = 0, int filter = 0)
        {
            // for displaying the agile board of the old sprints same as Indexproject

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

           // ViewBag.projectid = id;
           //// var zz = db.projects.Select(x => x).Where(x => x.id == id).FirstOrDefault();

           // ViewBag.currentsp = sprintnum;

           // var columns = db.columns.Include(c => c.project);
           // var xx = db.columns.Select(x => x).Where(x => x.projectid == id).ToList();
           // return View(xx);
            var columns = db.columns.Include(c => c.project);
            var pr = db.projects.Select(x => x).Where(x => x.id == id).FirstOrDefault();
            int currsp = sprintnum;

            var xx = db.issues.Select(x => x).Where(x => x.projectid == id && x.sprintid == currsp).ToList();
            var cols = db.columns.Select(x => x).Where(x => x.projectid == id).ToList();
            AgileClass cc = new AgileClass();
            cc.Columns = cols;


            // 1: priority high to low , 2: priority low to high , 3: key name , 4: key name desc
            if (sort == 1) { xx = db.issues.Select(x => x).Where(x => x.projectid == id && x.sprintid == currsp).OrderBy(x => x.priority).ToList(); }
            if (sort == 2) { xx = db.issues.Select(x => x).Where(x => x.projectid == id && x.sprintid == currsp).OrderByDescending(x => x.priority).ToList(); }
            if (sort == 3) { xx = db.issues.Select(x => x).Where(x => x.projectid == id && x.sprintid == currsp).OrderBy(x => x.keyname).ToList(); }
            if (sort == 4) { xx = db.issues.Select(x => x).Where(x => x.projectid == id && x.sprintid == currsp).OrderByDescending(x => x.keyname).ToList(); }
            if (sort == 5) { xx = db.issues.Select(x => x).Where(x => x.projectid == id && x.sprintid == currsp).OrderBy(x => x.descreption.Substring(0, 25)).ToList(); }
            if (sort == 6) { xx = db.issues.Select(x => x).Where(x => x.projectid == id && x.sprintid == currsp).OrderByDescending(x => x.descreption.Substring(0, 25)).ToList(); }
            // filter 1 : 

            if (filter == 1) { xx = xx.Where(x => x.type == "User Story").ToList(); }
            if (filter == 2) { xx = xx.Where(x => x.type == "Bug").ToList(); }


            cc.issues = xx;



            // 1: priority high to low , 2: priority low to high , 3: key name , 4: key name desc
            //if (sort == 1) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).OrderBy(x => x.priority).ToList(); }
            //if (sort == 2) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).OrderByDescending(x => x.priority).ToList(); }
            //if (sort == 3) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).OrderBy(x => x.keyname).ToList(); }
            //if (sort == 4) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).OrderByDescending(x => x.keyname).ToList(); }
            //if (sort == 5) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).OrderBy(x => x.descreption).ToList(); }
            //if (sort == 6) { xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).OrderByDescending(x => x.descreption).ToList(); }
            //// filter 1 : 
            //if (filter == 1) { xx = xx.Where(x => x.type == "Epic").ToList(); }
            //if (filter == 2) { xx = xx.Where(x => x.type == "User Story").ToList(); }
            //if (filter == 3) { xx = xx.Where(x => x.type == "Bug").ToList(); }
           
           // ViewBag.cid = colid;
            //ViewBag.currentsprintid = currsp;
            var zz = db.sprints.Select(x => x).Where(x => x.sid == sprintnum).FirstOrDefault();
            ViewBag.currentsprintid = zz.number;
            ViewBag.spid = zz.sid;
            ViewBag.pid = id;
            
            return View("omardrag2", cc);
        }
        //
        // GET: /Column/Details/5



        public ActionResult colissues2(int projid = 0, int colid = 0, int currsp = 0)
        {

            var columns = db.columns.Include(c => c.project);

            var xx = db.issues.Select(x => x).Where(x => x.projectid == projid && x.status == colid && x.sprintid == currsp).ToList();
            ViewBag.pid = projid;
            ViewBag.cid = colid;
            ViewBag.currentsprintid = currsp;

            return PartialView(xx);
        }

        //delete
        public ActionResult nextcol(int issueid = 0)
        {

            var columns = db.columns.Include(c => c.project);

            var xx = db.issues.Select(x => x).Where(x => x.id == issueid ).FirstOrDefault();
            var projectid = xx.projectid;
            var currentcol = xx.status;
            var zz = db.columns.Select(x => x).Where(x => x.projectid == projectid).OrderBy(x => x.colid).ToList();
            int index = zz.FindIndex(x => x.colid == currentcol);
            if (index < zz.Count() - 1)
            {   int prevcol = xx.status;
                xx.status = zz[index + 1].colid; db.SaveChanges();
                //for adding changing status action to activity stream
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "Change Status";
                activitystr.description = "Changed an issue Statue from col " + prevcol+"to col "+xx.status;
                activitystr.issueid = xx.id;
                activitystr.issuekey = xx.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = xx.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();
            
            }
            
            

            return RedirectToAction("Indexproject", new {id = projectid });
        }
        
        //delete 
        public ActionResult prevcol(int issueid = 0)
        {

            var columns = db.columns.Include(c => c.project);

            var xx = db.issues.Select(x => x).Where(x => x.id == issueid).FirstOrDefault();
            var projectid = xx.projectid;
            var currentcol = xx.status;
            var zz = db.columns.Select(x => x).Where(x => x.projectid == projectid).OrderBy(x => x.colid).ToList();
            int index = zz.FindIndex(x => x.colid == currentcol);
            if (index > 0) {
                int prevcol = xx.status;
                xx.status = zz[index - 1].colid; db.SaveChanges();
                //for adding changing status action to activity stream
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                activitystr.actiontype = "Change Status";
                activitystr.description = "Changed an issue Statue from col " + prevcol + "to col " + xx.status;
                activitystr.issueid = xx.id;
                activitystr.issuekey = xx.keyname;
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = xx.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();
            
            
            
            
            }



            return RedirectToAction("Indexproject", new { id = projectid });
        }





        public ActionResult UpdateIssuesAjax(string itemIds)
        {
            // for updating the issues position 
            List<int> itemIdList = new List<int>();
            // to put the ajax info from the agile board in a int list without the ',' between items
            itemIdList = itemIds.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            // for the column id of the issue
            int state = 1;
            // flag for column if true the next item is for colmun id
            Boolean colflag = false;
            // flag for issue if true the next item is for issue id
            Boolean issueflag = false;
            
           
            foreach (var item in itemIdList)
            {
                    // the next item is a column id
                    if (item == 0) {

                        colflag = true;
                        issueflag = false;
                        continue;
                    
                    }

                    if (colflag) { 
                        
                        //the current item is the column id and the next item is an issue id
                        state = item; colflag = false; issueflag = true; continue; }
                    if (issueflag)
                    {   //selecting the issue and put the state (column id) in the status field of the issue
                        var iss = db.issues.Select(x=>x).Where(m => m.id == item).FirstOrDefault();
                        if (iss.status != state) {
                            
                            var beforestate = iss.status;
                            iss.status = state;
                            db.SaveChanges();

                            //for adding changing status action to activity stream
                            MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                            var before = db.columns.Select(x => x).Where(x => x.colid == beforestate).FirstOrDefault();
                            var after = db.columns.Select(x => x).Where(x => x.colid == state).FirstOrDefault();
                            String username = Convert.ToString(Session["UserName"]);
                            activitystr.actiontype = "Change Issue Status";
                            activitystr.description = username + " Changed Issue " + iss.keyname + " Status From (" + before.name + ") To (" + after.name+")";
                            activitystr.issueid = iss.id;
                            activitystr.issuekey = iss.keyname;
                            activitystr.actiondate = DateTime.Now.ToString();
                            activitystr.projectid = iss.projectid;
                            int uid = int.Parse(Session["UserId"] + "");
                            activitystr.userid = uid;
                            db.activitystreams.Add(activitystr);
                            try
                            {
                                db.SaveChanges();
                               
                            }
                            catch (DbEntityValidationException e)
                            {
                                var newException = new FormattedDbEntityValidationException(e);
                                throw newException;
                            }
                           
                            

                          
                        
                        
                        }
                      

                      

                    }

               
               
               

            }
           
            
            return Json(true, JsonRequestBehavior.AllowGet);
        }



        public ActionResult Details(int id = 0)
        {
            column column = db.columns.Find(id);
            if (column == null)
            {
                return HttpNotFound();
            }
            return View(column);
        }

        //
        // GET: /Column/Create
        [Authorize(Roles = "admin, teamleader")]
        public ActionResult Create(int id=0)
        {
            // for checking if the user is allowed
            var adminproj = db.projects.Select(x => x).Where(x => x.id == id && x.projectleader == User.Identity.Name).FirstOrDefault();
            if (adminproj == null)
            {
                return RedirectToAction("Index", "project");
            }
            // to delete
            ViewBag.projectid = new SelectList(db.projects, "id", "projectkey");
            var xx = db.projects.Select(x => x).Where(x => x.id == id).FirstOrDefault();
            ViewBag.projid = id;
            ViewBag.projname = xx.projectname;
            return View();
        }

        //
        // POST: /Column/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(column column)
        {
            if (ModelState.IsValid)
            {
                db.columns.Add(column);
                db.SaveChanges();
                //for adding adding to the current sprint to activity stream
                MvcApplicationTest1.DAL.activitystream activitystr = new MvcApplicationTest1.DAL.activitystream();
                var xx = db.projects.Select(x=>x).Where(x => x.id == column.projectid).FirstOrDefault();

                String username = Convert.ToString(Session["UserName"]);
                activitystr.actiontype = "Add Column";
                activitystr.description = username + " Added Column "+column.name+" To Project " + xx.projectname;
                activitystr.issueid = 4;
                activitystr.issuekey = "-";
                activitystr.actiondate = DateTime.Now.ToString();
                activitystr.projectid = column.projectid;
                int uid = int.Parse(Session["UserId"] + "");
                activitystr.userid = uid;
                db.activitystreams.Add(activitystr);
                db.SaveChanges();
            
                return RedirectToAction("indexproject", "Column", new { id = column.projectid});;
            }

            ViewBag.projectid = new SelectList(db.projects, "id", "projectkey", column.projectid);
            return View(column);
        }


        public String StatusName(int id = 0)
        {   // for showing the column name 
            var xx = db.columns.Select(x => x).Where(x => x.colid == id).FirstOrDefault();
            return (xx.name);
        }

        //
        // GET: /Column/Edit/5

        public ActionResult Edit(int id = 0)
        {
            column column = db.columns.Find(id);
            if (column == null)
            {
                return HttpNotFound();
            }
            ViewBag.projectid = new SelectList(db.projects, "id", "projectkey", column.projectid);
            return View(column);
        }

        //
        // POST: /Column/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(column column)
        {
            if (ModelState.IsValid)
            {
                db.Entry(column).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.projectid = new SelectList(db.projects, "id", "projectkey", column.projectid);
            return View(column);
        }

        //
        // GET: /Column/Delete/5

        public ActionResult Delete(int id = 0)
        {
            column column = db.columns.Find(id);
            if (column == null)
            {
                return HttpNotFound();
            }
            return View(column);
        }

        //
        // POST: /Column/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            column column = db.columns.Find(id);
            db.columns.Remove(column);
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