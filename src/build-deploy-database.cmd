SET temp_dir=tempclasslib
SET tools_dir=tools
SET sqlpackage_version=14.0.3953.4
SET db_project=SFA.DAS.EmployerIncentives.Database

dotnet new classlib -n %temp_dir%
dotnet add %temp_dir% package SqlPackage.CommandLine --version %sqlpackage_version% --package-directory %tools_dir% 

RMDIR /Q/S %temp_dir%

dotnet build %db_project%

"%tools_dir%/sqlpackage.commandline/%sqlpackage_version%/tools/SqlPackage.exe" /Action:Publish /SourceFile:"%db_project%\bin\Debug\%db_project%.dacpac" /TargetServerName:. /TargetDatabaseName:SFA.DAS.EmployerIncentives.Database
