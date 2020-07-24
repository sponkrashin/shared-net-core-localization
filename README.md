# Implementation of having all resources in a shared project

`SharedStringLocalizer` class implements finding of resources in a shared project instead of domestic project (where type T is located).

Current implementation implies that resource will be searched in a shared project (`Shared.Localization.Resources`) in `Resources` folder and in a subfolder wich relates to type namespace.

For example, if we deal with `HomeController` class which has `Shared.Localization.Main.Controllers` namespace, resources for it will be searched in `Resources\Main\Controllers\HomeController\HomeController.{locale}.resx` file where locale refers to current culture locale name (**en**, **de**, **ru**, etc).
