using System;
using System.Configuration;
using System.Linq;
using CUSTIS.I18N.DAL.EF.Linq;
using CUSTIS.I18N.SampleDomainModel.DAL.EF;
using CUSTIS.I18N.SampleDomainModel.DAL.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;

namespace CUSTIS.I18N.SampleDomainModel.Tests.EF
{
    [TestFixture]
    public class MultiCulturalAttributeTests : MultiCulturalAttributeTestsBase<ProductProxy>
    {
        #region Session-related

        public override ISessionFactory CreateSessionFactory()
        {
            return new SessionFactoryImpl();
        }

        private class SessionFactoryImpl : ISessionFactory
        {
            private readonly DbContextOptions<DataContext> _options;

            public SessionFactoryImpl()
            {
                var connectionString = ConfigurationManager.ConnectionStrings["TestEfMcs"].ConnectionString;
                _options = new DbContextOptionsBuilder<DataContext>()
                    .UseSqlServer(connectionString)
                    // throw on client evaluation, e.g. IQueryable<ProductProxy>.SingleOrDefault(pr => pr.Name.ToString() == @p1)
                    .ConfigureWarnings(wcb => wcb.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .Options;
            }

            public void CleanAllEntities()
            {
                using (var ctx = new DataContext(_options))
                using (var tx = ctx.Database.BeginTransaction())
                {
                    ctx.Database.ExecuteSqlCommand("DELETE FROM t_product");
                    tx.Commit();
                }
            }

            public ISession Create()
            {
                return new SessionImpl(new DataContext(_options));
            }

            public void Dispose()
            {
            }
        }

        private class SessionImpl : ISession
        {
            private readonly IDbContextTransaction _tx;
            private readonly DataContext _dbCtx;

            public SessionImpl(DataContext dbCtx)
            {
                _dbCtx = dbCtx;
                _tx = _dbCtx.Database.BeginTransaction();
            }

            public void Dispose()
            {
                _dbCtx.SaveChanges();
                _tx.Commit();
                _dbCtx.Dispose();
            }

            public void Add(object entity)
            {
                _dbCtx.Add(entity);
            }

            public IQueryable<T> AsQueryable<T>() 
                where T: class
            {
                return _dbCtx.Set<T>();
            }
        }

        #endregion

        #region Test Implementations

        protected override void TestFilterByMultiCulturalAttr_ToString_WhenEn_Impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<ProductProxy>()
                    .SingleOrDefault(p => p.RawName.McsGetString() == ProductNameEn);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenAnyCulture_Impl()
        {
            CreateTwoLangProduct();
            using (var session = SessionFactory.Create())
            {
                var name = ProductNameRu;
                var product = session.AsQueryable<ProductProxy>()
                    .SingleOrDefault(p => p.RawName.McsGetString(ru, false) == name);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_FallbackRu_WhenKzKz_Impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var specificFallbackProcess =
                    new ChainsResourceFallbackProcess(new[] { new[] { "kz-KZ", "kz", "ru" }, new[] { "*", "en" } });
                var product = session.AsQueryable<ProductProxy>()
                    .SingleOrDefault(p => p.RawName.McsGetString(specificFallbackProcess, ru) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_NullFallbackRu_WhenKzKz_Impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<ProductProxy>()
                    .SingleOrDefault(p => p.RawName.McsGetString((IResourceFallbackProcess)null, ru) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_NullFallback_WhenKzKz_Impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<ProductProxy>()
                    .SingleOrDefault(p => p.RawName.McsGetString((IResourceFallbackProcess)null) == ProductNameEn);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_False_WhenEn_Impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<ProductProxy>()
                    .SingleOrDefault(p => p.RawName.McsGetString(false) == ProductNameEn);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_Fallback_WhenKzKz_impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var specificFallbackProcess =
                    new ChainsResourceFallbackProcess(new[] { new[] { "kz-KZ", "kz", "ru" }, new[] { "*", "en" } });
                var product = session.AsQueryable<ProductProxy>()
                    .SingleOrDefault(p => p.RawName.McsGetString(specificFallbackProcess) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByOneWithManyProducts_Impl()
        {
            using (var session = SessionFactory.Create())
            {
                foreach (var num in Enumerable.Range(1, 3000))
                {
                    var product = new ProductProxy
                    {
                        Code = num.ToString(),
                        Name = new MultiCulturalString(ru, "RU_" + num)
                            .SetLocalizedString(en, "EN_" + num)
                    };
                    session.Add(product);
                }
            }


            using (var session = SessionFactory.Create())
            {
                Func<ProductProxy> actualProduct = () => session.AsQueryable<ProductProxy>()
                    .SingleOrDefault(p => p.RawName.McsGetString() == "RU_2017");

                Assert.That(actualProduct, Is.Not.Null.After(100));
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_WhenRu_Impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<ProductProxy>()
                    .SingleOrDefault(p => p.RawName.McsGetString() == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_Ru_WhenEnUs_Impl()
        {
            CreateTwoLangProduct();
            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<ProductProxy>()
                    .SingleOrDefault(p => p.RawName.McsGetString(ru) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenEnUse_Impl()
        {
            CreateTwoLangProduct();
            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<ProductProxy>()
                    .SingleOrDefault(p => p.RawName.McsGetString(ru, false) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        #endregion
    }
}