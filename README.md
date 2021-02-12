[![pipeline status](https://gitlab.com/reductech/edr/connectors/sql/badges/master/pipeline.svg)](https://gitlab.com/reductech/edr/connectors/sql/-/commits/master)
[![coverage report](https://gitlab.com/reductech/edr/connectors/sql/badges/master/coverage.svg)](https://gitlab.com/reductech/edr/connectors/sql/-/commits/master)
[![Gitter](https://badges.gitter.im/reductech/community.svg)](https://gitter.im/reductech/community?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

# EDR SQL Database Connector

[Reductech EDR](https://gitlab.com/reductech/edr) is a collection of
libraries that automates cross-application e-discovery and forensic workflows.

The SQL connector containes Steps for interacting with SQL databases.

# How to Use

This connector contains six steps

|Step|Description|Result Type|
|-|-|-|
|`CreateConnectionString`|Creates a MsSql connection string|`String`|
|`SqlCommand`|Sends a command to a SQL database|`Unit`|
|`SqlCreateSchemaFromTable`|Creates a Schema entity from a SQL table|`Entity`|
|`SqlCreateTable`|Create a SQL table from a given schema|`Unit`|
|`SqlInsert`|Inserts data into a SQL table|`Unit`|
|`SqlQuery`|Executes a SQL query and returns the result as an entity stream.|`Array<Entity>`|

## Schemas

Schemas are used to create tables and insert entities.

You can create a Schema from an existing table using `SqlCreateSchemaFromTable`

The following schema properties are used in creating tables and inserting entities

|Property|Description|
|-|-|
|`Name`|Maps to the name of the SQL table.|
|`AllowExtraProperties`|Must be set to `False`|
|`Properties`|Dictionary Mapping column names to column details.|

The following nested properties are also used

|Property|Description|
|-|-|
|`Type`|The property type.|
|`Multiplicity`|Must be either `UpToOne` for a nullable property or `ExactlyOne` for a not null property.|

This is an example of declaring a schema
```scala
- <Schema> = (
	Name: "MyTable" 
	AllowExtraProperties: False 
	Properties: (
				Id: (
					Type: SchemaPropertyType.Integer 
					Multiplicity: Multiplicity.ExactlyOne
					) 
				Name:(
					Type: SchemaPropertyType.String 
					Multiplicity: Multiplicity.UpToOne
					)
				)
	)	
```

## Example

This is an example of a step that drops a table, recreates it, and inserts an entity.

```scala
- <ConnectionString> = CreateConnectionString 
						Server: "Server" 
						Database: "Database" 
						UserName: "UserName" 
						Password: "Password"
- <Schema> = (
	Name: "MyTable" 
	AllowExtraProperties: False 
	Properties: (
				Id: (
					Type: SchemaPropertyType.Integer 
					Multiplicity: Multiplicity.ExactlyOne
					) 
				Name:(
					Type: SchemaPropertyType.String 
					Multiplicity: Multiplicity.UpToOne
					)
				)
	)
- SqlCommand 
			ConnectionString: <ConnectionString> 
			Command: "DROP TABLE IF EXISTS MyTable" 
			DatabaseType: 'SQLite'
- SqlCreateTable 
				ConnectionString: <ConnectionString> 
				Schema: <Schema> 
				DatabaseType: 'SQLite'
- SqlInsert 
			ConnectionString: <ConnectionString> 
			Entities: [
					(Id: 1 Name:'Name1' ) 
					(Id: 2 Name:'Name2')
					] 
			Schema: <Schema> 
			DatabaseType: 'SQLite'
```

### [Try SQL Connector](https://gitlab.com/reductech/edr/edr/-/releases)

Using [EDR](https://gitlab.com/reductech/edr/edr),
the command line tool for running Sequences.

## Documentation

- Documentation is available here: https://docs.reductech.io

## E-discovery Reduct

The PowerShell Connector is part of a group of projects called
[E-discovery Reduct](https://gitlab.com/reductech/edr)
which consists of a collection of [Connectors](https://gitlab.com/reductech/edr/connectors)
and a command-line application for running Sequences, called
[EDR](https://gitlab.com/reductech/edr/edr/-/releases).

# Releases

Can be downloaded from the [Releases page](https://gitlab.com/reductech/edr/connectors/sql/-/releases).

# NuGet Packages

Are available for download from the [Releases page](https://gitlab.com/reductech/edr/connectors/sql/-/releases)
or from the `package nuget` jobs of the [CI pipelines](https://gitlab.com/reductech/edr/connectors/sql/-/pipelines). They're also available in:

- [Reductech Nuget feed](https://gitlab.com/reductech/nuget/-/packages) for releases
- [Reductech Nuget-dev feed](https://gitlab.com/reductech/nuget-dev/-/packages) for releases, master branch and dev builds
