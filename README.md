# MedicalReportDashboardAPI

Update appsettings.json = Change connection string.

Deleter Migrations folder.

Open Terminal in Visual Studio and Execute below commands:
dotnet tool install --global dotnet-ef
dotnet ef migrations add Init
dotnet ef database update
