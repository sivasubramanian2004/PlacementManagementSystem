using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using PMS.Data.Entities;
namespace PMS.Core.Validators.Applications
{
    public class ApplicationConfiguration
      : IEntityTypeConfiguration<Application>
    {
        public void Configure(EntityTypeBuilder<Application> builder)
        {
            builder.ToTable("Applications");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.Id)
                .ValueGeneratedOnAdd();

            builder.Property(a => a.StudentId)
                .IsRequired();

            builder.Property(a => a.PlacementDriveId)
                .IsRequired();

            builder.Property(a => a.AppliedDate)
                .IsRequired();

            builder.Property(a => a.Status)
                .IsRequired();

            builder.HasOne(a => a.Student)
                .WithMany(s => s.Applications)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.PlacementDrive)
                .WithMany(p => p.Applications)
                .HasForeignKey(a => a.PlacementDriveId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.ApplicationNumber)
                  .HasMaxLength(50)
                 .IsRequired();

            builder.HasIndex(a => a.ApplicationNumber)
                .IsUnique();

            builder.HasIndex(a => new
            {
                a.StudentId,
                a.PlacementDriveId
            })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        }
    }
}
