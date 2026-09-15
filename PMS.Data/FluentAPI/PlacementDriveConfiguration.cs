
using PMS.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PMS.Data.FluentAPI
{
    
    namespace PMS.Data.Configurations
    {
        public class PlacementDriveConfiguration : IEntityTypeConfiguration<PlacementDrive>
        {
            public void Configure(EntityTypeBuilder<PlacementDrive> builder)
            {
                builder.ToTable("PlacementDrives");

                builder.HasKey(p => p.Id);

                builder.Property(p => p.Id)
                    .ValueGeneratedOnAdd();

                builder.Property(p => p.CompanyId)
                    .IsRequired();

                builder.Property(p => p.JobTitle)
                    .HasMaxLength(150)
                    .IsRequired();

                builder.Property(p => p.JobDescription)
                    .HasMaxLength(2000)
                    .IsRequired();

                builder.Property(p => p.EmploymentType)
                    .IsRequired();

                builder.Property(p => p.WorkMode)
                    .IsRequired();

                builder.Property(p => p.Location)
                    .HasMaxLength(250)
                    .IsRequired();

                builder.Property(p => p.MinimumCgpa)
                    .HasPrecision(4, 2)
                    .IsRequired();

                builder.Property(p => p.MaximumBacklogs)
                    .IsRequired();

                builder.Property(p => p.GraduationYear)
                    .IsRequired();

                builder.Property(p => p.Salary)
                    .HasPrecision(18, 2)
                    .IsRequired(false);

                builder.Property(p => p.RequiredSkills)
                    .HasMaxLength(1000)
                    .IsRequired(false);

                builder.Property(p => p.ApplicationDeadline)
                    .IsRequired();

                builder.Property(p => p.DriveDate)
                    .IsRequired(false);

                builder.Property(p => p.Status)
                    .IsRequired();

                // Company 1 : N PlacementDrive
                builder.HasOne(p => p.Company)
                    .WithMany(c => c.PlacementDrives)
                    .HasForeignKey(p => p.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }
    }
}
