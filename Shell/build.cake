#load "utilities.shell.cake"

var target = Argument("target", "clean");

var GIT_PATH = EnvironmentVariable ("GIT_EXE") ?? (IsRunningOnWindows () ? "C:\\Program Files (x86)\\Git\\bin\\git.exe" : "git");
var REPO_URL = "git@github.com:Xamarin/Xamarin.Auth.git";
var REPO_BRANCH = "portable-bait-and-switch";
var REPO_COMMIT = "df9b0ab1cf3fbd81ea8a1aec9964300b87c2e962";  // moljac 2016-03-08 1.3-alpha-03

    
Task("external-01")
    .Does
        (
            () =>
            {
                Information($"target external");
                
                if (!DirectoryExists ("./source"))
                    CreateDirectory ("./source");

                StartProcess (GIT_PATH, "-C source init");
                StartProcess (GIT_PATH, "-C source remote add origin " + REPO_URL);
                StartProcess (GIT_PATH, "-C source fetch");
                StartProcess (GIT_PATH, "-C source checkout -tf origin/" + REPO_BRANCH);
                StartProcess (GIT_PATH, "-C source checkout -f " + REPO_COMMIT);
                // Cleaning will wipe out artifacts from upstream build
                //StartProcess (GIT_PATH, "-C source clean -fdx");
            }    
        );
        
Task("external-02")
    .Does
        (
            () =>
            {
                Information($"target external");
                
                Shell("rm -fr ./source");
                
                Shell("ls -al");
                
                if (!DirectoryExists ("./source"))
                    CreateDirectory ("./source");

                string command = 
                        $"
                            # git -C source init
                            # git -C source remote add origin {REPO_URL}
                            # git -C source fetch
                            # git -C source checkout -tf origin/{REPO_BRANCH}
                            # git -C source checkout -f {REPO_COMMIT}
                            
                            git clone --recursive https://github.com/xamarin/xamarin.auth.git source
                         ";
                         
                Shell("ls -al");
                Shell(command);
            }    
        );
            
    
//RunTarget("clean");
//RunTarget("external-01");
RunTarget("external-02");
