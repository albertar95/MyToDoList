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

        public virtual DbSet<Account> Accounts { get; set; } = null!;
        public virtual DbSet<Goal> Goals { get; set; } = null!;
        public virtual DbSet<Note> Notes { get; set; } = null!;
        public virtual DbSet<NoteGroup> NoteGroups { get; set; } = null!;
        public virtual DbSet<Progress> Progresses { get; set; } = null!;
        public virtual DbSet<Routine> Routines { get; set; } = null!;
        public virtual DbSet<RoutineProgress> RoutineProgresses { get; set; } = null!;
        public virtual DbSet<Schedule> Schedules { get; set; } = null!;
        public virtual DbSet<Task> Tasks { get; set; } = null!;
        public virtual DbSet<Shield> Shields { get; set; } = null!;
        public virtual DbSet<Transaction> Transactions { get; set; } = null!;
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
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.NidAccount);

                entity.Property(e => e.NidAccount).ValueGeneratedNever();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.LastModified).HasColumnType("datetime");

                entity.Property(e => e.LendAmount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.Title).HasMaxLength(50);
                entity.HasOne(d => d.User)
.WithMany(p => p.Accounts)
.HasForeignKey(d => d.UserId)
.OnDelete(DeleteBehavior.ClientSetNull)
.HasConstraintName("FK_Accounts_Users");

            });

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

            modelBuilder.Entity<Note>(entity =>
            {
                entity.HasKey(e => e.NidNote);

                entity.Property(e => e.NidNote).ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(250);

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Notes)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Notes_NoteGroups");
            });

            modelBuilder.Entity<NoteGroup>(entity =>
            {
                entity.HasKey(e => e.NidGroup);

                entity.Property(e => e.NidGroup).ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.Title).HasMaxLength(250);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.NoteGroups)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_NoteGroups_Users");
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

            modelBuilder.Entity<Routine>(entity =>
            {
                entity.HasKey(e => e.NidRoutine);

                entity.Property(e => e.NidRoutine).ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.FromDate).HasColumnType("date");

                entity.Property(e => e.ModifiedDate).HasColumnType("datetime");

                entity.Property(e => e.RepeatDays).HasMaxLength(25);

                entity.Property(e => e.Todate).HasColumnType("date");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Routines)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Routines_Users");
            });

            modelBuilder.Entity<RoutineProgress>(entity =>
            {
                entity.HasKey(e => e.NidRoutineProgress);

                entity.Property(e => e.NidRoutineProgress).ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.ProgressDate).HasColumnType("date");

                entity.HasOne(d => d.Routine)
                    .WithMany(p => p.RoutineProgresses)
                    .HasForeignKey(d => d.RoutineId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RoutineProgresses_Routines");
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
            modelBuilder.Entity<Shield>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.LastModified).HasColumnType("datetime");

                entity.HasOne(d => d.User)
                .WithMany(p => p.Shields)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Shields_Users");
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

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.NidTransaction);

                entity.Property(e => e.NidTransaction).ValueGeneratedNever();

                entity.Property(e => e.Amount).HasColumnType("decimal(18, 0)");

                entity.Property(e => e.CreateDate).HasColumnType("datetime");

                entity.Property(e => e.TransactionReason).HasMaxLength(50);
                entity.HasOne(d => d.User)
.WithMany(p => p.Transactions)
.HasForeignKey(d => d.UserId)
.OnDelete(DeleteBehavior.ClientSetNull)
.HasConstraintName("FK_Transactions_Users");
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
