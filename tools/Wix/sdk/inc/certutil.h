#pragma once
//-------------------------------------------------------------------------------------------------
// <copyright file="certutil.h" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// 
// <summary>
//    Certificate helper functions.
// </summary>
//-------------------------------------------------------------------------------------------------

#ifdef __cplusplus
extern "C" {
#endif

HRESULT DAPI CertReadProperty(
    __in PCCERT_CONTEXT pCertContext,
    __in DWORD dwProperty,
    __deref_out_bound LPVOID pvValue
    );

HRESULT DAPI GetCryptProvFromCert(
      __in_opt HWND hwnd,
      __in PCCERT_CONTEXT pCert,
      __out HCRYPTPROV *phCryptProv,
      __out DWORD *pdwKeySpec,
      __in BOOL *pfDidCryptAcquire,
      __deref_opt_out LPWSTR *ppwszTmpContainer,
      __deref_opt_out LPWSTR *ppwszProviderName,
      __out DWORD *pdwProviderType
      );

HRESULT DAPI FreeCryptProvFromCert(
    __in BOOL fAcquired,
    __in HCRYPTPROV hProv,
    __in_opt LPWSTR pwszCapiProvider,
    __in DWORD dwProviderType,
    __in_opt LPWSTR pwszTmpContainer
    );

HRESULT DAPI GetProvSecurityDesc(
      __in HCRYPTPROV hProv, 
      __deref_out SECURITY_DESCRIPTOR** pSecurity
      );

HRESULT DAPI SetProvSecurityDesc(
    __in HCRYPTPROV hProv,
    __in SECURITY_DESCRIPTOR* pSecurity
    );

#ifdef __cplusplus
}
#endif
