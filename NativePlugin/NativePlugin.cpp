#include "NativePlugin.h"
#import "C:\\src\\deviareengine2\\bin\\deviarecom64.dll" raw_interfaces_only, named_guids, raw_dispinterfaces, auto_rename

using namespace Deviare2;

//----------------------------------------------------------------------------
//
// Globals
//
//----------------------------------------------------------------------------
static HANDLE g_hDllInstance;
static HANDLE g_hPipe;

DWORD gRVA_SQLStrings_CbGetChars;

static BOOL g_fBlockQuery;


//----------------------------------------------------------------------------
//----------------------------------------------------------------------------

static void d_printf(const wchar_t* szFormat, ...)
{
	wchar_t wBuf[MAX_STRING_CCH];	
	va_list va;

	va_start(va, szFormat);
	vswprintf (wBuf, MAX_STRING_CCH, szFormat, va);
	va_end(va);

	OutputDebugString(wBuf);
}

static BOOL ConnectToPipeServer() 
{
	g_hPipe = CreateFile(L"\\\\.\\pipe\\NKT_SQLINTERCEPT_PIPE",
		GENERIC_READ|GENERIC_WRITE,
		0,
		NULL,
		OPEN_EXISTING,
		0,
		NULL);

	if (g_hPipe == INVALID_HANDLE_VALUE)
	{
		d_printf(L"Cannot connect to named pipe. Lasterr = 0x%x", GetLastError());
		return FALSE;
	}
	else
	{
		d_printf(L"Connected to named pipe.");
		return TRUE;
	}
}


//----------------------------------------------------------------------------
//
// DLL entry point
//
//----------------------------------------------------------------------------
LRESULT CALLBACK DllMain(_In_ HANDLE _HDllHandle, _In_ DWORD _Reason, _In_opt_ LPVOID _Reserved)
{	

	switch(_Reason)
	{
	case DLL_PROCESS_ATTACH:
		g_hDllInstance = _HDllHandle;
		break;

	case DLL_PROCESS_DETACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
		break;
	}

	return TRUE;
}

extern "C"  HRESULT WINAPI OnLoad()
{
	d_printf(__FUNCTIONW__);	
	return S_OK;
}

extern "C" HRESULT WINAPI OnUnload()
{

	d_printf(__FUNCTIONW__);
	return S_OK;
}


extern "C" HRESULT WINAPI OnHookAdded(__in INktHookInfo *lpHookInfo, __in DWORD dwChainIndex,
									  __in LPCWSTR szParametersW)
{	
	d_printf(__FUNCTIONW__);

	// If CSQL Source Execute, setup IPC mechanism
	//

	HRESULT hr = S_OK;

	CComBSTR funcName;
	lpHookInfo->get_FunctionName(&funcName);

	d_printf(L"FuncName:%s\n", funcName);

	if (funcName == L"CSQLSource_Execute")
	{
		g_fBlockQuery = FALSE;

		if (szParametersW[0] == L'1')
		{
			d_printf(L"Query abort Mode = ENABLED");
			g_fBlockQuery = TRUE;
		}

		d_printf(L"Interception hook added; connecting to pipe server");
		if (!ConnectToPipeServer())
		{
			return E_FAIL;
		}
	} 

	return S_OK;
}

extern "C" HRESULT WINAPI OnHookRemoved(__in INktHookInfo *lpHookInfo, __in DWORD dwChainIndex)
{
	d_printf(__FUNCTIONW__);
	CloseHandle(g_hPipe);
	return S_OK;
}

extern "C" HRESULT WINAPI OnFunctionCall(__in INktHookInfo *lpHookInfo, __in DWORD dwChainIndex,
										 __in INktHookCallInfoPlugin *lpHookCallInfoPlugin)
{	
	__int64 pThis = 0;
	lpHookCallInfoPlugin->get_Register(eNktRegister::asmRegRcx, &pThis);
	CSQLStrings_vtable* pSQLStrings = (CSQLStrings_vtable*) pThis;	


	if (g_hPipe)
	{
		// Get query string and transmit through pipe
		//

		wchar_t buf[MAX_STRING_CCH];
		DWORD dwBytesW;

		int cBytes = pSQLStrings->CbGetChars(buf, MAX_STRING_CCH*sizeof(wchar_t));		
		WriteFile(g_hPipe, buf, cBytes, &dwBytesW, NULL);		
	}
	else
	{
		d_printf(L"No pipe handle!");
	}

	if (g_fBlockQuery)
	{

		lpHookCallInfoPlugin->SkipCall();
		lpHookCallInfoPlugin->FilterSpyMgrEvent();
	}

	return S_OK;
}
