/*
#########################################################################################
Installing

	Windows - powershell
		
        Invoke-WebRequest http://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1
        .\build.ps1

	Windows - cmd.exe prompt	
	
        powershell ^
			Invoke-WebRequest http://cakebuild.net/download/bootstrapper/windows -OutFile build.ps1
        powershell ^
			.\build.ps1
	
	Mac OSX 

        rm -fr tools/; mkdir ./tools/ ; \
        cp cake.packages.config ./tools/packages.config ; \
        curl -Lsfo build.sh http://cakebuild.net/download/bootstrapper/osx ; \
        chmod +x ./build.sh ;
        ./build.sh

	Linux

        rm -fr tools/; mkdir ./tools/ ; \
        cp cake.packages.config ./tools/packages.config ; \
        curl -Lsfo build.sh http://cakebuild.net/download/bootstrapper/linux
        chmod +x ./build.sh ;
        ./build.sh

Running Cake to Build Xamarin.Auth targets

	Windows

		tools\Cake\Cake.exe --verbosity=diagnostic --target=libs
		tools\Cake\Cake.exe --verbosity=diagnostic --target=nuget
		tools\Cake\Cake.exe --verbosity=diagnostic --target=samples

	Mac OSX 
	
		mono tools/Cake/Cake.exe --verbosity=diagnostic --target=libs
		mono tools/Cake/Cake.exe --verbosity=diagnostic --target=nuget
		
NuGet Publish patterns

		BEFORE PASTING:
		NOTE: ** / 
		** /output/Xamarin.Auth.1.5.0-alpha-12.nupkg,
		** /output/Xamarin.Auth.XamarinForms.1.5.0-alpha-12.nupkg,
		** /output/Xamarin.Auth.Extensions.1.5.0-alpha-12.nupkg
		

#########################################################################################
*/	
#addin nuget:?package=Cake.Xamarin&version=1.3.0.15
#addin nuget:?package=Cake.Xamarin.Build&version=2.0.22
#addin nuget:?package=Cake.FileHelpers&version=1.0.4
#tool nuget:?package=vswhere

/*
-----------------------------------------------------------------------------------------
	choco install -y gitlink
	
-----------------------------------------------------------------------------------------
*/
#tool nuget:?package=gitlink

var TARGET = Argument ("t", Argument ("target", Argument ("Target", "Default")));
var VERBOSITY = Argument ("v", Argument ("verbosity", Argument ("Verbosity", "Diagnostic")));


Argument<bool>("experimental", true);	// enable C#6 string interpolation

Verbosity verbosity = Verbosity.Diagnostic;
Action<string> InformationFancy = 
	(text) 
		=>
		{
			Console.BackgroundColor = ConsoleColor.Yellow;
			Console.ForegroundColor = ConsoleColor.Blue;			
			Console.WriteLine(text);
			Console.ResetColor();

			return;
		};


// stuff needed for fixes!
DirectoryPath vsLatest = null;
FilePath msBuildPathX64 = null;

FilePath nuget_tool_path = null;
FilePath cake_tool_path = null;

string github_repo_url="https://github.com/xamarin/Xamarin.Auth";


Action<string> GitLinkAction = 
	(solution_file_name) 
		=>
		{ 
			return;
			
			if (! IsRunningOnWindows())
			{
				// GitLink still has issues on macosx
				return;
			}
			GitLink
			(
				"./", 
				new GitLinkSettings() 
				{
					RepositoryUrl = github_repo_url,
					SolutionFileName = solution_file_name,
				
					// nb: I would love to set this to `treatErrorsAsWarnings` which defaults to `false` 
					// but GitLink trips over Akavache.Tests :/
					// Handling project 'Akavache.Tests'
					// No pdb file found for 'Akavache.Tests', is project built in 'Release' mode with 
					// pdb files enabled? 
					// Expected file is 'C:\Dropbox\OSS\akavache\Akavache\src\Akavache.Tests\Akavache.Tests.pdb'
					ErrorsAsWarnings = true, 
				}
			);
		};




// https://docs.microsoft.com/en-us/nuget/tools/nuget-exe-cli-reference#restore
// http://cakebuild.net/api/Cake.Common.Tools.NuGet.Restore/NuGetRestoreSettings/
NuGetRestoreSettings nuget_restore_settings = new NuGetRestoreSettings 
	{ 
		ToolPath = nuget_tool_path,
		Verbosity = NuGetVerbosity.Detailed,
	};

NuGetUpdateSettings nuget_update_settings = new NuGetUpdateSettings 
	{ 
		ToolPath = nuget_tool_path,
		Verbosity = NuGetVerbosity.Detailed,
		Prerelease = false,
	};

Task ("dump-environment")
	.Does 
	(
		() =>
		{

			// Print out environment variables to console
			var ENV_VARS = EnvironmentVariables ();
			Information ("Environment Variables: {0}", "");
			foreach (var ev in ENV_VARS)
				Information ("\t{0} = {1}", ev.Key, ev.Value);

			// EnvironmentVariables evs = EnvironmentVariables ();
			// Information ("Environment Variables: {0}", "");
			// foreach (EnvironmentVariable ev in evs)
			// {
			// 	Information ($"\t{ev.Key}       = {ev.Value}");
			// }
			
			// From Cake.Xamarin.Build, dumps out versions of things
			LogSystemInfo ();

			return;
		}
	);

Task ("clean")
	.Does 
	(
		() => 
		{	
			// note no trailing backslash
			//DeleteDirectories (GetDirectories("./output"), recursive:true);
			// OK
			//CleanDirectories("**/obj");
			//CleanDirectories("**/Obj");
			//CleanDirectories("**/bin");
			//CleanDirectories("**/Bin");
			
			//CleanDirectories(GetDirectories("**/obj"));
			//CleanDirectories(GetDirectories("**/Obj"));
			//CleanDirectories(GetDirectories("**/bin"));
			//CleanDirectories(GetDirectories("**/Bin"));
			
			
			// OK
			DeleteDirectories(GetDirectories("**/obj"), recursive:true);
			DeleteDirectories(GetDirectories("**/Obj"), recursive:true);
			DeleteDirectories(GetDirectories("**/bin"), recursive:true);
			DeleteDirectories(GetDirectories("**/Bin"), recursive:true);
			
			// ! OK
			//DeleteDirectories("**/obj", true);
			// The best overloaded method match for 
			//		`CakeBuildScriptImpl.DeleteDirectories(System.Collections.Generic.IEnumerable<Cake.Core.IO.DirectoryPath>, bool)' 
			// has some invalid arguments
			//Information("NOGO: DeleteDirectories(\"**/obj\", true);");

		}
	);

Task ("distclean")
	.IsDependentOn ("clean")
	.Does 
	(
		() => 
		{				
			DeleteDirectories(GetDirectories("**/bin"), recursive:true);
			DeleteDirectories(GetDirectories("**/Bin"), recursive:true);
			DeleteDirectories(GetDirectories("**/obj"), recursive:true);
			DeleteDirectories(GetDirectories("**/Obj"), recursive:true);
			DeleteDirectories(GetDirectories("**/packages"), recursive:true);
			DeleteDirectories(GetDirectories("**/Components"), recursive:true);
		}
	);

Task ("rebuild")
	.IsDependentOn ("distclean")
	.IsDependentOn ("build")
	;	

Task ("build")
	.IsDependentOn ("libs")
	.IsDependentOn ("samples")
	;	

Task ("package")
	.IsDependentOn ("libs")
	.IsDependentOn ("nuget")
	;	

Task ("libs")
	.IsDependentOn ("libs-macosx")	
	.IsDependentOn ("libs-windows")	
	.Does 
	(
		() => 
		{	
		}
	);

//---------------------------------------------------------------
// building with custom preprocessor constants/defines
// used by some projects
//		Azure Mobile Services Client	
//	XAMARIN_AUTH_INTERNAL
//		to hide public Xamarin.Auth classes (API)
//  XAMARIN_CUSTOM_TABS_INTERNAL
//		to hide public CustomTabs classes (API)

bool is_using_custom_defines = false;

Task ("libs-custom")
	.Does 
	(
		() => 
		{	
			is_using_custom_defines = true;
			RunTarget("libs");

			return;
		}
	);
//---------------------------------------------------------------

string[] source_solutions = new string[]
{
	"./source/Xamarin.Auth-Library.sln",
	"./source/Xamarin.Auth-Library-MacOSX-Xamarin.Studio.sln",
	"./source/Xamarin.Auth-Library-VS2015.sln",
	"./source/Xamarin.Auth-Library-VS2017.sln",
};

string[] solutions_for_nuget_tests = new string[]
{
	"./samples//Traditional.Standard/Providers/Xamarin.Auth.Samples.TraditionalStandard-MacOSX-Xamarin.Studio.sln",
	"./samples//Traditional.Standard/Providers/Xamarin.Auth.Samples.TraditionalStandard.sln",
	"./samples//Xamarin.Forms/Evolve16Labs/05-OAuth/ComicBook.sln",
	"./samples//Xamarin.Forms/Providers/XamarinAuth.XamarinForms.sln",
};

string[] sample_solutions_macosx = new string[]
{
//	"./samples//Traditional.Standard/Providers/Xamarin.Auth.Samples.TraditionalStandard-MacOSX-Xamarin.Studio.sln",
//	"./samples//Traditional.Standard/Providers/Xamarin.Auth.Samples.TraditionalStandard.sln",
//	"./samples//Xamarin.Forms/Evolve16Labs/05-OAuth/ComicBook.sln",
//	"./samples//Xamarin.Forms/Providers/XamarinAuth.XamarinForms.sln",
};

string[] sample_solutions_windows = new string[]
{
//	"./samples//Traditional.Standard/Providers/Xamarin.Auth.Samples.TraditionalStandard-MacOSX-Xamarin.Studio.sln",
//	"./samples//Traditional.Standard/Providers/Xamarin.Auth.Samples.TraditionalStandard.sln",
//	"./samples//Xamarin.Forms/Evolve16Labs/05-OAuth/ComicBook.sln",
//	"./samples//Xamarin.Forms/Providers/XamarinAuth.XamarinForms.sln",
};

string[] sample_solutions = 
			sample_solutions_macosx
			.Concat(sample_solutions_windows)  // comment out this line if in need
			.ToArray()
			;

string[] solutions = 
			source_solutions
			.Concat(sample_solutions)  // comment out this line if in need
			.ToArray()
			;


string[] build_configurations =  new []
{
	"Debug",
	"Release",
};

//---------------------------------------------------------------------------------------
/*
Custome preprocessor defines
used by some projects (Azure Mobile Services) 

when passing preprocessor defines/constants through commandline all required defines
must be specified on commandline (even DEBUG for Debug configurations).

NOTE - there is deviation in Xamarin.Android behaviour which appends Android default
constants (like __ANDROID__) and even some non-standard ones (like __MOBILE__)
*/
Dictionary<string,List<string>> defines = new Dictionary<string,List<string> > 
{
	{ 
		"Default",
		new List<string>
		{
			"" ,
		}
	},
	{ 
		"Delimiter ;", 
		new List<string>
		{
			"%3B",
		}
	},	
	{ 
		"Custom", 
		new List<string>
		{
			"XAMARIN_AUTH_INTERNAL", 
			"XAMARIN_CUSTOM_TABS_INTERNAL",
			"AZURE_MOBILE_SERVICES",
		}
	},	
	{ 
		"Portable", 
		new List<string>
		{
			"PORTABLE", 	// non-standard (Xamarin.Auth legacy)
			"PCL",			// non-standard (Xamarin.Auth legacy)
		}
	},	
	{ 
		"Xamarin.Android", 
		new List<string>
		{
			"__ANDROID__",
			"__ANDROID_1__", "__ANDROID_2__", "__ANDROID_3__", "__ANDROID_4__", "__ANDROID_5__",
			"__ANDROID_6__", "__ANDROID_7__", "__ANDROID_8__", "__ANDROID_9__", "__ANDROID_10__",
			"__ANDROID_11__", "__ANDROID_12__", "__ANDROID_13__", "__ANDROID_14__", "__ANDROID_15__",
			"__ANDROID_17__", "__ANDROID_18__", "__ANDROID_19__", "__ANDROID_20__", "__ANDROID_21__",
			"__ANDROID_22__", "__ANDROID_23__", "__ANDROID_24__", "__ANDROID_25__", "__ANDROID_26__",
			"__MOBILE__",			// Xamarin Legacy
			"PLATFORM_ANDROID",	// Xamarin.Auth Legacy
		}
	},	
	{ 
		"Xamarin.IOS", 
		new List<string>
		{
			"__IOS__",
			"__UNIFIED__",
			"___MOBILE__",
			"PLATFORM_IOS",		// Xamarin.Auth Legacy
		}
	},	
	{ 
		"Universal Windows Platform", 
		new List<string>
		{
			"NETFX_CORE",
			"WINDOWS_UWP",
			"WINDOWS",
			"WINDOWS_APP",
			"WINDOWS_PHONE",
			"WINDOWS_PHONE_APP",
		}
	},	
	{ 
		"Windows 8.1 WinRT", 
		new List<string>
		{
			"NETFX_CORE",
			"WINDOWS",
			"WINDOWS_APP",
		}
	},	
	{ 
		"Windows Phone 8.1 WinRT", 
		new List<string>
		{
			"NETFX_CORE",
			"WINDOWS_PHONE",
			//"WINDOWS_PHONE_APP",
		}
	},	
	{ 
		"Windows Phone 8 Silverlight", 
		new List<string>
		{
			"WINDOWS_PHONE",
			"SILVERLIGHT",
			"PLATFORM_WINPHONE",	// Xamarin.Auth Legacy
		}
	},	
	{ 
		"DotNetStandard 1.6", 
		new List<string>
		{
			"NETSTANDARD1_6",
		}
	},	
};
//---------------------------------------------------------------------------------------

string define = null;

Task ("nuget-restore")
	.Does 
	(
		() => 
		{	
			InformationFancy("nuget-restore");
			Information("libs nuget_restore_settings.ToolPath = {0}", nuget_restore_settings.ToolPath);

			RunTarget("source-nuget-restore");
			RunTarget("samples-nuget-restore");
						
			return;
		}
	);

Task ("nuget-update")
	.IsDependentOn ("nuget-restore")
	.Does 
	(
		() => 
		{	
			FilePathCollection files_package_config = GetFiles("./**/packages.config");

			foreach(FilePath package_config_file in files_package_config)
			{
				if (IsRunningOnWindows() && package_config_file.ToString().Length < 200)
				{
					continue;
				}
				else
				{
				}
				Information("Nuget Update   = " + package_config_file);
				NuGetUpdate(package_config_file, nuget_update_settings);					
			}

			return;	
		}
	);

Task ("source-nuget-restore")
	.Does 
	(
		() => 
		{
			foreach (string source_solution in source_solutions)
			{
				NuGetRestore(source_solution, nuget_restore_settings); 
			}

			return;
		}
	);

Task ("samples-nuget-restore")
	.Does 
	(
		() => 
		{
			foreach (string sample_solution in sample_solutions)
			{
				if (IsRunningOnWindows() && sample_solution.Length > 200)
				{
					continue;
				}
				else
				{
				}

				NuGetRestore(sample_solution, nuget_restore_settings); 
			}
			
			return;
		}
	);
	
string solution_or_project = null;

Action<string,  MSBuildSettings> BuildLoop = 
	(
		sln_prj, 			// solution or project to be compiled
		msbuild_settings	// msbuild customization settings
	) 
	=>
	{
		if (sln_prj.Contains("Xamarin.Auth-Library.sln"))
		{
			/*
				2017-09
				Failing on Mac because of: 
				
					*	Uinversal Windows Platofrm
					*	WinRT (Windows and Windows Phone)
					*	WindowsPhone Silverlight
					*	.NET Standard
					
				Failing on Windows in Visual Studio 2015 because of:
				
					*	.NET Standard
					
				Failing on Windows in Visual Studio 2017 because of:
				
					*	WinRT (Windows and Windows Phone)
					*	WindowsPhone Silverlight
			*/
			return;
		}
		
		if 
		(
			IsRunningOnWindows() == false
			&&
			(
				sln_prj.Contains("VS2015.sln")
				||
				sln_prj.Contains("VS2017.sln")
			)
		)
		{
			/*
				2017-09
				Failing on Mac because of: 
				
					*	Uinversal Windows Platofrm
					*	WinRT (Windows and Windows Phone)
					*	WindowsPhone Silverlight
					*	.NET Standard
					
			*/		
			return;
		}

		foreach (string build_configuration in build_configurations)
		{
			InformationFancy($"Solution/Project = {sln_prj}");
			InformationFancy($"Configuration    = {build_configuration}");

			msbuild_settings.Verbosity = verbosity;
			msbuild_settings.Configuration = build_configuration;
			msbuild_settings.WithProperty
								(
									"consoleloggerparameters", 
									"ShowCommandLine"
								);

			if (sln_prj.Contains(".csproj"))
			{
				// NO OP - MSBuildToolVersion is set before calling
			}
			else if (sln_prj.Contains("VS2015.sln"))
			{
				/*
				Using Visual Studio 2015 tooling

				Fix for 

				source\Xamarin.Auth.XamarinIOS\Xamarin.Auth.XamarinIOS.csproj" 
				(Build target) (1) -> (CoreCompile target) ->
				C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\Roslyn\Microsoft.CSharp.Core.targets
				error MSB6004: 
				The specified task executable location 
				"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\amd64\Roslyn\csc.exe" is invalid.
				*/
				msbuild_settings.ToolVersion = MSBuildToolVersion.VS2015; 
				/*
				Fix for 

				  C:\Program Files (x86)\MSBuild\Microsoft\WindowsPhone\v8.0\Microsoft.WindowsPhone.v8.0.Overrides.targets(15,9)
				  error : 
				  Building Windows Phone application using MSBuild 64 bit is not supported. 
				  If you are using TFS build definitions, change the MSBuild platform to x86.
				*/
				msbuild_settings.PlatformTarget = PlatformTarget.x86;
			}
			else if(sln_prj.Contains("VS2017.sln"))
			{
				msbuild_settings.ToolVersion = MSBuildToolVersion.VS2017; 
				/*
				C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\Roslyn\Microsoft.CSharp.Core.targets 
				error MSB6004: 
				The specified task executable location 
					"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\amd64\Roslyn\csc.exe" 
				is invalid. [X:\x.a-m\source\Xamarin.Auth.XamarinIOS\Xamarin.Auth.XamarinIOS.csproj]
				*/
				msbuild_settings.ToolPath = msBuildPathX64;	
			}		
			else if(sln_prj.Contains("MacOSX-Xamarin.Studio.sln"))
			{
				msbuild_settings.ToolVersion = MSBuildToolVersion.VS2015; 
				msbuild_settings.ToolPath = msBuildPathX64;	
			}		


			MSBuild(sln_prj,msbuild_settings);
		}

		return;
	};


Task ("libs-macosx-filesystem")
	.Does 
	(
		() => 
		{	
			CreateDirectory ("./output/");
			CreateDirectory ("./output/pcl/");
			CreateDirectory ("./output/android/");
			CreateDirectory ("./output/ios/");

			return;
		}
	);

Task ("libs-macosx")
	.IsDependentOn ("libs-macosx-projects")
	.IsDependentOn ("libs-macosx-solutions")
	.Does 
	(
		() => 
		{
			return;
		}
	);


Task ("libs-macosx-solutions")
	.IsDependentOn ("nuget-restore")
	.IsDependentOn ("libs-macosx-filesystem")
	.Does 
	(
		() => 
		{	
			if ( ! IsRunningOnWindows() )
			{
				foreach(string sln in source_solutions)
				{
					InformationFancy($"Solution = {sln}");
					BuildLoop
						(
							sln, 
							new MSBuildSettings
							{
								// Default Settings for Solutions
								Verbosity = verbosity,
							}
						);
				}
			} 

			return;
		}
	);

Task ("libs-macosx-projects")
	.IsDependentOn ("nuget-restore")
	.IsDependentOn ("libs-macosx-filesystem")
	.Does 
	(
		() => 
		{	
			if ( ! IsRunningOnWindows() )
			{
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Xamarin.Auth.LinkSource/Xamarin.Auth.LinkSource.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Default"]));
				}
				else
				{
					define = string.Join("%3B", defines["Default"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Xamarin.Auth.Portable/Xamarin.Auth.Portable.csproj";					
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Portable"]));
				}
				else
				{
					define = string.Join("%3B", defines["Portable"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Xamarin.Auth.Portable/**/Release/Xamarin.Auth.dll", 
						"./output/pcl/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.Portable/**/Release/Xamarin.Auth.pdb", 
						"./output/pcl/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Xamarin.Auth.XamarinAndroid/Xamarin.Auth.XamarinAndroid.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Xamarin.Android"]));
				}
				else
				{
					define = string.Join("%3B", defines["Xamarin.Android"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Xamarin.Auth.XamarinAndroid/**/Release/Xamarin.Auth.dll", 
						"./output/android/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.XamarinAndroid/**/Release/Xamarin.Auth.pdb", 
						"./output/android/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Xamarin.Auth.XamarinIOS/Xamarin.Auth.XamarinIOS.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Xamarin.IOS"]));
				}
				else
				{
					define = string.Join("%3B", defines["Xamarin.IOS"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Xamarin.Auth.XamarinIOS/**/Release/Xamarin.Auth.dll", 
						"./output/iOS/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.XamarinIOS/**/Release/Xamarin.Auth.pdb", 
						"./output/iOS/"
					);
				//-------------------------------------------------------------------------------------


				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Extensions/Xamarin.Auth.Extensions.Portable/Xamarin.Auth.Extensions.Portable.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Portable"]));
				}
				else
				{
					define = string.Join("%3B", defines["Portable"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Extensions/Xamarin.Auth.Extensions.Portable/**/Release/Xamarin.Auth.Extensions.dll", 
						"./output/pcl/"
					);
				CopyFiles
					(
						"./source/Extensions/Xamarin.Auth.Extensions.Portable/**/Release/Xamarin.Auth.Extensions.pdb", 
						"./output/pcl/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Extensions/Xamarin.Auth.Extensions.XamarinAndroid/Xamarin.Auth.Extensions.XamarinAndroid.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Xamarin.Android"]));
				}
				else
				{
					define = string.Join("%3B", defines["Xamarin.Android"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Extensions/Xamarin.Auth.Extensions.XamarinAndroid/**/Release/Xamarin.Auth.Extensions.dll", 
						"./output/android/"
					);
				CopyFiles
					(
						"./source/Extensions/Xamarin.Auth.Extensions.XamarinAndroid/**/Release/Xamarin.Auth.Extensions.pdb", 
						"./output/android/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Extensions/Xamarin.Auth.Extensions.XamarinIOS/Xamarin.Auth.Extensions.XamarinIOS.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Xamarin.IOS"]));
				}
				else
				{
					define = string.Join("%3B", defines["Xamarin.IOS"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Extensions/Xamarin.Auth.Extensions.XamarinIOS/**/Release/Xamarin.Auth.Extensions.dll", 
						"./output/ios/"
					);
				CopyFiles
					(
						"./source/Extensions/Xamarin.Auth.Extensions.XamarinIOS/**/Release/Xamarin.Auth.Extensions.pdb", 
						"./output/ios/"
					);
				//-------------------------------------------------------------------------------------
				
				
				
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/XamarinForms/Xamarin.Auth.Forms/Xamarin.Auth.Forms.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Portable"]));
				}
				else
				{
					define = string.Join("%3B", defines["Portable"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/XamarinForms/Xamarin.Auth.Forms/**/Release/Xamarin.Auth.XamarinForms.dll", 
						"./output/pcl/"
					);
				CopyFiles
					(
						"./source/XamarinForms/Xamarin.Auth.Forms/**/Release/Xamarin.Auth.XamarinForms.pdb", 
						"./output/pcl/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/XamarinForms/Xamarin.Auth.Forms.Droid/Xamarin.Auth.Forms.Droid.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Xamarin.Android"]));
				}
				else
				{
					define = string.Join("%3B", defines["Xamarin.Android"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/XamarinForms/Xamarin.Auth.Forms.Droid/**/Release/Xamarin.Auth.XamarinForms.dll", 
						"./output/android/"
					);
				CopyFiles
					(
						"./source/XamarinForms/Xamarin.Auth.Forms.Droid/**/Release/Xamarin.Auth.XamarinForms.pdb", 
						"./output/android/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/XamarinForms/Xamarin.Auth.Forms.iOS/Xamarin.Auth.Forms.iOS.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Xamarin.IOS"]));
				}
				else
				{
					define = string.Join("%3B", defines["Xamarin.IOS"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/XamarinForms/Xamarin.Auth.Forms.iOS/**/Release/Xamarin.Auth.XamarinForms.dll", 
						"./output/ios/"
					);
				CopyFiles
					(
						"./source/XamarinForms/Xamarin.Auth.Forms.iOS/**/Release/Xamarin.Auth.XamarinForms.pdb", 
						"./output/ios/"
					);
				//-------------------------------------------------------------------------------------
			} // if ( ! IsRunningOnWindows() )

			return;
		}
	);


Task ("libs-windows")
	.IsDependentOn ("libs-windows-projects")
	.IsDependentOn ("libs-windows-solutions")
	.Does 
	(
		() => 
		{
			return;
		}
	);

Task ("libs-windows-tooling")
	.Does 
	(
		() => 
		{	
			if (IsRunningOnWindows ()) 
			{	
				vsLatest  = VSWhereLatest();
				msBuildPathX64 = 
					(vsLatest==null)
					? null
					: vsLatest.CombineWithFilePath("./MSBuild/15.0/Bin/amd64/MSBuild.exe")
					;

				InformationFancy("msBuildPathX64       = " + msBuildPathX64);

				// FIX csc path is invalid 
				msBuildPathX64 = "C:/Program Files (x86)/Microsoft Visual Studio/2017/Community/MSBuild/15.0/Bin/MSBuild.exe";
				InformationFancy("msBuildPathX64 FIXED = " + msBuildPathX64);
			}
			
			return;
		}
	);

Task ("libs-windows-filesystem")
	.IsDependentOn ("nuget-restore")
	.Does 
	(
		() => 
		{				
			CreateDirectory ("./output/");
			CreateDirectory ("./output/pcl/");
			CreateDirectory ("./output/android/");
			CreateDirectory ("./output/ios/");
			CreateDirectory ("./output/wp80/");
			CreateDirectory ("./output/wp81/");
			CreateDirectory ("./output/wpa81/");
			CreateDirectory ("./output/wpa81/Xamarin.Auth/");
			CreateDirectory ("./output/win81/");
			CreateDirectory ("./output/win81/Xamarin.Auth/");
			CreateDirectory ("./output/uap10.0/");
			CreateDirectory ("./output/uap10.0/Xamarin.Auth/");
		}
	);

Task ("libs-windows-solutions")
	.IsDependentOn ("nuget-restore")
	.IsDependentOn ("libs-windows-filesystem")
	.IsDependentOn ("libs-windows-tooling")
	.Does 
	(
		() => 
		{	
			if (IsRunningOnWindows ()) 
			{	
				foreach(string sln_prj in source_solutions)
				{
					if (sln_prj.Contains("Xamarin.Auth-Library.sln"))
					{
						// Xamarin.Auth-Library.sln contains all projects
						// cannot be built xplatform
						
						continue;
					}
					BuildLoop(sln_prj, new MSBuildSettings{});
				}
			
				GitLinkAction("./source/Xamarin.Auth-Library.sln");
				GitLinkAction("./source/Xamarin.Auth-Library-MacOSX-Xamarin.Studio.sln");

				return;
			} 
		}
	);

Task ("libs-windows-projects")
	.IsDependentOn ("nuget-restore")
	.IsDependentOn ("libs-windows-filesystem")
	.IsDependentOn ("libs-windows-tooling")
	.Does 
	(
		() => 
		{	
			if (IsRunningOnWindows ()) 
			{	
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Xamarin.Auth.LinkSource/Xamarin.Auth.LinkSource.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Default"]));
				}
				else
				{
					define = string.Join("%3B", defines["Default"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);


				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Xamarin.Auth.Portable/Xamarin.Auth.Portable.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Portable"]));
				}
				else
				{
					define = string.Join("%3B", defines["Portable"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Xamarin.Auth.Portable/**/Release/Xamarin.Auth.dll", 
						"./output/pcl/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.Portable/**/Release/Xamarin.Auth.pdb", 
						"./output/pcl/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Xamarin.Auth.XamarinAndroid/Xamarin.Auth.XamarinAndroid.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Xamarin.Android"]));
				}
				else
				{
					define = string.Join("%3B", defines["Xamarin.Android"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
						/*
						C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\Xamarin\Android\Xamarin.Android.Common.targets
						error : 
						Could not find android.jar for API Level 23. This means the Android SDK platform for API Level 23 is not installed. 
						Either install it in the Android SDK Manager (Tools > Open Android SDK Manager...), or change your Xamarin.Android 
						project to target an API version that is installed. 
						(C:\Program Files (x86)\Android\android-sdk\platforms\android-23\android.jar missing.) 
						*/
						ToolPath = msBuildPathX64,
						ToolVersion = MSBuildToolVersion.VS2015,
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Xamarin.Auth.XamarinAndroid/**/Release/Xamarin.Auth.dll", 
						"./output/android/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.XamarinAndroid/**/Release/Xamarin.Auth.pdb", 
						"./output/android/"
					);
				//-------------------------------------------------------------------------------------
				InformationFancy("msBuildPathX64 = " + msBuildPathX64);
				solution_or_project = "./source/Xamarin.Auth.XamarinIOS/Xamarin.Auth.XamarinIOS.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Xamarin.IOS"]));
				}
				else
				{
					define = string.Join("%3B", defines["Xamarin.IOS"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
						ToolPath = msBuildPathX64,
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Xamarin.Auth.XamarinIOS/**/Release/Xamarin.Auth.dll", 
						"./output/ios/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.XamarinIOS/**/Release/Xamarin.Auth.pdb", 
						"./output/ios/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Xamarin.Auth.WindowsPhone8/Xamarin.Auth.WindowsPhone8.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Windows Phone 8 Silverlight"]));
				}
				else
				{
					define = string.Join("%3B", defines["Windows Phone 8 Silverlight"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
						ToolVersion = MSBuildToolVersion.VS2015,
						PlatformTarget = PlatformTarget.x86,
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Xamarin.Auth.WindowsPhone8/**/Release/Xamarin.Auth.dll", 
						"./output/wp80/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.WindowsPhone8/**/Release/Xamarin.Auth.pdb", 
						"./output/wp80/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Xamarin.Auth.WindowsPhone81/Xamarin.Auth.WindowsPhone81.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Windows Phone 8 Silverlight"]));
				}
				else
				{
					define = string.Join("%3B", defines["Windows Phone 8 Silverlight"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
						ToolVersion = MSBuildToolVersion.VS2015,
						PlatformTarget = PlatformTarget.x86,
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Xamarin.Auth.WindowsPhone81/**/Release/Xamarin.Auth.dll", 
						"./output/wp81/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.WindowsPhone81/**/Release/Xamarin.Auth.pdb", 
						"./output/wp81/"
					);
				//-------------------------------------------------------------------------------------
				/*
					Dependencies omitted!! 
					.
					├── Release
					│   ├── Xamarin.Auth
					│   │   ├── WebAuthenticatorPage.xaml
					│   │   ├── WebAuthenticatorPage.xbf
					│   │   └── Xamarin.Auth.xr.xml
					│   ├── Xamarin.Auth.dll
					│   ├── Xamarin.Auth.pdb
					│   └── Xamarin.Auth.pri
				*/
				solution_or_project = "./source/Xamarin.Auth.WinRTWindows81/Xamarin.Auth.WinRTWindows81.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Windows 8.1 WinRT"]));
				}
				else
				{
					define = string.Join("%3B", defines["Windows 8.1 WinRT"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Xamarin.Auth.WinRTWindows81/bin/Release/Xamarin.Auth.dll", 
						"./output/win81/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.WinRTWindows81/bin/Release/Xamarin.Auth.pdb", 
						"./output/win81/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.WinRTWindows81/bin/Release/Xamarin.Auth.pri", 
						"./output/win81/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.WinRTWindows81/bin/Release/Xamarin.Auth/Xamarin.Auth.xr.xml", 
						"./output/win81/Xamarin.Auth/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.WinRTWindows81/bin/Release/Xamarin.Auth/WebAuthenticatorPage.xaml", 
						"./output/win81/Xamarin.Auth/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.WinRTWindows81/bin/Release/Xamarin.Auth/WebAuthenticatorPage.xbf", 
						"./output/win81/Xamarin.Auth/"
					);
				//-------------------------------------------------------------------------------------
				/*
					Dependencies omitted!! 
					.
					├── Release
					│   ├── Xamarin.Auth
					│   │   ├── WebAuthenticatorPage.xaml
					│   │   ├── WebAuthenticatorPage.xbf
					│   │   └── Xamarin.Auth.xr.xml
					│   ├── Xamarin.Auth.dll
					│   ├── Xamarin.Auth.pdb
					│   └── Xamarin.Auth.pri
				*/
				solution_or_project = "./source/Xamarin.Auth.WinRTWindowsPhone81/Xamarin.Auth.WinRTWindowsPhone81.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Windows Phone 8.1 WinRT"]));
				}
				else
				{
					define = string.Join("%3B", defines["Windows Phone 8.1 WinRT"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
						ToolVersion = MSBuildToolVersion.VS2015,
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Xamarin.Auth.WinRTWindowsPhone81/bin/Release/Xamarin.Auth.dll", 
						"./output/wpa81/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.WinRTWindowsPhone81/bin/Release/Xamarin.Auth.pdb", 
						"./output/wpa81/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.WinRTWindowsPhone81/bin/Release/Xamarin.Auth.pri", 
						"./output/wpa81/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.WinRTWindowsPhone81/bin/Release/Xamarin.Auth/Xamarin.Auth.xr.xml", 
						"./output/wpa81/Xamarin.Auth/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.WinRTWindowsPhone81/bin/Release/Xamarin.Auth/WebAuthenticatorPage.xaml", 
						"./output/wpa81/Xamarin.Auth/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.WinRTWindowsPhone81/bin/Release/Xamarin.Auth/WebAuthenticatorPage.xbf", 
						"./output/wpa81/Xamarin.Auth/"
					);
				//-------------------------------------------------------------------------------------
				/*
					Dependencies omitted!! 
					.
					├── Release
					│   ├── Xamarin.Auth
					│   │   ├── WebAuthenticatorPage.xaml
					│   │   └── Xamarin.Auth.xr.xml
					│   ├── Xamarin.Auth.dll
					│   ├── Xamarin.Auth.pdb
					│   └── Xamarin.Auth.pri
				*/
				solution_or_project = "./source/Xamarin.Auth.UniversalWindowsPlatform/Xamarin.Auth.UniversalWindowsPlatform.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Universal Windows Platform"]));
				}
				else
				{
					define = string.Join("%3B", defines["Universal Windows Platform"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
						ToolVersion = MSBuildToolVersion.VS2015,
					}.WithProperty("DefineConstants", define)
				);
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
						ToolVersion = MSBuildToolVersion.VS2017,
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Xamarin.Auth.UniversalWindowsPlatform/bin/Release/Xamarin.Auth.dll", 
						"./output/uap10.0/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.UniversalWindowsPlatform/bin/Release/Xamarin.Auth.pdb", 
						"./output/uap10.0/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.UniversalWindowsPlatform/bin/Release/Xamarin.Auth.pri", 
						"./output/uap10.0/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.UniversalWindowsPlatform/bin/Release/Xamarin.Auth/Xamarin.Auth.xr.xml", 
						"./output/uap10.0/Xamarin.Auth/"
					);
				CopyFiles
					(
						"./source/Xamarin.Auth.UniversalWindowsPlatform/bin/Release/Xamarin.Auth/WebAuthenticatorPage.xaml", 
						"./output/uap10.0/Xamarin.Auth/"
					);
				/*
					.net Native - Linking stuff - not needed
				CopyFiles
					(
						"./source/Xamarin.Auth.UniversalWindowsPlatform/bin/Release/Properties/Xamarin.Auth.rd.xml", 
						"./output/uap10.0/Properties/"
					);
				*/
				//-------------------------------------------------------------------------------------


				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Extensions/Xamarin.Auth.Extensions.Portable/Xamarin.Auth.Extensions.Portable.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Portable"]));
				}
				else
				{
					define = string.Join("%3B", defines["Portable"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Extensions/Xamarin.Auth.Extensions.Portable/**/Release/Xamarin.Auth.Extensions.dll", 
						"./output/pcl/"
					);
				CopyFiles
					(
						"./source/Extensions/Xamarin.Auth.Extensions.Portable/**/Release/Xamarin.Auth.Extensions.pdb", 
						"./output/pcl/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Extensions/Xamarin.Auth.Extensions.XamarinAndroid/Xamarin.Auth.Extensions.XamarinAndroid.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Xamarin.Android"]));
				}
				else
				{
					define = string.Join("%3B", defines["Xamarin.Android"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
						/*
						C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\Xamarin\Android\Xamarin.Android.Common.targets
						error : 
						Could not find android.jar for API Level 23. This means the Android SDK platform for API Level 23 is not installed. 
						Either install it in the Android SDK Manager (Tools > Open Android SDK Manager...), or change your Xamarin.Android 
						project to target an API version that is installed. 
						(C:\Program Files (x86)\Android\android-sdk\platforms\android-23\android.jar missing.) 
						*/
						ToolPath = msBuildPathX64,
						ToolVersion = MSBuildToolVersion.VS2015,
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Extensions/Xamarin.Auth.Extensions.XamarinAndroid/**/Release/Xamarin.Auth.Extensions.dll", 
						"./output/android/"
					);
				CopyFiles
					(
						"./source/Extensions/Xamarin.Auth.Extensions.XamarinAndroid/**/Release/Xamarin.Auth.Extensions.pdb", 
						"./output/android/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/Extensions/Xamarin.Auth.Extensions.XamarinIOS/Xamarin.Auth.Extensions.XamarinIOS.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Xamarin.IOS"]));
				}
				else
				{
					define = string.Join("%3B", defines["Xamarin.IOS"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{						
						ToolPath = msBuildPathX64,
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/Extensions/Xamarin.Auth.Extensions.XamarinIOS/**/Release/Xamarin.Auth.Extensions.dll", 
						"./output/ios/"
					);
				CopyFiles
					(
						"./source/Extensions/Xamarin.Auth.Extensions.XamarinIOS/**/Release/Xamarin.Auth.Extensions.pdb", 
						"./output/ios/"
					);
				//-------------------------------------------------------------------------------------
				
				
				
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/XamarinForms/Xamarin.Auth.Forms/Xamarin.Auth.Forms.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Portable"]));
				}
				else
				{
					define = string.Join("%3B", defines["Portable"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/XamarinForms/Xamarin.Auth.Forms/**/Release/Xamarin.Auth.XamarinForms.dll", 
						"./output/pcl/"
					);
				CopyFiles
					(
						"./source/XamarinForms/Xamarin.Auth.Forms/**/Release/Xamarin.Auth.XamarinForms.pdb", 
						"./output/pcl/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/XamarinForms/Xamarin.Auth.Forms.Droid/Xamarin.Auth.Forms.Droid.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Xamarin.Android"]));
				}
				else
				{
					define = string.Join("%3B", defines["Xamarin.Android"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
						/*
						C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\Xamarin\Android\Xamarin.Android.Common.targets
						error : 
						Could not find android.jar for API Level 23. This means the Android SDK platform for API Level 23 is not installed. 
						Either install it in the Android SDK Manager (Tools > Open Android SDK Manager...), or change your Xamarin.Android 
						project to target an API version that is installed. 
						(C:\Program Files (x86)\Android\android-sdk\platforms\android-23\android.jar missing.) 
						*/
						ToolPath = msBuildPathX64,
						ToolVersion = MSBuildToolVersion.VS2015,
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/XamarinForms/Xamarin.Auth.Forms.Droid/**/Release/Xamarin.Auth.XamarinForms.dll", 
						"./output/android/"
					);
				CopyFiles
					(
						"./source/XamarinForms/Xamarin.Auth.Forms.Droid/**/Release/Xamarin.Auth.XamarinForms.pdb", 
						"./output/android/"
					);
				//-------------------------------------------------------------------------------------
				solution_or_project = "./source/XamarinForms/Xamarin.Auth.Forms.iOS/Xamarin.Auth.Forms.iOS.csproj";
				if (is_using_custom_defines == true)
				{
					define = string.Join("%3B", defines["Custom"].Concat(defines["Xamarin.IOS"]));
				}
				else
				{
					define = string.Join("%3B", defines["Xamarin.IOS"]);
				}
				BuildLoop
				(
					solution_or_project, 
					new MSBuildSettings
					{
						ToolPath = msBuildPathX64,
					}.WithProperty("DefineConstants", define)
				);

				CopyFiles
					(
						"./source/XamarinForms/Xamarin.Auth.Forms.iOS/**/Release/Xamarin.Auth.XamarinForms.dll", 
						"./output/ios/"
					);
				CopyFiles
					(
						"./source/XamarinForms/Xamarin.Auth.Forms.iOS/**/Release/Xamarin.Auth.XamarinForms.pdb", 
						"./output/ios/"
					);
				//-------------------------------------------------------------------------------------
			}

			return;
		}
	);




Task ("samples")
	.Does 
	(
		() => 
		{
			if ( IsRunningOnWindows() )
			{
				RunTarget ("samples-windows");
			}
			RunTarget ("samples-macosx");
		}
	);

Task ("samples-macosx")
	.IsDependentOn ("samples-nuget-restore")
	.IsDependentOn ("libs")
	.Does 
	(
		() => 
		{
			foreach (string sample_solution in sample_solutions_macosx)
			{
				foreach (string configuration in build_configurations)
				{
					if ( IsRunningOnWindows() )
					{
						MSBuild
							(
								sample_solution, 
								c => 
								{
									c.SetConfiguration(configuration);
								}
							);						
					}
					else
					{
						MSBuild
							(
								sample_solution, 
								c => 
								{
									c.SetConfiguration(configuration);
								}
							);						
					}
				}
			}

			return;
		}
	);

Task ("samples-windows")
	.IsDependentOn ("samples-nuget-restore")
	.IsDependentOn ("libs")
	.Does 
	(
		() => 
		{
			foreach (string sample_solution in sample_solutions_windows)
			{
				foreach (string configuration in build_configurations)
				{
					if ( IsRunningOnWindows() )
					{
					}
					else
					{
						MSBuild
							(
								sample_solution, 
								c => 
								{
									c.SetConfiguration(configuration);
								}
							);						
					}
				}
			}

			return;
		}
	);
	
Task ("nuget")
	.IsDependentOn ("libs")
	.Does 
	(
		() => 
		{
			if 
			(
				! FileExists("./output/wp80/Xamarin.Auth.dll")
				||
				! FileExists("./output/wp81/Xamarin.Auth.dll")
				||
				! FileExists("./output/win81/Xamarin.Auth.dll")
				||
				! FileExists("./output/wpa81/Xamarin.Auth.dll")
				||
				! FileExists("./output/uap10.0/Xamarin.Auth.dll")
			)
			{
				string msg =
				"Missing Windows dll artifacts"
				+ System.Environment.NewLine +
				"Please, build on Windows first!";

				throw new System.ArgumentNullException(msg);
			}
			
			NuGetPack 
				(
					"./nuget/Xamarin.Auth.nuspec", 
					new NuGetPackSettings 
					{ 
						Verbosity = NuGetVerbosity.Detailed,
						OutputDirectory = "./output/",        
						BasePath = "./",
						ToolPath = nuget_tool_path,
						Symbols = true
					}
				);                
			NuGetPack 
				(
					"./nuget/Xamarin.Auth.XamarinForms.nuspec", 
					new NuGetPackSettings 
					{ 
						Verbosity = NuGetVerbosity.Detailed,
						OutputDirectory = "./output/",        
						BasePath = "./",
						ToolPath = nuget_tool_path,
						Symbols = true
					}
				);                
			NuGetPack 
				(
					"./nuget/Xamarin.Auth.Extensions.nuspec", 
					new NuGetPackSettings 
					{ 
						Verbosity = NuGetVerbosity.Detailed,
						OutputDirectory = "./output/",        
						BasePath = "./",
						ToolPath = nuget_tool_path,
						Symbols = true
					}
				);                
		}
	);

Task ("externals")
	.Does 
	(
		() => 
		{
			return;
		}
	);

Task ("component")
	.IsDependentOn ("nuget")
	.IsDependentOn ("samples")
	.Does 
	(
		() => 
		{
			var COMPONENT_VERSION = "1.3.1.1";
			var yamls = GetFiles ("./**/component.yaml");

			foreach (var yaml in yamls) 
			{
				Information("yaml = " + yaml);
				var contents = FileReadText (yaml).Replace ("$version$", COMPONENT_VERSION);
				
				var fixedFile = yaml.GetDirectory ().CombineWithFilePath ("component.yaml");
				FileWriteText (fixedFile, contents);
				
				PackageComponent 
					(
						fixedFile.GetDirectory (), 
						new XamarinComponentSettings ()
					);
			}

			if (!DirectoryExists ("./output"))
			{
				CreateDirectory ("./output");
			}

			CopyFiles ("./component/**/*.xam", "./output");		
		}
	);

FilePath GetToolPath (FilePath toolPath)
{
    var appRoot = Context.Environment.GetApplicationRoot ();
     var appRootExe = appRoot.CombineWithFilePath (toolPath);
     if (FileExists (appRootExe))
	 {
         return appRootExe;
	 }

    throw new FileNotFoundException ("Unable to find tool: " + appRootExe); 
}


//=================================================================================================
// Put those 2 CI targets at the end of the file after all targets
// If those targets are before 1st RunTarget() call following error occusrs on 
//		*	MacOSX under Mono
//		*	Windows
// 
//	Task 'ci-osx' is dependent on task 'libs' which do not exist.
//
// Xamarin CI - Jenkins job targets
Task ("ci-osx")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    //.IsDependentOn ("samples")
	;
Task ("ci-windows")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    //.IsDependentOn ("samples")
	;	
//=================================================================================================

Task("Default")
    .Does
	(
		() =>
		{
			Information($"Arguments: ");
			Information($"\t\t TARGET: " + TARGET);
			Information($"\t\t VERBOSITY: " + VERBOSITY);

			Information($"Usage: " + Environment.NewLine);
			Information($"-v | --verbosity | --Verbosity ");
			Information($"-t | --target | --Target ");
			Information($"		Target task to be executed:");
			Information($"			libs			-	compile source (libs only");
			Information($"			libs-custom");
			Information($"			clean");
			Information($"			distclean");
			Information($"			rebuild");
			Information($"			build");
			Information($"			package");
			Information($"			nuget-restore");
			Information($"			nuget-update");
			Information($"			source-nuget-restore	- ");
			Information($"			samples-nuget-restore	-");
			Information($"			libs-macosx-filesystem	-");
			Information($"			libs-macosx				- ");
			Information($"			libs-macosx-solutions");
			Information($"			libs-macosx-projects");
			Information($"			libs-windows");
			Information($"			libs-windows-tooling");
			Information($"			libs-windows-filesystem");
			Information($"			libs-windows-solutions");
			Information($"			libs-windows-projects");
			Information($"			samples");
			Information($"			samples-macosx");
			Information($"			samples-windows");
			Information($"			nuget");
			Information($"			externals");
			Information($"			component");
			
			//verbosity = (VERBOSITY == null) : 

			RunTarget("nuget");
		}
	);


RunTarget("dump-environment");

RunTarget (TARGET);
