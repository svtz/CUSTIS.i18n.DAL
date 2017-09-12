using System;
using System.Globalization;
using System.Linq;
using NUnit.Framework;

namespace CUSTIS.I18N.SampleDomainModel.DAL.Tests
{
    public abstract class MultiCulturalAttributeTestsBase<TProduct>
        where TProduct : Product, new()
    {
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

        public readonly CultureInfo ru = CultureInfo.GetCultureInfo("ru");
        public readonly CultureInfo en = CultureInfo.GetCultureInfo("en");

        public abstract ISessionFactory CreateSessionFactory();

        public ISessionFactory SessionFactory { get; private set; }

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
            // but we need destroy query plan cache because of the defect NH-2500
            SessionFactory = CreateSessionFactory();

            SessionFactory.CleanAllEntities();
        }

        [TearDown]
        public void TearDown()
        {
            SessionFactory?.Dispose();
        }

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

        private void TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenAnyCulture_Impl()
        {
            CreateTwoLangProduct();
            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<TProduct>()
                    .SingleOrDefault(p => p.Name.ToString(ru, false) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        [Test]
        [SetUICulture("en-US")]
        public void TestFilterByMultiCulturalAttr_ToString_RuFalse_WhenEnUse_Impl()
        {
            CreateTwoLangProduct();
            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<TProduct>()
                    .SingleOrDefault(p => p.Name.ToString(ru, false) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        [Test]
        [SetUICulture("en-US")]
        public void TestFilterByMultiCulturalAttr_ToString_Ru_WhenEnUs()
        {
            CreateTwoLangProduct();
            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<TProduct>()
                    .SingleOrDefault(p => p.Name.ToString(ru) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
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

        private void TestFilterByMultiCulturalAttr_ToString_WhenRu_Impl()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<TProduct>()
                    .SingleOrDefault(p => p.Name.ToString() == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        [Test]
        [SetUICulture("en")]
        public void TestFilterByMultiCulturalAttr_ToString_WhenEn()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<TProduct>()
                    .SingleOrDefault(p => p.Name.ToString() == ProductNameEn);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        [Test]
        [SetUICulture("kz-KZ")]
        public void TestFilterByMultiCulturalAttr_ToString_Fallback_WhenKzKz()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var specificFallbackProcess =
                    new ChainsResourceFallbackProcess(new[] {new[] {"kz-KZ", "kz", "ru"}, new[] {"*", "en"}});
                var product = session.AsQueryable<TProduct>()
                    .SingleOrDefault(p => p.Name.ToString(specificFallbackProcess) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        [Test]
        [SetUICulture("kz-KZ")]
        public void TestFilterByMultiCulturalAttr_ToString_NullFallback_WhenKzKz()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<TProduct>()
                    .SingleOrDefault(p => p.Name.ToString((IResourceFallbackProcess) null) == ProductNameEn);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        [Test]
        [SetUICulture("kz-KZ")]
        public void TestFilterByMultiCulturalAttr_ToString_NullFallbackRu_WhenKzKz()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var product = session.AsQueryable<TProduct>()
                    .SingleOrDefault(p => p.Name.ToString((IResourceFallbackProcess) null, ru) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        [Test]
        [SetUICulture("kz-KZ")]
        public void TestFilterByMultiCulturalAttr_ToString_FallbackRu_WhenKzKz()
        {
            CreateTwoLangProduct();

            using (var session = SessionFactory.Create())
            {
                var specificFallbackProcess =
                    new ChainsResourceFallbackProcess(new[] {new[] {"kz-KZ", "kz", "ru"}, new[] {"*", "en"}});
                var product = session.AsQueryable<TProduct>()
                    .SingleOrDefault(p => p.Name.ToString(specificFallbackProcess, ru) == ProductNameRu);

                Assert.IsNotNull(product);
                Assert.AreEqual(ProductCode, product.Code);
            }
        }

        [Test]
        [SetUICulture("ru-RU")]
        [Explicit("Long-running test")]
        public void TestFilterByOneWithManyProducts()
        {
            using (var session = SessionFactory.Create())
            {
                foreach (var num in Enumerable.Range(1, 3000))
                {
                    var product = new TProduct
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
                Func<TProduct> actualProduct = () => session.AsQueryable<TProduct>()
                    .SingleOrDefault(p => p.Name.ToString() == "RU_2017");

                Assert.That(actualProduct, Is.Not.Null.After(100));
            }
        }

        public const string ProductNameRu = "Шоколад Алина";
        public const string ProductNameEn = "Chocolate Alina";
        public const string ProductCode = "V0016887";

        private void CreateTwoLangProduct()
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
    }
}