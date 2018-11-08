using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using MvcApplicationTest1.DAL;
using System.Web.Routing;

namespace MvcApplicationTest1.Controllers
{
    public class BacklogapiController : ApiController
    {
        private ftestEntities db = new ftestEntities();

        public BacklogapiController()
        {
            db.Configuration.ProxyCreationEnabled = false;

        }

        // GET api/Backlogapi
        public IEnumerable<issue> Getissues()
        {

            return db.issues.AsEnumerable();
        }



        // GET api/Backlogapi/5
        public IEnumerable<issue> Getissue(int id)
        {
           

            return db.issues.Select(x => x).Where(x => x.projectid == id && x.sprintid == null ).AsEnumerable();;
        }

        // PUT api/Backlogapi/5
        public HttpResponseMessage Putissue(int id, issue issue)
        {
            if (!ModelState.IsValid)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
            }

            if (id != issue.id)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            db.Entry(issue).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/Backlogapi
        public HttpResponseMessage Postissue(issue issue)
        {
            var zq = db.columns.Select(x => x).Where(x => x.projectid == issue.projectid).FirstOrDefault();
            issue.status = zq.colid;
                db.issues.Add(issue);
                try { db.SaveChanges();   
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
                {
                    Exception raise = dbEx;
                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            string message = string.Format("{0}:{1}",
                                validationErrors.Entry.Entity.ToString(),
                                validationError.ErrorMessage);
                            // raise a new exception nesting
                            // the current instance as InnerException
                            raise = new InvalidOperationException(message, raise);
                        }
                    }
                    throw raise;
                }
                

                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, issue);
                response.Headers.Location = new Uri(Url.Link("DefaultApi", new { id = issue.id }));
                return response;
           
        }

        // DELETE api/Backlogapi/5
        public HttpResponseMessage Deleteissue(int id)
        {
            issue issue = db.issues.Find(id);
            if (issue == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }

            db.issues.Remove(issue);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.NotFound, ex);
            }

            return Request.CreateResponse(HttpStatusCode.OK, issue);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}