#r "nuget: Microsoft.SqlServer.DACFx, 150.4769.1"

using Microsoft.SqlServer.Dac;

var dbName = "SFA.DAS.EmployerIncentives.Database";
var connectionString = $"Data Source=.;Initial Catalog={dbName};Integrated Security=True;";
var dacpacLocation = Directory.GetFiles(@".\bin", "SFA.DAS.EmployerIncentives.Database.Build.dacpac", SearchOption.AllDirectories)[0];
Console.WriteLine("Found database package file: " + dacpacLocation);
Console.WriteLine("Deploying database...");
var dbPackage = DacPackage.Load(dacpacLocation);
var services = new DacServices(connectionString);
services.Deploy(dbPackage, dbName, true);