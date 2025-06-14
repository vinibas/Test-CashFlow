﻿// <auto-generated />
using System;
using CashFlow.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CashFlow.Api.Data.Migrations
{
    [DbContext(typeof(CashFlowContext))]
    [Migration("20250607204909_AddedFieldTransactionAtUtc")]
    partial class AddedFieldTransactionAtUtc
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("CashFlow.Api.Models.DailyConsolidated", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateOnly>("Date")
                        .HasColumnType("date");

                    b.Property<long>("LastLineNumberCalculated")
                        .HasColumnType("bigint");

                    b.Property<decimal>("TotalCredits")
                        .HasColumnType("numeric");

                    b.Property<decimal>("TotalDebits")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("Date")
                        .IsUnique()
                        .HasDatabaseName("IX_DailyConsolidated_Date");

                    b.HasIndex("LastLineNumberCalculated")
                        .IsUnique();

                    b.ToTable("DailyConsolidated");
                });

            modelBuilder.Entity("CashFlow.Api.Models.Entry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .HasMaxLength(250)
                        .IsUnicode(false)
                        .HasColumnType("character varying(250)");

                    b.Property<long>("LineNumber")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<long>("LineNumber"));

                    b.Property<DateTime>("TransactionAtUtc")
                        .HasColumnType("timestamp with time zone");

                    b.Property<char>("Type")
                        .HasColumnType("character(1)");

                    b.Property<decimal>("Value")
                        .HasColumnType("numeric");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAtUtc")
                        .HasDatabaseName("IX_Entry_CreatedAtUtc");

                    b.HasIndex("LineNumber")
                        .IsUnique()
                        .HasDatabaseName("IX_Entry_LineNumber");

                    b.ToTable("Entries");
                });

            modelBuilder.Entity("CashFlow.Api.Models.DailyConsolidated", b =>
                {
                    b.HasOne("CashFlow.Api.Models.Entry", null)
                        .WithOne()
                        .HasForeignKey("CashFlow.Api.Models.DailyConsolidated", "LastLineNumberCalculated")
                        .HasPrincipalKey("CashFlow.Api.Models.Entry", "LineNumber")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
