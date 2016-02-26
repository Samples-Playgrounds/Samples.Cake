Task("clean")
    .Does
    (
        () =>
        {
           Information("Test");
        }
    );
    
    
RunTarget("clean");

    
    