﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Connectors.Sql.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Sql.Tests
{

/// <summary>
/// These are not really tests but ways to quickly and easily run steps
/// </summary>
[AutoTheory.UseTestOutputHelper]
public partial class ExampleTests
{
    [Fact(Skip = "skip")]
    //[Fact]
    [Trait("Category", "Integration")]
    public async Task RunSCLSequence()
    {
        const string connectionString =
            "User ID=postgres;Password=postgres;Host=localhost;Port=5432;Database=postgres;";

        string scl =
            // @"SqlQuery (CreateConnectionString 'DESKTOP-GPBS4SN' 'Introspect') 'SELECT *  FROM [AUN_CUSTODIAN]'";
            $@"SqlCreateSchemaFromTable '{connectionString}' 'MyTable' 'postgres'";

        var logger = TestOutputHelper.BuildLogger(LogLevel.Information);
        var sfs    = StepFactoryStore.CreateUsingReflection(typeof(CreateMsSQLConnectionString));

        var context = new ExternalContext(
            ExternalContext.Default.FileSystemHelper,
            ExternalContext.Default.ExternalProcessRunner,
            ExternalContext.Default.Console,
            (DbConnectionFactory.DbConnectionName, DbConnectionFactory.Instance)
        );

        var runner = new SCLRunner(
            SCLSettings.EmptySettings,
            logger,
            sfs,
            context
        );

        var r = await runner.RunSequenceFromTextAsync(
            scl,
            new Dictionary<string, object>(),
            CancellationToken.None
        );

        r.ShouldBeSuccessful(x => x.ToString()!);
    }

    [Fact(Skip = "skip")]
    //[Fact]
    [Trait("Category", "Integration")]
    public async Task RunObjectSequence()
    {
        //const string connectionString =
        //    @"Data Source=C:\Users\wainw\source\repos\MarkPersonal\ProgressiveAnagram\ProgressiveAnagram\Quotes.db; Version=3;";

        const string tableName = "MyTable4";

        var schema = new Schema()
        {
            Name                 = tableName,
            AllowExtraProperties = false,
            Properties = new Dictionary<string, SchemaProperty>()
            {
                {
                    "Id",
                    new SchemaProperty()
                    {
                        Type         = SchemaPropertyType.Integer,
                        Multiplicity = Multiplicity.ExactlyOne
                    }
                },
                {
                    "Name",
                    new SchemaProperty()
                    {
                        Type = SchemaPropertyType.String, Multiplicity = Multiplicity.UpToOne
                    }
                },
                {
                    "TestDouble",
                    new SchemaProperty()
                    {
                        Type = SchemaPropertyType.Double, Multiplicity = Multiplicity.UpToOne
                    }
                },
                {
                    "TestDate",
                    new SchemaProperty()
                    {
                        Type = SchemaPropertyType.Date, Multiplicity = Multiplicity.UpToOne
                    }
                },
                {
                    "TestBool",
                    new SchemaProperty()
                    {
                        Type = SchemaPropertyType.Bool, Multiplicity = Multiplicity.UpToOne
                    }
                },
                {
                    "TestEnum",
                    new SchemaProperty()
                    {
                        Type         = SchemaPropertyType.Enum,
                        Multiplicity = Multiplicity.UpToOne,
                        Values       = new List<string> { "EnumValue", "EnumValue2" },
                        EnumType     = "MyEnum"
                    }
                },
            }
        };

        static Entity[] CreateEntityArray(int number)
        {
            var entities = new List<Entity>();

            for (var i = 0; i < number; i++)
            {
                entities.Add(CreateEntity(i));
            }

            static Entity CreateEntity(int i)
            {
                return Entity.Create(
                    ("Id", i),
                    ("Name", $"Mark{i}"),
                    ("TestDouble", 3.142 + i),
                    ("TestDate", new DateTime(1970 + (i / 12), (i % 12) + 1, 6)),
                    ("TestBool", i % 2 == 0),
                    ("TestEnum", i % 2 == 0 ? "EnumValue" : "EnumValue2")
                );
            }

            return entities.ToArray();
        }

        const int numberOfEntities = 10000;

        var dbType = DatabaseType.MariaDb;

        var step = new Sequence<Unit>()
        {
            InitialSteps = new List<IStep<Unit>>
            {
                new SetVariable<StringStream>()
                {
                    Variable = new VariableName("ConnectionString"),
                    Value = new CreateMySQLConnectionString()
                    {
                        UId      = Constant("root"),
                        Database = Constant("mydatabase"),
                        Server   = Constant("localhost"),
                        Pwd      = Constant("maria"),
                    }
                },
                new SqlCommand()
                {
                    ConnectionString = GetVariable<StringStream>("ConnectionString"),
                    DatabaseType     = Constant(dbType),
                    Command          = Constant($"Drop table if exists {schema.Name};")
                },
                new SqlCreateTable()
                {
                    ConnectionString = GetVariable<StringStream>("ConnectionString"),
                    DatabaseType     = Constant(dbType),
                    Schema           = Constant(schema.ConvertToEntity())
                },
                new SqlInsert()
                {
                    ConnectionString = GetVariable<StringStream>("ConnectionString"),
                    Schema           = Constant(schema.ConvertToEntity()),
                    Entities         = Array(CreateEntityArray(numberOfEntities)),
                    DatabaseType     = Constant(dbType)
                }
            },
            FinalStep = new DoNothing()
        };

        var scl = step.Serialize();

        TestOutputHelper.WriteLine(scl);

        var context = new ExternalContext(
            ExternalContext.Default.FileSystemHelper,
            ExternalContext.Default.ExternalProcessRunner,
            ExternalContext.Default.Console,
            (DbConnectionFactory.DbConnectionName, DbConnectionFactory.Instance)
        );

        var monad = new StateMonad(
            TestOutputHelper.BuildLogger(),
            SCLSettings.EmptySettings,
            StepFactoryStore.CreateUsingReflection(),
            context,
            new object()
        );

        var r = await (step as IStep<Unit>).Run(monad, CancellationToken.None);

        r.ShouldBeSuccessful(x => x.ToString()!);
    }
}

}
