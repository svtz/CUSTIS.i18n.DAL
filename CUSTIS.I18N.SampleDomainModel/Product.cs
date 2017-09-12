namespace CUSTIS.I18N.SampleDomainModel
{
    /// <summary> Product </summary>
    public class Product
    {
        /// <summary> Id </summary>
        public virtual long Id { get; protected set; }

        /// <summary> Code </summary>
        public virtual string Code { get; set; }

        /// <summary> Name </summary>
        public virtual MultiCulturalString Name { get; set; }
    }
}