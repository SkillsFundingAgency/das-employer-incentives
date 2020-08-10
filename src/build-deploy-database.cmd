SET db_project=SFA.DAS.EmployerIncentives.Database.Build

#rem install dotnet-script package (https://github.com/filipw/dotnet-script)
where /q dotnet-script
IF ERRORLEVEL 1 (dotnet tool install -g dotnet-script)

cd %db_project%
dotnet build
dotnet-script Program.csx