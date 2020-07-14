#! /bin/bash
PLATFORM=$1
TYPE=$2
WHERE="cat != ManualTest"
TEST_PATTERN="*Test.dll"
ASSEMBLIES=""
TEST_LOG_FILE="TestLog.txt"

echo "test dir: $TEST_DIR"
if [ -z "$TEST_DIR" ]; then
    TEST_DIR="."
fi

if [ -d "$TEST_DIR/_tests" ]; then
  TEST_DIR="$TEST_DIR/_tests"
fi

COVERAGE_RESULT_DIRECTORY="$TEST_DIR/CoverageResults/"

rm -f "$TEST_LOG_FILE"

# Uncomment to log test output to a file instead of the console
export RADARR_TESTS_LOG_OUTPUT="File"

NUNIT="$TEST_DIR/NUnit.ConsoleRunner.3.10.0/tools/nunit3-console.exe"
NUNIT_COMMAND="$NUNIT"
NUNIT_PARAMS="--workers=1"

if [ "$PLATFORM" = "Mac" ]; then

  export DYLD_FALLBACK_LIBRARY_PATH="$TEST_DIR:/usr/local/lib:/lib:/usr/lib"
  echo $LD_LIBRARY_PATH
  echo $DYLD_LIBRARY_PATH
  echo $DYLD_FALLBACK_LIBRARY_PATH

  # To debug which libraries are being loaded:
  # export DYLD_PRINT_LIBRARIES=YES
fi

if [ "$PLATFORM" = "Windows" ]; then
  mkdir -p "$ProgramData/Radarr"
  WHERE="$WHERE && cat != LINUX"
elif [ "$PLATFORM" = "Linux" ] || [ "$PLATFORM" = "Mac" ] ; then
  mkdir -p ~/.config/Radarr
  WHERE="$WHERE && cat != WINDOWS"
  NUNIT_COMMAND="mono --debug --runtime=v4.0 $NUNIT"
else
  echo "Platform must be provided as first arguement: Windows, Linux or Mac"
  exit 1
fi

if [ "$TYPE" = "Unit" ]; then
  WHERE="$WHERE && cat != IntegrationTest && cat != AutomationTest"
elif [ "$TYPE" = "Integration" ] || [ "$TYPE" = "int" ] ; then
  WHERE="$WHERE && cat == IntegrationTest"
elif [ "$TYPE" = "Automation" ] ; then
  WHERE="$WHERE && cat == AutomationTest"
else
  echo "Type must be provided as second argument: Unit, Integration or Automation"
  exit 2
fi

for i in `find $TEST_DIR -name "$TEST_PATTERN"`;
  do ASSEMBLIES="$ASSEMBLIES $i"
done

if [ "$EXIT_CODE" -ge 0 ]; then
  echo "Failed tests: $EXIT_CODE"
  exit 0
else
  exit $EXIT_CODE
fi