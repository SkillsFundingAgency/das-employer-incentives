#r "nuget: Microsoft.SqlServer.DACFx, 150.4769.1"

using Microsoft.SqlServer.Dac;

var dbName = "SFA.DAS.EmployerIncentives.Database";
var connectionString = $"Data Source=.;Initial Catalog={dbName};Integrated Security=True;";
var dacpacLocation = Path.Combine(Directory.GetCurrentDirectory(),  @".\bin\Debug\netstandard2.0\SFA.DAS.EmployerIncentives.Database.Build.dacpac");

var dbPackage = DacPackage.Load(dacpacLocation);
var services = new DacServices(connectionString);
services.Deploy(dbPackage, dbName, true);