#ifndef SQLLANG_IF_H
#define SQLLANG_IF_H

//
// Raw interfaces for SQLLANG library
//

__interface CSQLStrings_vtable
{
	virtual long PushSqlScope(void) = 0;
	virtual long PopSqlScope(void) = 0;	
	virtual long SetError(void) = 0;
	virtual long Compile(void*, void*,bool) = 0;
	virtual long FGetOwnerShipInfo(unsigned long*, unsigned long*, void*, unsigned long*, long*,bool*) = 0;
	virtual long GetSqlHandle(void*, int) = 0;
	virtual long GetPlanHandle(void* ) = 0;
	virtual long FEncryptedLevel(void) = 0;
	virtual long PwsProcNameForError(unsigned short *) = 0;
	virtual long GetStatementData(void*) = 0;
	virtual long GetStatementAndFingerPrintData(void* *,unsigned __int64 *,unsigned __int64 *,void *, void *, int) = 0;
	virtual long GetFingerprintDataUnsafe(unsigned __int64 *,unsigned __int64 *)= 0;
	virtual long PropogateDbPushedToPrevLvl(void) = 0;
	virtual long PropogateDbPushedFromPrevLvl(void) = 0;
	virtual long NotifyThatDbWasInvalidated(unsigned long) = 0;
	virtual long _gc_sql(unsigned int) = 0;
	virtual long FError(void) = 0;
	virtual long Execute(void*,void*,unsigned long) = 0;
	virtual long SetInDbccInputBuffer(void) = 0;
	virtual long DbIdTargGet(void) = 0;
	virtual long NotifyDbMgrBeforeCompile(void) = 0;
	virtual long CleanupCompileXactState(void) = 0;
	virtual long SwitchDbsIfNecessaryOnEntry(bool) = 0;
	virtual long CallAfterCompileOnBatchRecompile(void) = 0;
	virtual long PrintLevelInfo(void) = 0;
	virtual long SetupForLookup(void const *,void const *,bool) = 0;
	virtual long DoCacheLookup(void*,bool,void *) = 0;
	virtual long FPostCacheLookup(void*,void *) = 0;
	virtual long OnTransformComplete(void*,bool) = 0;
	virtual long CbGetChars(wchar_t *,int) = 0;
	virtual long CbMoveTo(int) = 0;
	virtual long EssPrepRecompile(void*, void*) = 0;
    virtual long InsertIntoCache(void*) = 0;
	virtual long ResetStateToInitial(void*) = 0;
	virtual long ResetCharsImp(void) = 0;
	virtual long CbSize(void) = 0;
	virtual long PtrGetCmd(void *,bool) = 0;
	virtual long FUnivParam(void)  = 0;
	virtual long FTemplatesPresent(void) = 0;
	virtual long PiMedObject(void) = 0;
	virtual long MDObjType(void) = 0;

};

#endif