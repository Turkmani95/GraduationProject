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
    
    public partial class column
    {
        public column()
        {
            this.issues = new HashSet<issue>();
        }
    
        public int colid { get; set; }
        [StringLength(20, ErrorMessage = "Must be less than 20 letters")]
        [Required(ErrorMessage = "Column Name is Required")]
        public string name { get; set; }
        public int projectid { get; set; }
    
        public virtual ICollection<issue> issues { get; set; }
        public virtual project project { get; set; }
    }
}
