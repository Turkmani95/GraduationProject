//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MvcApplicationTest1.DAL
{
    using System;
    using System.Collections.Generic;
    
    public partial class sprint
    {
        public sprint()
        {
            this.issues = new HashSet<issue>();
            this.projects = new HashSet<project>();
        }
    
        public int sid { get; set; }
        public int number { get; set; }
        public string date { get; set; }
        public int projectid { get; set; }
        public Nullable<int> numoftissues { get; set; }
        public Nullable<int> estimate { get; set; }
    
        public virtual ICollection<issue> issues { get; set; }
        public virtual ICollection<project> projects { get; set; }
        public virtual project project { get; set; }
    }
}
