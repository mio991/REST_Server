using System;

namespace mio991.REST.Server
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			try
			{
				Server.Init(Environment.CurrentDirectory);

				Server.Start();

				bool runing = true;

				while(runing)
				{
					string command = Console.ReadLine();
					switch(command)
					{
					case "shutdown":
					case "exit":
						runing = false;
						Server.Stop();
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
