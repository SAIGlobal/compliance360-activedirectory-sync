# Compliance 360 LDAP (Active Directory) Sync Release Notes
## v1.1 - February 2019
* Added new "Lookup" type field processing. To use this feature, the "to" value should be the name of the Employee field that is of type "Lookup". Please also set the "type" attribute to "lookup." This instructs the system to create missing lookup values and to fetch the correct type of field reference sine a lookup is a reference to another entity in the API.
```
<map from="{status}" to="EmployeeStatusId" type="lookup"/>
```


* Removed JobTitle as a system field and moved to a lookup type. You will need to include the attribute "type" of "lookup" on this mapping value.
```
<map from="{title}" to="JobTitleId" type="lookup"/>
```

* Added new Company type field. You will need to include the attribute "type" of "company" on this mapping value.
```
<map from="My Company" to="Company" type="company"/>
```

* "CanLogin" is a new system field. This value is a boolean and must be set to either "true" or "false." The value MUST be true in order for the user to be able to login to the application.
```
<map from="true" to="CanLogin"/>
```
