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

// Utility methods
public void RemoveEmptyFolders(string startLocation) {
    foreach (var directory in System.IO.Directory.GetDirectories(startLocation))
    {
        RemoveEmptyFolders(directory);

        if (System.IO.Directory.GetFiles(directory).Length == 0 && 
            System.IO.Directory.GetDirectories(directory).Length == 0)
        {
            DeleteDirectory(directory, false);
        }
    }
}

public void CleanFolder(string path, bool keepConfigFiles) {
	DeleteFiles(path + "/**/*.transform");

	if (!keepConfigFiles) {
		DeleteFiles(path + "/**/*.dll.config");
	}

	DeleteFiles(path + "/**/FluentValidation.resources.dll");
	DeleteFiles(path + "/**/App.config");

	DeleteFiles(path + "/**/*.less");

	DeleteFiles(path + "/**/*.vshost.exe");

	DeleteFiles(path + "/**/*.dylib");

	RemoveEmptyFolders(path);
}

// Tasks
Task("Build").Does(() => {
	// Build
	CleanDirectories(outputFolder);

	MSBuild(solutionFile, config => 
		config.UseToolVersion(MSBuildToolVersion.VS2015)
			.WithTarget("Clean"));

	NuGetRestore(solutionFile);

	MSBuild(solutionFile, config => 
		config.UseToolVersion(MSBuildToolVersion.VS2015)
			.SetPlatformTarget(PlatformTarget.x86)
			.SetConfiguration("Release")
			.WithProperty("AllowedReferenceRelatedFileExtensions", new string[] { ".pdb" })
			.WithTarget("Build"));

	CleanFolder(outputFolder, false);

	// Add JsonNet
	DeleteFiles(outputFolder + "/Newtonsoft.Json.*");
	CopyFiles(sourceFolder + "/packages/Newtonsoft.Json.*/lib/net35/*.dll", outputFolder);
	CopyFiles(sourceFolder + "/packages/Newtonsoft.Json.*/lib/net35/*.dll", updateFolder);

	// Remove Mono stuff
	DeleteFile(outputFolder + "/Mono.Posix.dll");
});

// Run
RunTarget("Build");
// RunTarget("RunGulp");
// RunTarget("PackageMono");
// RunTarget("PackageOsx");
// RunTarget("PackageOsxApp");
// RunTarget("PackageTests");
// RunTarget("CleanupWindowsPackage");