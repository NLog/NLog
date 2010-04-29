#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="wcawow64.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
//    
//    The use and distribution terms for this software are covered by the
//    Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
//    which can be found in the file CPL.TXT at the root of this distribution.
//    By using this software in any fashion, you are agreeing to be bound by
//    the terms of this license.
//    
//    You must not remove this notice, or any other, from this software.
// </copyright>
// 
// <summary>
//    Windows Installer XML CustomAction utility library for Wow64 API-related functionality.
// </summary>
//-------------------------------------------------------------------------------------------------

#include "wcautil.h"

#ifdef __cplusplus
extern "C" {
#endif

HRESULT WIXAPI WcaInitializeWow64();
BOOL WIXAPI WcaIsWow64Process();
BOOL WIXAPI WcaIsWow64Initialized();
HRESULT WIXAPI WcaDisableWow64FSRedirection();
HRESULT WIXAPI WcaRevertWow64FSRedirection();
HRESULT WIXAPI WcaFinalizeWow64();

#ifdef __cplusplus
}
#endif
