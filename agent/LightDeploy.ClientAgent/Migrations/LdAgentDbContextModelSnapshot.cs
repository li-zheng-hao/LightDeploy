﻿// <auto-generated />
using LightDeploy.ClientAgent.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace LightDeploy.ClientAgent.Migrations
{
    [DbContext(typeof(LdAgentDbContext))]
    partial class LdAgentDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.10");

            modelBuilder.Entity("LightDeploy.ClientAgent.Domain.FileRecord", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AbsoluteDirectory")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("MD5")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<long>("PublishTimestamp")
                        .HasColumnType("INTEGER");

                    b.Property<string>("RelativeDirectory")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ServiceName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("FileRecord");
                });
#pragma warning restore 612, 618
        }
    }
}
