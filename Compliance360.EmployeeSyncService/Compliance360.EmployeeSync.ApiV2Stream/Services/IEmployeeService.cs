using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compliance360.EmployeeSync.ApiV2Stream.Data;
using Newtonsoft.Json.Linq;

namespace Compliance360.EmployeeSync.ApiV2Stream.Services
{
    public interface IEmployeeService
    {
        /// <summary>
        /// Sets the base uri of the Http client.
        /// </summary>
        /// <param name="baseAddress">The bsae address of the API.</param>
        void SetBaseAddress(string baseAddress);

        /// <summary>
        /// Creates a new employee based on the supplied employee object.
        /// </summary>
        /// <param name="employee">Objective describing the employee to create.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>Entity reference to the new employee id.</returns>
        Task<Entity> CreateEmployeeAsync(Employee employee, string token);

        /// <summary>
        /// Creates a new employee based on the supplied employee object.
        /// </summary>
        /// <param name="employee">Objective describing the employee to create.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>Entity reference to the new employee id.</returns>
        Entity CreateEmployee(Employee employee, string token);

        /// <summary>
        /// Gets the id of the default workflow template.
        /// </summary>
        /// <param name="token">The current auth token.</param>
        /// <returns>Entity reference to the workflow.</returns>
        Task<Entity> GetDefaultWorkflowTemplateAsync(string token);

        /// <summary>
        /// Gets the id of the default workflow template.
        /// </summary>
        /// <param name="token">The current auth token.</param>
        /// <returns>Entity reference to the workflow.</returns>
        Entity GetDefaultWorkflowTemplate(string token);

        /// <summary>
        /// Gets an employee based on the supplied employee number.
        /// </summary>
        /// <param name="employeeNumber">The employee number (unique id) of the employee to find.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>Entity reference to the employee.</returns>
        Task<Entity> GetEmployeeByEmployeeNumAsync(string employeeNumber, string token);

        /// <summary>
        /// Gets an employee based on the supplied employee number.
        /// </summary>
        /// <param name="employeeNumber">The employee number (unique id) of the employee to find.</param>
        /// <param name="token">The active AuthToken.</param>
        /// <returns>Entity reference to the employee.</returns>
        Entity GetEmployeeByEmployeeNum(string employeeNumber, string token);

        /// <summary>
        /// Gets the local profile for the specified user.
        /// </summary>
        /// <param name="employee">The current employee</param>
        /// <param name="token">Auth token</param>
        /// <returns>Employee's local profile</returns>
        Task<Entity> GetEmployeeProfileAsync(Entity employee, string token);

        /// <summary>
        /// Gets the local profile id for the specified user.
        /// </summary>
        /// <param name="employee">The current employee</param>
        /// <param name="token">Auth token</param>
        /// <returns>Employee's local profile</returns>
        Entity GetEmployeeProfile(Entity employee, string token);

        /// <summary>
        /// Gets the specified Job Title by its name
        /// </summary>
        /// <param name="name">Job title name.</param>
        /// <param name="token">Auth token.</param>
        /// <returns>Entity reference to the job title</returns>
        Task<Entity> GetJobTitleAsync(string name, string token);

        /// <summary>
        /// Gets the specified Job Title by its name
        /// </summary>
        /// <param name="name">Job title name.</param>
        /// <param name="token">Auth token.</param>
        /// <returns>Entity reference to the job title</returns>
        Entity GetJobTitle(string name, string token);

        /// <summary>
        /// Updates an employee.
        /// </summary>
        /// <param name="employee">Id token of the employee to update.</param>
        /// <param name="employee">The employee object.</param>
        /// <param name="token">Auth token.</param>
        /// <returns>True, if successful</returns>
        Task<bool> UpdateEmployeeAsync(Employee employee, string token);

        /// <summary>
        /// Updates an employee.
        /// </summary>
        /// <param name="employee">Id token of the employee to update.</param>
        /// <param name="employee">The employee object.</param>
        /// <param name="token">Auth token.</param>
        void UpdateEmployee(Employee employee, string token);

        /// <summary>
        /// Updates an employee profile to add/remove group membership
        /// </summary>
        /// <param name="profile">The profile to update.</param>
        /// <param name="groupsToAdd">List of groups to add to the user.</param>
        /// <param name="groupsToRemove">List of groups that should be removed from the user.</param>
        /// <param name="token">Current auth token.</param>
        /// <returns>True, if successful.</returns>
        Task<bool> UpdateEmployeeProfileAsync(Profile profile, List<Entity> groupsToAdd, List<Entity> groupsToRemove, string token);

        /// <summary>
        /// Updates an employee profile to add/remove group membership
        /// </summary>
        /// <param name="profile">The profile to update.</param>
        /// <param name="groupsToAdd">List of groups to add to the user.</param>
        /// <param name="groupsToRemove">List of groups that should be removed from the user.</param>
        /// <param name="token">Current auth token.</param>
        void UpdateEmployeeProfile(Profile profile, List<Entity> groupsToAdd, List<Entity> groupsToRemove, string token);
    }
}
