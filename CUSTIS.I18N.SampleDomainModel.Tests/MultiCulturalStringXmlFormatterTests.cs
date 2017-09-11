using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using CUSTIS.I18N.DAL;
using NUnit.Framework;

namespace CUSTIS.I18N.SampleDomainModel.DAL.Tests
{
    [TestFixture]
    public class MultiCulturalStringXmlFormatterTests
    {
        private readonly MultiCulturalString _mcs =
            new MultiCulturalString(CultureInfo.GetCultureInfo("ru"), "Тест")
                .SetLocalizedString(CultureInfo.GetCultureInfo("ru-RU"), "Тест Россия")
                .SetLocalizedString(CultureInfo.GetCultureInfo("en"), "Test")
                .SetLocalizedString(CultureInfo.GetCultureInfo("en-IN"), "Test India");

        private readonly string _serializedMcs =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><MultiCulturalString p1:type=\"MultiCulturalString\" xmlns:p1=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns=\"http://custis.ru/i18n\"><ru>Тест</ru><ru-RU>Тест Россия</ru-RU><en>Test</en><en-IN>Test India</en-IN></MultiCulturalString>"
            ;


        [Test]
        public void TestSerialize()
        {
            using (Stream stream = new MemoryStream())
            {
                IFormatter formatter = new XmlFormatter(typeof(MultiCulturalString), "http://custis.ru/i18n");
                formatter.Serialize(stream, _mcs);
                stream.Flush();
                stream.Position = 0;
                using (TextReader reader = new StreamReader(stream))
                {
                    var result = reader.ReadToEnd();
                    Assert.AreEqual(_serializedMcs, result);
                }
            }
        }

        [Test]
        public void TestDeserialize()
        {
            using (Stream stream = new MemoryStream())
            using (TextWriter writer = new StreamWriter(stream))
            {
                writer.Write(_serializedMcs);
                writer.Flush();
                stream.Position = 0;

                IFormatter formatter = new XmlFormatter(typeof(MultiCulturalString), "http://custis.ru/i18n");
                var mcs = formatter.Deserialize(stream);
                Assert.AreEqual(_mcs, mcs);
            }
        }
    }
}