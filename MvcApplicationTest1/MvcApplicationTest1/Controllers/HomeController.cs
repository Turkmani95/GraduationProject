using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MvcApplicationTest1.Models;
using MvcApplicationTest1.DAL;

namespace MvcApplicationTest1.Controllers
{
    public class HomeController : Controller
    {
        private ftestEntities db = new ftestEntities();
        public ActionResult Index()
        {
           
            var a = new Dashboard();

            //for selecting the issues without its copies in sprints
            a.issues = db.issues.Select(zz => zz).Where(zz => zz.assignee == User.Identity.Name ).GroupBy(y => y.keyname).Select(z => z.OrderByDescending(q => q.sprintid).FirstOrDefault());
             List<pojectdev> nn = new List<pojectdev>();
            
            if (User.IsInRole("admin")) 
            { 
                //for returning all the projects and converting it to projectdev
                var adminproj = db.projects.Select(x=>x).ToList();
                
                foreach(var zz in adminproj){
                    var q = db.pojectdevs.Select(x => x).Where(x => x.projectid == zz.id).FirstOrDefault();
                    if (q != null) { nn.Add(q); } else {
                        //if the projects doesnt contains developers yet so it isnt in projectdev
                        pojectdev z = new pojectdev();
                        z.projectid = zz.id;
                        nn.Add(z);
                    
                    }
                   
              }
               
                a.Projects = nn.ToList();
               // a.Projects = nn;
            }
         
            if (User.IsInRole("developer")) { 
                var adminproj = db.pojectdevs.Select(x => x).Where(x => x.devname == User.Identity.Name).ToList();
                a.Projects = adminproj;
            
            }

            if (User.IsInRole("projectowner"))
            {
                //for returning all the projects the user is its projectowner and converting it to projectdev
                var adminproj = db.projects.Select(x => x).Where(x => x.projectowner == User.Identity.Name).ToList();
                foreach (var zz in adminproj)
                {
                    var q = db.pojectdevs.Select(x => x).Where(x => x.projectid == zz.id).FirstOrDefault();
                    if (q != null) { nn.Add(q); }
                    else
                    {   //if the projects doesnt contains developers yet so it isnt in projectdev
                        pojectdev z = new pojectdev();
                        z.projectid = zz.id;
                        nn.Add(z);

                    }
                }
                a.Projects = nn;
            }
            
            if (User.IsInRole("teamleader")) { var adminproj = db.projects.Select(x => x).Where(x => x.projectleader == User.Identity.Name).ToList();

           
            foreach (var zz in adminproj)
            {
                var q = db.pojectdevs.Select(x => x).Where(x => x.projectid == zz.id).FirstOrDefault();
                if (q != null) { nn.Add(q); }
                else
                {
                    pojectdev z = new pojectdev();
                    z.projectid = zz.id;
                    nn.Add(z);

                }
            }
            a.Projects = nn;
            }
            
            
            a.Stream = db.activitystreams.Select(x=>x).OrderByDescending(x => x.actiontype).ToList();
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";
            ViewBag.land = "y";
            return View(a);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Viewusers()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [Authorize(Roles = "admin")] 
        public String hi(string st = "")
        {
             return st;
        }

        [Authorize(Roles = "developer")]
        public String hidev()
        {
            return "hello dev";
        }
        [Authorize(Roles = "projectowner")]
        public String hipro()
        {
            return "hello p";
        }
        [Authorize(Roles = "teamleader")]
        public String hiteam()
        {
            return "hello t";
        }
    }
}
