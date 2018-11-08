using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MvcApplicationTest1.DAL;


namespace MvcApplicationTest1.Models
{
    public class AgileClass
    {
        public IEnumerable<column> Columns { get; set; }
        public IEnumerable<issue> issues { get; set; }
    }
}