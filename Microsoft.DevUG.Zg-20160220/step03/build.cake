var target = Argument<string>("target", "Package");
var config = Argument<string>("config", "Release");

Task("clean")
    .Does
        (
            () =>
            {
                Information("clean");
				// Clean directories.
				CleanDirectory("./output");
				CleanDirectory("./output/bin");
				CleanDirectories("./src/**/bin/" + config);
            }    
        );
    
Task("build")
    .Does
        (
            () =>
            {
                Information("build");
				if(IsRunningOnWindows())
				{
					// Build the solution using MSBuild.
					MSBuild	
					(
						"./Solution.sln", 
						settings => settings.SetConfiguration(config)
					);     
				}
				else
				{
					XBuild	
					(
						"./Solution.sln", 
						settings => settings.SetConfiguration(config)
					);     
				}
			}
		);
        
Task("rebuild")
    .IsDependentOn("clean")
    .IsDependentOn("build")
    .Does
        (
            () =>
            {
                Information("rebuild");
            }    
        );
        
		
		
Task("package")
    .IsDependentOn("build")
    .Does
        (
            () =>
            {
                Information("package");
				
				var path = "./App.XamarinAndroid/bin/" + config;    
				var files = 
						GetFiles(path + "/**/*.dll") 
						+ 
						GetFiles(path + "/**/*.exe")
						;

				// Copy all exe and dll files to the output directory.
				CopyFiles(files, "./output/bin");
				
				// Zip all files in the bin directory.
				Zip("./output/bin", "./output/build.zip");
            }    
        );
		
    
//RunTarget("clean");
RunTarget("package");
