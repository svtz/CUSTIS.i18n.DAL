using CUSTIS.I18N.DAL.NH.UserTypes;
using FluentNHibernate.Mapping;

namespace CUSTIS.I18N.SampleDomainModel.DAL.NH
{
    public class ProductMapping : ClassMap<Product>
    {
        public ProductMapping()
        {
            Table("t_product");
            DynamicUpdate();
            Id(x => x.Id).GeneratedBy.Sequence("seq_product").Column("id_product");
            Map(x => x.Code).Column("code").Not.Nullable().Unique();
            Map(x => x.Name).Column("name").CustomType<MultiCulturalString2OneColumnType>();
        }
    }
}