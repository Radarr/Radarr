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

	// DotNetBuild(solutionFile, settings => 
	// 						  settings.SetConfiguration("Release")
	// 								  .WithTarget("Build")
	// 								  .WithProperty("Platform", new string[] { "x86" })
	// 								  .WithProperty("AllowedReferenceRelatedFileExtensions", new string[] { ".pdb" }));
});

RunTarget("Build");
// RunTarget("RunGulp");
// RunTarget("PackageMono");
// RunTarget("PackageOsx");
// RunTarget("PackageOsxApp");
// RunTarget("PackageTests");
// RunTarget("CleanupWindowsPackage");