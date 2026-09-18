using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Data.FluentAPI
{
    public class ApplicationNumberCounterConfiguration
     : IEntityTypeConfiguration<ApplicationNumberCounter>
    {
        public void Configure(EntityTypeBuilder<ApplicationNumberCounter> builder)
        {
            builder.ToTable("ApplicationNumberCounters");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Year)
                .IsRequired();

            builder.Property(x => x.LastSequence)
                .IsRequired();

            builder.HasIndex(x => x.Year)
                .IsUnique();
        }
    }
}
