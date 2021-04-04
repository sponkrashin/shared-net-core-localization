namespace Shared.Localization.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Localization;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    using Microsoft.Extensions.Localization;

    public class SharedViewLocalizer : IViewContextAware, IViewLocalizer
    {
        private const string AssemblyNamePrefix = "Shared.Localization";

        private readonly IHtmlLocalizerFactory factory;

        private IHtmlLocalizer localLocalizer;
        private IHtmlLocalizer sharedLocalizer;

        public SharedViewLocalizer(IHtmlLocalizerFactory factory)
        {
            this.factory = factory;
        }

        public void Contextualize(ViewContext viewContext)
        {
            var viewPath = viewContext.ExecutingFilePath;

            if (string.IsNullOrEmpty(viewPath))
            {
                viewPath = viewContext.View.Path;
            }

            if (!(viewContext.ActionDescriptor is ControllerActionDescriptor actionDescriptor))
            {
                throw new NotImplementedException("Razor pages are not supported yet.");
            }

            var viewName = this.GetViewName(viewPath);
            var viewResourceName = this.GetViewResourceName(viewPath);
            var viewAssemblyName = actionDescriptor.ControllerTypeInfo.Assembly.GetName().Name;

            var localResourceName = $"{viewAssemblyName}.{viewResourceName}";
            this.localLocalizer = this.sharedLocalizer = this.factory.Create(localResourceName, viewAssemblyName);

            if (!viewAssemblyName.StartsWith(AssemblyNamePrefix))
            {
                return;
            }
            
            var viewAssemblyWithoutPrefix = viewAssemblyName.Remove(0, AssemblyNamePrefix.Length + 1);

            var sharedResourceName = $"{viewAssemblyWithoutPrefix}.{viewResourceName}.{viewName}";
            var sharedAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            this.sharedLocalizer = this.factory.Create(sharedResourceName, sharedAssemblyName);
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

        private string GetViewName(string viewPath)
        {
            var extension = Path.GetExtension(viewPath);
            var lastDividerIndex = viewPath.LastIndexOfAny(new[] { '/', '\\' });
            var viewNameLength = viewPath.Length - extension.Length - lastDividerIndex - 1;

            var viewName = viewPath.Substring(lastDividerIndex + 1, viewNameLength);
            return viewName;
        }

        private string GetViewResourceName(string path)
        {
            var extension = Path.GetExtension(path);
            var startIndex = path[0] == '/' || path[0] == '\\' ? 1 : 0;
            var length = path.Length - startIndex - extension.Length;

            var result = path
               .Substring(startIndex, length)
               .Replace('/', '.')
               .Replace('\\', '.');

            return result;
        }
    }
}
