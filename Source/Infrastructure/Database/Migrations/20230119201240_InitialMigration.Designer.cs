﻿// <auto-generated />
using System;

using Infrastructure.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Infrastructure.Migrations
{
    [DbContext(typeof(FuturesTradingDbContext))]
    [Migration("20230119201240_InitialMigration")]
    partial class InitialMigration
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Application.Data.Entities.CandlestickDbEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<decimal>("Close")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.Property<string>("CurrencyPair")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("nvarchar(16)")
                        .HasColumnName("Currency Pair");

                    b.Property<DateTime>("DateTime")
                        .HasColumnType("datetime2");

                    b.Property<decimal>("High")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.Property<decimal>("Low")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.Property<decimal>("Open")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.Property<DateTime>("RecordCreatedDate")
                        .HasMaxLength(50)
                        .HasColumnType("datetime2")
                        .HasColumnName("Record Created Date");

                    b.Property<DateTime>("RecordModifiedDate")
                        .HasMaxLength(50)
                        .HasColumnType("datetime2")
                        .HasColumnName("Record Modified Date");

                    b.Property<decimal>("Volume")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyPair");

                    b.HasIndex("CurrencyPair", "DateTime")
                        .IsUnique();

                    b.ToTable("Candlesticks");
                });

            modelBuilder.Entity("Application.Data.Entities.FuturesOrderDbEntity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<long>("BinanceID")
                        .HasColumnType("bigint")
                        .HasColumnName("Binance ID");

                    b.Property<int>("CandlestickId")
                        .HasColumnType("int")
                        .HasColumnName("Candlestick Id");

                    b.Property<DateTime>("CreateTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("OrderSide")
                        .HasMaxLength(8)
                        .HasColumnType("int")
                        .HasColumnName("Order Side");

                    b.Property<int>("OrderType")
                        .HasMaxLength(32)
                        .HasColumnType("int")
                        .HasColumnName("Order Type");

                    b.Property<decimal>("Price")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.Property<decimal>("Quantity")
                        .HasPrecision(18, 4)
                        .HasColumnType("decimal(18,4)");

                    b.Property<DateTime>("RecordCreatedDate")
                        .HasMaxLength(50)
                        .HasColumnType("datetime2")
                        .HasColumnName("Record Created Date");

                    b.Property<DateTime>("RecordModifiedDate")
                        .HasMaxLength(50)
                        .HasColumnType("datetime2")
                        .HasColumnName("Record Modified Date");

                    b.HasKey("Id");

                    b.HasIndex("BinanceID")
                        .IsUnique();

                    b.HasIndex("CandlestickId");

                    b.ToTable("FuturesOrders");
                });

            modelBuilder.Entity("Application.Data.Entities.FuturesOrderDbEntity", b =>
                {
                    b.HasOne("Application.Data.Entities.CandlestickDbEntity", "Candlestick")
                        .WithMany("FuturesOrders")
                        .HasForeignKey("CandlestickId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Candlestick");
                });

            modelBuilder.Entity("Application.Data.Entities.CandlestickDbEntity", b =>
                {
                    b.Navigation("FuturesOrders");
                });
#pragma warning restore 612, 618
        }
    }
}
