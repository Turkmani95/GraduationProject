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
    using System.ComponentModel.DataAnnotations;
    
    public partial class attachment
    {
        public int id { get; set; }
        [Required(ErrorMessage = "Attachment Name is Required")]
        public string Name { get; set; }
        public string attachmentdest { get; set; }
        public int issueid { get; set; }
        public int projectid { get; set; }
    
        public virtual issue issue { get; set; }
        public virtual project project { get; set; }
    }
}
