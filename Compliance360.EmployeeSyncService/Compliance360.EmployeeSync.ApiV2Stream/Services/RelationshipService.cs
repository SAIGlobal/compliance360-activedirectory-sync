using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using Compliance360.EmployeeSync.Library.Data;
using Compliance360.EmployeeSync.ApiV2Stream.Data;

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public class RelationshipService : IRelationshipService
    {
        private ILogger Logger { get; }
        private IHttpDataService Http { get; }

        public RelationshipService(ILogger logger, IHttpDataService http)
        {
            Logger = logger;
            Http = http;
        }

        public void SetBaseAddress(string baseAddress)
        {
            Http.Initialize(baseAddress);
        }
        
        public async Task<Entity> CreateRelationshipTypeAsync(string relationshipType, string token)
        {
            Logger.Debug("Creating Relationship Type [{0}]", relationshipType);

            var createRelationshipTypeUri = $"/API/2.0/Data/Lookup/EmployeeRelationship/Type?token={token}";

            var typeToCreate = new Dictionary<string, object>
            {
                { "Text", relationshipType }
            };

            var result = await Http.PostAsync<CreateResponse>(createRelationshipTypeUri, typeToCreate);

            return result != null ? new Entity {Id = result.Id} : null;
        }

        public Entity CreateRelationshipType(string relationshipType, string token)
        {
            return CreateRelationshipTypeAsync(relationshipType, token).Result;
        }

        public async Task<Entity> CreateRelationshipAsync(Employee employee, Entity destEmployee, Entity relType, string token)
        {
            Logger.Debug("Creating Relationship for Employee [{0}], related to [{1}]", employee.Id, destEmployee.Id);

            var createRelationshipUri = $"/API/2.0/Data/EmployeeManagement/EmployeeRelationship/Default?token={token}";

            var relationshipToCreate = new Dictionary<string, object>
            {
                { "Employee", new Entity { Id = destEmployee.Id } },
                { "Type", relType }
            };
            var newRelationship = await Http.PostAsync<CreateResponse>(createRelationshipUri, relationshipToCreate);

            // add the new relationship to the employee's list of
            // related employees
            var updateEmployeeUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default/{employee.InstanceId}?token={token}";
            var updateEmployeeRequest = new EmployeeRelationshipRequest
            {
                RelatedEmployees = new List<EntityReference>
                {
                    {new EntityReference {Action = "Add", Id = newRelationship.Id}}
                }
            };
            var newRelationshipXref = await Http.PostAsync<CreateResponse>(updateEmployeeUri, updateEmployeeRequest);

            return new Entity {Id = newRelationship.Id};
        }

        public Entity CreateRelationship(Employee employee, Entity destEmployee, Entity relType, string token)
        {
            return CreateRelationshipAsync(employee, destEmployee, relType, token).Result;
        }
        
        public async Task<List<Entity>> GetEmployeeRelationshipsAsync(Employee employee, string token)
        {
            Logger.Debug("Getting Relationships for Employee [{0}]", employee.Id);

            var getEmployeeRelationshipsUri = $"/API/2.0/Data/EmployeeManagement/Employee/Default?select=RelatedEmployees&where=InstanceId='{employee.Id}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<GetEmployeeRelationshipResponse>>(getEmployeeRelationshipsUri);

            var ids = resp.Data?.FirstOrDefault()?.RelatedEmployees?.Select(re => new Entity { Id = re.Id});

            return ids?.ToList();
        }

        public List<Entity> GetEmployeeRelationships(Employee employee, string token)
        {
            return GetEmployeeRelationshipsAsync(employee, token).Result;
        }
        
        public async Task<Relationship> GetEmployeeRelationshipAsync(Entity relationship, string token)
        {
            Logger.Debug("Getting details for Relationship [{0}]", relationship.Id);

            var getEmployeeRelationshipUri = $"/API/2.0/Data/EmployeeManagement/EmployeeRelationship/Default?select=Employee,Type&where=InstanceId='{relationship.Id}'&token={token}";

            var resp = await Http.GetAsync<GetResponse<Relationship>>(getEmployeeRelationshipUri);

            return resp.Data?.FirstOrDefault();
        }

        public Relationship GetEmployeeRelationship(Entity relationship, string token)
        {
            return GetEmployeeRelationshipAsync(relationship, token).Result;
        }
        
        public async Task<Entity> GetRelationshipTypeByNameAsync(string name, string token)
        {
            Logger.Debug("Getting Relationship Type [{0}]", name);

            var jobTitlesUri = $"/API/2.0/Data/Lookup/EmployeeRelationship/Type?select=Text&take=1&where=Text='{Uri.EscapeDataString(name)}'&token={token}";
            var resp = await Http.GetAsync<GetResponse<Entity>>(jobTitlesUri);

            var id = resp.Data?.FirstOrDefault()?.Id;
            return id != null ? new Entity {Id = id} : null;
        }

        public Entity GetRelationshipTypeByName(string name, string token)
        {
            return GetRelationshipTypeByNameAsync(name, token).Result;
        }

        public async Task<bool> UpdateRelationshipAsync(
            Relationship relationship,
            Entity destEmployee,
            string token)
        {
            Logger.Debug("Updating Employee Relationship [{0}]", relationship.Id);

            var updateRelationshipUri = $"/API/2.0/Data/EmployeeManagement/EmployeeRelationship/Default/{relationship.InstanceId}?token={token}";

            var relationshipToUpdate = new Dictionary<string, object>
            {
                { "Employee", new Entity { Id = destEmployee.Id } }
            };

            await Http.PostAsync<CreateResponse>(updateRelationshipUri, relationshipToUpdate);

            return true;
        }

        public void UpdateRelationship(
            Relationship relationship,
            Entity destEmployee,
            string token)
        {
            UpdateRelationshipAsync(relationship, destEmployee, token).GetAwaiter().GetResult();
        }
    }
}
