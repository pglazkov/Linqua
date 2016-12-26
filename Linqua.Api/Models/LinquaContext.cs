using Microsoft.EntityFrameworkCore;

namespace Linqua.Api.Models
{
    public partial class LinquaContext : DbContext
    {
        public virtual DbSet<Entry> Entries { get; set; }
        public virtual DbSet<User> Users { get; set; }

        public LinquaContext(DbContextOptions<LinquaContext> options) : base(options)
        {
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
        //    //optionsBuilder.UseSqlServer(@"Server=tcp:linqua.database.windows.net,1433;Data Source=linqua.database.windows.net;Initial Catalog=linqua;Persist Security Info=False;User ID=LinquaUser;Password=master5843.Pleasant.row;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30");
        //    //optionsBuilder.UseSqlServer(@"Server=tcp:linqua.database.windows.net,1433;Data Source=linqua.database.windows.net;Initial Catalog=linqua;Persist Security Info=False;User ID=pg;Password=Uk@prjd38;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30");
        //}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entry>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt)
                    .HasName("IX_CreatedAt");

                entity.HasIndex(e => e.Text)
                    .HasName("IX_Text");

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ClientCreatedAt).HasDefaultValueSql("sysutcdatetime()");

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("sysutcdatetime()");

                entity.Property(e => e.DefinitionLanguageCode).HasDefaultValueSql("'en'");

                entity.Property(e => e.IsLearnt).HasDefaultValueSql("0");

                entity.Property(e => e.Text).HasMaxLength(256);

                entity.Property(e => e.TextLanguageCode);

                entity.Property(e => e.TranslationState).HasDefaultValueSql(((int)TranslationState.Unknown).ToString());

                entity.Property(e => e.Version)
                    .IsRequired()
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate();

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK_linqua.Entries_linqua.Users_UserId");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.Auth0Id)
                    .HasName("IX_Auth0Id")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedOnAdd();

                entity.Property(e => e.Email).HasMaxLength(512);

                entity.Property(e => e.Auth0Id)
                    .IsRequired()
                    .HasMaxLength(256);
            });
        }
    }
}