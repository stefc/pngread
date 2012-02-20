using System;
using System.Reflection;

namespace Tests
{
	class MainClass
	{
		[STAThread]
		static int Main(string[] args)
		{
			return NUnit.ConsoleRunner.Runner.Main(new string[] { Assembly.GetExecutingAssembly().Location });
			
			// return NUnit.Gui.AppEntry.Main(new string[] { Assembly.GetExecutingAssembly().Location });
		}
	}
}
