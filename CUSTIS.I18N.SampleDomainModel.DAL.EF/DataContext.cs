//using System.ComponentModel.DataAnnotations.Schema;
//using System.Data.Entity;

//namespace CUSTIS.I18N.SampleDomainModel.DAL.EF
//{
//    public class DataContext : DbContext
//    {
//        //public DbSet<ProductEx> Products {
//        //    get { return Set<ProductEx>()/*.Covariant<Product>()*/; }
//        //}

//        protected override void OnModelCreating(DbModelBuilder modelBuilder)
//        {
//            //http://www.ladislavmrnka.com/2012/03/do-you-want-simple-type-mapping-or-conversions-in-ef/

//            // Соглашения по макс. длине строковых атрибутов
//            // С EF6 все непросто, жа устарел, переходим на EF Core 2.0
//            // upgrade NuGet to 3.6.0 https://www.nuget.org/downloads
//            //modelBuilder.Conventions.Add(new StringMaxLengthConvention());
//            //modelBuilder.Conventions.Add(new PropOraKeywordsConvention());

//            //Configuration.ProxyCreationEnabled = true;

//            //modelBuilder.ComplexType<MultiCulturalString>()
//            //    .Property();

//            //modelBuilder.Types<MultiCulturalString>()
//            //    .Configure(ctc => ctc.);

//            var productEntityTypeConfiguration = modelBuilder.Entity<ProductEx>()
//                .ToTable("t_product")
//                .HasKey(pr => pr.Id);

//            productEntityTypeConfiguration
//                .Property(pr => pr.Id)
//                .HasColumnName("id_product")
//                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
//            productEntityTypeConfiguration
//                .Property(pr => pr.Code)
//                .HasColumnName("code")
//                .IsRequired();
//            // product.Name

//            base.OnModelCreating(modelBuilder);
//        }
//    }
//}