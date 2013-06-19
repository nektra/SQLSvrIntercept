using System;
using Nektra.Deviare2;

namespace SQLSvrIntercept
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SQL Server Interception Utility");
            Console.WriteLine("2013 Nektra SA");
            Console.WriteLine();

            HookEngine hookEngine = new HookEngine();
            PipeServer pipeServer = new PipeServer();

            try
            {
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
                
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception has occurred: {0} at {1} in {2} \n\n({3})\n\n{4}", e.InnerException, 
                    e.TargetSite, e.Source, e.Message, e.StackTrace);
            }  
        }
    }
}
