using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Nektra.Deviare2;


namespace SQLSvrIntercept
{
    internal class HookEngine
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);

        const string SqlServerBin = "SQLSERVR.EXE";

        private NktSpyMgr _spyMgr;
        private NktProcess _sqlServerProcess;
        private IntPtr _RVA_SQLSource_Execute;
        private NktHook _functionHook;

        public HookEngine()
        {
            _spyMgr = new Nektra.Deviare2.NktSpyMgr();            
        }

        public void Initialize()
        {
            if (_spyMgr.Initialize() != 0)
            {
                throw new SpyMgrInitializationException();
            }
        }

        public void FindSqlService()
        {
            NktProcessesEnum pEnum = _spyMgr.Processes();
            _sqlServerProcess = pEnum.GetByName("sqlservr.exe");

            if (_sqlServerProcess == null)
            {
                throw new SqlServiceNotFoundException();
            }
        }

        public void LoadSymbolTable()
        {
            string path = System.IO.Path.GetDirectoryName(_sqlServerProcess.Path);

            NktTools nktt = new NktTools();

            NktPdbFunctionSymbol pdbSym_exec = nktt.LocateFunctionSymbolInPdb(path + @"\SQLLANG.DLL",
                "CSQLSource::Execute",
                @"http://msdl.microsoft.com/download/symbols",
                @"C:\symbols");

            if (pdbSym_exec == null)
            {
                throw new SymbolNotFoundException();
            }
            else
            {
                _RVA_SQLSource_Execute = pdbSym_exec.AddrOffset;
            }
        }

        public void Hook()
        {
            IntPtr EA = (IntPtr) new IntPtr(_sqlServerProcess.ModuleByName("sqllang.dll").BaseAddress.ToInt64() + _RVA_SQLSource_Execute.ToInt64());
            _functionHook = _spyMgr.CreateHookForAddress(EA, "CSQLSource::Execute", (int) eNktHookFlags.flgOnlyPreCall);

            string dllFileSpec = AppDomain.CurrentDomain.BaseDirectory + "NativePlugin.dll";
            _functionHook.AddCustomHandler(dllFileSpec, 0, "");

            if (_functionHook != null)
            {
                _functionHook.Attach(_sqlServerProcess);
                _functionHook.Hook();            }
            else
            {
                throw new HookException();
            }
        }
    }
}
