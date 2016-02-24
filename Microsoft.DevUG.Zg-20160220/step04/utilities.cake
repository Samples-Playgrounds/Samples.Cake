
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
                Information("target = build");
            }    
        )
    .OnError
        (
            exception =>
            {
                // Handle the error here.
            }
        );        
        
Task("rebuild")
    .IsDependentOn("clean")
    .IsDependentOn("build")
    .ContinueOnError()  //  igonre errors
    .Does
        (
            () =>
            {
                Information("target = rebuild");
            }    
        )
    .ReportError
        (
            exception =>
            {  
                // Report the error.
            }
        )
    .Finally
        (
            () =>
            {  
                // execute egardless of how a task exited
            }
        );
        
// usually not done in included files
//  
// RunTarget("clean");
// RunTarget("rebuild");
