using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApi.Controllers
{
    [AttributeUsage(AttributeTargets.Class)]
    sealed class RouteAttribute : Attribute
    {
        readonly string url;

        public RouteAttribute(string url)
        {
            this.url = url;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    abstract class HttpMethodAttribute : Attribute
    { }

    sealed class HttpGetAttribute : HttpMethodAttribute
    {
        readonly string url;

        public HttpGetAttribute(string url = null)
        {
            this.url = url;
        }
    }

    sealed class HttpPostAttribute : HttpMethodAttribute
    {
        readonly string url;

        public HttpPostAttribute(string url)
        {
            this.url = url;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    sealed class FromQueryAttribute : Attribute
    {
        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    sealed class FromServicesAttribute : Attribute
    { }

    [AttributeUsage(AttributeTargets.Parameter)]
    sealed class FromBodyAttribute : Attribute
    { }
}
