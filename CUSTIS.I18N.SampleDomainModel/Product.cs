namespace CUSTIS.I18N.SampleDomainModel
{
    public class Product
    {
        public virtual long Id { get; protected set; }

        public virtual string Code { get; set; }

        public virtual MultiCulturalString Name { get; set; }
    }
}