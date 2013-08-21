using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Text;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace SQLSvrIntercept
{
    internal class PipeServer
    {
        const int MAX_STRING_CCH = 1024;
      
        private NamedPipeServerStream _pipeServer;
        private Thread _pipeThread;
        private bool _blockQuery;

        public PipeServer(bool blockQuery)
        {
            _blockQuery = blockQuery;

        }

        public void Run()
        {
            try
            {
                _pipeThread = new Thread(PipeThreadStart);
                _pipeThread.IsBackground = true;
                _pipeThread.Start();
            }
            catch
            {
                throw new PipeThreadRunException();
            }
        }

        private void PipeThreadStart()
        {
            PipeSecurity pSec = new PipeSecurity();
            PipeAccessRule pAccRule = new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null)
            , PipeAccessRights.ReadWrite | PipeAccessRights.Synchronize, System.Security.AccessControl.AccessControlType.Allow);
            pSec.AddAccessRule(pAccRule);

            using (_pipeServer = new NamedPipeServerStream("NKT_SQLINTERCEPT_PIPE",
                PipeDirection.InOut, 
                NamedPipeServerStream.MaxAllowedServerInstances, 
                PipeTransmissionMode.Byte,
                PipeOptions.None, 
                MAX_STRING_CCH * 2,
                MAX_STRING_CCH * 2,
                pSec,          
                HandleInheritability.Inheritable))
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("(pipe thread) Waiting for connection...");
                Console.ForegroundColor = ConsoleColor.Gray;

                try
                {
                    _pipeServer.WaitForConnection();

                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("(pipe thread) Client connected.");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    while (true)
                    {
                        byte[] readBuf = new byte[MAX_STRING_CCH * 2];

                        int cbRead = _pipeServer.Read(readBuf, 0, MAX_STRING_CCH * 2);

                        string str = Encoding.Unicode.GetString(readBuf, 0, cbRead);

                        Console.WriteLine(str);
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("--------------------------------------------------------");
                        Console.ForegroundColor = ConsoleColor.Gray;

                        if (_blockQuery)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("(pipe thread) QUERY ABORTED");
                            Console.ForegroundColor = ConsoleColor.Gray;
                        }
                        
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine("(pipethread) Pipe or data marshaling operation exception! ({0})", ex.Message);
                }
            }
        }
    }
        
}