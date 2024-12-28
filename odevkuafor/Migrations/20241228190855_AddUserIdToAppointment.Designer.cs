﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace odevkuafor.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20241228190855_AddUserIdToAppointment")]
    partial class AddUserIdToAppointment
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.27")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("odevkuafor.Models.Appointment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<DateTimeOffset>("AppointmentDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("CustomerName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("EmployeeId")
                        .HasColumnType("integer");

                    b.Property<int>("ServiceId")
                        .HasColumnType("integer");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeId");

                    b.HasIndex("ServiceId");

                    b.ToTable("Appointments");
                });

            modelBuilder.Entity("odevkuafor.Models.Employee", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Specialization")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("odevkuafor.Models.EmployeeService", b =>
                {
                    b.Property<int>("EmployeeId")
                        .HasColumnType("integer");

                    b.Property<int>("ServiceId")
                        .HasColumnType("integer");

                    b.HasKey("EmployeeId", "ServiceId");

                    b.HasIndex("ServiceId");

                    b.ToTable("EmployeeServices");
                });

            modelBuilder.Entity("odevkuafor.Models.Service", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<int>("DurationInMinutes")
                        .HasColumnType("integer");

                    b.Property<int?>("EmployeeId")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<decimal>("Price")
                        .HasColumnType("numeric");

                    b.Property<string>("ServiceType")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("EmployeeId");

                    b.ToTable("Services");
                });

            modelBuilder.Entity("odevkuafor.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("boolean");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("odevkuafor.Models.Appointment", b =>
                {
                    b.HasOne("odevkuafor.Models.Employee", "Employee")
                        .WithMany()
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("odevkuafor.Models.Service", "Service")
                        .WithMany("Appointments")
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Employee");

                    b.Navigation("Service");
                });

            modelBuilder.Entity("odevkuafor.Models.EmployeeService", b =>
                {
                    b.HasOne("odevkuafor.Models.Employee", "Employee")
                        .WithMany("EmployeeServices")
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("odevkuafor.Models.Service", "Service")
                        .WithMany("EmployeeServices")
                        .HasForeignKey("ServiceId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Employee");

                    b.Navigation("Service");
                });

            modelBuilder.Entity("odevkuafor.Models.Service", b =>
                {
                    b.HasOne("odevkuafor.Models.Employee", null)
                        .WithMany("Services")
                        .HasForeignKey("EmployeeId");
                });

            modelBuilder.Entity("odevkuafor.Models.Employee", b =>
                {
                    b.Navigation("EmployeeServices");

                    b.Navigation("Services");
                });

            modelBuilder.Entity("odevkuafor.Models.Service", b =>
                {
                    b.Navigation("Appointments");

                    b.Navigation("EmployeeServices");
                });
#pragma warning restore 612, 618
        }
    }
}
