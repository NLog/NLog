<?xml version="1.0"?>
<xsl:stylesheet
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  version="1.1">

  <!-- Create HxC project file for hxcomp.exe. The input can be any well formed
       XML file; it is used only to satisfy XslTransform, which needs an input
       file to run the transform. Make sure to set the fileNamePrefix. -->

  <xsl:output doctype-system="MS-Help://Hx/Resources/HelpCollection.dtd" indent="yes" encoding="utf-8" />

  <!-- $fileNamePrefix is the prefix used for all files names. -->
  <xsl:param name="fileNamePrefix">test</xsl:param>

  <xsl:template match="/">
    <HelpCollection DTDVersion="1.0" FileVersion="08.00.50720.2102" LangId="1033" Title="Common Scripts" Copyright="© 2005 Microsoft Corporation. All rights reserved.">
      <CompilerOptions OutputFile="{$fileNamePrefix}.HxS" CreateFullTextIndex="Yes" CompileResult="Hxs">
        <IncludeFile File="{$fileNamePrefix}.HxF" />
      </CompilerOptions>
      <TOCDef File="{$fileNamePrefix}.HxT" />
      <KeywordIndexDef File="{$fileNamePrefix}_A.HxK" />
      <KeywordIndexDef File="{$fileNamePrefix}_K.HxK" />
      <KeywordIndexDef File="{$fileNamePrefix}_F.HxK" />
      <KeywordIndexDef File="{$fileNamePrefix}_N.HxK" />
      <KeywordIndexDef File="{$fileNamePrefix}_S.HxK" />
      <KeywordIndexDef File="{$fileNamePrefix}_B.HxK" />
      <ItemMoniker Name="!DefaultTOC" ProgId="HxDs.HxHierarchy" InitData="AnyString" />
      <ItemMoniker Name="!DefaultFullTextSearch" ProgId="HxDs.HxFullTextSearch" InitData="AnyString" />
      <ItemMoniker Name="!DefaultAssociativeIndex" ProgId="HxDs.HxIndex" InitData="A" />
      <ItemMoniker Name="!DefaultKeywordIndex" ProgId="HxDs.HxIndex" InitData="K" />
      <ItemMoniker Name="!DefaultContextWindowIndex" ProgId="HxDs.HxIndex" InitData="F" />
      <ItemMoniker Name="!DefaultNamedUrlIndex" ProgId="HxDs.HxIndex" InitData="NamedUrl" />
      <ItemMoniker Name="!DefaultSearchWindowIndex" ProgId="HxDs.HxIndex" InitData="S" />
      <ItemMoniker Name="!DefaultDynamicLinkIndex" ProgId="HxDs.HxIndex" InitData="B" />
    </HelpCollection>
  </xsl:template>

</xsl:stylesheet>