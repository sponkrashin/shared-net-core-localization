namespace Shared.Localization.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using Microsoft.AspNetCore.Mvc.Localization;
    using Microsoft.Extensions.Localization;

    public class SharedHtmlLocalizer<T> : IHtmlLocalizer<T>
    {
        private const string AssemblyNamePrefix = "Shared.Localization";

        private readonly IHtmlLocalizer localLocalizer;
        private readonly IHtmlLocalizer sharedLocalizer;

        public SharedHtmlLocalizer(IHtmlLocalizerFactory factory)
        {
            this.localLocalizer = this.sharedLocalizer = factory.Create(typeof(T));

            var resourceType = typeof(T);

            if (resourceType.Namespace?.StartsWith(AssemblyNamePrefix) != true)
            {
                return;
            }

            var resourceNamespaceNameWithoutPrefix = resourceType.Namespace?.Remove(0, AssemblyNamePrefix.Length + 1);
            var resourceTypeName = resourceType.Name;

            var resourceName = $"{resourceNamespaceNameWithoutPrefix}.{resourceTypeName}.{resourceTypeName}";
            var sharedAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            this.sharedLocalizer = factory.Create(resourceName, sharedAssemblyName);
        }

        public LocalizedString GetString(string name)
        {
            var localString = this.localLocalizer.GetString(name);

            return localString.ResourceNotFound
                ? this.sharedLocalizer.GetString(name)
                : localString;
        }

        public LocalizedString GetString(string name, params object[] arguments)
        {
            var localString = this.localLocalizer.GetString(name, arguments);

            return localString.ResourceNotFound
                ? this.sharedLocalizer.GetString(name, arguments)
                : localString;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            try
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                // It will throw an exception if there is no resource for current culture
                this.localLocalizer.GetAllStrings(includeParentCultures).Any();

                return this.localLocalizer.GetAllStrings(includeParentCultures);
            }
            catch
            {
                return this.sharedLocalizer.GetAllStrings(includeParentCultures);
            }
        }

        [Obsolete]
        public IHtmlLocalizer WithCulture(CultureInfo culture)
        {
            return this;
        }

        public LocalizedHtmlString this[string name]
        {
            get
            {
                var localString = this.localLocalizer[name];

                return localString.IsResourceNotFound
                    ? this.sharedLocalizer[name]
                    : localString;
            }
        }

        public LocalizedHtmlString this[string name, params object[] arguments]
        {
            get
            {
                var localString = this.localLocalizer[name, arguments];

                return localString.IsResourceNotFound
                    ? this.sharedLocalizer[name, arguments]
                    : localString;
            }
        }
    }
}
