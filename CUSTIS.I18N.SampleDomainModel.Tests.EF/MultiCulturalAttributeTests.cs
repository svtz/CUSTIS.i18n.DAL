using System.Configuration;
using System.Linq;
using System.Transactions;
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
                    // throw on client evaluation, e.g. IQueryable<Product>.SingleOrDefault(pr => pr.Name.ToString() == @p1)
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
    }
}