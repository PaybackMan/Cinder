using Cinder.Platform.RestClients;
using System.Threading.Tasks;
using Cinder.Core.Domain.Administrative;

namespace Cinder.Core.Interfaces
{
    public interface IOrganizationService
    {
        Task<ApiResponse<Organization>> SaveOrganization(Organization organization);
        Task<ApiResponse<Organization>> GetOrganization(int OrganizationId);
        Task<ApiResponse<bool>> DeleteOrganization(int OrganizationId);

    }
}
