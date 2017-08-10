# Available types:
# - Unit
# - Integration
# - Automation

param([string]$type = "Unit")

$where = "cat != ManualTest && cat != LINUX"
$testDir = "."
$testPattern = "*Test.dll"

$nunit = "nunit3-console.exe"
$nunitCommand = $nunit

if (!(Get-Command $nunit -ErrorAction SilentlyContinue)) {
    Write-Error "nunit3-console.exe was not found in your PATH, please install https://github.com/nunit/nunit-console/releases."
    exit
}

switch ($type) {
    "unit" { 
        $where = $where + " && cat != IntegrationTest && cat != AutomationTest"
    }
    "integration" {
        $where = $where + " && cat == IntegrationTest"
    }
    "automation" {
        $where = $where + " && cat == AutomationTest"
    }
    Default {
        Write-Error "Invalid test type specified."
        exit
    }
}

$assemblies = (Get-ChildItem -Path $testDir -Filter $testPattern -Recurse -File -Name) -join " "

$command = $nunitCommand + " --where '" + $where + "' " + $assemblies 

Invoke-Expression $command