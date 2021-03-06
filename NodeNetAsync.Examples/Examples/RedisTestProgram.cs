﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodeNetAsync.Db.Redis;
using NodeNetAsync.Net.Http;

namespace NodeNetAsync.Examples
{
	public class RedisTestProgram
	{
		static public void Main(string[] args)
		{
			Core.Loop(async () =>
			{
				Core.SetInterval(TimeSpan.FromMilliseconds(1000), async () =>
				{
					await Console.Out.WriteLineAsync("Hello World!");
				});

				await HttpServer.Create(async (Request, Response) =>
				{
					Response.Buffering = true;

					Response.SetHttpCode(HttpCode.OK_200);
					Response.Headers["Content-Type"] = "text/plain";

					var Redis = await RedisClient.CreateAndConnectAsync(Host: "127.0.0.1");

					await Redis.SetAsync("foo", "bar");
					var Item = await Redis.GetAsync("foo");

					await Response.WriteAsync("TEST: " + Item);

					await Redis.CloseAsync();
				}).ListenAsync(80);
			});
		}
	}
}