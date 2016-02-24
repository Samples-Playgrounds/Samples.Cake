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
                Information("target = build");
            }    
        );
        
Task("rebuild")
    .IsDependentOn("clean")
    .IsDependentOn("build")
    .Does
        (
            () =>
            {
                Information("target = rebuild");
            }    
        );
        
Task("package")
    .IsDependentOn("build")
    .WithCriteria
        (
            DateTime.Now.DayOfWeek == DayOfWeek.Sunday
            //() => DateTime.Now.DayOfWeek == DayOfWeek.Sunday
        )
    .Does
        (
            () =>
            {
                Information("target = package");
            }    
        );
    
//RunTarget("clean");
//RunTarget("rebuild");
RunTarget("package");
