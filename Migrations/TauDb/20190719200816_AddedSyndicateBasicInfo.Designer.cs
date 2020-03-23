﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TauManager;

namespace TauManager.Migrations.TauDb
{
    [DbContext(typeof(TauDbContext))]
    [Migration("20190719200816_AddedSyndicateBasicInfo")]
    partial class AddedSyndicateBasicInfo
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("TauManager.Models.Campaign", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Comments");

                    b.Property<byte?>("Difficulty");

                    b.Property<int?>("ManagerId");

                    b.Property<string>("Name");

                    b.Property<string>("Station");

                    b.Property<byte>("Status");

                    b.Property<int?>("SyndicateId");

                    b.Property<int?>("Tiers");

                    b.Property<DateTime?>("UTCDateTime");

                    b.HasKey("Id");

                    b.HasIndex("ManagerId");

                    b.HasIndex("SyndicateId");

                    b.ToTable("Campaign");
                });

            modelBuilder.Entity("TauManager.Models.CampaignAttendance", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CampaignId");

                    b.Property<int>("PlayerId");

                    b.HasKey("Id");

                    b.HasIndex("CampaignId");

                    b.HasIndex("PlayerId");

                    b.ToTable("CampaignAttendance");
                });

            modelBuilder.Entity("TauManager.Models.CampaignLoot", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("CampaignId");

                    b.Property<string>("Comments");

                    b.Property<int?>("HolderId");

                    b.Property<int>("ItemId");

                    b.Property<byte>("Status");

                    b.HasKey("Id");

                    b.HasIndex("CampaignId");

                    b.HasIndex("HolderId");

                    b.HasIndex("ItemId");

                    b.ToTable("CampaignLoot");
                });

            modelBuilder.Entity("TauManager.Models.CampaignSignup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool?>("Attending");

                    b.Property<int>("CampaignId");

                    b.Property<int>("PlayerId");

                    b.HasKey("Id");

                    b.HasIndex("CampaignId");

                    b.HasIndex("PlayerId");

                    b.ToTable("CampaignSignup");
                });

            modelBuilder.Entity("TauManager.Models.Item", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal?>("Accuracy");

                    b.Property<string>("Description");

                    b.Property<decimal?>("Energy");

                    b.Property<bool?>("HandToHand");

                    b.Property<string>("ImageUrl");

                    b.Property<decimal?>("Impact");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<decimal?>("Piercing");

                    b.Property<decimal>("Price");

                    b.Property<byte>("Rarity");

                    b.Property<string>("Slug");

                    b.Property<int>("Tier");

                    b.Property<byte>("Type");

                    b.Property<byte?>("WeaponRange");

                    b.Property<byte?>("WeaponType");

                    b.Property<decimal>("Weight");

                    b.HasKey("Id");

                    b.ToTable("Item");
                });

            modelBuilder.Entity("TauManager.Models.LootRequest", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("LootId");

                    b.Property<int>("RequestedById");

                    b.Property<int>("RequestedForId");

                    b.Property<string>("SpecialOfferDescription");

                    b.Property<byte>("Status");

                    b.HasKey("Id");

                    b.HasIndex("LootId");

                    b.HasIndex("RequestedById");

                    b.HasIndex("RequestedForId");

                    b.ToTable("LootRequest");
                });

            modelBuilder.Entity("TauManager.Models.Player", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Active");

                    b.Property<decimal>("Agility");

                    b.Property<decimal>("Bank");

                    b.Property<int>("Bonds");

                    b.Property<decimal>("Intelligence");

                    b.Property<DateTime>("LastUpdate");

                    b.Property<decimal>("Level");

                    b.Property<string>("Name");

                    b.Property<decimal>("Social");

                    b.Property<decimal>("Stamina");

                    b.Property<decimal>("Strength");

                    b.Property<int?>("SyndicateId");

                    b.Property<DateTime?>("UniCourseDate");

                    b.Property<decimal>("Wallet");

                    b.HasKey("Id");

                    b.HasIndex("SyndicateId");

                    b.ToTable("Player");
                });

            modelBuilder.Entity("TauManager.Models.PlayerHistory", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Agility");

                    b.Property<decimal>("Bank");

                    b.Property<int>("Bonds");

                    b.Property<decimal>("Intelligence");

                    b.Property<decimal>("Level");

                    b.Property<int>("PlayerId");

                    b.Property<DateTime>("RecordedAt");

                    b.Property<decimal>("Social");

                    b.Property<decimal>("Stamina");

                    b.Property<decimal>("Strength");

                    b.Property<decimal>("Wallet");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.ToTable("PlayerHistory");
                });

            modelBuilder.Entity("TauManager.Models.PlayerListPositionHistory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Comment");

                    b.Property<DateTime>("CreatedAt");

                    b.Property<int?>("LootRequestId");

                    b.Property<int>("PlayerId");

                    b.HasKey("Id");

                    b.HasIndex("LootRequestId")
                        .IsUnique();

                    b.HasIndex("PlayerId");

                    b.ToTable("PlayerListPositionHistory");
                });

            modelBuilder.Entity("TauManager.Models.PlayerSkill", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("PlayerId");

                    b.Property<int>("SkillId");

                    b.Property<int>("SkillLevel");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.HasIndex("SkillId");

                    b.ToTable("PlayerSkill");
                });

            modelBuilder.Entity("TauManager.Models.Skill", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Skill");
                });

            modelBuilder.Entity("TauManager.Models.SkillGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("SkillId");

                    b.HasKey("Id");

                    b.HasIndex("SkillId")
                        .IsUnique();

                    b.ToTable("SkillGroup");
                });

            modelBuilder.Entity("TauManager.Models.Syndicate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Tag");

                    b.HasKey("Id");

                    b.ToTable("Syndicate");
                });

            modelBuilder.Entity("TauManager.Models.Campaign", b =>
                {
                    b.HasOne("TauManager.Models.Player", "Manager")
                        .WithMany()
                        .HasForeignKey("ManagerId");

                    b.HasOne("TauManager.Models.Syndicate", "Syndicate")
                        .WithMany()
                        .HasForeignKey("SyndicateId");
                });

            modelBuilder.Entity("TauManager.Models.CampaignAttendance", b =>
                {
                    b.HasOne("TauManager.Models.Campaign", "Campaign")
                        .WithMany("Attendance")
                        .HasForeignKey("CampaignId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TauManager.Models.Player", "Player")
                        .WithMany("Attendance")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TauManager.Models.CampaignLoot", b =>
                {
                    b.HasOne("TauManager.Models.Campaign", "Campaign")
                        .WithMany("Loot")
                        .HasForeignKey("CampaignId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TauManager.Models.Player", "Holder")
                        .WithMany()
                        .HasForeignKey("HolderId");

                    b.HasOne("TauManager.Models.Item", "Item")
                        .WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TauManager.Models.CampaignSignup", b =>
                {
                    b.HasOne("TauManager.Models.Campaign", "Campaign")
                        .WithMany("Signups")
                        .HasForeignKey("CampaignId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TauManager.Models.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TauManager.Models.LootRequest", b =>
                {
                    b.HasOne("TauManager.Models.CampaignLoot", "Loot")
                        .WithMany("Requests")
                        .HasForeignKey("LootId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TauManager.Models.Player", "RequestedBy")
                        .WithMany()
                        .HasForeignKey("RequestedById")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TauManager.Models.Player", "RequestedFor")
                        .WithMany()
                        .HasForeignKey("RequestedForId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TauManager.Models.Player", b =>
                {
                    b.HasOne("TauManager.Models.Syndicate", "Syndicate")
                        .WithMany()
                        .HasForeignKey("SyndicateId");
                });

            modelBuilder.Entity("TauManager.Models.PlayerHistory", b =>
                {
                    b.HasOne("TauManager.Models.Player", "Player")
                        .WithMany("History")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TauManager.Models.PlayerListPositionHistory", b =>
                {
                    b.HasOne("TauManager.Models.LootRequest", "LootRequest")
                        .WithOne("HistoryEntry")
                        .HasForeignKey("TauManager.Models.PlayerListPositionHistory", "LootRequestId");

                    b.HasOne("TauManager.Models.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TauManager.Models.PlayerSkill", b =>
                {
                    b.HasOne("TauManager.Models.Player", "Player")
                        .WithMany("PlayerSkills")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("TauManager.Models.Skill", "Skill")
                        .WithMany("PlayerRelations")
                        .HasForeignKey("SkillId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("TauManager.Models.SkillGroup", b =>
                {
                    b.HasOne("TauManager.Models.Skill", "Skill")
                        .WithOne("Groups")
                        .HasForeignKey("TauManager.Models.SkillGroup", "SkillId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
