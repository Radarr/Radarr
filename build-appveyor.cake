var outputFolder = "./_output";
var outputFolderMono = outputFolder + "_mono";
var outputFolderOsx = outputFolder + "_osx";
var outputFolderOsxApp = outputFolderOsx + "_app";
var testPackageFolder = "./_tests";
var testSearchPattern = "*.Test/bin/x86/Release";
var sourceFolder = "./src";
var solutionFile = sourceFolder + "/NzbDrone.sln";
var updateFolder = outputFolder + "/NzbDrone.Update";
var updateFolderMono = outputFolderMono + "/NzbDrone.Update";

Task("Build")
	.Does(() => 
{
	Information("Running AppVeyor build.");

	CleanDirectories(outputFolder);

	MSBuild(solutionFile, new MSBuildSettings {
		ToolVersion = MSBuildToolVersion.VS2015
	}.WithTarget("Clean"));

	Restore(solutionFile);

	MSBuild(solutionFile, new MSBuildSettings {
		ToolVersion = MSBuildToolVersion.VS2015,
		PlatformTarget = PlatformTarget.x86,
		Configuration = "Release",
		Properties = new Dictionary<string, List<string>>() {
			{ "AllowedReferenceRelatedFileExtensions", new List<string> { ".pdb" } }
		}
	}.WithTarget("Build"));
});

RunTarget("Build");
// RunTarget("RunGulp");
// RunTarget("PackageMono");
// RunTarget("PackageOsx");
// RunTarget("PackageOsxApp");
// RunTarget("PackageTests");
// RunTarget("CleanupWindowsPackage");