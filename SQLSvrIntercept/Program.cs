using System;
using Nektra.Deviare2;

namespace SQLSvrIntercept
{
    class Program
    {
        static bool _blockQuery = false;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.WriteLine("                                   ");
            Console.WriteLine("  SQL Server Interception Utility  ");
            Console.WriteLine("  2013 Nektra S.A -- Version 1.1   ");
            Console.WriteLine("                                   ");
            Console.BackgroundColor = ConsoleColor.Black;

            Console.ForegroundColor = ConsoleColor.Gray;

            if (args.Length == 0)
            {
                _blockQuery = false;
            }
            else if (args.Length == 1)
            {
                if (args[0].ToLower() == "-a")
                {
                    _blockQuery = true;
                    
                    
                }
                else
                {
                    DisplayUsage();
                }
            }
            else
            {
                DisplayUsage();
            }

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nQuery abort behavior is {0}\n", _blockQuery ? "ON" : "OFF");
            Console.ForegroundColor = ConsoleColor.Gray;


            try
            {
                HookEngine hookEngine = new HookEngine(_blockQuery);
                PipeServer pipeServer = new PipeServer(_blockQuery);

                Console.WriteLine("Initializing hooking engine...");
                hookEngine.Initialize();

                Console.WriteLine("Creating pipe client...");
                pipeServer.Run();

                Console.WriteLine("Finding SQLSERVR service...");
                hookEngine.FindSqlService();

                Console.WriteLine("Loading symbol table...");
                hookEngine.LoadSymbolTable();

                Console.WriteLine("Hooking function...");
                hookEngine.Hook();

                Console.WriteLine("Ready.");
                Console.ReadKey();

                Console.WriteLine("Exiting...");

            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has occurred: {0} at {1} in {2} \n\n({3})\n\n{4}", e.InnerException,
                    e.TargetSite, e.Source, e.Message, e.StackTrace);
            }
        }

        private static void DisplayUsage()
        {
            Console.WriteLine("Syntax error. Available options:");
            Console.WriteLine("-a\t\tAbort queries, and returns success state to client");
            Environment.Exit(0);

        }
    }
}
