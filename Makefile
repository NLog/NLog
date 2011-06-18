# 
# Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
# 
# All rights reserved.
# 
# Redistribution and use in source and binary forms, with or without 
# modification, are permitted provided that the following conditions 
# are met:
# 
# * Redistributions of source code must retain the above copyright notice, 
#   this list of conditions and the following disclaimer. 
# 
# * Redistributions in binary form must reproduce the above copyright notice,
#   this list of conditions and the following disclaimer in the documentation
#   and/or other materials provided with the distribution. 
# 
# * Neither the name of Jaroslaw Kowalski nor the names of its 
#   contributors may be used to endorse or promote products derived from this
#   software without specific prior written permission. 
# 
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
# AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
# IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
# ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
# LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
# CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
# SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
# INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
# CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
# ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
# THE POSSIBILITY OF SUCH DAMAGE.
# 

CONFIG=Debug
TOOLS=build/bin/Tools
OUTPUT_DIR=build/bin/$(CONFIG)/Mono\ 2.x
MONO_LIB_DIR=$(shell pkg-config --variable=libdir mono)/mono/4.0
XBUILD=xbuild /nologo

buildnlog:
	$(XBUILD) src/NLog.Extended/NLog.Extended.monodevelop.csproj /p:Configuration=$(CONFIG)

buildtests:
	$(XBUILD) tests/NLog.UnitTests/NLog.UnitTests.monodevelop.csproj /p:Configuration=$(CONFIG)  

makexsdtool:
	$(XBUILD) tools/MakeNLogXSD/MakeNLogXSD.csproj /p:Configuration=$(CONFIG)  

dumpapitool:
	$(XBUILD) tools/DumpApiXml/DumpApiXml.csproj /p:Configuration=$(CONFIG)  

mergeapitool:
	$(XBUILD) tools/MergeApiXml/MergeApiXml.csproj /p:Configuration=$(CONFIG)  

builddocpagestool:
	$(XBUILD) tools/BuildDocPages/BuildDocPages.csproj /p:Configuration=$(CONFIG)  

syncprojectitemstool:
	$(XBUILD) tools/SyncProjectItems/SyncProjectItems.csproj /p:Configuration=$(CONFIG)  

syncprojectitems: syncprojectitemstool
	(cd src/NLog && mono ../../build/bin/Tools/SyncProjectItems.exe ProjectFileInfo.xml)
	(cd src/NLog.Extended && mono ../../build/bin/Tools/SyncProjectItems.exe ProjectFileInfo.xml)
	(cd tests/NLog.UnitTests && mono ../../build/bin/Tools/SyncProjectItems.exe ProjectFileInfo.xml)
	(cd tests/SampleExtensions && mono ../../build/bin/Tools/SyncProjectItems.exe ProjectFileInfo.xml)

dumpapi: dumpapitool mergeapitool buildnlog
	(cd $(OUTPUT_DIR) && mono ../../Tools/DumpApiXml.exe -comments NLog.xml -assembly NLog.dll -assembly NLog.Extended.dll -ref $(MONO_LIB_DIR) -output API/NLog.api)
	(mono build/bin/Tools/MergeApiXml.exe "build/bin/$(CONFIG)")

builddocpages: builddocpagestool
	(cd build/bin/$(CONFIG) && mono ../Tools/BuildDocPages.exe "NLogMerged.api.xml" "../../../tools/WebsiteFiles/style.xsl" "Website/generated" "../../.." html web)
 
xsd: makexsdtool
	(cd $(OUTPUT_DIR) && mono ../../Tools/MakeNLogXSD.exe -api API/NLog.api -out NLog.mono2.xsd -xmlns http://www.nlog-project.org/schemas/NLog.mono2.xsd)

runtests: buildtests
	(cd $(OUTPUT_DIR) && nunit-console -labels NLog.UnitTests.dll)
