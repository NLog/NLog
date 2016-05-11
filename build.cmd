CALL dnvm install 1.0.0-rc1-update1

CALL dnu restore --quiet
CALL dnu build src/NLog --quiet
CALL dnu build src/NLog.Extended --quiet
CALL dnu build src/NLog.Extended --quiet
CALL dnu build src/NLogAutoLoadExtension --quiet
CALL dnu build tests/SampleExtensions --quiet
CALL dnu build tests/NLog.UnitTests --quiet

call dnx -p tests\NLog.UnitTests test
