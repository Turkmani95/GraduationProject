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
    
    public partial class activitystream
    {
        public int id { get; set; }
        public int issueid { get; set; }
        public int userid { get; set; }
        public string actiontype { get; set; }
        public string issuekey { get; set; }
        public string actiondate { get; set; }
        public string description { get; set; }
        public int projectid { get; set; }
        public string attachment { get; set; }
    
        public virtual activitystream activitystream1 { get; set; }
        public virtual activitystream activitystream2 { get; set; }
        public virtual issue issue { get; set; }
        public virtual project project { get; set; }
    }
}
