#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="perfutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Performance helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

// structs


// functions
void DAPI PerfInitialize(
    );
void DAPI PerfClickTime(
    __out_opt LARGE_INTEGER* pliElapsed
    );
double DAPI PerfConvertToSeconds(
    __in const LARGE_INTEGER* pli
    );

#ifdef __cplusplus
}
#endif
