using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLSvrIntercept
{
    class SpyMgrInitializationException : Exception
    {

    }

    class SqlServiceNotFoundException : Exception
    {
    }

    class SymbolNotFoundException : Exception { }
    class HookException : Exception { }
    class UnknownTargetFunctionParamsException : Exception { }
    class NativeDllLoadException : Exception { }
    class PipeThreadRunException : Exception { }
    class DeviareDBNotFoundException : Exception { }

}
