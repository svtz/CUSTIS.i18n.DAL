using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace CUSTIS.I18N.SampleDomainModel.DAL.Tests
{
    public abstract class MultiCulturalAttributeTestsBase<TProduct>
        where TProduct : Product, new()
    {
        #region Session-related

        public interface ISessionFactory : IDisposable
        {
            void CleanAllEntities();

            ISession Create();
        }

        public interface ISession : IDisposable
        {
            void Add(object entity);

            IQueryable<T> AsQueryable<T>()
                where T : class;
        }

        public abstract ISessionFactory CreateSessionFactory();

        public ISessionFactory SessionFactory { get; private set; } 

        #endregion

        public readonly CultureInfo ru = CultureInfo.GetCultureInfo("ru");
        public readonly CultureInfo en = CultureInfo.GetCultureInfo("en");

        public const string ProductNameRu = "Шоколад Алина";
        public const string ProductNameEn = "Chocolate Alina";
        public const string ProductCode = "V0016887";

        #region Setup/Teardown

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            GlobalizationSettings.Current.MultiCulturalStringResourceFallbackProcess
                = new ChainsResourceFallbackProcess(new[] {new[] {"*", "en"}});
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [SetUp]
        public void SetUp()
        {
            // WARN: Creation/disposing of a session factory SHOULD be placed into OneTimeSetUp/OneTimeTearDown
            // but we need destroy query plan cache because of the defect NH-2500 https://nhibernate.jira.com/browse/NH-2500
            SessionFactory = CreateSessionFactory();

            SessionFactory.CleanAllEntities();
        }

        [TearDown]
        public void TearDown()
        {
            SessionFactory?.Dispose();
        }

        #endregion

        #region Test Methods

        [Test]
        public void TestStoreNullName()
        {
            using (var session = SessionFactory.Create())
            {
                var product = new TProduct
                {
                    Code = ProductCode,
                    Name = null
                };

                session.Add(product);
            }
            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<TProduct>().SingleOrDefault(p => p.Code == ProductCode);

                Assert.IsNotNull(product);
            }
        }

        [Test]
        public void TestStoreEmptyName()
        {
            using (var session = SessionFactory.Create())
            {
                var product = new TProduct
                {
                    Code = ProductCode,
                    Name = MultiCulturalString.Empty
                };

                session.Add(product);
            }
            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<TProduct>().SingleOrDefault(p => p.Code == ProductCode);

                Assert.IsNotNull(product);
            }
        }

        [Test]
        public void TestStoreSingleLang()
        {
            using (var session = SessionFactory.Create())
            {
                var product = new TProduct
                {
                    Code = ProductCode,
                    Name = new MultiCulturalString(ru, ProductNameRu)
                };

                session.Add(product);
            }
            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<TProduct>().SingleOrDefault(p => p.Code == ProductCode);

                Assert.IsNotNull(product);
                Assert.AreEqual(new MultiCulturalString(ru, ProductNameRu), product.Name);
            }
        }

        [Test]
        public void TestStoreMultipleLangs()
        {
            CreateTwoLangProduct();
            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<TProduct>().SingleOrDefault(p => p.Code == ProductCode);

                var multLangName = new MultiCulturalString(ru, ProductNameRu)
                    .SetLocalizedString(en, ProductNameEn);
                Assert.IsNotNull(product);
                Assert.AreEqual(multLangName, product.Name);
            }
        }

        [Test]
        [SetUICulture("ru-RU")]
        public void TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenRuRu()
        {
            TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenAnyCulture_Impl();
        }

        [Test]
        [SetUICulture("en")]
        public void TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenEn()
        {
            TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenAnyCulture_Impl();
        }

        [Test]
        [SetUICulture("kz-KZ")]
        public void TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenKzKz()
        {
            TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenAnyCulture_Impl();
        }

        [Test]
        [SetUICulture("en-US")]
        public void TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenEnUse()
        {
            TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenEnUse_Impl();
        }

        [Test]
        [SetUICulture("en-US")]
        public void TestFilterByMultiCulturalAttr_ToString_Ru_WhenEnUs()
        {
            TestFilterByMultiCulturalAttr_ToString_Ru_WhenEnUs_Impl();
        }

        [Test]
        [SetUICulture("ru-RU")]
        public void TestFilterByMultiCulturalAttr_ToString_WhenRuRu()
        {
            TestFilterByMultiCulturalAttr_ToString_WhenRu_Impl();
        }

        [Test]
        [SetUICulture("ru")]
        public void TestFilterByMultiCulturalAttr_ToString_WhenRu()
        {
            TestFilterByMultiCulturalAttr_ToString_WhenRu_Impl();
        }

        [Test]
        [SetUICulture("en")]
        public void TestFilterByMultiCulturalAttr_ToString_WhenEn()
        {
            TestFilterByMultiCulturalAttr_ToString_WhenEn_Impl();
        }

        [Test]
        [SetUICulture("kz-KZ")]
        public void TestFilterByMultiCulturalAttr_ToString_Fallback_WhenKzKz()
        {
            TestFilterByMultiCulturalAttr_ToString_Fallback_WhenKzKz_impl();
        }

        [Test]
        [SetUICulture("kz-KZ")]
        public void TestFilterByMultiCulturalAttr_ToString_NullFallback_WhenKzKz()
        {
            TestFilterByMultiCulturalAttr_ToString_NullFallback_WhenKzKz_Impl();
        }

        [Test]
        [SetUICulture("en")]
        public void TestFilterByMultiCulturalAttr_ToString_False_WhenEn()
        {
            TestFilterByMultiCulturalAttr_ToString_False_WhenEn_Impl();
        }

        [Test]
        [SetUICulture("kz-KZ")]
        public void TestFilterByMultiCulturalAttr_ToString_NullFallbackRu_WhenKzKz()
        {
            TestFilterByMultiCulturalAttr_ToString_NullFallbackRu_WhenKzKz_Impl();
        }

        [Test]
        [SetUICulture("kz-KZ")]
        public void TestFilterByMultiCulturalAttr_ToString_FallbackRu_WhenKzKz()
        {
            TestFilterByMultiCulturalAttr_ToString_FallbackRu_WhenKzKz_Impl();
        }

        [Test]
        [SetUICulture("ru-RU")]
        [Explicit("Long-running test")]
        public void TestFilterByOneWithManyProducts()
        {
            TestFilterByOneWithManyProducts_Impl();
        }

        #endregion

        #region Helper Methods

protected void CreateTwoLangProduct()
{
    using (var session = SessionFactory.Create())
    {
        var product = new TProduct
        {
            Code = ProductCode,
            Name = new MultiCulturalString(ru, ProductNameRu)
                .SetLocalizedString(en, ProductNameEn)
        };

        session.Add(product);
    }
}

        protected abstract void TestFilterByMultiCulturalAttr_ToString_WhenEn_Impl();

        protected abstract void TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenAnyCulture_Impl();

        protected abstract void TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenEnUse_Impl();

        protected abstract void TestFilterByMultiCulturalAttr_ToString_Ru_WhenEnUs_Impl();

        protected abstract void TestFilterByMultiCulturalAttr_ToString_WhenRu_Impl();

        protected abstract void TestFilterByMultiCulturalAttr_ToString_Fallback_WhenKzKz_impl();

        protected abstract void TestFilterByMultiCulturalAttr_ToString_NullFallback_WhenKzKz_Impl();

        protected abstract void TestFilterByMultiCulturalAttr_ToString_False_WhenEn_Impl();

        protected abstract void TestFilterByMultiCulturalAttr_ToString_NullFallbackRu_WhenKzKz_Impl();

        protected abstract void TestFilterByMultiCulturalAttr_ToString_FallbackRu_WhenKzKz_Impl();

        protected abstract void TestFilterByOneWithManyProducts_Impl();

        #endregion
    }
}