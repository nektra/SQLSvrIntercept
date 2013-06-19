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

        public PipeServer()
        {

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
                Console.WriteLine("(pipe thread) Waiting for connection...");

                try
                {
                    _pipeServer.WaitForConnection();

                    while (true)
                    {
                        byte[] readBuf = new byte[MAX_STRING_CCH*2];
                        int cbRead = _pipeServer.Read(readBuf, 0, MAX_STRING_CCH*2);

                        Console.WriteLine(Encoding.Unicode.GetString(readBuf, 0, cbRead));
                        Console.WriteLine("--------------------------------------------------------");
                        
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