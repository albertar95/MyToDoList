using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace ToDoListWebApi.Models
{
    public partial class ToDoListDbContext : DbContext
    {
        public ToDoListDbContext()
        {
        }

        public ToDoListDbContext(DbContextOptions<ToDoListDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Goal> Goals { get; set; } = null!;
        public virtual DbSet<Progress> Progresses { get; set; } = null!;
        public virtual DbSet<Task> Tasks { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("CONNECTION_STRING");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Goal>(entity =>
            {
                entity.HasKey(e => e.NidGoal);

                entity.Property(e => e.NidGoal).ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.LastModifiedDate).HasColumnType("datetime");

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

                entity.HasOne(d => d.Goal)
                    .WithMany(p => p.Progresses)
                    .HasForeignKey(d => d.GoalId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Progresses_Goals");

                entity.HasOne(d => d.Task)
                    .WithMany(p => p.Progresses)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Progresses_Tasks");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Progresses)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Progresses_Users");
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
