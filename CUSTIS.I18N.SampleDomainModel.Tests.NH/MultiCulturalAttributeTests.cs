using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Transactions;
using CUSTIS.I18N.DAL.NH.Linq;
using CUSTIS.I18N.DAL.NH.SqlFunctions;
using CUSTIS.I18N.SampleDomainModel.DAL.NH;
using CUSTIS.I18N.SampleDomainModel.DAL.Tests;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using NHibernate.Cfg;
using NHibernate.Linq;

namespace CUSTIS.I18N.SampleDomainModel.Tests.NH
{
    [TestFixture]
    public class MultiCulturalAttributeTests : MultiCulturalAttributeTestsBase<Product>
    {
        #region Session-related

        public override ISessionFactory CreateSessionFactory()
        {
            return new SessionFactoryImpl();
        }

        private class SessionFactoryImpl : ISessionFactory
        {
            private readonly NHibernate.ISessionFactory _sessionFactory;

            public SessionFactoryImpl()
            {
                var dbConfig = OracleManagedDataClientConfiguration.Oracle10
                    .ConnectionString(csb => csb.FromConnectionStringWithKey("TestNhMcs"))
#if DEBUG
                    .ShowSql()
#endif
                    .FormatSql();

                _sessionFactory = Fluently.Configure()
                    .Mappings(m =>
                    {
                        m.FluentMappings.AddFromAssemblyOf<ProductMapping>();
                    })
                    .Database(dbConfig)
                    .ExposeConfiguration(cfg =>
                    {
                        cfg.SqlFunctions.Add(MultiCulturalStringGet.FunctionName, new MultiCulturalStringGet());
                        cfg.LinqToHqlGeneratorsRegistry<McsLinqToHqlGeneratorsRegistry>();
                        cfg.SetProperty("hbm2ddl.keywords", "auto-quote");
                        cfg.SetProperty("adonet.batch_size", "50");
                        new SchemaUpdate(cfg).Execute(true, true);
                    })
                    .BuildSessionFactory();
            }

            public void CleanAllEntities()
            {
                using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
                using (var nhSession = _sessionFactory.OpenSession())
                {
                    nhSession.CreateQuery("delete CUSTIS.I18N.SampleDomainModel.Product").ExecuteUpdate();
                    tx.Complete();
                }
            }

            public ISession Create()
            {
                return new SessionImpl(new TransactionScope(), _sessionFactory.OpenSession());
            }

            public void Dispose()
            {
                _sessionFactory.Dispose();
            }
        }

        private class SessionImpl : ISession
        {
            private readonly TransactionScope _transactionScope;
            private readonly NHibernate.ISession _nhSession;

            public SessionImpl(TransactionScope transactionScope, NHibernate.ISession nhSession)
            {
                _transactionScope = transactionScope;
                _nhSession = nhSession;
            }

            public void Dispose()
            {
                _nhSession.Dispose();
                _transactionScope.Complete();
                _transactionScope.Dispose();
            }

            public void Add(object entity)
            {
                _nhSession.Save(entity);
            }

            public IQueryable<T> AsQueryable<T>()
                where T : class
            {
                return _nhSession.Query<T>();
            }
        }

        #endregion

        #region Test Implementations

        protected override void TestFilterByMultiCulturalAttr_ToString_WhenEn_Impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString() == ProductNameEn);

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
                var product = session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString(ru, false) == name);

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
                var product = session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString(specificFallbackProcess, ru) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_NullFallbackRu_WhenKzKz_Impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString((IResourceFallbackProcess)null, ru) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_NullFallback_WhenKzKz_Impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString((IResourceFallbackProcess)null) == ProductNameEn);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_False_WhenEn_Impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString(false) == ProductNameEn);

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
                var product = session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString(specificFallbackProcess) == ProductNameRu);

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
                    var product = new Product
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
                Func<Product> actualProduct = () => session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString() == "RU_2017");

                Assert.That(actualProduct, Is.Not.Null.After(100));
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_WhenRu_Impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString() == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_Ru_WhenEnUs_Impl()
        {
            CreateTwoLangProduct();
            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString(ru) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        protected override void TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenEnUse_Impl()
        {
            CreateTwoLangProduct();
            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString(ru, false) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        #endregion


        #region NH-specific tests

        /// <summary> Tests NH-2500 defect </summary>
        [Test]
        [Ignore("NH-2500")]
        public void TestNh2500()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString(CultureInfo.GetCultureInfo("zh-CHS")) == ProductNameEn);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<Product>()
                    .SingleOrDefault(p => p.Name.ToString(CultureInfo.GetCultureInfo("ru-RU")) == ProductNameEn);

                // The next line throws AssertionException
                Assert.IsNull(product);
            }
        }

        #endregion
    }
}