#! /bin/bash
PLATFORM=$1
TYPE=$2
COVERAGE=$3
WHERE="Category!=ManualTest"
TEST_PATTERN="*Test.dll"
FILES=( "Lidarr.Api.Test.dll" "Lidarr.Automation.Test.dll" "Lidarr.Common.Test.dll" "Lidarr.Core.Test.dll" "Lidarr.Host.Test.dll" "Lidarr.Integration.Test.dll" "Lidarr.Libraries.Test.dll" "Lidarr.Mono.Test.dll" "Lidarr.Update.Test.dll" "Lidarr.Windows.Test.dll" )
ASSMEBLIES=""
TEST_LOG_FILE="TestLog.txt"

echo "test dir: $TEST_DIR"
if [ -z "$TEST_DIR" ]; then
    TEST_DIR="."
fi

if [ -d "$TEST_DIR/_tests" ]; then
  TEST_DIR="$TEST_DIR/_tests"
fi

rm -f "$TEST_LOG_FILE"

# Uncomment to log test output to a file instead of the console
export LIDARR_TESTS_LOG_OUTPUT="File"

VSTEST_PARAMS="--Platform:x64 --logger:nunit;LogFilePath=TestResult.xml"

if [ "$PLATFORM" = "Mac" ]; then

  export DYLD_FALLBACK_LIBRARY_PATH="$TEST_DIR:$MONOPREFIX/lib:/usr/local/lib:/lib:/usr/lib"
  echo $DYLD_FALLBACK_LIBRARY_PATH
  mono --version

  # To debug which libraries are being loaded:
  # export DYLD_PRINT_LIBRARIES=YES
fi

if [ "$PLATFORM" = "Windows" ]; then
  mkdir -p "$ProgramData/Lidarr"
  WHERE="$WHERE&Category!=LINUX"
elif [ "$PLATFORM" = "Linux" ] || [ "$PLATFORM" = "Mac" ] ; then
  mkdir -p ~/.config/Lidarr
  WHERE="$WHERE&Category!=WINDOWS"
else
  echo "Platform must be provided as first arguement: Windows, Linux or Mac"
  exit 1
fi

if [ "$TYPE" = "Unit" ]; then
  WHERE="$WHERE&Category!=IntegrationTest&Category!=AutomationTest"
elif [ "$TYPE" = "Integration" ] || [ "$TYPE" = "int" ] ; then
  WHERE="$WHERE&Category=IntegrationTest"
elif [ "$TYPE" = "Automation" ] ; then
  WHERE="$WHERE&Category=AutomationTest"
else
  echo "Type must be provided as second argument: Unit, Integration or Automation"
  exit 2
fi

for i in "${FILES[@]}";
  do ASSEMBLIES="$ASSEMBLIES $TEST_DIR/$i"
done

DOTNET_PARAMS="$ASSEMBLIES --TestCaseFilter:$WHERE $VSTEST_PARAMS"

if [ "$COVERAGE" = "Coverage" ]; then
  dotnet vstest $DOTNET_PARAMS --settings:"src/coverlet.runsettings" --ResultsDirectory:./CoverageResults
  EXIT_CODE=$?
elif [ "$COVERAGE" = "Test" ] ; then
  dotnet vstest $DOTNET_PARAMS
  EXIT_CODE=$?
else
  echo "Run Type must be provided as third argument: Coverage or Test"
  exit 3
fi

if [ "$EXIT_CODE" -ge 0 ]; then
  echo "Failed tests: $EXIT_CODE"
  exit 0
else
  exit $EXIT_CODE
fi
