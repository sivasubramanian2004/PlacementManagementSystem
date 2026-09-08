using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Data.FluentAPI
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {

        public void Configure(EntityTypeBuilder<User> builder)
        {
            //Table Configuration
            builder.ToTable("Users");

            //Key configuration
            builder.HasKey(u => u.Id);

            //Property configuration
            builder.Property(u => u.FirstName)
                   .HasColumnName("FirstName")
                   .IsRequired(true)
                   .HasMaxLength(100);

            builder.Property(u => u.LastName)
                   .HasColumnName("LastName")
                   .IsRequired(true)
                   .HasMaxLength(100);

            builder.Property(u => u.Email)
                   .HasColumnName("EmailID")
                   .IsRequired(true)
                   .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                   .HasColumnName("PasswordHash")
                   .IsRequired(true)
                   .HasMaxLength(100);

            builder.Property(u => u.Role)
                     .HasColumnName("Role")
                     .IsRequired(true)
                     .HasDefaultValue("student")
                     .HasMaxLength(100);

            builder.Property(u => u.CreatedDate)
                 .HasColumnName("CreatedDate")
                 .IsRequired()
                 .HasDefaultValueSql("GETDATE()");

            builder.Property(u => u.CreatedBy)
                   .HasColumnName("CreatedBy")
                   .IsRequired(false);

            builder.Property(u => u.UpdatedDate)
                   .HasColumnName("ModifiedDate")
                   .IsRequired(false);

            builder.Property(u => u.UpdatedBy)
                   .HasColumnName("ModifiedBy")
                   .IsRequired(false);

            builder.Property(u => u.DeletedDate)
                   .HasColumnName("DeletedDate")
                   .IsRequired(false);

            builder.Property(u => u.DeletedBy)
                   .HasColumnName("DeletedBy")
                   .IsRequired(false)
                   .HasColumnType("int");

            builder.Property(u => u.IsActive)
                   .HasColumnName("IsActive")
                   .HasDefaultValue(true)
                   .IsRequired(true);

            builder.Property(u => u.IsDeleted)
                   .HasColumnName("IsDeleted")
                   .HasDefaultValue(false)
                   .IsRequired(true);

            //Index Configuration
            builder.HasIndex(u => u.Email)
                   .IsUnique(true);

            //Query Filters
            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
