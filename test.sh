#! /bin/bash
PLATFORM=$1
TYPE=$2
WHERE="cat != ManualTest"
TEST_DIR="."
TEST_PATTERN="*Test.dll"
ASSEMBLIES=""
TEST_LOG_FILE="TestLog.txt"

if [ -d "$TEST_DIR/_tests" ]; then
  TEST_DIR="$TEST_DIR/_tests"
fi

rm -f "$TEST_LOG_FILE"

# Uncomment to log test output to a file instead of the console
export LIDARR_TESTS_LOG_OUTPUT="File"

if [[ -z "${APPVEYOR}" ]]; then
  NUNIT="$TEST_DIR/NUnit.ConsoleRunner.3.7.0/tools/nunit3-console.exe"
  NUNIT_COMMAND="$NUNIT"
  NUNIT_PARAMS="--workers=1"
else
  NUNIT="nunit3-console"
  NUNIT_COMMAND="$NUNIT"
  NUNIT_PARAMS="--result=myresults.xml;format=AppVeyor --workers=1"
  unset TMP
  unset TEMP
fi

if [ "$PLATFORM" = "Windows" ]; then
  WHERE="$WHERE && cat != LINUX"
elif [ "$PLATFORM" = "Linux" ]; then
  WHERE="$WHERE && cat != WINDOWS"
  NUNIT_COMMAND="mono --debug --runtime=v4.0 $NUNIT"
elif [ "$PLATFORM" = "Mac" ]; then
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

$NUNIT_COMMAND --where "$WHERE" $NUNIT_PARAMS $ASSEMBLIES;
EXIT_CODE=$?

if [ "$EXIT_CODE" -ge 0 ]; then
  if [[ -z "${APPVEYOR}" ]]; then
    echo "Failed tests: $EXIT_CODE"
  else
    echo "Failed tests: $EXIT_CODE"
    appveyor AddMessage "Failed tests: $EXIT_CODE"
  fi
  exit 0
else
  exit $EXIT_CODE
fi
