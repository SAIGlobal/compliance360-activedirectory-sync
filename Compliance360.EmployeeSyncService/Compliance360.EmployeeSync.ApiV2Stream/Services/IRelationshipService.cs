using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream.Data;
using Newtonsoft.Json.Linq;

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public interface IRelationshipService
    {
        /// <summary>
        /// Sets the base uri of the Http client.
        /// </summary>
        /// <param name="baseAddress">The bsae address of the API.</param>
        void SetBaseAddress(string baseAddress);

        /// <summary>
        /// Creates a new relationship type value like "Manager".
        /// </summary>
        /// <param name="relationshipType">The type of relationship.</param>
        /// <param name="token">The current auth token.</param>
        /// <returns>Id of the relationship type</returns>
        Task<Entity> CreateRelationshipTypeAsync(string relationshipType, string token);

        /// <summary>
        /// Creates a new relationship type value like "Manager".
        /// </summary>
        /// <param name="relationshipType">The type of relationship.</param>
        /// <param name="token">The current auth token.</param>
        /// <returns>Id of the relationship type</returns>
        Entity CreateRelationshipType(string relationshipType, string token);

        /// <summary>
        /// Creates a new relationship
        /// </summary>
        /// <param name="employee">Employee that owns the relationship.</param>
        /// <param name="destEmployee">Entity reference to the destination employee.</param>
        /// <param name="relationshipType">Type of relationship</param>
        /// <param name="token">Auth token</param>
        /// <returns>Entity reference to the new relationship.</returns>
        Task<Entity> CreateRelationshipAsync(Employee employee, Entity destEmployee, Entity relationshipType, string token);

        /// <summary>
        /// Creates a new relationship
        /// </summary>
        /// <param name="employee">Employee that owns the relationship.</param>
        /// <param name="destEmployee">Entity reference to the destination employee.</param>
        /// <param name="relationshipType">Type of relationship</param>
        /// <param name="token">Auth token</param>
        /// <returns>Entity reference to the new relationship.</returns>
        Entity CreateRelationship(Employee employee, Entity destEmployee, Entity relationshipType, string token);

        /// <summary>
        /// Gets a Relationship Type by name
        /// </summary>
        /// <param name="name">Name of the relationship.</param>
        /// <param name="token">Auth token</param>
        /// <returns>Entity reference to the new relationship.</returns>
        Task<Entity> GetRelationshipTypeByNameAsync(string name, string token);

        /// <summary>
        /// Gets a Relationship Type by name
        /// </summary>
        /// <param name="name">Name of the relationship.</param>
        /// <param name="token">Auth token</param>
        /// <returns>Entity reference to the new relationship.</returns>
        Entity GetRelationshipTypeByName(string name, string token);

        /// <summary>
        /// Gets the a list of employee relationships that belong to the specified user.
        /// </summary>
        /// <param name="employee">The current employee</param>
        /// <param name="token">Auth token</param>
        /// <returns>List of string identifiers for the owned relationships</returns>
        Task<List<Entity>> GetEmployeeRelationshipsAsync(Employee employee, string token);

        /// <summary>
        /// Gets the a list of employee relationships that belong to the specified user.
        /// </summary>
        /// <param name="employee">The current employee</param>
        /// <param name="token">Auth token</param>
        /// <returns>List of string identifiers for the owned relationships</returns>
        List<Entity> GetEmployeeRelationships(Employee employee, string token);

        /// <summary>
        /// Returns the Relationship based on its Id.
        /// </summary>
        /// <param name="relationship">Entity refernce to the relationship.</param>
        /// <param name="token">Auth token.</param>
        /// <returns>Entity reference to the Relationship</returns>
        Task<Relationship> GetEmployeeRelationshipAsync(Entity relationship, string token);

        /// <summary>
        /// Returns the Relationship based on its Id.
        /// </summary>
        /// <param name="relationship">Entity refernce to the relationship.</param>
        /// <param name="token">Auth token.</param>
        /// <returns>Entity reference to the Relationship</returns>
        Relationship GetEmployeeRelationship(Entity relationship, string token);

        /// <summary>
        /// Updates the specified relationship.
        /// </summary>
        /// <param name="relationship">Relationship to update.</param>
        /// <param name="destEmployee">Destination employee reference.</param>
        /// <param name="token">Auth token.</param>
        /// <returns>True, if successful</returns>
        Task<bool> UpdateRelationshipAsync(Relationship relationship, Entity destEmployee, string token);

        /// <summary>
        /// Updates the specified relationship.
        /// </summary>
        /// <param name="relationship">Relationship to update.</param>
        /// <param name="destEmployee">Destination employee reference.</param>
        /// <param name="token">Auth token.</param>
        /// <returns>True, if successful</returns>
        void UpdateRelationship(Relationship relationship, Entity destEmployee, string token);
    }
}
