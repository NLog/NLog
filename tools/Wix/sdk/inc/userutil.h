#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="userutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    User helper funtions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

HRESULT DAPI UserBuildDomainUserName(
    __out_ecount_z(cchDest) LPWSTR wzDest,
    __in int cchDest,
    __in_z LPCWSTR pwzName,
    __in_z LPCWSTR pwzDomain
    );

HRESULT DAPI UserCheckIsMember(
    __in_z LPCWSTR pwzName,
    __in_z LPCWSTR pwzDomain,
    __in_z LPCWSTR pwzGroupName,
    __in_z LPCWSTR pwzGroupDomain,
    __out LPBOOL lpfMember
    );

HRESULT DAPI UserCreateADsPath(
    __in_z LPCWSTR wzObjectDomain, 
    __in_z LPCWSTR wzObjectName,
    __out BSTR *pbstrAdsPath
    );

#ifdef __cplusplus
}
#endif
