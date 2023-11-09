﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

#nullable disable

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.Migrations.ConfigurationDb
{
    [DbContext(typeof(RelayDbContext))]
    [Migration("20231109143956_Add_RequireAuthentication")]
    partial class Add_RequireAuthentication
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.21")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.ClientSecret", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Created")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("Expiration")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("TenantName")
                        .IsRequired()
                        .HasColumnType("character varying(100)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("character varying(4000)");

                    b.HasKey("Id");

                    b.HasIndex("TenantName");

                    b.ToTable("ClientSecrets");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Config", b =>
                {
                    b.Property<string>("TenantName")
                        .HasColumnType("text");

                    b.Property<bool?>("EnableTracing")
                        .HasColumnType("boolean");

                    b.Property<TimeSpan?>("KeepAliveInterval")
                        .HasColumnType("interval");

                    b.Property<TimeSpan?>("ReconnectMaximumDelay")
                        .HasColumnType("interval");

                    b.Property<TimeSpan?>("ReconnectMinimumDelay")
                        .HasColumnType("interval");

                    b.HasKey("TenantName");

                    b.ToTable("Configs");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Connection", b =>
                {
                    b.Property<string>("Id")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<DateTimeOffset>("ConnectTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DisconnectTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("LastSeenTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("OriginId")
                        .HasColumnType("uuid");

                    b.Property<string>("RemoteIpAddress")
                        .HasColumnType("text");

                    b.Property<string>("TenantName")
                        .IsRequired()
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.HasIndex("OriginId");

                    b.HasIndex("TenantName");

                    b.ToTable("Connections");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Origin", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("LastSeenTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("ShutdownTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset>("StartupTime")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Origins");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Request", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Aborted")
                        .HasColumnType("boolean");

                    b.Property<bool>("Errored")
                        .HasColumnType("boolean");

                    b.Property<bool>("Expired")
                        .HasColumnType("boolean");

                    b.Property<bool>("Failed")
                        .HasColumnType("boolean");

                    b.Property<string>("HttpMethod")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<int?>("HttpStatusCode")
                        .HasColumnType("integer");

                    b.Property<long>("RequestBodySize")
                        .HasColumnType("bigint");

                    b.Property<DateTimeOffset>("RequestDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<long>("RequestDuration")
                        .HasColumnType("bigint");

                    b.Property<Guid>("RequestId")
                        .HasColumnType("uuid");

                    b.Property<long>("RequestOriginalBodySize")
                        .HasColumnType("bigint");

                    b.Property<string>("RequestUrl")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<long?>("ResponseBodySize")
                        .HasColumnType("bigint");

                    b.Property<long?>("ResponseOriginalBodySize")
                        .HasColumnType("bigint");

                    b.Property<string>("Target")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("TenantName")
                        .IsRequired()
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.HasIndex("TenantName");

                    b.ToTable("Requests");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Tenant", b =>
                {
                    b.Property<string>("NormalizedName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<string>("ConfigTenantName")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .HasMaxLength(1000)
                        .HasColumnType("character varying(1000)");

                    b.Property<string>("DisplayName")
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.Property<bool>("RequireAuthentication")
                        .HasColumnType("boolean");

                    b.HasKey("NormalizedName");

                    b.HasIndex("ConfigTenantName");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Tenants");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.ClientSecret", b =>
                {
                    b.HasOne("Thinktecture.Relay.Server.Persistence.Models.Tenant", null)
                        .WithMany("ClientSecrets")
                        .HasForeignKey("TenantName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Connection", b =>
                {
                    b.HasOne("Thinktecture.Relay.Server.Persistence.Models.Origin", null)
                        .WithMany("Connections")
                        .HasForeignKey("OriginId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Thinktecture.Relay.Server.Persistence.Models.Tenant", null)
                        .WithMany("Connections")
                        .HasForeignKey("TenantName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Request", b =>
                {
                    b.HasOne("Thinktecture.Relay.Server.Persistence.Models.Tenant", null)
                        .WithMany("Requests")
                        .HasForeignKey("TenantName")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Tenant", b =>
                {
                    b.HasOne("Thinktecture.Relay.Server.Persistence.Models.Config", "Config")
                        .WithMany()
                        .HasForeignKey("ConfigTenantName");

                    b.Navigation("Config");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Origin", b =>
                {
                    b.Navigation("Connections");
                });

            modelBuilder.Entity("Thinktecture.Relay.Server.Persistence.Models.Tenant", b =>
                {
                    b.Navigation("ClientSecrets");

                    b.Navigation("Connections");

                    b.Navigation("Requests");
                });
#pragma warning restore 612, 618
        }
    }
}
