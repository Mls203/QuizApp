//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QuizApp.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class report
    {
        public int id { get; set; }
        public string cat_encrytped_string { get; set; }
        public string cat_name { get; set; }
        public string std_name { get; set; }
        public string std_score { get; set; }
        public Nullable<int> std_id { get; set; }
        public Nullable<int> cat_id { get; set; }
    }
}
