Task Manager written in C#

To run you need to create PostgreSQL database "<b>taskmanager</b>" then run
<pre>
$ cd backend
$ dotnet ef migrations add Initial -p TaskManager.Infrastructure -s TaskManager.WebApi
$ dotnet ef database update -p TaskManager.WebApi  
</pre>

Then open appsettings.json and specify google client id, secret and jwt security key
<pre>
"GoogleOAuthOptions": {
    "ClientId": "*",
    "ClientSecret": "*"
  },

  "JwtSettings": {
    "Key": "*",
    ...
</pre>

Then start web app

<pre>
$ cd backend
$ dotnet run --project TaskManager.WebApi
</pre>