using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using NHibernate;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace CUSTIS.I18N.DAL.NH.UserTypes
{
    [Serializable]
    public sealed class MultiCulturalString2OneColumnType : IUserType, IParameterizedType
    {
        public MultiCulturalString2OneColumnType()
        {
        }

        bool IUserType.Equals(object x, object y)
        {
            return Equals(x, y);
        }

        int IUserType.GetHashCode(object x)
        {
            return x != null ? x.GetHashCode() : 0;
        }

        object IUserType.NullSafeGet(IDataReader rs, string[] names, object owner)
        {  
            var valueToGet = NHibernateUtil.String.NullSafeGet(rs, names[0]) as string;
            if (string.IsNullOrWhiteSpace(valueToGet))
                return null;

            return ParseStoredValue(valueToGet);
        }

        void IUserType.NullSafeSet(IDbCommand cmd, object value, int index)
        {
            var typedValue = value as MultiCulturalString;
            if (typedValue == null)
            {
                NHibernateUtil.String.NullSafeSet(cmd, null, index);
            }
            else
            {
                NHibernateUtil.String.NullSafeSet(cmd, ConvertToStoredValue(typedValue), index);
            }
        }

        object IUserType.DeepCopy(object value)
        {
            var typedValue = value as MultiCulturalString;
            if (typedValue != null)
            {
                return
                    new MultiCulturalString(typedValue.Cultures.ToDictionary(c => c, c => typedValue.GetString(c, false)));
            }
            else
                return null;
        }

        object IUserType.Replace(object original, object target, object owner)
        {
            return original;
        }

        object IUserType.Assemble(object cached, object owner)
        {
            var valueToGet = cached as string;
            if (string.IsNullOrWhiteSpace(valueToGet))
                return null;
            return ParseStoredValue(valueToGet);
        }

        object IUserType.Disassemble(object value)
        {
            var typedValue = value as MultiCulturalString;
            if (typedValue == null)
            {
                return null;
            }
            else
            {
                return ConvertToStoredValue(typedValue);
            }
        }

        SqlType[] IUserType.SqlTypes
        {
            get
            {
                return new SqlType[]
                {
                    new XmlSqlType()
                };
            }
        }

        Type IUserType.ReturnedType
        {
            get { return typeof(MultiCulturalString); }
        }

        bool IUserType.IsMutable
        {
            get { return false; }
        }

        void IParameterizedType.SetParameterValues(IDictionary<string, string> parameters)
        {
        }

        private MultiCulturalString ParseStoredValue(string storedValue)
        {
            using (Stream stream = new MemoryStream())
            using (TextWriter writer = new StreamWriter(stream))
            {
                writer.Write(storedValue);
                writer.Flush();
                stream.Position = 0;

                IFormatter formatter = new XmlFormatter(typeof(MultiCulturalString), "http://custis.ru/i18n");
                return (MultiCulturalString)formatter.Deserialize(stream);
            }
        }

        private string ConvertToStoredValue(MultiCulturalString typedValue)
        {
            using (Stream stream = new MemoryStream())
            {
                IFormatter formatter = new XmlFormatter(typeof(MultiCulturalString), "http://custis.ru/i18n");
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