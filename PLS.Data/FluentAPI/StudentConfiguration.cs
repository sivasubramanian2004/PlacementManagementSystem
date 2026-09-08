using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.Core.Helpers;
using PMS.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Data.FluentAPI
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            //Table Configuration
            builder.ToTable("Students");

            //Key configuration
            builder.HasKey(s => s.Id);

            //Property configuration
            builder.Property(s => s.RegisterNumber)
                   .HasColumnName("RegisterNumber")
                   .IsRequired(true)
                   .HasMaxLength(100);

            builder.Property(s => s.Name)
                   .HasColumnName("Name")
                   .IsRequired(true)
                   .HasMaxLength(100);

            builder.Property(s => s.Gender)
                  .HasColumnName("Gender")
                  .IsRequired(true);

            builder.Property(s => s.Phone)
                   .HasColumnName("Phone")
                   .IsRequired(false)
                   .HasMaxLength(30);

            builder.Property(s => s.Skills)
                   .HasColumnName("SkillSets")
                   .IsRequired(false)
                   .HasMaxLength(500);

            builder.Property(s => s.Phone)
                   .HasColumnName("Phone")
                   .IsRequired(false)
                   .HasMaxLength(30);


            builder.Property(s => s.PlacementStatus)
                   .HasColumnName(" PlacementStatus")
                   .IsRequired(true);
                  

            builder.Property(s => s.ResumeUrl)
                   .HasColumnName("ResumeUrl")
                   .IsRequired(true)
                   .HasMaxLength(500);

            builder.Property(s => s.ProfilePictureUrl)
                   .HasColumnName("ProfilePictureUrl")
                   .IsRequired(true)
                   .HasMaxLength(500);

            builder.Property(d => d.CreatedDate)
                   .HasColumnName("CreatedDate")
                   .IsRequired(true)
                   .HasDefaultValueSql("GETDATE()");

            builder.Property(d => d.CreatedBy)
                   .HasColumnName("CreatedBy")
                   .IsRequired(false);

            builder.Property(d => d.UpdatedDate)
                   .HasColumnName("ModifiedDate")
                   .IsRequired(false);

            builder.Property(d => d.UpdatedBy)
                   .HasColumnName("ModifiedBy")
                   .IsRequired(false);

            builder.Property(d => d.DeletedDate)
                   .HasColumnName("DeletedDate")
                   .IsRequired(false);

            builder.Property(d => d.DeletedBy)
                   .HasColumnName("DeletedBy")
                   .IsRequired(false)
                   .HasColumnType("int");

            builder.Property(d => d.IsActive)
                   .HasColumnName("IsActive")
                   .HasDefaultValue(true)
                   .IsRequired(true);

            builder.Property(d => d.IsDeleted)
                   .HasColumnName("IsDeleted")
                   .HasDefaultValue(false)
                   .IsRequired(true);

            //Index Configuration
            builder.HasIndex(u => u.RegisterNumber)
                   .IsUnique(true);

            //Query Filters
            builder.HasQueryFilter(d => !d.IsDeleted);

            //Relationship Configuration

            builder.HasOne(s=>s.User)
                   .WithOne(u=>u.Student)
                   .HasForeignKey<Student>(s => s.UserId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(s => s.Department)
                   .WithMany(d => d.Students)
                   .HasForeignKey(s => s.DepartmentId)
                   .IsRequired()
                   .OnDelete(DeleteBehavior.Cascade);

            

        }
    }
}