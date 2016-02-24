var target = Argument("target", "clean");

Task("clean")
    .Does
        (
            () =>
            {
                Information("target = clean");
            }    
        );
    
Task("build")
    .Does
        (
            () =>
            {
                Information("target = clean");
            }    
        );
        
Task("rebuild")
    .Does
        (
            () =>
            {
                Information("target = rebuild");
            }    
        );
    
RunTarget("clean");
//RunTarget("build");
//RunTarget("rebuild");
