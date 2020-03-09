using System;
using System.Threading.Tasks;
using Cnf.CodeBase.Serialize;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Models
{
    public interface ISysAdminService
    {
        Task<Organization[]> GetOrganiztions();
        Task<Organization> GetOrganiztion(int orgId);
        Task<bool> SaveOrganization(Organization org);
        Task<bool> DeleteOrganization(int orgId);
        Task<Reference[]> GetReferences(ReferenceTypeEnum type);
        Task<Reference> GetReference(int id);
        Task SaveReference(Reference reference);
        Task DeleteReference(int id);
    }
    public class SysAdminService : ISysAdminService
    {
        const string ROUTE_ORG_GETALL = "api/Organization/Get";
        const string ROUTE_ORG_GETONE = "api/Organization"; //need add /{id}
        const string ROUTE_ORG_SAVE = "api/Organization/Save";
        const string ROUTE_ORG_DELETE="api/Organization/Delete";
        const string ROUTE_GET_SPECIALTIES = "api/Reference/GetSpecialties";
        const string ROUTE_GET_QUALIFICATIONS = "api/Reference/GetQualifications";
        const string ROUTE_GET_DUTIES = "api/Reference/GetDuties";
        const string ROUTE_GET_REFERENCE_ID = "api/Reference"; // need add /{id}
        const string ROUTE_SAVE_REFERENCE = "api/Reference/Save";
        const string ROUTE_DELETE_REFERENCE = "api/Reference/Delete"; // post json(id)

        readonly IApiConnector _apiConnector;
        public SysAdminService(IApiConnector apiConnector)
        {
            _apiConnector = apiConnector;
        }

        public async Task<bool> DeleteOrganization(int orgId)
        {
            return await _apiConnector.HttpPost<int, bool>(ROUTE_ORG_DELETE, orgId);
        }

        public async Task<Organization> GetOrganiztion(int orgId)
        {
            return await _apiConnector.HttpGet<Organization>(ROUTE_ORG_GETONE + $"/{orgId}");
        }

        public async Task<Organization[]> GetOrganiztions()
        {
            PagedQuery<Organization> result =
                await _apiConnector.HttpGet<PagedQuery<Organization>>(ROUTE_ORG_GETALL);
            return result.Records;
        }

        public async Task<bool> SaveOrganization(Organization org)
        {
            try
            {
                await _apiConnector.HttpPost<Organization, int>(ROUTE_ORG_SAVE, org);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Reference[]> GetReferences(ReferenceTypeEnum type)
        {
            var route = type==ReferenceTypeEnum.Specialty?
                ROUTE_GET_SPECIALTIES: type==ReferenceTypeEnum.Duty?
                ROUTE_GET_DUTIES: ROUTE_GET_QUALIFICATIONS;
            
            var result = await _apiConnector.HttpGet<PagedQuery<Reference>>(route);
            return result.Records;
        }

        public async Task<Reference> GetReference(int id)
        {
            return await _apiConnector.HttpGet<Reference>(ROUTE_GET_REFERENCE_ID + $"/{id}");
        }

        public async Task SaveReference(Reference reference)
        {
            await _apiConnector.HttpPost<Reference, int>(ROUTE_SAVE_REFERENCE, reference);
        }

        public async Task DeleteReference(int id)
        {
            await _apiConnector.HttpPost<int, bool>(ROUTE_DELETE_REFERENCE, id);
        }
    }
}