# Compliance 360 LDAP (Active Directory) Sync
## Overview
The Compliance 360 LDAP Sync application is a Microsoft Windows Service that synchronizes your local AD with
the Compliance 360 application. The service is very configurable. This document provides instructions for
configuring and supporting the LDAP sync service. 

## Installation
The Compliance 360 LDAP Sync service can be installed by obtaining the [*.msi] installer from [here](https://secure.compliance360.com)

Step 1 - Click "Next"

![](https://s3.amazonaws.com/public.galvanic.io/saiglobal/setup_001.png)

Step 2 - Accept the license agreement.

![](https://s3.amazonaws.com/public.galvanic.io/saiglobal/setup_002.png)

Step 3 - Choose "Typical"

![](https://s3.amazonaws.com/public.galvanic.io/saiglobal/setup_003.png)

Step 4 - Choose "Install." When prompted for elevated access rights, choose Yes as the install will need administrative privileges to install the Windows service.

![](https://s3.amazonaws.com/public.galvanic.io/saiglobal/setup_004.png)

Step 5 - Choose "Finish"

![](https://s3.amazonaws.com/public.galvanic.io/saiglobal/setup_005.png)


## Configuration
You may configure the Sync service by modifying the Compliance360.EmployeeSyncService.exe.config file that is located in the install directory. The default location is ```C:\Program Files (x86)\SAI Global\Compliance360 LDAP Sync\```. 

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
        errorNotificationSubject="LDAP Sync error threshold exceeded">
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
          <stream name="Logger" />
          <stream name="Compliance360ApiV2">
            <settings>
              <setting name="baseAddress" value="https://secure.compliance360.com" />
              <setting name="organization" value="[ORG_NAME]" />
              <setting name="username" value="[USER_NAME]" />
              <setting name="password" value="[PASSWORD]" />
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
              <map from="My Company" to="Company" type="company"/>
              <map from="true" to="CanLogin"/>
            </mapping>
          </stream>
        </outputStreams>          
      </job>
    </jobs>
  </compliance360.sync>
</configuration>
```

### &lt;Jobs&gt; Element
The Jobs element contains one or more &lt;Job&gt; elements. Each job defined will run independently and concurrently.
``` xml
<jobs>
  <job .../>
</jobs>
```

### &lt;Job&gt; Element
Synchronization activities are defined as Jobs. You can configure one or more jobs within the service allowing a single instance of the service
to handle multiple sources of users including those in multiple OUs or multiple Domains.

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
  errorNotificationSubject="LDAP Sync error threshold exceeded">
```
* __name__
  * The name of the job.
* __type__
  * The type of job to process. There is only one supported job "type" in this version of the service. This value must be set to "ActiveDirectory" for the service to work correctly.
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
  * A prefix value that will be removed from each group name. If within Active Directory you create a series of Compliance 360 related groups with a common prefix like "Compliance360-Administrators." When the group is added to Compliance 360, you can use the removeGroupPrefix value to create the group "Administrators."
* __intervalSeconds__
  * The number of seconds the system waits between job executions. The default value is one (1) hour.
* __errorThreshold__
  * The number of errors that may occur before stopping the sync process. If this value is exceeded, the 
  system will also optionally send a notification email.
* __errorNotificationHost__
  * The SMTP host address: Ex smtp.google.com
* __errorNotificationPort__
  * The SMTP port number: Ex: 25
* __errorNotificationUseSsl__
  * "true" if the smtp client should use SSL when communicating with the server.
* __errorNotificationUserName__
  * The SMTP user name
* __errorNotificationPassword__
  * The SMTP user password
* __errorNotificationDomain__
  * The SMTP windows domain (optional)
* __errorNotificationEmailFrom__
  * The "From" email address
* __errorNotificationEmailTo__
  * The "To" email address
* __errorNotificationSubject__
  * The subject of the email, Ex: "LDAP Sync error threshold for [CLIENT_NAME] was exceeded"

### &lt;attrbiute&gt; Element
Attribute elements are used to configure the attributes that are fetched from Active Directory which will then be mapped to the Compliance 360
employee property values.
``` xml
  <attribute 
    name="userAccountControl" 
    filter="UacAttributeFilter" 
    includeInQuery="false" 
    alias="isActive" />
```
* __name__
  * The name of the LDAP attribute to return. See <https://msdn.microsoft.com/en-us/library/ms675090(v=vs.85).aspx> for a complete listing of available LDAP (Active Directory) attributes and their meanings.
* __filter (optional)__
  * Filters provide special post-processing behavior. The list of available filters and their behavior are listed below.
* __includeInQuery (optional)__
  * True, if the attribute should be included in the query. The value is helpful when deriving a new value from other values returned from 
  AD. Example: You can return the user's domain value by defining a new attribute called "Domain." Then use the DomainAttributeFilter to return the domain value.
* __alias (optional)__
  * Optional alternate name to be used to rename an attribute in the returned result set.

### Attrbiute Filters
* __DomainAttributeFilter__
  * The domain attribute filter will return the domain name using the ```distinguisedName``` AD attribute to find the first domain component. So if the Distinguished Name value is ```CN=Thomas Lee,Ou=Users,DC=saig,DC=frd,DC=global"``` The returned value will be ```saig```. This attribute filter depends on the ```distinguishedName``` being present in the attribute list. 

  Example use: 
  ``` xml
  <attribute
    name="domain"
    filter="DomainAttributeFilter"
    includeInQuery="false"
  />
  ```

* __GroupsAttributeFilter__
  * The GroupsAttributeFilter is used to return the user's group membership. The values returned respect the ```removeGroupPrefix``` job attribute and the ```<allowedGroups/>``` configuration.

  Example use:
  ``` xml
  <attribute name="memberOf" filter="GroupsAttributeFilter" />
  ```

* __UacAttributeFilter__
  * The UacAttributeFilter is used to analyze the ```userAccountControl``` value and return a value indicating whether the 
  user is active or has been deleted.

  Example use:
  ``` xml
  <attribute name="userAccountControl" filter="UacAttributeFilter" alias="isActive" />
  ```
  

### &lt;allowedGroups&gt; Element
The ```<allowedGroups>``` element is used to filter the group membership and users that will be sent to the Compliance 360 application.
If groups are specified, then Active Directory users must be a member of one of the specified groups or they will be filtered out of the result set. This list will also filter the group membership list. 

Example: User Thomas Lee is a member of the "Compliance 360 Users" and "Hospital Employees" groups. Mike Gilbert is a member of just the "Hospital Employees" group. If the allowedGroups section looks like the following:
``` xml
<allowedGroups>
  <group name="Compliance 360 Users" />
</allowedGroups>
```
Then, only Thomas Lee will be a Compliance 360 user. Also, Thomas Lee will only be a member of the "Compliance 360 Users" group in Compliance 360, "Hospital Employees" will be ignored since it is not on the ```<allowedGroups>``` list.


### Output Streams
Output streams are used to send the user content retrieved from Active Directory to a specific destination. There are currently two (2) supported streams in the current version of the service. 

``` xml
<outputStreams>
  <stream name="Logger" />
  <stream name="Compliance360ApiV2">
    <settings>
      <setting name="baseAddress" value="https://secure.compliance360.com" />
      ...
    </settings>
    <mapping>
      <map from="Main Division" to="PrimaryDivision"/>
      ...
    </mapping>
  </stream>
</outputStreams>
```

#### Logger stream
The Logger Stream is used for testing the output of the service. Since streams are written to in order, it is suggested that the Logger Stream be the first stream in the list so that you can see output before it being written to more complex streams like API.

Example:
``` xml
<stream name="Logger" />
```

#### CSV stream
The CSV stream can be used to write the results of the LDAP query to a *.csv file. This is very useful for testing out the queries and configuration to ensure the specified jobs are configured correctly.

Example:
``` xml
<stream name="Csv">
  <settings>
    <setting name="path" value="C:\temp\ad-output.csv" />
  </settings>
</stream>
```
Required CSV stream settings values:
* __path:__ The full file path to the *.csv file that should be created.


#### Compliance360ApiV2 Stream
The Compliance 360Apiv2 Stream handles the REST API calls to the Compliance 360 application, creating, updating and deleting employee accounts based on the Active Directory information.

``` xml
<stream name="Compliance360ApiV2">
  <settings>
    <setting name="baseAddress" value="https://secure.compliance360.com" />
    <setting name="organization" value="[ORG_NAME]" />
    <setting name="username" value="[USER_NAME]" />
    <setting name="password" value="[PASSWORD]" />
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
    <map from="My Company" to="Company" type="company"/>
    <map from="true" to="CanLogin"/>
  </mapping>
</stream>
```
#### &lt;setting&gt; Element
* __name:__ The setting name
* __value:__ The setting value

The four (4) settings listed below are required:
* __baseAddress:__ The base URL of the Compliance 360 application.
* __organization:__ The organization you are connecting to.
* __usename:__ The user name with API access rights
* __password:__ The user's password

#### &lt;map&gt; Element
* __from:__ The source attribute name or statement. Attributes can be retreived with the syntax ```{ATTRIB_NAME}```. Multiple
  attributes can be used together and combined with text to achieve the desired source value.
  * Example:
    ``` xml
    <map from="{domain}\{sAMAccountName}" to="EmployeeNum"/>
    ```
    will result in "from" value being the domain name, a slash "\\", followed by the account like saig\thomasl
* __to:__ The Compliance 360 employee field name. There are several fields that are required in order for the system to function correctly.
* Required __"to"__ fields:
  * __PrimaryDivision:__ The Compliance 360 employee base division path in the format ```[DIVISION_NAME] \ [CHILD_DIVISION_NAME]```. Please note that there is a space both before and after the slash.
  * __EmployeeNum:__ The identifier supplied by the client used to uniquely identify the employee in the Compliance 360 application.
  * __CanLogin:__ This is the value that sets the employee's user status which determines whether the user may login to the system.
* "type" Attributes: There are several system attribute "Types" that can be used to let the system handle complex field type mapping.
  * __lookup:__ The "lookup" type tells the system that the destination field, the "to" value, is a Lookup type field. This attribute is needed to create the lookup value if it is not present and/or to lookup an existing lookup value.
  * __company:__ The "company" type tells the system that the destination field is of type "Company" and enables the creation of new Company entities if they are not already present in the application.



## Best Practices and Notes
* The Sync service will NOT create any missing Divisions in the Compliance 360 application. Please create the division structure before starting the sync process.
* The service will create the following new objects if they are not present in the Compliance 360 application:
  * Job Title
  * Department
  * User Group
  * Employee
* Application logging / tracing is done using [NLog](http://nlog-project.org/). By default, the application comes preconfigured to log all errors to the "Application" event log. To troubleshoot the service, please enable the file logging at the debug level as seen below by uncommenting the "file" logger rule entry in the NLog.config file
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
* The Sync service will cache Compliance 360 data information locally using isolated storage for security purposes. Please inspect the log files for the location of the cache files in the case that you need to remove them for a clean install.


## Dependencies
This application depends on the following frameworks (Please note that the application installer handles the installation of all dependencies):
1. Newtonsoft.Json - for JSON processing
2. NLog - for logging
3. Structuremap - for dependency injection
4. Rebex SFTP - for streaming user content to a Sftp location
5. .NET Framework 4.7
6. The service is designed to run on Windows Server 2012 and above.