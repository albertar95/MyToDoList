using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ToDoListWebApp.Models
{
    public partial class ToDoListDbContext : DbContext
    {
        public string DbPath { get; }
        public ToDoListDbContext()
        {
            //var folder = Environment.SpecialFolder.LocalApplicationData;
            //var path = Environment.GetFolderPath(folder);
            //DbPath = System.IO.Path.Join(path, "MyTodoListDb.db");
            DbPath = System.IO.Path.Join("C:\\sqliteDb", "MyTodoListDb.db");
        }

        public ToDoListDbContext(DbContextOptions<ToDoListDbContext> options)
            : base(options)
        {
            //var folder = Environment.SpecialFolder.LocalApplicationData;
            //var path = Environment.GetFolderPath(folder);
            //DbPath = System.IO.Path.Join(path, "MyTodoListDb.db");
            DbPath = System.IO.Path.Join("C:\\sqliteDb", "MyTodoListDb.db");
        }

        public virtual DbSet<Goal> Goals { get; set; } = null!;
        public virtual DbSet<Progress> Progresses { get; set; } = null!;
        public virtual DbSet<Schedule> Schedules { get; set; } = null!;
        public virtual DbSet<Task> Tasks { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //sql database

            //    IConfigurationRoot configuration = new ConfigurationBuilder()
            //.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            //.AddJsonFile("appsettings.json")
            //.Build();
            //    optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

            //sqlite database
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Goal>(entity =>
            {
                entity.HasKey(e => e.NidGoal);

                entity.Property(e => e.NidGoal).ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.FromDate).HasColumnType("date");

                entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.ToDate).HasColumnType("date");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Goals)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Goals_Users");
            });

            modelBuilder.Entity<Progress>(entity =>
            {
                entity.HasKey(e => e.NidProgress);

                entity.Property(e => e.NidProgress).ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.HasOne(d => d.Schedule)
                    .WithMany(p => p.Progresses)
                    .HasForeignKey(d => d.ScheduleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Progresses_Schedules");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Progresses)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Progresses_Users");
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.HasKey(e => e.NidSchedule);

                entity.Property(e => e.NidSchedule).ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.ScheduleDate).HasColumnType("date");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.Schedules)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Schedules_Tasks");
            });

            modelBuilder.Entity<Task>(entity =>
            {
                entity.HasKey(e => e.NidTask);

                entity.Property(e => e.NidTask).ValueGeneratedNever();

                entity.Property(e => e.ClosureDate).HasColumnType("datetime");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");

                entity.HasOne(d => d.Goal)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.GoalId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Goals_Tasks");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tasks_Users");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.NidUser);

                entity.HasIndex(e => e.Username, "IX_Username")
                    .IsUnique();

                entity.Property(e => e.NidUser).ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.LastLoginDate).HasColumnType("datetime");

                entity.Property(e => e.Password).HasMaxLength(150);

                entity.Property(e => e.Username).HasMaxLength(150);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
