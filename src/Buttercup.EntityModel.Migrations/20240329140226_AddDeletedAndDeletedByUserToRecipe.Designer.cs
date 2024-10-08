﻿// <auto-generated />
using System;
using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Buttercup.EntityModel.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20240329140226_AddDeletedAndDeletedByUserToRecipe")]
    partial class AddDeletedAndDeletedByUserToRecipe
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("Buttercup.EntityModel.PasswordResetToken", b =>
                {
                    b.Property<string>("Token")
                        .HasMaxLength(48)
                        .HasColumnType("char")
                        .HasColumnName("token");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("created");

                    b.Property<long>("UserId")
                        .HasColumnType("bigint")
                        .HasColumnName("user_id");

                    b.HasKey("Token")
                        .HasName("pk_password_reset_tokens");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_password_reset_tokens_user_id");

                    b.ToTable("password_reset_tokens", (string)null);
                });

            modelBuilder.Entity("Buttercup.EntityModel.Recipe", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));

                    b.Property<int?>("CookingMinutes")
                        .HasColumnType("int")
                        .HasColumnName("cooking_minutes");

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("created");

                    b.Property<long?>("CreatedByUserId")
                        .HasColumnType("bigint")
                        .HasColumnName("created_by_user_id");

                    b.Property<DateTime?>("Deleted")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("deleted");

                    b.Property<long?>("DeletedByUserId")
                        .HasColumnType("bigint")
                        .HasColumnName("deleted_by_user_id");

                    b.Property<string>("Ingredients")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("ingredients");

                    b.Property<string>("Method")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("method");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("modified");

                    b.Property<long?>("ModifiedByUserId")
                        .HasColumnType("bigint")
                        .HasColumnName("modified_by_user_id");

                    b.Property<int?>("PreparationMinutes")
                        .HasColumnType("int")
                        .HasColumnName("preparation_minutes");

                    b.Property<string>("Remarks")
                        .HasColumnType("text")
                        .HasColumnName("remarks");

                    b.Property<int>("Revision")
                        .HasColumnType("int")
                        .HasColumnName("revision");

                    b.Property<int?>("Servings")
                        .HasColumnType("int")
                        .HasColumnName("servings");

                    b.Property<string>("Source")
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)")
                        .HasColumnName("source");

                    b.Property<string>("Suggestions")
                        .HasColumnType("text")
                        .HasColumnName("suggestions");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)")
                        .HasColumnName("title");

                    b.HasKey("Id")
                        .HasName("pk_recipes");

                    b.HasIndex("CreatedByUserId")
                        .HasDatabaseName("ix_recipes_created_by_user_id");

                    b.HasIndex("Deleted")
                        .HasDatabaseName("ix_recipes_deleted");

                    b.HasIndex("DeletedByUserId")
                        .HasDatabaseName("ix_recipes_deleted_by_user_id");

                    b.HasIndex("ModifiedByUserId")
                        .HasDatabaseName("ix_recipes_modified_by_user_id");

                    b.ToTable("recipes", (string)null);
                });

            modelBuilder.Entity("Buttercup.EntityModel.SecurityEvent", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));

                    b.Property<string>("Event")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)")
                        .HasColumnName("event");

                    b.Property<byte[]>("IpAddress")
                        .HasColumnType("varbinary(16)")
                        .HasColumnName("ip_address");

                    b.Property<DateTime>("Time")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("time");

                    b.Property<long?>("UserId")
                        .HasColumnType("bigint")
                        .HasColumnName("user_id");

                    b.HasKey("Id")
                        .HasName("pk_security_events");

                    b.HasIndex("UserId")
                        .HasDatabaseName("ix_security_events_user_id");

                    b.ToTable("security_events", (string)null);
                });

            modelBuilder.Entity("Buttercup.EntityModel.User", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasColumnName("id");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<long>("Id"));

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("created");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)")
                        .HasColumnName("email");

                    b.Property<string>("HashedPassword")
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)")
                        .HasColumnName("hashed_password");

                    b.Property<bool>("IsAdmin")
                        .HasColumnType("tinyint(1)")
                        .HasColumnName("is_admin");

                    b.Property<DateTime>("Modified")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("modified");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("varchar(250)")
                        .HasColumnName("name");

                    b.Property<DateTime?>("PasswordCreated")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("password_created");

                    b.Property<int>("Revision")
                        .IsConcurrencyToken()
                        .HasColumnType("int")
                        .HasColumnName("revision");

                    b.Property<string>("SecurityStamp")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("char")
                        .HasColumnName("security_stamp");

                    b.Property<string>("TimeZone")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)")
                        .HasColumnName("time_zone");

                    b.HasKey("Id")
                        .HasName("pk_users");

                    b.HasIndex("Email")
                        .IsUnique()
                        .HasDatabaseName("ix_users_email");

                    b.ToTable("users", (string)null);
                });

            modelBuilder.Entity("Buttercup.EntityModel.PasswordResetToken", b =>
                {
                    b.HasOne("Buttercup.EntityModel.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired()
                        .HasConstraintName("fk_password_reset_tokens_users_user_id");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Buttercup.EntityModel.Recipe", b =>
                {
                    b.HasOne("Buttercup.EntityModel.User", "CreatedByUser")
                        .WithMany()
                        .HasForeignKey("CreatedByUserId")
                        .HasConstraintName("fk_recipes_users_created_by_user_id");

                    b.HasOne("Buttercup.EntityModel.User", "DeletedByUser")
                        .WithMany()
                        .HasForeignKey("DeletedByUserId")
                        .HasConstraintName("fk_recipes_users_deleted_by_user_id");

                    b.HasOne("Buttercup.EntityModel.User", "ModifiedByUser")
                        .WithMany()
                        .HasForeignKey("ModifiedByUserId")
                        .HasConstraintName("fk_recipes_users_modified_by_user_id");

                    b.Navigation("CreatedByUser");

                    b.Navigation("DeletedByUser");

                    b.Navigation("ModifiedByUser");
                });

            modelBuilder.Entity("Buttercup.EntityModel.SecurityEvent", b =>
                {
                    b.HasOne("Buttercup.EntityModel.User", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("fk_security_events_users_user_id");

                    b.Navigation("User");
                });
#pragma warning restore 612, 618
        }
    }
}
