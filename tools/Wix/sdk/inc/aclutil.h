#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="aclutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Access Control List helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#include <aclapi.h>
#include <sddl.h>

#define ReleaseSid(x) if (x) { AclFreeSid(x); }
#define ReleaseNullSid(x) if (x) { AclFreeSid(x); x = NULL; }

#ifdef __cplusplus
extern "C" {
#endif

// structs
struct ACL_ACCESS
{
    BOOL fDenyAccess;
    DWORD dwAccessMask;

    // TODO: consider using a union
    LPCWSTR pwzAccountName;   // NOTE: the last three items in this structure are ignored if this is not NULL

    SID_IDENTIFIER_AUTHORITY sia;  // used if pwzAccountName is NULL
    BYTE nSubAuthorityCount;
    DWORD nSubAuthority[8];
};

struct ACL_ACE
{
    DWORD dwFlags;
    DWORD dwMask;
    PSID psid;
};


// functions
HRESULT DAPI AclCheckAccess(
    __in HANDLE hToken, 
    ACL_ACCESS* paa
    );
HRESULT DAPI AclCheckAdministratorAccess(
    __in HANDLE hToken
    );
HRESULT DAPI AclCheckLocalSystemAccess(
    __in HANDLE hToken
    );

HRESULT DAPI AclGetWellKnownSid(
    __in WELL_KNOWN_SID_TYPE wkst,
    __out PSID* ppsid
    );
HRESULT DAPI AclGetAccountSid(
    __in_opt LPCWSTR wzSystem,
    __in_z LPCWSTR wzAccount,
    __out PSID* ppsid
    );
HRESULT DAPI AclGetAccountSidString(
    __in_z LPCWSTR wzSystem,
    __in_z LPCWSTR wzAccount,
    __out LPWSTR* ppwzSid
    );

HRESULT DAPI AclCreateDacl(
    __in_ecount(cDeny) ACL_ACE rgaaDeny[],
    __in DWORD cDeny,
    __in_ecount(cAllow) ACL_ACE rgaaAllow[],
    __in DWORD cAllow,
    __out ACL** ppAcl
    );
HRESULT DAPI AclAddToDacl(
    __in ACL* pAcl,
    __in_ecount_opt(cDeny) const ACL_ACE rgaaDeny[],
    __in DWORD cDeny,
    __in_ecount_opt(cAllow) const ACL_ACE rgaaAllow[],
    __in DWORD cAllow,
    __out ACL** ppAclNew
    );
HRESULT DAPI AclMergeDacls(
    __in const ACL* pAcl1,
    __in const ACL* pAcl2,
    __out ACL** ppAclNew
    );
HRESULT DAPI AclCreateDaclOld(
    __in_ecount(cAclAccesses) ACL_ACCESS* paa,
    __in DWORD cAclAccesses,
    __out ACL** ppAcl
    );
HRESULT DAPI AclCreateSecurityDescriptor(
    __in_ecount(cAclAccesses) ACL_ACCESS* paa,
    __in DWORD cAclAccesses,
    __out SECURITY_DESCRIPTOR** ppsd
    );
HRESULT DAPI AclCreateSecurityDescriptorFromDacl(
    __in ACL* pACL,
    __out SECURITY_DESCRIPTOR** ppsd
    );
HRESULT __cdecl AclCreateSecurityDescriptorFromString(
    __out SECURITY_DESCRIPTOR** ppsd,
    __in_z LPCWSTR wzSddlFormat,
    ...
    );
HRESULT DAPI AclDuplicateSecurityDescriptor(
    __in SECURITY_DESCRIPTOR* psd,
    __out SECURITY_DESCRIPTOR** ppsd
    );
HRESULT DAPI AclGetSecurityDescriptor(
    __in_z LPCWSTR wzObject,
    __in SE_OBJECT_TYPE sot,
    __out SECURITY_DESCRIPTOR** ppsd
    );

HRESULT DAPI AclFreeSid(
    __in PSID psid
    );
HRESULT DAPI AclFreeDacl(
    __in ACL* pACL
    );
HRESULT DAPI AclFreeSecurityDescriptor(
    __in SECURITY_DESCRIPTOR* psd
    );

#ifdef __cplusplus
}
#endif
