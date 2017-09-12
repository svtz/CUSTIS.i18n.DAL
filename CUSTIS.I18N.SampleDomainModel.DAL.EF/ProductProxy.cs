using System.IO;
using System.Runtime.Serialization;
using System.Text;
using CUSTIS.I18N.DAL;

namespace CUSTIS.I18N.SampleDomainModel.DAL.EF
{
    /// <summary> Product proxy type for (de)serializing custom attrs </summary>
    public class ProductProxy : Product
    {
        /// <inheritdoc/>
        public override MultiCulturalString Name
        {
            get => base.Name;
            set
            {
                _serializedName = ConvertToStoredValue(value);
                base.Name = value;
            }
        }

        /// <summary> Serialized <see cref="Name"/> </summary>
        protected internal virtual string SerializedName
        {
            get => _serializedName;
            set
            {
                base.Name = ParseStoredValue(value);
                _serializedName = value;
            }
        }
        private string _serializedName;

        private MultiCulturalString ParseStoredValue(string storedValue)
        {
            if (storedValue == null)
                return null;

            using (Stream stream = new MemoryStream())
            using (TextWriter writer = new StreamWriter(stream))
            {
                writer.Write(storedValue);
                writer.Flush();
                stream.Position = 0;

                IFormatter formatter = new XmlFormatter(typeof(MultiCulturalString), "http://custis.ru/i18n", Encoding.Unicode);
                return (MultiCulturalString)formatter.Deserialize(stream);
            }
        }

        private string ConvertToStoredValue(MultiCulturalString typedValue)
        {
            if (typedValue == null)
                return null;

            using (Stream stream = new MemoryStream())
            {
                IFormatter formatter = new XmlFormatter(typeof(MultiCulturalString), "http://custis.ru/i18n", Encoding.Unicode);
                formatter.Serialize(stream, typedValue);
                stream.Flush();
                stream.Position = 0;
                using (TextReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
