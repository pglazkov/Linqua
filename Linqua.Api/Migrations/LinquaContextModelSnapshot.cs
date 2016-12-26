using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Linqua.Api.Models;

namespace Linqua.Api.Migrations
{
    [DbContext(typeof(LinquaContext))]
    partial class LinquaContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.0-rtm-22752")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Linqua.Api.Models.Entry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTimeOffset>("ClientCreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("sysutcdatetime()");

                    b.Property<DateTimeOffset?>("CreatedAt")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("sysutcdatetime()");

                    b.Property<string>("Definition");

                    b.Property<string>("DefinitionLanguageCode")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("'en'");

                    b.Property<bool>("Deleted");

                    b.Property<bool>("IsLearnt")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<string>("Text")
                        .HasMaxLength(256);

                    b.Property<string>("TextLanguageCode");

                    b.Property<int>("TranslationState")
                        .ValueGeneratedOnAdd()
                        .HasDefaultValueSql("0");

                    b.Property<DateTimeOffset?>("UpdatedAt");

                    b.Property<Guid?>("UserId");

                    b.Property<byte[]>("Version")
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("timestamp");

                    b.HasKey("Id");

                    b.HasIndex("CreatedAt")
                        .HasName("IX_CreatedAt");

                    b.HasIndex("Text")
                        .HasName("IX_Text");

                    b.HasIndex("UserId");

                    b.ToTable("Entries");
                });

            modelBuilder.Entity("Linqua.Api.Models.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Auth0Id")
                        .IsRequired()
                        .HasMaxLength(256);

                    b.Property<string>("Email")
                        .HasMaxLength(512);

                    b.HasKey("Id");

                    b.HasIndex("Auth0Id")
                        .IsUnique()
                        .HasName("IX_Auth0Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Linqua.Api.Models.Entry", b =>
                {
                    b.HasOne("Linqua.Api.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK_linqua.Entries_linqua.Users_UserId");
                });
        }
    }
}
