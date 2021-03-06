﻿namespace Shared.Localization.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    using Microsoft.Extensions.Localization;

    public class SharedStringLocalizer<T> : IStringLocalizer<T>
    {
        private const string AssemblyNamePrefix = "Shared.Localization";

        private readonly IStringLocalizer localLocalizer;
        private readonly IStringLocalizer sharedLocalizer;

        public SharedStringLocalizer(IStringLocalizerFactory factory)
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
        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return this;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var localString = this.localLocalizer[name];

                return localString.ResourceNotFound
                    ? this.sharedLocalizer[name]
                    : localString;
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var localString = this.localLocalizer[name];

                return localString.ResourceNotFound
                    ? this.sharedLocalizer[name, arguments]
                    : this.localLocalizer[name, arguments];
            }
        }
    }
}