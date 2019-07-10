# Compliance 360 LDAP (Active Directory) Sync
The Compliance 360 Lightweight Directory Access Protocol Sync (LDAP Sync) application is a Microsoft Windows Service that synchronizes your local Active Directory (AD) with the Compliance 360 application. The service is very configurable. This document provides instructions for configuring and supporting LDAP Sync.
## Overview
The Lightweight Directory Access Protocol (LDAP) is an open, vendor-neutral, industry standard application protocol for accessing and maintaining distributed directory information services over an Internet Protocol (IP) network. 

The Compliance 360 LDAP Sync application application creates the following new objects based on mapping supplied in the configuration if they are not present in the Compliance 360 application: 

   * Employee with the following information:  
     * \* primary division (LDAP Sync will NOT create any missing divisions in the Compliance 360 application. Please create the division structure before starting the sync process.)        
     * number      
     * user name   
     * first and last name
     * email
     * job title
     * department
     * groups assigned to
     * company
     * relationships (the related employee must already exist, and the type of relationship is created if it is not available)
     * \* user status (which determines if the employee can log in or not)  
   Note: Information preceded by an \* is required to be included in the configuration mapping.
   * Job Title (new values created in the Global division)
   * Department (new values created in the employee's division)
   * User Group (new values created in the Global division)
   * Company (new values created in the Global division)

### Error Processing and Notifications
LDAP Sync also has configurable parameters for error processing and email notifications to the SAI Global Operations team member specified. The system will stop processing after the error threshold value is exceeded. This will stop processing for the current sync job, which will then execute again after the specified wait period. In addition to stopping the current processing, the system will attempt to send an email notification message using the configuration settings.

## Installation
Compliance 360 LDAP Sync can be installed after obtaining the installation files and following the following steps. If you are interested in configuring this service, contact your account manager or GRC Professional Services representative.

1. Click **Next** on the first page of the **Compliance 360 LDAP Sync Service Setup** page.
2. Accept the license agreement.
3. Choose **Typical**.
4. Choose **Install**. When prompted for elevated access rights, click **Yes** because the installation will need administrative privileges to install the Windows service.
5. Choose **Finish**.

## Configuration
You may configure LDAP Sync by modifying the **Compliance360.EmployeeSyncService.exe.config** file that is located in the install directory. The default location is ```C:\Program Files (x86)\SAI Global\Compliance360 LDAP Sync\```. 

A full sample configuration is listed below. Details on how to configure each section and the purpose of each setting are listed below the sample.
```xml
<?xml version="1.0" encoding="utf-8"?>

<configuration>
  <configSections>
    <section
      name="compliance360.sync"
      type="Compliance360.EmployeeSync.Library.Configuration.SyncConfigurationSection, Compliance360.EmployeeSync.Library"
      allowLocation="true"
      allowDefinition="Everywhere" />
  </configSections>
  <compliance360.sync>
    <jobs>
      <job
        name="All Users"
        type="ActiveDirectory"
        domain="sample.org"
        ou="corporate"
        userName=""
        password=""
        ldapQuery="(&amp;(objectClass=user)(objectCategory=person)(!msExchResourceMetaData=ResourceType:Room))"
        removeGroupPrefix=""
        intervalSeconds="3600"
        errorThreshold="10"
        errorNotificationHost=""
        errorNotificationPort="587"
        errorNotificationUseSsl="true"
        errorNotificationUserName=""
        errorNotificationPassword=""
        errorNotificationDomain=""
        errorNotificationEmailFrom=""
        errorNotificationEmailTo=""
        errorNotificationSubject="LDAP Sync error threshold exceeded [ORG_NAME]">
        <attributes>
          <attribute name="domain" includeInQuery="false" filter="DomainAttributeFilter" />
          <attribute name="sAMAccountName" />
          <attribute name="manager" />
          <attribute name="department" />
          <attribute name="title" />
          <attribute name="userPrincipalName" />
          <attribute name="distinguishedName" />
          <attribute name="sn" />
          <attribute name="givenName" />
          <attribute name="mail" />
          <attribute name="isDeleted" />
          <attribute name="userAccountControl" filter="UacAttributeFilter" alias="isActive" />
          <attribute name="memberOf" filter="GroupsAttributeFilter" />
        </attributes>
        <allowedGroups>
          <group name="Compliance360-Users" />
        </allowedGroups>
        <outputStreams>
          <stream name="Csv" />
          <stream name="Compliance360ApiV2">
            <settings>
              <setting name="baseAddress" value="https://secure.compliance360.com" />
              <setting name="organization" value="[ORG_NAME]" />
              <setting name="username" value="[USER_NAME]" />
              <setting name="password" value="[PASSWORD]" />
              <setting name="culture" value="en-US" />
              <setting name="loginIntervalMinutes" value="20">
            </settings>
            <mapping>
              <map from="Main Division" to="PrimaryDivision"/>
              <map from="{domain}\{sAMAccountName}" to="EmployeeNum"/>
              <map from="{domain}\{sAMAccountName}" to="UserName"/>
              <map from="{givenName}" to="FirstName"/>
              <map from="{sn}" to="LastName"/>
              <map from="{mail}" to="Email"/>
              <map from="{title}" to="JobTitleId" type="lookup"/>
              <map from="{department}" to="Department"/>
              <map from="{memberOf}" to="Groups"/>
              <map from="{manager}" to="Relationships" type="Manager"/>
              <map from=="{company}" to="Company" type="company"/>
              <map from="true" to="CanLogin"/>
            </mapping>
          </stream>
        </outputStreams>          
      </job>
    </jobs>
  </compliance360.sync>
</configuration>
```

### \<jobs\> Element
The **job** element contains one or more **job** elements. Each **job** defined will run independently and concurrently.
``` xml
<jobs>
  <job .../>
</jobs>
```

### \<job\> Element
Synchronization activities are defined as **jobs**. You can configure one or more **jobs** within LDAP Sync allowing a single instance of the service to handle multiple sources of users including those in multiple OUs or multiple Domains.

``` xml
<job
  name="All Users"
  type="ActiveDirectory"
  domain="sample.org"
  ou="corporate"
  userName=""
  password=""
  ldapQuery="(&amp;(objectClass=user)(objectCategory=person)(!msExchResourceMetaData=ResourceType:Room))"
  removeGroupPrefix=""
  intervalSeconds="3600"
  errorThreshold="10"
  errorNotificationHost=""
  errorNotificationPort="587"
  errorNotificationUseSsl="true"
  errorNotificationUserName=""
  errorNotificationPassword=""
  errorNotificationDomain=""
  errorNotificationEmailFrom=""
  errorNotificationEmailTo=""
  errorNotificationSubject="LDAP Sync error threshold exceeded [ORG_NAME]">
```
The parameters are:
* __name__
  * The name of the job.
* __type__
  * The type of job to process. There is only one supported job type in this version of the service. This value must be set to **ActiveDirectory** for the service to work correctly.
* __domain__
  * The domain that contains the users you wish to synchronize with the Compliance 360 application specified in a dot-separated format like "mycompany.mydomain.com".
* __ou (optional)__
  * The organizational unit name which contains the users.
* __userName (optional)__
  * The username that should be used to connect to the LDAP server. If not specified then the service will connect to the server in the context of the current user.
* __password (optional)__
  * The password that should be used to connect to the LDAP server.
* __ldapQuery__
  * The base query used to fetch the Active Directory users. The example listed above will return all users that are not Microsoft Exchange rooms.
* __removeGroupPrefix__
  * A prefix value that will be removed from each group name. If within Active Directory you create a series of Compliance 360 related groups with a common prefix like **Compliance360-Administrators**. When the group is added to Compliance 360, you can use the **removeGroupPrefix** value to create the group **Administrators**.
* __intervalSeconds__
  * The number of seconds the system waits between job executions. The default value is one (1) hour.
* __errorThreshold__
  * The number of errors that may occur before stopping the sync process. If this value is exceeded, the 
  system will also optionally send a notification email.
* __errorNotificationHost__
  * The SMTP host address. Example: **smtp.google.com**.
* __errorNotificationPort__
  * The SMTP port number. Example:: **25**
* __errorNotificationUseSsl__
  * **true** if the smtp client should use SSL when communicating with the server.
* __errorNotificationUserName__
  * The SMTP user name
* __errorNotificationPassword__
  * The SMTP user password
* __errorNotificationDomain (optional)__
  * The SMTP windows domain
* __errorNotificationEmailFrom__
  * The address that the email is sent from
* __errorNotificationEmailTo__
  * The address the email is sent to
* __errorNotificationSubject__
  * The subject of the email, Ex: **LDAP Sync error threshold for [ORG_NAME] was exceeded**

### \<attribute\> Element
**Attribute** elements are used to configure the attributes that are fetched from Active Directory which will then be mapped to the Compliance 360 employee property values.
``` xml
  <attribute 
    name="userAccountControl" 
    filter="UacAttributeFilter" 
    includeInQuery="false" 
    alias="isActive" />
```
The parameters are:
* __name__
  * The name of the LDAP attribute to return. See <https://msdn.microsoft.com/en-us/library/ms675090(v=vs.85).aspx> for a complete listing of available LDAP (Active Directory) attributes and their meanings.
* __filter (optional)__
  * Filters provide special post-processing behavior. The list of available filters and their behavior are listed below in the **Attribute Filters** section.
* __includeInQuery (optional)__
  * Enter **true** if the attribute should be included in the query. The value is helpful when deriving a new value from other values returned from AD. Example: You can return the user's domain value by defining a new attribute called **Domain**. Then use the **DomainAttributeFilter** to return the domain value.
* __alias (optional)__
  * Optional alternate name to be used to rename an attribute in the returned result set.

#### Attrbiute Filters
* __DomainAttributeFilter__
  * Returns the domain name using the ```distinguisedName``` AD attribute to find the first domain component. So if the Distinguished Name value is ```CN=Marco Lombard,Ou=Users,DC=saig,DC=frd,DC=global"``` The returned value will be ```saig```. This attribute filter depends on the ```distinguishedName``` being present in the attribute list. 

  Example use: 
  ``` xml
  <attribute
    name="domain"
    filter="DomainAttributeFilter"
    includeInQuery="false"
  />
  ```

* __GroupsAttributeFilter__
  * Returns the user's group membership. The values returned respect the ```removeGroupPrefix``` job attribute and the ```<allowedGroups/>``` configuration.

  Example use:
  ``` xml
  <attribute name="memberOf" filter="GroupsAttributeFilter" />
  ```

* __UacAttributeFilter__
  * Analyzes the ```userAccountControl``` value and returns a value indicating whether the user is active or has been deleted.

  Example use:
  ``` xml
  <attribute name="userAccountControl" filter="UacAttributeFilter" alias="isActive" />
  ```
  

### \<allowedGroups\> Element
The **allowedGroups** element is used to filter the group membership and users that will be sent to the Compliance 360 application.
If groups are specified, then Active Directory users must be a member of one of the specified groups or they will be filtered out of the result set. This list will also filter the group membership list. 

Example: Marco Lombard is a member of the **Compliance 360 Users** and **Hospital Employees** groups. Gabriele Hoffman is a member of just the **Hospital Employees** group. If the allowedGroups section looks like the following:
``` xml
<allowedGroups>
  <group name="Compliance 360 Users" />
</allowedGroups>
```
Then, only Marco Lombard will be a Compliance 360 user. Also, Marco Lombard will only be a member of the **Compliance 360 Users** group in Compliance 360. **Hospital Employees** will be ignored since it is not on the **\<allowedGroups\>** list.


### Output Streams
Output streams are used to send the user content retrieved from Active Directory to a specific destination. There are currently two (2) supported streams in the current version of the service: **CSsv** and **Compliance360ApiV2**.

#### Csv stream
The **Csv** stream is used to write the results of the LDAP query to a **\*.csv** file. This is very useful for testing out the queries and configuration to ensure the specified jobs are configured correctly. 

Example:
``` xml
<outputStream>
   <stream name="Csv">
     <settings>
       <setting name="path" value="C:\temp\ad-output.csv" />
     </settings>
   </stream>
</outputStreams>
```
The following setting is required:
* __path:__ The full file path to the **\*.csv** file that should be created.


#### Compliance360ApiV2 Stream
The **Compliance360Apiv2** stream handles the REST API calls to the Compliance 360 application, creating, updating, and deleting employee accounts based on the Active Directory information.

Example:
``` xml
<stream name="Compliance360ApiV2">
  <settings>
    <setting name="baseAddress" value="https://secure.compliance360.com" />
    <setting name="organization" value="[ORG_NAME]" />
    <setting name="username" value="[USER_NAME]" />
    <setting name="password" value="[PASSWORD]" />
    <setting name="culture" value="[CULTURE_CODE]" />
    <setting name="loginIntervalMinues" value="[NUMBER_OF_MINUTES]" />
  </settings>
  <mapping>
    <map from="Main Division" to="PrimaryDivision"/>
    <map from="{domain}\{sAMAccountName}" to="EmployeeNum"/>
    <map from="{domain}\{sAMAccountName}" to="UserName"/>
    <map from="{givenName}" to="FirstName"/>
    <map from="{sn}" to="LastName"/>
    <map from="{mail}" to="Email"/>
    <map from="{title}" to="JobTitleId" type="lookup"/>
    <map from="{department}" to="Department"/>
    <map from="{memberOf}" to="Groups"/>
    <map from="{manager}" to="Relationships" type="Manager"/>
    <map from="{company}" to="Company" type="company"/>
    <map from="true" to="CanLogin"/>
  </mapping>
</stream>
```
#### \<setting\> Element
The **setting** elements provide the information needed to connect with the Compliance 360 application. The parameters are:

* __name:__ The setting name
* __value:__ The setting value

The following settings are required:
* __baseAddress:__ The base URL of the Compliance 360 application. Use the URL for the appropriate data center for this organization.   
    **UK** data center: https://uk.compliance360.com/ with Culture of **en-GB**  
    **US** data center: https://secure.compliance360.com/ with Culture of **en-US**  
    **APAC** data center: https://c3602.grc.saiglobal.com/ with Culture of **en-AU**  
* __organization:__ The organization you are connecting to
* __usename:__ The user name with API access rights
* __password:__ The user's password
* __culture:__ The Microsoft code to identify the culture that corresponds to the data center associated with the baseAddress setting. See baseAddress setting for cultures.
* __loginIntervalMinutes:__ The number of minutes to wait before refreshing the authentication token. This token expires after a certain amount of time and needs to be replaced when long-running sync events are occurring. By default, that is 20 minutes.

#### \<map\> Element
The **map** elements provide the source attribute name/statement and the target field name in the Compliance 360 application so that the information can correctly populate the application. The parameters are:

* __from:__ The source attribute name or statement. Attributes can be retreived with the syntax **{ATTRIB_NAME}**. Multiple
  attributes can be used together and combined with text to achieve the desired source value.
  * Example:
    ``` xml
    <map from="{domain}\{sAMAccountName}" to="EmployeeNum"/>
    ```
    will result in **from** value being the domain name, a slash (**\\**) followed by the account, as in **saig\\marcol**.
* __to:__ The Compliance 360 employee field name. The following **To** fields are required in order for Compliance 360 to function correctly:
  * __PrimaryDivision:__ The Compliance 360 employee base division path in the format **[DIVISION_NAME] \ [CHILD_DIVISION_NAME]**. Please note that there is a space both before and after the slash.
  * __EmployeeNum:__ The identifier supplied by the client used to uniquely identify the employee in the Compliance 360 application.
  * __CanLogin:__ This is the value that sets the employee's user status which determines whether the user may login to the system.
* __type__ attributes: There are several system attribute types that can be used to let the system handle complex field type mappings:
  * __lookup:__ Tells the system that the destination field, the **to** value, is a Lookup type field. It is needed to find an existing lookup value or to create the lookup value if it is not present in the application.
  * __company:__ Tells the system that the destination field is of Company  type field. It is needed to find an existing company entity or to create the new company entity if it is not present in the application.



## Best Practices and Notes
### Create Divisison First in Compliance 360
LDAP Sync will NOT create any missing Divisions in the Compliance 360 application. Please create the division structure before starting the sync process.
### Objects and Lookup Values Automatically Created
LDAP Sync will create the following new objects if they are not present in the Compliance 360 application:
  * Job Title
  * Department
  * User Group
  * Employee
  * Company
  
  It will also create lookup values if they are not present in the Compliance 360 application.
  
### Application Logging
  
Application logging \/ tracing is done using [NLog](http://nlog-project.org/). By default, the application comes preconfigured to log all errors to the **Application** event log. To troubleshoot the service, please enable the file logging at the debug level as seen below by uncommenting the **file** logger rule entry in the **NLog.config** file.
``` xml
<?xml version="1.0" encoding="utf-8"?>

<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <targets>
    <target name="debugger" xsi:type="Debugger" layout="${shortdate}::${time}::${level}::${message}" />
    <target name="file" xsi:type="File"
            layout="${longdate} ${message} ${exception}"
            fileName="/logs/logfile.txt"
            keepFileOpen="false"
            encoding="iso-8859-2" />
    <target name="eventlog" xsi:type="EventLog" layout="${message} ${exception}" source="Compliance 360" log="Application" />
  </targets>

  <rules>
    <logger name="*" minlevel="Debug" writeTo="file" />
    <logger name="*" minlevel="Error" writeTo="eventlog" />
  </rules>
</nlog>
```
### Caching of Compliance 360 Data
LDAP Sync will cache Compliance 360 data information locally using isolated storage for security purposes. Please inspect the log files for the location of the cache files in the case that you need to remove them for a clean install.


## Dependencies
This application depends on the following frameworks (Please note that the application installer handles the installation of all dependencies):
* Microsoft Window Server 2012 or newer with the latest .NET Framework 4.7.x installed.
* Installation package includes the following dependenceis:
  * Newtonsoft.Json - for JSON processing
  * NLog - for logging
  * Structuremap - for dependency injection
  * Rebex SFTP - for streaming user content to an sftp location
