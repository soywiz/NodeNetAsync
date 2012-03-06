﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NodeNetAsync.Utils;

namespace NodeNetAsync.Net
{
	public class TcpSocket
	{
		TcpClient TcpClient;
		NetworkStream Stream;

		public TcpSocket(string Host, int Port)
		{
			this.TcpClient = new TcpClient(Host, Port);
			TcpClient.NoDelay = true;
			_Init();
		}

		internal TcpSocket(TcpClient TcpClient)
		{
			this.TcpClient = TcpClient;
			_Init();
		}

		private void _Init()
		{
			TcpClient.ReceiveBufferSize = 1024;
			TcpClient.SendBufferSize = 1024;
			TcpClient.NoDelay = true;
			this.Stream = TcpClient.GetStream();
			//this.Stream = TcpClient.GetStream();
		}

		byte[] TempBuffer = new byte[1024];
		ByteRingBuffer RingBuffer = new ByteRingBuffer(1024);

		async private Task FillBuffer(int MinimumSize)
		{
			MinimumSize = Math.Min(MinimumSize, RingBuffer.Size);
			while (RingBuffer.AvailableForRead < MinimumSize)
			{
				if (RingBuffer.AvailableForWrite > 0)
				{
					//if (TcpClient.Available > 0)
					{
						int ToRead = Math.Min(RingBuffer.AvailableForWrite, TcpClient.Available);
						int Readed = await Stream.ReadAsync(TempBuffer, 0, ToRead);
						RingBuffer.Write(TempBuffer, 0, Readed);
					}
				}
			}
		}

		async public Task FlushAsync()
		{
			await this.Stream.FlushAsync();
		}

		async public Task CloseAsync()
		{
			//await this.StreamWriter.FlushAsync();
			await FlushAsync();
			this.TcpClient.Close();
		}

		async public Task<string> ReadLineAsync(Encoding Encoding)
		{
			var String = Encoding.GetString(await ReadLineAsByteArrayAsync());
			//Console.WriteLine(String);
			return String;
		}

		async public Task<byte[]> ReadLineAsByteArrayAsync()
		{
			var Data = (await ReadLineAsMemoryStreamAsync());
			var Return = new byte[Data.Length];
			Array.Copy(Data.GetBuffer(), 0, Return, 0, Return.Length);
			return Return;
		}

		async public Task<MemoryStream> ReadLineAsMemoryStreamAsync()
		{
			var Return = new MemoryStream();
			bool Found = false;
			do
			{
				await FillBuffer(1);
				int Readed = RingBuffer.Peek(TempBuffer, 0, RingBuffer.AvailableForRead);
				//Console.WriteLine(Readed);
				for (int n = 0; n < Readed; n++)
				{
					byte c = TempBuffer[n];
					if (c == '\r' || c == '\n')
					{
						Readed = n;
						Found = true;
						break;
					}
				}
				//Console.WriteLine(Readed);
				if (Readed > 0)
				{
					Return.Write(TempBuffer, 0, Readed);
					RingBuffer.Skip(Readed);
				}
			} while (!Found);

			await FillBuffer(1);
			if (RingBuffer.Peek(TempBuffer, 0, 2) >= 2)
			{
				if (TempBuffer[0] == '\r' && TempBuffer[1] == '\n')
				{
					//Return.Write(TempBuffer, 0, 2);
					RingBuffer.Skip(2);
					return Return;
				}
			}
			//Return.Write(TempBuffer, 0, 1);
			RingBuffer.Skip(1);
			return Return;
		}

		async public Task<int> ReadAsync(byte[] Buffer, int Offset = 0, int Count = -1)
		{
			if (Count < 0) Count = Buffer.Length;
			int Readed = 0;
			while (Count > 0)
			{
				await FillBuffer(1);
				int ToReadStep = Math.Min(Count, RingBuffer.AvailableForRead);
				int ReadedStep = RingBuffer.Read(Buffer, Offset, ToReadStep);
				Offset += ReadedStep;
				Readed += ReadedStep;
				Count -= ReadedStep;
			}
			return Readed;
		}

		async public Task WriteAsync(byte[] Buffer, int Offset = 0, int Count = -1)
		{
			if (Count == -1) Count = Buffer.Length;
			await Stream.WriteAsync(Buffer, Offset, Count);
		}

		async public Task WriteAsync(string Text, Encoding Encoding)
		{
			await WriteAsync(Encoding.GetBytes(Text));
		}
	}
}
