using Cinder.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cinder.Core.Domain.Administrative;
using Cinder.Platform.RestClients;

namespace Cinder.Clients.Bindings.Organizations
{
    public class OganizationService : IOrganizationService
    {
        IRestClient _client;

        public OganizationService(IRestClient client)
        {
            _client = client;
        }

        public Task<ApiResponse<bool>> DeleteOrganization(int OrganizationId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<Organization>> FindOrganizationDecendant(Organization organization)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<Organization>> GetOrganization(int OrganizationId)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse<Organization>> SaveOrganization(Organization organization)
        {
            throw new NotImplementedException();
        }
    }
}
