using VDS.Dependency;
using Microsoft.AspNetCore.Mvc;

namespace VDS.AspNetCore.Mvc.Controllers
{
    public class AppController : Controller, ITransientDependency
    {
    }
}