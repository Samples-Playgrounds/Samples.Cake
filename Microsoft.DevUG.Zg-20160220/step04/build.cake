//==============================================================
// Load directive
// load directive is used to reference external Cake scripts. 
//  Useful i.e. if you have common utility functions.
//
//  Usage
//
//  The directive has one parameter which is the location to 
//  the script.

#load "utilities.cake"
//# l "utilities.cake"
//==============================================================


//==============================================================
// Add-in directive
// reference assemblies
//  The add-in directive is used to install and reference assemblies 
//  using NuGet.
//
//  Usage
//
//  The directive takes a single package URI parameter 
//  Right now, only NuGet packages are supported.

//#addin nuget:?package=Cake.Foo
//#addin nuget:?package=Cake.Foo&version=1.2.3
//#addin nuget:?package=Cake.Foo&prerelease
//#addin nuget:https://myget.org/f/Cake/?package=Cake.Foo&prerelease
//==============================================================


//==============================================================
// Reference directive
//  The reference directive is used to reference external assemblies 
//  for use in your scripts.
//
//  Usage
//
//  The directive has one parameter which is the path to the dll to 
//  load.

//#r "bin/myassembly.dll"
//#reference "bin/myassembly.dll"
//==============================================================


//==============================================================
// Tool directive
// installs external command-line tools using NuGet.
//
//  Usage
//
//  The directive takes a single package URI parameter 
//  Right now, only NuGet packages are supported.

// #tool nuget:?package=Cake.Foo
// #tool nuget:?package=Cake.Foo&version=1.2.3
// #tool nuget:?package=Cake.Foo&prerelease
// #tool nuget:https://myget.org/f/Cake/?package=Cake.Foo&prerelease
//==============================================================
s
var target = Argument("target", "clean");
        
    
//RunTarget("clean");
RunTarget("rebuild");
