# 
# Copyright (c) 2004-2010 Jaroslaw Kowalski <jaak@jkowalski.net>
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

OUTPUT_DIR=build/bin/mono2
NUNIT_TEMP_TESTS=$(OUTPUT_DIR)/NLog.UnitTests.NUnit
REFERENCE_ASSEMBLIES=-r:System.Web.Services.dll -r:System.Drawing.dll -r:System.Web.dll -r:System.Data.dll -r:System.Windows.Forms.dll -r:System.Messaging.dll -r:System.Configuration.dll -r:Mono.Posix.dll -r:System.Runtime.Serialization.dll -r:System.ServiceModel.dll
DEFINES=-define:MONO_2_0 -define:MONO -define:WCF_SUPPORTED
MCS=gmcs
PERL=perl
MCS_OPTIONS= -debug+
NUNIT_CONSOLE=nunit-console2
NUNIT_OPTIONS=-nodots -labels -nologo

help:
	@echo Supported targets are:
	@echo""
	@echo "  help - displays this help"
	@echo "  build - builds NLog.dll to $(OUTPUT_DIR)"
	@echo "  buildtests - builds NLog.UnitTests.dll to $(OUTPUT_DIR)"
	@echo "  clean - removes $(OUTPUT_DIR)"
	@echo "  all - rebuilds everything and runs tests"
	@echo ""
	@echo "The following parameters can be overridden:"
	@echo""
	@echo "  OUTPUT_DIR - output directory - default '$(OUTPUT_DIR)'"
	@echo "  MCS - location of Mono gmcs compiler - default '$(MCS)'"
	@echo "  PERL - location of perl interpreter - default '$(PERL)'"
	@echo "  NUNIT_CONSOLE - location of nunit-console - default '$(NUNIT_CONSOLE)'"
	@echo "  NUNIT_OPTIONS - options to nunit-console - default '$(NUNIT_OPTIONS)'"
	@echo ""
	@echo "See the 'Makefile' for more options."

build: prepareoutputdir
	$(MCS) -t:library -out:$(OUTPUT_DIR)/NLog.dll $(DEFINES) $(MCS_OPTIONS) -recurse:src/NLog/*.cs $(REFERENCE_ASSEMBLIES) -keyfile:src/NLog.snk
	$(MCS) -t:library -out:$(OUTPUT_DIR)/NLog.Extended.dll $(DEFINES) $(MCS_OPTIONS) -recurse:src/NLog.Extended/*.cs -r:$(OUTPUT_DIR)/NLog.dll $(REFERENCE_ASSEMBLIES) -keyfile:src/NLog.snk

buildtests: sampleextensions
	rm -rf $(NUNIT_TEMP_TESTS)
	$(PERL) tools/mstest2nunit.pl tests/NLog.UnitTests $(NUNIT_TEMP_TESTS)
	$(MCS) -t:library -out:$(OUTPUT_DIR)/NLog.UnitTests.dll $(DEFINES) $(MCS_OPTIONS) -recurse:$(NUNIT_TEMP_TESTS)/*.cs $(REFERENCE_ASSEMBLIES) -r:nunit.framework.dll  -keyfile:tests/NLog.UnitTests/NLogTests.snk -r:$(OUTPUT_DIR)/NLog.dll -r:$(OUTPUT_DIR)/NLog.Extended.dll -r:$(OUTPUT_DIR)/SampleExtensions.dll -r:System.Xml.Linq.dll -r:System.Runtime.Serialization.dll -r:System.ServiceModel.dll

sampleextensions: build
	$(MCS) -t:library -out:$(OUTPUT_DIR)/SampleExtensions.dll $(DEFINES) $(MCS_OPTIONS) -recurse:tests/SampleExtensions/*.cs -r:$(OUTPUT_DIR)/NLog.dll -keyfile:tests/NLog.UnitTests/NLogTests.snk -r:System.Xml.Linq.dll -r:System.Runtime.Serialization.dll
	
runtests:
	(cd $(OUTPUT_DIR) && $(NUNIT_CONSOLE) $(NUNIT_OPTIONS) NLog.UnitTests.dll)

clean:
	rm -rf $(OUTPUT_DIR)

all: clean build buildtests runtests

prepareoutputdir:
	mkdir -p $(OUTPUT_DIR)

