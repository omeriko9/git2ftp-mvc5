﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace git2ftp_mvc5.App_Code.DAL
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class omeriko9Entities : DbContext
    {
        public omeriko9Entities()
            : base("name=omeriko9Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<git2ftp_Log> git2ftp_Log { get; set; }
        public virtual DbSet<git2ftp_Projects> git2ftp_Projects { get; set; }
        public virtual DbSet<git2ftp_Users> git2ftp_Users { get; set; }
    }
}
