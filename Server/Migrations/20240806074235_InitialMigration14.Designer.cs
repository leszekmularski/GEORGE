﻿// <auto-generated />
using System;
using GEORGE.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GEORGE.Server.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240806074235_InitialMigration14")]
    partial class InitialMigration14
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("GEORGE.Shared.Models.KantowkaDoZlecen", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("DataRealizacji")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataZamowienia")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataZapisu")
                        .HasColumnType("datetime2");

                    b.Property<int>("DlugoscNaGotowo")
                        .HasColumnType("int");

                    b.Property<string>("DlugoscNaGotowoGrupa")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("DlugoscZamawiana")
                        .HasColumnType("int");

                    b.Property<string>("GatunekKantowki")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("IloscSztuk")
                        .HasColumnType("int");

                    b.Property<string>("KodProduktu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KtoZapisal")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("MaterialZeStanMagazyn")
                        .HasColumnType("bit");

                    b.Property<string>("NazwaProduktu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OstatniaZmiana")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Przekroj")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowIdZlecenia")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uwagi")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("KantowkaDoZlecen");
                });

            modelBuilder.Entity("GEORGE.Shared.Models.KartyInstrukcyjne", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("DataZapisu")
                        .HasColumnType("datetime2");

                    b.Property<int>("IloscStron")
                        .HasColumnType("int");

                    b.Property<string>("KodProduktu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KtoZapisal")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LinkDoKartyNaSerwerze")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NazwaProduktu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NazwaProduktu2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NumerKarty")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OpisKarty")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OstatniaZmiana")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uwagi")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("KartyInstrukcyjne");
                });

            modelBuilder.Entity("GEORGE.Shared.Models.LinieProdukcyjne", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("DataZapisu")
                        .HasColumnType("datetime2");

                    b.Property<int>("DziennaZdolnoscProdukcyjna")
                        .HasColumnType("int");

                    b.Property<string>("KtoZapisal")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NazwaLiniiProdukcyjnej")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NumerKarty")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OstatniaZmiana")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uwagi")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("LinieProdukcyjne");
                });

            modelBuilder.Entity("GEORGE.Shared.Models.PlikiZlecenProdukcyjnych", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("DataZapisu")
                        .HasColumnType("datetime2");

                    b.Property<string>("KtoZapisal")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NazwaPliku")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OryginalnaNazwaPliku")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OstatniaZmiana")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowIdZleceniaProdukcyjne")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TypPliku")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uwagi")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("PlikiZlecenProdukcyjnych");
                });

            modelBuilder.Entity("GEORGE.Shared.Models.RodzajeKartInstrukcyjnych", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("DataZapisu")
                        .HasColumnType("datetime2");

                    b.Property<string>("KodProduktu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KtoZapisal")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NazwaProduktu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NazwaProduktu2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NumerRodzajuKart")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("OpisRodzajuKart")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OstatniaZmiana")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uwagi")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("NumerRodzajuKart")
                        .IsUnique()
                        .HasFilter("[NumerRodzajuKart] IS NOT NULL");

                    b.ToTable("RodzajeKartInstrukcyjnych");
                });

            modelBuilder.Entity("GEORGE.Shared.Models.ZleceniaNaLinii", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("DataZapisu")
                        .HasColumnType("datetime2");

                    b.Property<string>("OstatniaZmiana")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowIdLinieProdukcyjne")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RowIdZleceniaProdukcyjne")
                        .HasColumnType("int");

                    b.Property<string>("Uwagi")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ZleceniaNaLinii");
                });

            modelBuilder.Entity("GEORGE.Shared.Models.ZleceniaProdukcyjne", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Adres")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DataMontazu")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataProdukcji")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataWysylki")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataZapisu")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Ilosc")
                        .HasColumnType("int");

                    b.Property<string>("Klient")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KodProduktu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KtoZapisal")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Miejscowosc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NazwaProduktu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NazwaProduktu2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NumerUmowy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NumerZamowienia")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OstatniaZmiana")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Tags")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Telefon")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TypZamowienia")
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("Wartosc")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.ToTable("ZleceniaProdukcyjne");
                });

            modelBuilder.Entity("GEORGE.Shared.Models.ZleceniaProdukcyjneWew", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Adres")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("DataMontazu")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataProdukcji")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataWysylki")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataZapisu")
                        .HasColumnType("datetime2");

                    b.Property<int>("Ilosc")
                        .HasColumnType("int");

                    b.Property<string>("Klient")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KodProduktu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KtoZapisal")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Miejscowosc")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NazwaProduktu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NazwaProduktu2")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NumerUmowy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NumerZamowienia")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OstatniaZmiana")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Tags")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Telefon")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TypZamowienia")
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("Wartosc")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.ToTable("ZleceniaProdukcyjneWew");
                });
#pragma warning restore 612, 618
        }
    }
}
