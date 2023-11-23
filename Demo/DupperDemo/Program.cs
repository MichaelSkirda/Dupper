using Dupper;
using DupperDemo;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
string connectionString = "Host = 192.168.0.1; Port = 5432; User ID = postgres; Password = <MyStrongPassword>; Database = dupper_demo; Pooling = true;";

var dupper = new DupperProvider(connectionString, (connString) => new NpgsqlConnection(connString));
var dbProvider = new NpgsqlDbConnectionProvider(connectionString);

services.AddSingleton<IDupperProvider>(dupper);
services.AddSingleton<IDbConnectionProvider>(dbProvider);

services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
