using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;

namespace CUSTIS.I18N.SampleDomainModel.DAL.Tests
{
    public class UICultureContext: IDisposable
    {
        private readonly CultureInfo previousUICulture;

        private UICultureContext(string uiCultureName)
        {
            previousUICulture = Thread.CurrentThread.CurrentUICulture;
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(uiCultureName, false);
        }

        public static IDisposable Create(string uiCultureName)
        {
            Contract.Ensures(uiCultureName != null);
            return new UICultureContext(uiCultureName);
        }

        void IDisposable.Dispose()
        {
            Thread.CurrentThread.CurrentUICulture = previousUICulture;
        }
    }
}