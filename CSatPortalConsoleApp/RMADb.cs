//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CSatPortalConsoleApp
{
    using System;
    using System.Collections.Generic;
    
    public partial class RMADb
    {
        public int RMAID { get; set; }
        public System.DateTime DateCreated { get; set; }
        public string RMANo { get; set; }
        public string IssueNo { get; set; }
        public string Product { get; set; }
        public string AffectedPN { get; set; }
        public string Description { get; set; }
        public string ProblemDesc { get; set; }
        public int QTY { get; set; }
    }
}