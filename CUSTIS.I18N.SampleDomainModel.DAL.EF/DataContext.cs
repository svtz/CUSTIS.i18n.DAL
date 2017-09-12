using Microsoft.EntityFrameworkCore;

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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //http://www.ladislavmrnka.com/2012/03/do-you-want-simple-type-mapping-or-conversions-in-ef/
            //https://github.com/aspnet/EntityFrameworkCore/issues/242

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
                .Property(pr => pr.SerializedName)
                .HasColumnName("name")
                .UsePropertyAccessMode(PropertyAccessMode.Property);
            
            base.OnModelCreating(modelBuilder);
        }
    }
}