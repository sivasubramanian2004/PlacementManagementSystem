using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.Data.Entities;
namespace PMS.Data.FluentAPI
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {

        public void Configure(EntityTypeBuilder<Department> builder)
        {
            //Table Configuration
            builder.ToTable("Departments");

            //Key configuration
            builder.HasKey(d => d.Id);

            //Property configuration
            builder.Property(d => d.Name)
                   .HasColumnName("DepartmentName")
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(d => d.Code)
                   .HasColumnName("DepartmentCode")
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(d => d.Manager)
                   .HasColumnName("DepartmentManager")
                   .IsRequired(false)
                   .HasMaxLength(100);

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
                   .IsRequired(true);

            builder.Property(d => d.IsDeleted)
                   .HasColumnName("IsDeleted")
                   .HasDefaultValue(false)
                   .IsRequired(true);

            //Index Configuration
            builder.HasIndex(d => d.Name)
                   .IsUnique(true);

            //Query Filters
            builder.HasQueryFilter(d => !d.IsDeleted);


        }
    }
}
