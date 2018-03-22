#addin nuget:?package=Cake.Npm&version=0.12.1
#addin nuget:?package=SharpZipLib&version=0.86.0
#addin nuget:?package=Cake.Compression&version=0.1.4

// Build variables
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

// Artifact variables
var artifactsFolder = "./_artifacts";
var artifactsFolderWindows = artifactsFolder + "/windows";
var artifactsFolderLinux = artifactsFolder + "/linux";
var artifactsFolderOsx = artifactsFolder + "/osx";
var artifactsFolderOsxApp = artifactsFolder + "/osx-app";

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

// Build Tasks
Task("Compile").Does(() => {
	// Build
	if (DirectoryExists(outputFolder)) {
		DeleteDirectory(outputFolder, true);
	}

	MSBuild(solutionFile, config =>
		config.UseToolVersion(MSBuildToolVersion.VS2015)
			.WithTarget("Clean")
			.SetVerbosity(Verbosity.Minimal));

	NuGetRestore(solutionFile);

	MSBuild(solutionFile, config =>
		config.UseToolVersion(MSBuildToolVersion.VS2015)
			.SetPlatformTarget(PlatformTarget.x86)
			.SetConfiguration("Release")
			.WithProperty("AllowedReferenceRelatedFileExtensions", new string[] { ".pdb" })
			.WithTarget("Build")
			.SetVerbosity(Verbosity.Minimal));

	CleanFolder(outputFolder, false);

	// Add JsonNet
	DeleteFiles(outputFolder + "/Newtonsoft.Json.*");
	CopyFiles(sourceFolder + "/packages/Newtonsoft.Json.*/lib/net35/*.dll", outputFolder);
	CopyFiles(sourceFolder + "/packages/Newtonsoft.Json.*/lib/net35/*.dll", updateFolder);

	// Remove Mono stuff
	DeleteFile(outputFolder + "/Mono.Posix.dll");
});

Task("Gulp").Does(() => {
	NpmInstall(new NpmInstallSettings {
		LogLevel = NpmLogLevel.Silent,
		WorkingDirectory = "./",
		Production = true
	});

	NpmRunScript("build");
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
	// Start osx package
	if (DirectoryExists(outputFolderOsx)) {
		DeleteDirectory(outputFolderOsx, true);
	}

	CopyDirectory(outputFolderMono, outputFolderOsx);

	// Adding sqlite dylibs
	CopyFiles(sourceFolder + "/Libraries/Sqlite/*.dylib", outputFolderOsx);

	// Adding MediaInfo dylib
	CopyFiles(sourceFolder + "/Libraries/MediaInfo/*.dylib", outputFolderOsx);

	// Chmod as executable
	StartProcess(@"C:\cygwin64\bin\chmod.exe", new ProcessSettings()
		.WithArguments(args => args
			.Append("+x")
			.Append(outputFolderOsx + "/Radarr")));

	// Adding Startup script
	CopyFile("./osx/Radarr", outputFolderOsx + "/Radarr");
});

Task("PackageOsxApp").Does(() => {
	// Start osx app package
	if (DirectoryExists(outputFolderOsxApp)) {
		DeleteDirectory(outputFolderOsxApp, true);
	}

	CreateDirectory(outputFolderOsxApp);

	// Copy osx package files
	CopyDirectory("./osx/Radarr.app", outputFolderOsxApp + "/Radarr.app");
	CopyDirectory(outputFolderOsx, outputFolderOsxApp + "/Radarr.app/Contents/MacOS");
});

Task("PackageTests").Does(() => {
	// Start tests package
	if (DirectoryExists(testPackageFolder)) {
		DeleteDirectory(testPackageFolder, true);
	}

	CreateDirectory(testPackageFolder);

	// Copy tests
	CopyFiles(sourceFolder + "/" + testSearchPattern + "/*", testPackageFolder);
	foreach (var directory in System.IO.Directory.GetDirectories(sourceFolder, "*.Test")) {
		var releaseDirectory = directory + "/bin/x86/Release";
		if (DirectoryExists(releaseDirectory)) {
			foreach (var releaseSubDirectory in System.IO.Directory.GetDirectories(releaseDirectory)) {
				Information(System.IO.Path.GetDirectoryName(releaseSubDirectory));
				CopyDirectory(releaseSubDirectory, testPackageFolder + "/" + System.IO.Path.GetFileName(releaseSubDirectory));
			}
		}
	}

	// Install NUnit.ConsoleRunner
	NuGetInstall("NUnit.ConsoleRunner", new NuGetInstallSettings {
		Version = "3.2.0",
		OutputDirectory = testPackageFolder
	});

	// Copy dlls
	CopyFiles(outputFolder + "/*.dll", testPackageFolder);

	// Copy scripts
	CopyFiles("./*.sh", testPackageFolder);

	// Create MDBs for tests
	CreateMdbs(testPackageFolder);

	// Remove config
	DeleteFiles(testPackageFolder + "/*.log.config");

	// Clean
	CleanFolder(testPackageFolder, true);

	// Adding NzbDrone.Core.dll.config (for dllmap)
	CopyFile(sourceFolder + "/NzbDrone.Core/NzbDrone.Core.dll.config", testPackageFolder + "/NzbDrone.Core.dll.config");

	// Adding CurlSharp.dll.config (for dllmap)
	CopyFile(sourceFolder + "/NzbDrone.Common/CurlSharp.dll.config", testPackageFolder + "/CurlSharp.dll.config");

	// Adding CurlSharp libraries
	CopyFiles(sourceFolder + "/ExternalModules/CurlSharp/libs/i386/*", testPackageFolder);
});

Task("CleanupWindowsPackage").Does(() => {
	// Remove mono
	DeleteFiles(outputFolder + "/NzbDrone.Mono.*");

	// Adding NzbDrone.Windows to updatePackage
	CopyFiles(outputFolder + "/NzbDrone.Windows.*", updateFolder);
});

Task("Build")
	.IsDependentOn("Compile")
	.IsDependentOn("Gulp")
	.IsDependentOn("PackageMono")
	.IsDependentOn("PackageOsx")
	.IsDependentOn("PackageOsxApp")
	.IsDependentOn("PackageTests")
	.IsDependentOn("CleanupWindowsPackage");

// Build Artifacts
Task("CleanArtifacts").Does(() => {
	if (DirectoryExists(artifactsFolder)) {
		DeleteDirectory(artifactsFolder, true);
	}

	CreateDirectory(artifactsFolder);
});

Task("ArtifactsWindows").Does(() => {
	CopyDirectory(outputFolder, artifactsFolderWindows + "/Radarr");
});

Task("ArtifactsWindowsInstaller").Does(() => {
	InnoSetup("./setup/nzbdrone.iss", new InnoSetupSettings {
		OutputDirectory = artifactsFolder,
		ToolPath = "./setup/inno/ISCC.exe"
    });
});

Task("ArtifactsLinux").Does(() => {
	CopyDirectory(outputFolderMono, artifactsFolderLinux + "/Radarr");
});

Task("ArtifactsOsx").Does(() => {
	CopyDirectory(outputFolderOsx, artifactsFolderOsx + "/Radarr");
});

Task("ArtifactsOsxApp").Does(() => {
	CopyDirectory(outputFolderOsxApp, artifactsFolderOsxApp);
});

Task("CompressArtifacts").Does(() => {
	var prefix = "";

	if (AppVeyor.IsRunningOnAppVeyor) {
		prefix += AppVeyor.Environment.Repository.Branch.Replace("/", "-") + ".";
		prefix += AppVeyor.Environment.Build.Version + ".";
	}

	Zip(artifactsFolderWindows, artifactsFolder + "/Radarr." + prefix + "windows.zip");
	GZipCompress(artifactsFolderLinux, artifactsFolder + "/Radarr." + prefix + "linux.tar.gz");
	GZipCompress(artifactsFolderOsx, artifactsFolder + "/Radarr." + prefix + "osx.tar.gz");
	Zip(artifactsFolderOsxApp, artifactsFolder + "/Radarr." + prefix + "osx-app.zip");
});

Task("Artifacts")
	.IsDependentOn("CleanArtifacts")
	.IsDependentOn("ArtifactsWindows")
	//.IsDependentOn("ArtifactsWindowsInstaller")
	.IsDependentOn("ArtifactsLinux")
	.IsDependentOn("ArtifactsOsx")
	.IsDependentOn("ArtifactsOsxApp")
	.IsDependentOn("CompressArtifacts");

// Run
RunTarget("Build");
RunTarget("Artifacts");
