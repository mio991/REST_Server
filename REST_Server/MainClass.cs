using System;

namespace mio991.REST.Server
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try
			{
				Server server = new Server(Environment.CurrentDirectory);

				server.Start();

				bool runing = true;

				while(runing)
				{
					string command = Console.ReadLine();
					switch(command)
					{
					case "shutdown":
					case "exit":
						runing = false;
						server.Dispose();
						break;
					}
				}

				Console.WriteLine("Zum beenden Taste drücken...");
				Console.ReadKey();
			}
			catch(Exception ex) {
				Console.WriteLine (ex);

				Console.WriteLine("Zum beenden Taste drücken...");
				Console.ReadKey();
			}
		}
	}
}
