using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Weda.Template.Domain.Employees.Entities;
using Weda.Template.Domain.Employees.Enums;
using Weda.Template.Domain.Employees.ValueObjects;

namespace Weda.Template.Infrastructure.Employees.Persistence;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .HasConversion(
                name => name.Value,
                value => EmployeeName.Create(value).Value)
            .HasMaxLength(EmployeeName.MaxLength)
            .IsRequired();

        builder.Property(e => e.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value).Value)
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        builder.HasIndex(e => e.Email)
            .IsUnique();

        builder.Property(e => e.Department)
            .HasConversion(
                department => department.Value,
                value => Department.Create(value).Value)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.Position)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.HireDate)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(EmployeeStatus.Active);

        builder.Property(e => e.SupervisorId)
            .IsRequired(false);

        builder.HasOne<Employee>()
            .WithMany()
            .HasForeignKey(e => e.SupervisorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.UpdatedAt)
            .IsRequired(false);
    }
}
