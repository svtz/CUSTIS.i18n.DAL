using CUSTIS.I18N.DAL.EF.Linq;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace CUSTIS.I18N.SampleDomainModel.DAL.EF
{
    /// <inheritdoc />
    public class DataContext : DbContext
    {
        /// <inheritdoc />
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        /// <summary> Products </summary>
        public DbSet<ProductProxy> Products => Set<ProductProxy>();

        /// <inheritdoc />
        /// <remarks>
        /// We could improve our code after fixing the next issues: 
        /// https://github.com/aspnet/EntityFrameworkCore/issues/242 
        /// https://github.com/aspnet/EntityFrameworkCore/issues/9213
        /// </remarks>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasMcsGetStringDbFunction(() => DbUserDefinedMethods.McsGetString(default(string)));
            modelBuilder.HasMcsGetStringDbFunction(() => DbUserDefinedMethods.McsGetString(default(string), default(bool)));
            modelBuilder.HasMcsGetStringDbFunction(() => DbUserDefinedMethods.McsGetString(default(string), default(CultureInfo)));
            modelBuilder.HasMcsGetStringDbFunction(() => DbUserDefinedMethods.McsGetString(default(string), default(IResourceFallbackProcess)));
            modelBuilder.HasMcsGetStringDbFunction(() => DbUserDefinedMethods.McsGetString(default(string), default(CultureInfo), default(bool)));
            modelBuilder.HasMcsGetStringDbFunction(() => DbUserDefinedMethods.McsGetString(default(string), default(IResourceFallbackProcess), default(CultureInfo)));
            modelBuilder.HasMcsGetStringDbFunction(() => DbUserDefinedMethods.McsGetString(default(string), default(IResourceFallbackProcess), default(CultureInfo), default(bool)));

            var productEntityTypeConfiguration = modelBuilder.Entity<ProductProxy>()
                .ToTable("t_product");

            productEntityTypeConfiguration
                .HasKey(pr => pr.Id);

            productEntityTypeConfiguration
                .Property(pr => pr.Id)
                .HasColumnName("id_product")
                .UseSqlServerIdentityColumn();

            productEntityTypeConfiguration
                .Property(pr => pr.Code)
                .HasColumnName("code")
                .IsRequired();

            productEntityTypeConfiguration
                .Ignore(pr => pr.Name);

            productEntityTypeConfiguration
                .Property(pr => pr.RawName)
                .HasColumnName("name")
                .UsePropertyAccessMode(PropertyAccessMode.Property);
            
            base.OnModelCreating(modelBuilder);
        }

    }
}