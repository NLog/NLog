#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="thmutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//  Theme helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

enum THEME_CONTROL_DATA
{
    THEME_CONTROL_DATA_HOVER = 1,
};

enum THEME_CONTROL_TYPE
{
    THEME_CONTROL_TYPE_UNKNOWN,
    THEME_CONTROL_TYPE_BUTTON,
    THEME_CONTROL_TYPE_CHECKBOX,
    THEME_CONTROL_TYPE_EDITBOX,
    THEME_CONTROL_TYPE_HYPERLINK,
    THEME_CONTROL_TYPE_IMAGE,
    THEME_CONTROL_TYPE_PROGRESSBAR,
    THEME_CONTROL_TYPE_RICHEDIT,
    THEME_CONTROL_TYPE_STATIC,
    THEME_CONTROL_TYPE_TEXT,
};

struct THEME_CONTROL
{
    THEME_CONTROL_TYPE type;

    LPWSTR wzText;
    int nX;
    int nY;
    int nHeight;
    int nWidth;
    int nSourceX;
    int nSourceY;

    DWORD dwStyle;
    DWORD dwFontId;
    DWORD dwFontHoverId;
    DWORD dwFontSelectedId;
    HWND hWnd;
};


struct THEME_FONT
{
    HFONT hFont;
    COLORREF crForeground;
    HBRUSH hForeground;
    COLORREF crBackground;
    HBRUSH hBackground;
};


struct THEME
{
    DWORD dwStyle;
    DWORD dwFontId;
    HANDLE hIcon;
    LPWSTR wzCaption;
    int nHeight;
    int nWidth;
    int nSourceX;
    int nSourceY;

    HBITMAP hImage;

    DWORD cFonts;
    THEME_FONT* rgFonts;

    DWORD cControls;
    THEME_CONTROL* rgControls;

    // state variables that should be ignored
    HWND hwndHover; // currently 
};


HRESULT DAPI ThemeInitialize(
    __in HMODULE hModule
    );

void DAPI ThemeUninitialize();

HRESULT DAPI ThemeLoadFromFile(
    __in_z LPCWSTR wzThemeFile,
    __out THEME** ppTheme
    );

HRESULT DAPI ThemeLoadFromResource(
    __in_opt HMODULE hModule,
    __in_z LPCSTR szResource,
    __out THEME** ppTheme
    );

void DAPI ThemeFree(
    __in THEME* pTheme
    );

HRESULT DAPI ThemeLoadControls(
    __in THEME* pTheme,
    __in HWND hwndParent
    );

HRESULT DAPI ThemeDrawBackground(
    __in THEME* pTheme,
    __in PAINTSTRUCT* pps
    );

HRESULT DAPI ThemeDrawControl(
    __in THEME* pTheme,
    __in DRAWITEMSTRUCT* pdis
    );

void DAPI ThemeHoverControl(
    __in THEME* pTheme,
    __in HWND hwndParent,
    __in HWND hwndControl
    );

BOOL DAPI ThemeIsControlChecked(
    __in THEME* pTheme,
    __in DWORD dwControl
    );

BOOL DAPI ThemeSetControlColor(
    __in THEME* pTheme,
    __in HDC hdc,
    __in HWND hWnd,
    __out HBRUSH* phBackgroundBrush
    );

HRESULT DAPI ThemeSetProgressControl(
    __in THEME* pTheme,
    __in DWORD dwControl,
    __in DWORD dwProgressPercentage
    );

HRESULT DAPI ThemeSetProgressControlColor(
    __in THEME* pTheme,
    __in DWORD dwControl,
    __in DWORD dwColorIndex
    );

HRESULT DAPI ThemeSetTextControl(
    __in THEME* pTheme,
    __in DWORD dwControl,
    __in_z LPCWSTR wzText
    );

HRESULT DAPI ThemeGetTextControl(
    __in const THEME* pTheme,
    __in DWORD dwControl,
    __out LPWSTR* psczText
    );

HRESULT DAPI ThemeLoadRichEditFromFile(
    __in THEME* pTheme,
    __in DWORD dwControl,
    __in_z LPCWSTR wzFileName, 
    __in HMODULE hModule
    );

HRESULT DAPI ThemeLoadLocFromFile(
    __in THEME* pTheme,
    __in_z LPCWSTR wzFileName,
    __in HMODULE hModule
    );

#ifdef __cplusplus
}
#endif

