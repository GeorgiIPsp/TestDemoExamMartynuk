using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ExamenDem_First.Models;

public partial class ConferencesDataBaseContext : DbContext
{
    public ConferencesDataBaseContext()
    {
    }

    public ConferencesDataBaseContext(DbContextOptions<ConferencesDataBaseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Audience> Audiences { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<EquipmentWriteOff> EquipmentWriteOffs { get; set; }

    public virtual DbSet<Floor> Floors { get; set; }

    public virtual DbSet<Office> Offices { get; set; }

    public virtual DbSet<OfficesAudience> OfficesAudiences { get; set; }

    public virtual DbSet<Post> Posts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Worker> Workers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=DESKTOP-R78OBKG\\SQLEXPRESS;Initial Catalog='ConferencesDataBase';Integrated Security=True;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Audience>(entity =>
        {
            entity.HasKey(e => e.IdAudience);

            entity.ToTable("Audience");

            entity.Property(e => e.IdAudience)
                .ValueGeneratedNever()
                .HasColumnName("ID_audience");
            entity.Property(e => e.IdFloor).HasColumnName("ID_floor");
            entity.Property(e => e.NumberAudience)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Number_audience");

            entity.HasOne(d => d.IdFloorNavigation).WithMany(p => p.Audiences)
                .HasForeignKey(d => d.IdFloor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Audience_Floor");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.IdEquipment);

            entity.Property(e => e.IdEquipment)
                .ValueGeneratedNever()
                .HasColumnName("ID_equipment");
            entity.Property(e => e.DateTransferToCompanyBalance).HasColumnName("Date_transfer_to_company_balance");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.IdAudience).HasColumnName("ID_audience");
            entity.Property(e => e.IdOffices).HasColumnName("ID_offices");
            entity.Property(e => e.IdWorker).HasColumnName("ID_worker");
            entity.Property(e => e.InventoryNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Inventory_number");
            entity.Property(e => e.Photo)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TitleEquipment)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("Title_equipment");
            entity.Property(e => e.WeightInKg).HasColumnName("Weight_in_kg");

            entity.HasOne(d => d.IdAudienceNavigation).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.IdAudience)
                .HasConstraintName("FK_Equipment_Audience");

            entity.HasOne(d => d.IdOfficesNavigation).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.IdOffices)
                .HasConstraintName("FK_Equipment_Offices");

            entity.HasOne(d => d.IdWorkerNavigation).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.IdWorker)
                .HasConstraintName("FK_Equipment_Worker");
        });

        modelBuilder.Entity<EquipmentWriteOff>(entity =>
        {
            entity.HasKey(e => e.IdWriteoff).HasName("PK__Equipmen__94E57C1738AD9955");

            entity.ToTable("Equipment_WriteOff");

            entity.Property(e => e.IdWriteoff).HasColumnName("ID_writeoff");
            entity.Property(e => e.IdEquipment).HasColumnName("ID_equipment");
            entity.Property(e => e.IdWorker).HasColumnName("ID_worker");
            entity.Property(e => e.Reason)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.WriteOffDate).HasColumnName("WriteOff_date");

            entity.HasOne(d => d.IdEquipmentNavigation).WithMany(p => p.EquipmentWriteOffs)
                .HasForeignKey(d => d.IdEquipment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Equipment__ID_eq__151B244E");

            entity.HasOne(d => d.IdWorkerNavigation).WithMany(p => p.EquipmentWriteOffs)
                .HasForeignKey(d => d.IdWorker)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Equipment__ID_wo__160F4887");
        });

        modelBuilder.Entity<Floor>(entity =>
        {
            entity.HasKey(e => e.IdFloor);

            entity.ToTable("Floor");

            entity.Property(e => e.IdFloor)
                .ValueGeneratedNever()
                .HasColumnName("ID_floor");
            entity.Property(e => e.FloorTitle).HasColumnName("Floor_title");
        });

        modelBuilder.Entity<Office>(entity =>
        {
            entity.HasKey(e => e.IdOffice);

            entity.Property(e => e.IdOffice)
                .ValueGeneratedNever()
                .HasColumnName("ID_office");
            entity.Property(e => e.Abbreviated)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.FullTitle)
                .HasMaxLength(250)
                .IsUnicode(false)
                .HasColumnName("Full_title");
        });

        modelBuilder.Entity<OfficesAudience>(entity =>
        {
            entity.HasKey(e => e.IdOfficeAudiences);

            entity.ToTable("Offices_audiences");

            entity.Property(e => e.IdOfficeAudiences)
                .ValueGeneratedNever()
                .HasColumnName("ID_office_audiences");
            entity.Property(e => e.IdAudience).HasColumnName("ID_audience");
            entity.Property(e => e.IdOffice).HasColumnName("ID_office");

            entity.HasOne(d => d.IdAudienceNavigation).WithMany(p => p.OfficesAudiences)
                .HasForeignKey(d => d.IdAudience)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Offices_audiences_Audience");

            entity.HasOne(d => d.IdOfficeNavigation).WithMany(p => p.OfficesAudiences)
                .HasForeignKey(d => d.IdOffice)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Offices_audiences_Offices");
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.IdPost);

            entity.ToTable("Post");

            entity.Property(e => e.IdPost)
                .ValueGeneratedNever()
                .HasColumnName("ID_post");
            entity.Property(e => e.TitlePost)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("Title_post");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser);

            entity.ToTable("User");

            entity.Property(e => e.IdUser)
                .ValueGeneratedNever()
                .HasColumnName("ID_user");
            entity.Property(e => e.IdWorker).HasColumnName("ID_worker");
            entity.Property(e => e.Login)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.IdWorkerNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdWorker)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Worker");
        });

        modelBuilder.Entity<Worker>(entity =>
        {
            entity.HasKey(e => e.IdWorker);

            entity.ToTable("Worker");

            entity.Property(e => e.IdWorker)
                .ValueGeneratedNever()
                .HasColumnName("ID_worker");
            entity.Property(e => e.IdOffices).HasColumnName("ID_offices");
            entity.Property(e => e.IdPost).HasColumnName("ID_post");
            entity.Property(e => e.LastName)
                .HasMaxLength(150)
                .IsUnicode(false)
                .HasColumnName("Last_name");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Patronymic)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Salary).HasColumnType("money");
            entity.Property(e => e.YearBirthday).HasColumnName("Year_birthday");

            entity.HasOne(d => d.IdOfficesNavigation).WithMany(p => p.Workers)
                .HasForeignKey(d => d.IdOffices)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Worker_Offices");

            entity.HasOne(d => d.IdPostNavigation).WithMany(p => p.Workers)
                .HasForeignKey(d => d.IdPost)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Worker_Post");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
