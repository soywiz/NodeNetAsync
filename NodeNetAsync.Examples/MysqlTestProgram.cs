﻿#if false
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNetAsync.Db.Mysql;
using NodeNetAsync.Net.Http;
using NodeNetAsync.Net.Http.Router;
using NodeNetAsync.Utils;
using NodeNetAsync.Json;
using NodeNetAsync.Net.Http.Static;

namespace NodeNetAsync.Examples
{
	class MysqlTestProgram
	{
		static void Main(string[] args)
		{
			Core.Loop(async () =>
			{
				var Server = new HttpServer();
				var Router = new HttpRouter();

				Router.AddRoute("/test", async (Request, Response) =>
				{
					Response.Buffering = true;

					Response.SetHttpCode(HttpCode.OK_200);
					Response.Headers["Content-Type"] = "application/json";

					var MysqlClient = new MysqlClient("FEDORADEV", User: "test", Password: "test");
					await MysqlClient.ConnectAsync();

					foreach (var Row in await MysqlClient.QueryAsync("SELECT 1 as 'k1', 2 as 'k2', 3 * 999, 'test', 1 as 'Ok';"))
					{
						await Response.WriteAsync(Row.ToJsonString());
					}

					await MysqlClient.CloseAsync();
				});

				// Default file serving
				Router.SetDefaultRoute(new HttpStaticFileServer(Path: @"C:\projects\csharp\NodeNet\static", Cache: true));

				Server.AddFilterLast(Router);
				await Server.ListenAsync(80, "127.0.0.1");
			});
		}
	}
}
#endif