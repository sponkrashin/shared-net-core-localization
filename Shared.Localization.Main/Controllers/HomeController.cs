namespace Shared.Localization.Main.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Localization;

    public class HomeController : Controller
    {
        private readonly IStringLocalizer<HomeController> localizer;

        public HomeController(IStringLocalizer<HomeController> localizer)
        {
            this.localizer = localizer;
        }

        public IActionResult Index()
        {
            var allResources = this.localizer.GetAllStrings();

            this.ViewBag.Test = this.localizer["Test"];
            return this.View();
        }
    }
}
