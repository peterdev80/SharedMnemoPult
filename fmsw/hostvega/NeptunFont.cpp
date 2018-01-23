#include <Windows.h>
#include <tchar.h>

extern HMODULE ThisModule;
#include "resource.h"

static HRSRC FontResource = NULL;
static HGLOBAL FontGlobal = NULL;
static HANDLE FontHandle = NULL;

void LoadNeptunFont()
{
	if (ThisModule == NULL)
		return;

	FontResource = FindResource(ThisModule, MAKEINTRESOURCE(IDR_FONT_NEPTUN), _T("RC_DATA"));
	FontGlobal = LoadResource(ThisModule, FontResource);
	
	auto lpfont = LockResource(FontGlobal);
	auto fontsize = SizeofResource(ThisModule, FontResource);

	DWORD pc = 0;
	FontHandle = AddFontMemResourceEx(lpfont, fontsize, 0, &pc);
}

void UnloadNeptunFont()
{
	if (FontHandle == NULL || FontHandle == INVALID_HANDLE_VALUE)
		return;

	RemoveFontMemResourceEx(FontHandle);
	FreeResource(FontResource);

	FontHandle = NULL;
}