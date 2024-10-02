Steps needed in order to run this locally
- In order to keep sensitive data out of github, this project takes advantage of the .NET User Secrets functionality, so one will need to be created for this project and a ConnectionStrings/DefaultConnection needs to be added
- Once a database connection string has been created and is pointing to a valid Sql Server instance, navigate to the UserGroupSite.Web directory and run "dotnet ef database update -p ../UserGroupSite.Data/UserGroupSite.Data.csproj"
- From there you should be able to run "dotnet watch -lp https" and that will fire up the application utilizing the "Hot Reload" features
