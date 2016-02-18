var target = Argument("target", "clean");

Task("clean")
    .Does
        (
            () =>
            {
                Information("target = {0}", this);
            }    
        );
    
Task("build")
    .Does
        (
            () =>
            {
                Information("target = {0}", this);
            }    
        );
        
Task("rebuild")
    .IsDependentOn("clean")
    .IsDependentOn("build")
    .Does
        (
            () =>
            {
                Information("target = {0}", this);
            }    
        );
        
    
//RunTarget("clean");
RunTarget("rebuild");
