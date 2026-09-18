using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Data.FluentAPI
{
    public class PlacementDriveDepartmentConfiguration
    : IEntityTypeConfiguration<PlacementDriveDepartment>
    {
        public void Configure(
            EntityTypeBuilder<PlacementDriveDepartment> builder)
        {
            builder.ToTable("PlacementDriveDepartments");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.HasOne(x => x.PlacementDrive)
                .WithMany(x => x.PlacementDriveDepartments)
                .HasForeignKey(x => x.PlacementDriveId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Department)
                .WithMany()
                .HasForeignKey(x => x.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Same department should not be added twice
            builder.HasIndex(x => new
            {
                x.PlacementDriveId,
                x.DepartmentId
            })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        }
    }
}
