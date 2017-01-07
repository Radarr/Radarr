#addin "Cake.Npm"

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

public void CreateMdbs(string path) {
	foreach (var file in System.IO.Directory.EnumerateFiles(path, "*.pdb", System.IO.SearchOption.AllDirectories)) {
		var actualFile = file.Substring(0, file.Length - 4);
		
		if (FileExists(actualFile + ".exe")) {
			StartProcess("./tools/pdb2mdb/pdb2mdb.exe", new ProcessSettings()
				.WithArguments(args => args.Append(actualFile + ".exe")));
		}
		
		if (FileExists(actualFile + ".dll")) {
			StartProcess("./tools/pdb2mdb/pdb2mdb.exe", new ProcessSettings()
				.WithArguments(args => args.Append(actualFile + ".dll")));
		}
	}
}

// Tasks
Task("Build").Does(() => {
	// Build
	if (DirectoryExists(outputFolder)) {
		DeleteDirectory(outputFolder, true);
	}

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

Task("Gulp").Does(() => {
	Npm.Install();
	Npm.RunScript("build");
});

Task("PackageMono").Does(() => {
	// Start mono package
	if (DirectoryExists(outputFolderMono)) {
		DeleteDirectory(outputFolderMono, true);
	}

	CopyDirectory(outputFolder, outputFolderMono);

	// Create MDBs
	CreateMdbs(outputFolderMono);

	// Remove PDBs
	DeleteFiles(outputFolderMono + "/**/*.pdb");

	// Remove service helpers
	DeleteFiles(outputFolderMono + "/ServiceUninstall.*");
	DeleteFiles(outputFolderMono + "/ServiceInstall.*");
	
	// Remove native windows binaries
	DeleteFiles(outputFolderMono + "/sqlite3.*");
	DeleteFiles(outputFolderMono + "/MediaInfo.*");

	// Adding NzbDrone.Core.dll.config (for dllmap)
	CopyFile(sourceFolder + "/NzbDrone.Core/NzbDrone.Core.dll.config", outputFolderMono + "/NzbDrone.Core.dll.config");

	// Adding CurlSharp.dll.config (for dllmap)
	CopyFile(sourceFolder + "/NzbDrone.Common/CurlSharp.dll.config", outputFolderMono + "/CurlSharp.dll.config");

	// Renaming Radarr.Console.exe to Radarr.exe
	DeleteFiles(outputFolderMono + "/Radarr.exe*");
	MoveFile(outputFolderMono + "/Radarr.Console.exe", outputFolderMono + "/Radarr.exe");
	MoveFile(outputFolderMono + "/Radarr.Console.exe.config", outputFolderMono + "/Radarr.exe.config");
	MoveFile(outputFolderMono + "/Radarr.Console.exe.mdb", outputFolderMono + "/Radarr.exe.mdb");

	// Remove NzbDrone.Windows.*
	DeleteFiles(outputFolderMono + "/NzbDrone.Windows.*");

	// Adding NzbDrone.Mono to updatePackage
	CopyFiles(outputFolderMono + "/NzbDrone.Mono.*", updateFolderMono);
});

Task("PackageOsx").Does(() => {
	
});

Task("PackageOsxApp").Does(() => {
	
});

// Run
// RunTarget("Build");
// RunTarget("Gulp");
RunTarget("PackageMono");
// RunTarget("PackageOsx");
// RunTarget("PackageOsxApp");
// RunTarget("PackageTests");
// RunTarget("CleanupWindowsPackage");