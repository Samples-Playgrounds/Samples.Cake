# Cake build system

Microsoft DevUG Zagreb 2016-02-24

## Build Systems

	*	make
	*	CMake
	*	TFS Build 
	*	rake
	*	FAKE
	*	psake
	*	nake
		[https://github.com/yevhen/Nake](https://github.com/yevhen/Nake)		


## Intro

Cake (C# Make)

*	Domain = Build System
*	DSL = Cake Build Language
*	cross platform build automation system 
*	with a C# DSL to do things like 
	*	compiling code, 
	*	copy files/folders, 
	*	running unit tests, 
	*	compress files 
	*	build NuGet packages.
	*	....
*	built on top of the Roslyn and Mono compiler 
	*	enables user to write build scripts in C#.
*	Cross platform - available on
	*	Windows, 
	*	Linux and 
	*	OS X.

	
*	[http://cakebuild.net/](http://cakebuild.net/)	


### DSL Domain Specific Language

	*	various methods and properties which make up the Cake build language
	*	made up of script aliases, that offers specific functionality within 		
		the context of a Cake build script. A script alias is an extension 		
		method for ICakeContext.


[http://cakebuild.net/dsl](http://cakebuild.net/dsl)		



## Build Systems supported

build systems supported:
	*	Cake is built to behave in the same way on:
		*	local (own machine)
		*	CI systems	
			*	TFS, 
			*	Jenkins
			*	AppVeyor, 
			*	TeamCity, 
			*	VSO 


## Build Tools Supported

	*	MSBuild
	*	MSTest
	*	Xamarin tools
		*	xbuild
		*	mdtool
	*	xUnit
	*	NUnit
	*	NuGet
	*	ILMerge
	*	WiX 
	*	SignTool


## Open source

*	source code for Cake is hosted on GitHub 
	includes everything needed to build it

## Installation

### Chocolatey Package Manager 

Chocolatey Package version of Cake. 

	choco install cake.portable

### Install the Cake bootstrapper

The bootstrapper is used to download Cake and the tools required by 
the build script.

Windows

    Invoke-WebRequest http://cakebuild.net/bootstrapper/windows -OutFile build.ps1

Linux

    curl -Lsfo build.sh http://cakebuild.net/bootstrapper/linux
    wget -O build.sh http://cakebuild.net/bootstrapper/linux    
    
OS X

    curl -Lsfo build.sh http://cakebuild.net/bootstrapper/osx
    wget -O build.sh http://cakebuild.net/bootstrapper/linux    


	
## Sample

*	[https://github.com/cake-build/example/blob/master/build.cake](https://github.com/cake-build/example/blob/master/build.cake)


https://github.com/cake-build/bootstrapper/blob/master/res/scripts/build.cake

### Running

Cake.exe build.cake -target=Package -verbosity=diagnostic

### Task

	Task("")
		.Does
		(
			() =>
			{
				Information("Task {0}", );
			}
		);




## Advanced

### File inclusion


### AddIns

Addins han be fetched from any feed i.e.


	// Syntax of new URI package format:
	// packagemanager:[scheme:[//[user:password@]host[:port]][/]]?package=id[&query]

	// Examples of how to use the tool directive.
	#tool nuget:?package=ILMerge&version=2.14.1203

	// Example of how to use the addin directive.
	#addin nuget:http://mysource.org/?package=Cake.Foo&version=1.2.3&prerelease
	#addin nuget:?package=Cake.Foo
	#addin nuget:?package=Cake.Foo&version=1.2.3
	#addin nuget:?package=Cake.Foo&prerelease
	#addin nuget:https://myget.org/f/Cake/?package=Cake.Foo&prerelease

