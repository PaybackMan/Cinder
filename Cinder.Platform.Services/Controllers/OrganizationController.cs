using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.OData;
using Microsoft.WindowsAzure.Mobile.Service;
using Cinder.Platform.Services.DataObjects;
using Cinder.Platform.Services.Models;
using Cinder.Core.Domain.Administrative;

namespace Cinder.Platform.Services.Controllers
{
    public class OrganizationController : TableController<OrganizationDto>
    {
        protected override void Initialize(HttpControllerContext controllerContext)
        {
            base.Initialize(controllerContext);
            MobileServiceContext context = new MobileServiceContext();
            DomainManager = new EntityDomainManager<OrganizationDto>(context, Request, Services);
        }

        // GET tables/TodoItem
        public IQueryable<OrganizationDto> GetAllTodoItems()
        {
            return Query();
        }

        // GET tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public SingleResult<OrganizationDto> GetTodoItem(string id)
        {
            return Lookup(id);
        }

        // PATCH tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task<OrganizationDto> PatchTodoItem(string id, Delta<OrganizationDto> patch)
        {
            return UpdateAsync(id, patch);
        }

        // POST tables/TodoItem
        public async Task<IHttpActionResult> PostOrganization(OrganizationDto item)
        {
            OrganizationDto current = await InsertAsync(item);
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }

        // DELETE tables/TodoItem/48D68C86-6EA6-4C25-AA33-223FC9A27959
        public Task DeleteOrganization(string id)
        {
            return DeleteAsync(id);
        }
    }
}