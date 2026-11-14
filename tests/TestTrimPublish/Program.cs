//
// Copyright (c) 2004-2024 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
//
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
//
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
// * Neither the name of Jaroslaw Kowalski nor the names of its
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//

using NLog;

var consoleWriter = new StringWriter();
var orgConsole = System.Console.Out;
try
{
    System.Console.SetOut(consoleWriter);

    NLog.LogManager.ThrowExceptions = true; // unit-test-mode

    var logger = NLog.LogManager.Setup().LoadConfigurationFromXml(@"
    <nlog>
      <targets>
        <target type='console' name='console' layout='${threadid}|${message}' />
      </targets>
      <rules>
        <logger name='*' minLevel='Info' writeTo='console' />
      </rules>
    </nlog>
    ").GetCurrentClassLogger();

    logger.Debug("Almost ready");
    logger.Info("Success");
    NLog.LogManager.Shutdown();
    logger.Debug("Almost done");
}
catch (Exception ex)
{
    System.Console.SetOut(orgConsole);
    Console.WriteLine(ex.ToString());
    throw;
}
finally
{
    System.Console.SetOut(orgConsole);
}

var result = consoleWriter.ToString();

Console.WriteLine(result);

var processPath = Environment.ProcessPath ?? string.Empty;
Console.WriteLine($"ProcessPath: '{processPath}' with FileSize: {new FileInfo(processPath).Length}");

if (result == $"{Environment.CurrentManagedThreadId}|Success{System.Environment.NewLine}")
    return 0;
else
    return -1;
