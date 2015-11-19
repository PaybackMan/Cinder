using System.Web.Http;
using System.Web.Routing;
using AutoMapper.Configuration;
using AutoMapper;
using Cinder.Core.Domain.Administrative;

namespace Cinder.Platform.Services
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            WebApiConfig.Register();

        }


        private void MapTypes()
        {
            Mapper.CreateMap<Organization, OrganizationDto>();
        }

    }
}