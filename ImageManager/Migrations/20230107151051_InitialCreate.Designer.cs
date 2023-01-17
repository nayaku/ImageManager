﻿// <auto-generated />
using System;
using ImageManager.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ImageManager.Migrations
{
    [DbContext(typeof(ImageContext))]
    [Migration("20230107151051_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.1")
                .HasAnnotation("Proxies:ChangeTracking", false)
                .HasAnnotation("Proxies:CheckEquality", false)
                .HasAnnotation("Proxies:LazyLoading", true);

            modelBuilder.Entity("ImageManager.Data.Model.Label", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Num")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Labels");
                });

            modelBuilder.Entity("ImageManager.Data.Model.Picture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AddTime")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT")
                        .HasDefaultValueSql("datetime('now','localtime')");

                    b.Property<string>("Hash")
                        .HasColumnType("TEXT");

                    b.Property<int>("Height")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("ThumbnailPath")
                        .HasColumnType("TEXT");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.Property<ulong?>("WeakHash")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Width")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("Hash")
                        .IsUnique();

                    b.ToTable("Pictures");
                });

            modelBuilder.Entity("ImageManager.Data.Model.Workspace", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<int>("Index")
                        .HasColumnType("INTEGER");

                    b.HasKey("Name");

                    b.ToTable("Workspaces");
                });

            modelBuilder.Entity("ImageManager.Data.Model.WorkspacePicture", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsFolded")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Left")
                        .HasColumnType("REAL");

                    b.Property<double>("Opacity")
                        .HasColumnType("REAL");

                    b.Property<int>("PictureId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RotateFlip")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Top")
                        .HasColumnType("REAL");

                    b.Property<string>("WorkspaceName")
                        .HasColumnType("TEXT");

                    b.Property<double>("ZoomRate")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.HasIndex("PictureId");

                    b.HasIndex("WorkspaceName");

                    b.ToTable("WorkspacePicture");
                });

            modelBuilder.Entity("LabelPicture", b =>
                {
                    b.Property<int>("LabelsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PictureId")
                        .HasColumnType("INTEGER");

                    b.HasKey("LabelsId", "PictureId");

                    b.HasIndex("PictureId");

                    b.ToTable("LabelPicture");
                });

            modelBuilder.Entity("ImageManager.Data.Model.WorkspacePicture", b =>
                {
                    b.HasOne("ImageManager.Data.Model.Picture", "Picture")
                        .WithMany()
                        .HasForeignKey("PictureId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ImageManager.Data.Model.Workspace", null)
                        .WithMany("WorkspacePictures")
                        .HasForeignKey("WorkspaceName");

                    b.Navigation("Picture");
                });

            modelBuilder.Entity("LabelPicture", b =>
                {
                    b.HasOne("ImageManager.Data.Model.Label", null)
                        .WithMany()
                        .HasForeignKey("LabelsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ImageManager.Data.Model.Picture", null)
                        .WithMany()
                        .HasForeignKey("PictureId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ImageManager.Data.Model.Workspace", b =>
                {
                    b.Navigation("WorkspacePictures");
                });
#pragma warning restore 612, 618
        }
    }
}
