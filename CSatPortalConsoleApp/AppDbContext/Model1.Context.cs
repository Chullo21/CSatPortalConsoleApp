﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CSatPortalConsoleApp.AppDbContext
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class CSATDbEntities : DbContext
    {
        public CSATDbEntities()
            : base("name=CSATDbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<C__EFMigrationsHistory> C__EFMigrationsHistory { get; set; }
        public virtual DbSet<C_8DDb> C_8DDb { get; set; }
        public virtual DbSet<AccountsDb> AccountsDb { get; set; }
        public virtual DbSet<ActionDb> ActionDb { get; set; }
        public virtual DbSet<AnnDb> AnnDb { get; set; }
        public virtual DbSet<ART_8D> ART_8D { get; set; }
        public virtual DbSet<DefDb> DefDb { get; set; }
        public virtual DbSet<ERDb> ERDb { get; set; }
        public virtual DbSet<IssueDb> IssueDb { get; set; }
        public virtual DbSet<NotifDb> NotifDb { get; set; }
        public virtual DbSet<RMADb> RMADb { get; set; }
        public virtual DbSet<SEDb> SEDb { get; set; }
        public virtual DbSet<TDDb> TDDb { get; set; }
        public virtual DbSet<TESDb> TESDb { get; set; }
        public virtual DbSet<VerDb> VerDb { get; set; }
    }
}
