using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Data.FluentAPI
{
    public class CompanyConfiguration : IEntityTypeConfiguration<Company>
    {
        public void Configure(EntityTypeBuilder<Company> builder)
        {
            // Table Configuration
            builder.ToTable("Companies");

            // Key Configuration
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                   .ValueGeneratedOnAdd();

            // Company Name
            builder.Property(c => c.Name)
                   .HasColumnName("Name")
                   .IsRequired()
                   .HasMaxLength(150);
            
            builder.HasIndex(c => c.Name)
                   .IsUnique()
                   .HasFilter("[IsDeleted] = 0");
            // Industry
            builder.Property(c => c.IndustryType)
                   .HasColumnName("IndustryType")
                   .IsRequired();

            builder.Property(c => c.OtherIndustry)
                   .HasColumnName("OtherIndustry")
                   .IsRequired(false)
                   .HasMaxLength(100);

            // Website
            builder.Property(c => c.Website)
                   .HasColumnName("Website")
                   .IsRequired(false)
                   .HasMaxLength(500);

            // Email
            builder.Property(c => c.Email)
                   .HasColumnName("Email")
                   .IsRequired(false)
                   .HasMaxLength(255);

            // Phone
            builder.Property(c => c.Phone)
                   .HasColumnName("Phone")
                   .IsRequired(false)
                   .HasMaxLength(30);

            // Description
            builder.Property(c => c.Description)
                   .HasColumnName("Description")
                   .IsRequired(false)
                   .HasMaxLength(1000);

            // Location
            builder.Property(c => c.Location)
                   .HasColumnName("Location")
                   .IsRequired(false)
                   .HasMaxLength(250);

            //Audit Fields
            builder.Property(d => d.CreatedDate)
                  .HasColumnName("CreatedDate")
                  .IsRequired()
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
                   .IsRequired();

            builder.Property(d => d.IsDeleted)
                   .HasColumnName("IsDeleted")
                   .HasDefaultValue(false)
                   .IsRequired();

            // Relationships
            builder.HasMany(c => c.PlacementDrives)
                   .WithOne(pd => pd.Company)
                   .HasForeignKey(pd => pd.CompanyId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}