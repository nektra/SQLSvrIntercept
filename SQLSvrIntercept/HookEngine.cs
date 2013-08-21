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

        private const int MAX_CCH_LICENSE = 8192;

        private NktSpyMgr _spyMgr;
        private NktProcess _sqlServerProcess;
        private IntPtr _RVA_SQLSource_Execute;
        private NktHook _functionHook;
        private bool _blockQuery;

        public HookEngine(bool blockQuery)
        {
            _blockQuery = blockQuery;

            _spyMgr = new Nektra.Deviare2.NktSpyMgr();
            string asmPath = AppDomain.CurrentDomain.BaseDirectory;
            string licFullPath = asmPath + "\\license.txt";

            Console.WriteLine("--- Checking {0}...", licFullPath);

            string licKey = "";
            if (!LoadLicenseKey(licFullPath, ref licKey))
            {
                Console.WriteLine("(!) Cannot load license from file.");
            }
            else
            {
                Console.WriteLine("--- Using Deviare license file.");
                _spyMgr.LicenseKey = licKey;            
            }
                      
            Console.WriteLine("--- Setting database path to {0}", asmPath);
            _spyMgr.DatabasePath = asmPath;

            if ( System.IO.File.Exists(asmPath + "\\deviare32.db") == false
                || System.IO.File.Exists(asmPath + "\\deviare64.db") == false)
            {
                throw new DeviareDBNotFoundException();
            }

        }

        private bool LoadLicenseKey(string file, ref string licKey)
        {
            try
            {
                using (System.IO.FileStream fs = System.IO.File.OpenRead(file))
                {
                    byte[] ba = new byte[MAX_CCH_LICENSE];
                    fs.Read(ba, 0, ba.Length);

                    licKey = System.Text.Encoding.UTF8.GetString(ba);
                }               

            }
            catch
            {
                return false;
            }

            return true;           
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

            Console.WriteLine("--- DLL path: {0}", path + @"\sqllang.dll");

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

                Console.WriteLine("--- SQLSource::Execute address offset: {0:x}", _RVA_SQLSource_Execute);
            }
        }

        public void Hook()
        {
            IntPtr EA = (IntPtr) new IntPtr(_sqlServerProcess.ModuleByName("sqllang.dll").BaseAddress.ToInt64() + _RVA_SQLSource_Execute.ToInt64());
            _functionHook = _spyMgr.CreateHookForAddress(EA, "CSQLSource_Execute", (int) eNktHookFlags.flgOnlyPreCall);

            string dllFileSpec = AppDomain.CurrentDomain.BaseDirectory + "NativePlugin.dll";
            _functionHook.AddCustomHandler(dllFileSpec, 0, _blockQuery ? "1" : "");
            _functionHook.OnStateChanged += _functionHook_OnStateChanged;

            Console.WriteLine("--- Registering custom handler DLL: {0}", dllFileSpec);

            if (_functionHook != null)
            {
                _functionHook.Attach(_sqlServerProcess, true);
                _functionHook.Hook(true);            
            }
            else
            {
                throw new HookException();
            }
        }

        void _functionHook_OnStateChanged(NktHook Hook, NktProcess proc, eNktHookState newState, eNktHookState oldState)
        {
            Console.WriteLine("--- Hook state changed {0} from {1} to {2}", Hook.FunctionName, 
                oldState.ToString(), newState.ToString());

            if (newState == eNktHookState.stRemoved)
            {
                Environment.Exit(0);                
            }
        }
    }
}
