using System.Linq;
using System.Transactions;
using CUSTIS.I18N.DAL.NH.Linq;
using CUSTIS.I18N.DAL.NH.SqlFunctions;
using CUSTIS.I18N.SampleDomainModel.DAL.NH;
using CUSTIS.I18N.SampleDomainModel.DAL.Tests;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate.Cfg;
using NHibernate.Linq;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;

namespace CUSTIS.I18N.SampleDomainModel.Tests.NH
{
    [TestFixture]
    public class MultiCulturalAttributeTests : MultiCulturalAttributeTestsBase<Product>
    {
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

    }
}