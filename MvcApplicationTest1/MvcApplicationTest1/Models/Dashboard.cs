using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcApplicationTest1.DAL;


namespace MvcApplicationTest1.Models
{
    public class Dashboard
    {
        public IEnumerable<issue> issues { set; get; }

        public IEnumerable<activitystream> Stream { get; set; }

        public IEnumerable<pojectdev> Projects { set; get; }
    }
}