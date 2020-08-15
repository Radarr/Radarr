#! /bin/bash
PLATFORM=$1
TYPE=$2
COVERAGE=$3
WHERE="Category!=ManualTest"
TEST_PATTERN="*Test.dll"
FILES=( "Readarr.Api.Test.dll" "Readarr.Automation.Test.dll" "Readarr.Common.Test.dll" "Readarr.Core.Test.dll" "Readarr.Host.Test.dll" "Readarr.Integration.Test.dll" "Readarr.Libraries.Test.dll" "Readarr.Mono.Test.dll" "Readarr.Update.Test.dll" "Readarr.Windows.Test.dll" )
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
export READARR_TESTS_LOG_OUTPUT="File"

VSTEST_PARAMS="--logger:nunit;LogFilePath=TestResult.xml"

if [ "$PLATFORM" = "Mac" ]; then

  export DYLD_FALLBACK_LIBRARY_PATH="$TEST_DIR:$MONOPREFIX/lib:/usr/local/lib:/lib:/usr/lib"
  echo $DYLD_FALLBACK_LIBRARY_PATH
  mono --version

  # To debug which libraries are being loaded:
  # export DYLD_PRINT_LIBRARIES=YES
fi

if [ "$PLATFORM" = "Windows" ]; then
  mkdir -p "$ProgramData/Readarr"
  WHERE="$WHERE&Category!=LINUX"
elif [ "$PLATFORM" = "Linux" ] || [ "$PLATFORM" = "Mac" ] ; then
  mkdir -p ~/.config/Readarr
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

DOTNET_PARAMS="$ASSEMBLIES --filter:$WHERE $VSTEST_PARAMS"

if [ "$COVERAGE" = "Coverage" ]; then
  dotnet test $DOTNET_PARAMS --settings:"src/coverlet.runsettings" --results-directory:./CoverageResults
  EXIT_CODE=$?
elif [ "$COVERAGE" = "Test" ] ; then
  dotnet test $DOTNET_PARAMS
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
