#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="timeutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//  Time helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------


#ifdef __cplusplus
extern "C" {
#endif

HRESULT DAPI TimeFromString(
    __in_z LPCWSTR wzTime,
    __out FILETIME* pFileTime
    );

#ifdef __cplusplus
}
#endif

