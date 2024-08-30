﻿// <auto-generated />
using System;
using GEORGE.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace GEORGE.Server.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
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

                    b.Property<string>("IdLiniiProdukcyjnej")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KtoZapisal")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NazwaLiniiProdukcyjnej")
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

            modelBuilder.Entity("GEORGE.Shared.Models.Logowania", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("Datalogowania")
                        .HasColumnType("datetime2");

                    b.Property<string>("RodzajPrzegladarki")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uzytkownik")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Logowania");
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

                    b.Property<bool>("WidocznyDlaWszystkich")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("PlikiZlecenProdukcyjnych");
                });

            modelBuilder.Entity("GEORGE.Shared.Models.PozDoZlecen", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<float>("Ciezar1Sztuki")
                        .HasColumnType("real");

                    b.Property<DateTime>("DataZapisu")
                        .HasColumnType("datetime2");

                    b.Property<int>("IloscOkien")
                        .HasColumnType("int");

                    b.Property<int>("Iloscskrzydel")
                        .HasColumnType("int");

                    b.Property<float>("JednostkiOkienDoPoz")
                        .HasColumnType("real");

                    b.Property<float>("JednostkiOkienDoPozZrobione")
                        .HasColumnType("real");

                    b.Property<float>("JednostkiOkienSumaDoPoz")
                        .HasColumnType("real");

                    b.Property<string>("Kolor")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("KtoZapisal")
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("Nr")
                        .HasColumnType("real");

                    b.Property<string>("OstatniaZmiana")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowIdLiniiProdukcyjnej")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowIdZlecenia")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("System")
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("Szerokosc")
                        .HasColumnType("real");

                    b.Property<string>("Szyba")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Technologia")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uwagi")
                        .HasColumnType("nvarchar(max)");

                    b.Property<float>("Wysokosc")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.ToTable("PozDoZlecen");
                });

            modelBuilder.Entity("GEORGE.Shared.Models.Pracownicy", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Autorzmiany")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Datautowrzenia")
                        .HasColumnType("datetime2");

                    b.Property<string>("Dzial")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("HasloSQL")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Imie")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Kodkreskowy")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Nazwisko")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Nieaktywny")
                        .HasColumnType("bit");

                    b.Property<string>("Notatka")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowIdDzialu")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Stanowisko")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("StanowiskoSystem")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Telefon")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uwagi")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UzytkownikSQL")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Pracownicy");
                });

            modelBuilder.Entity("GEORGE.Shared.Models.RodzajeDzialow", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Autorzmiany")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Datautowrzenia")
                        .HasColumnType("datetime2");

                    b.Property<string>("NazwaDzialu")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notatka")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uwagi")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("RodzajeDzialow");
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

            modelBuilder.Entity("GEORGE.Shared.Models.Uprawnieniapracownika", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<bool>("Administrator")
                        .HasColumnType("bit");

                    b.Property<string>("Autorzmiany")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Datautowrzenia")
                        .HasColumnType("datetime2");

                    b.Property<bool>("Odczyt")
                        .HasColumnType("bit");

                    b.Property<string>("RowId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowIdPracownicy")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowIdRejestrejestrow")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TableName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Usuniecie")
                        .HasColumnType("bit");

                    b.Property<string>("Uwagi")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Zapis")
                        .HasColumnType("bit");

                    b.Property<bool>("Zmiana")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("Uprawnieniapracownika");
                });

            modelBuilder.Entity("GEORGE.Shared.Models.ZleceniaCzasNaLinieProd", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<long>("Id"));

                    b.Property<int>("CzasNaZlecenie")
                        .HasColumnType("int");

                    b.Property<DateTime>("DataZapisu")
                        .HasColumnType("datetime2");

                    b.Property<string>("KtoZapisal")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OstatniaZmiana")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowId")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowIdLinieProdukcyjne")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RowIdZleceniaProdukcyjne")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uwagi")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("ZlecenieWewnetrzne")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("ZleceniaCzasNaLinieProd");
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

                    b.Property<string>("RowIdZleceniaProdukcyjne")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Uwagi")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("ZlecenieWewnetrzne")
                        .HasColumnType("bit");

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

                    b.Property<DateTime>("DataGotowosci")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataMontazu")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataProdukcji")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataRozpProdukcji")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataWysylki")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataZapisu")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Ilosc")
                        .HasColumnType("int");

                    b.Property<float>("JednostkiNaZlecenie")
                        .HasColumnType("real");

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

                    b.Property<bool>("ZlecZrealizowane")
                        .HasColumnType("bit");

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

                    b.Property<DateTime>("DataGotowosci")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataMontazu")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataProdukcji")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataRozpProdukcji")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataWysylki")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DataZapisu")
                        .HasColumnType("datetime2");

                    b.Property<int>("Ilosc")
                        .HasColumnType("int");

                    b.Property<float>("JednostkiNaZlecenie")
                        .HasColumnType("real");

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

                    b.Property<bool>("ZlecZrealizowane")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("ZleceniaProdukcyjneWew");
                });
#pragma warning restore 612, 618
        }
    }
}
