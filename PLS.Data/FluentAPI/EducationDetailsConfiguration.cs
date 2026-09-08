using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Data.FluentAPI
{
    public class EducationDetailsConfiguration : IEntityTypeConfiguration<EducationDetails>
    {

        public void Configure(EntityTypeBuilder<EducationDetails> builder)
        {
            //Table Configuration
            builder.ToTable("EducationDetails");

            //Key configuration
            builder.HasKey(e => e.Id);

            //Property configuration
            builder.Property(e => e.EducationType)
                   .HasColumnName("EducationType")
                   .IsRequired(true);

            builder.Property(e => e.Institution)
                   .HasColumnName("Institution")
                   .IsRequired(true)
                   .HasMaxLength(200);

            builder.Property(e => e.PercentageOrCgpa)
                   .HasColumnName("PercentageOrCgpa")
                   .IsRequired(true)
                   .HasPrecision(10,2);

            builder.Property(e => e.Backlogs)
                   .HasColumnName("Backlogs")
                   .IsRequired(true);

            builder.Property(e => e.YearOfPassing)
                   .HasColumnName("YearOfPassing")
                   .IsRequired(true);

            builder.Property(e => e.Location)
                   .HasColumnName("Location")
                   .IsRequired(false)
                   .HasMaxLength(500);

            builder.Property(e => e.CreatedDate)
                  .HasColumnName("CreatedDate")
                  .IsRequired()
                  .HasDefaultValueSql("GETDATE()");

            builder.Property(e => e.CreatedBy)
                   .HasColumnName("CreatedBy")
                   .IsRequired(false);

            builder.Property(e => e.UpdatedDate)
                   .HasColumnName("ModifiedDate")
                   .IsRequired(false);

            builder.Property(e => e.UpdatedBy)
                   .HasColumnName("ModifiedBy")
                   .IsRequired(false);

            builder.Property(e => e.DeletedDate)
                   .HasColumnName("DeletedDate")
                   .IsRequired(false);

            builder.Property(e => e.DeletedBy)
                   .HasColumnName("DeletedBy")
                   .IsRequired(false)
                   .HasColumnType("int");

            builder.Property(e => e.IsActive)
                   .HasColumnName("IsActive")
                   .HasDefaultValue(true)
                   .IsRequired(true);

            builder.Property(e => e.IsDeleted)
                   .HasColumnName("IsDeleted")
                   .HasDefaultValue(false)
                   .IsRequired(true);

            //Relationship Configuration
            builder.HasOne(e => e.Student)
                   .WithMany(s=> s.Educations)
                   .HasForeignKey(e => e.StudentId)
                   .IsRequired(true)
                   .OnDelete(DeleteBehavior.Cascade);

           builder.HasQueryFilter(e => !e.IsDeleted);

        }
    }
}
