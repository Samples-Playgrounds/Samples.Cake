# Microsoft DevUG Zagreb 2016-02-24

*	build systems
	*	make
	*	rake
	*	fake
	*	nake


## Intro

http://cakebuild.net/

Cake (C# Make)

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


Domain - Build System

DSL - Cake Build Language

DSL Domain Specific Language

	*	various methods and properties which make up the Cake build language
	*	made up of script aliases, that offers specific functionality within 		
		the context of a Cake build script. A script alias is an extension 		
		method for ICakeContext.


http://cakebuild.net/dsl



Cross platform - available on

	*	Windows, 
	*	Linux and 
	*	OS X.


build systems supported:
	*Cake is built to behave in the same way on:
		*	local (own machine)
		*	CI systems	
			*	TFS, 
			*	Jenkins
			*	AppVeyor, 
			*	TeamCity, 
			*	VSO 


Build Tools Supported

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


Open source

*	source code for Cake is hosted on GitHub 
	includes everything needed to build it



# Installation


Chocolatey Package version of Cake. This means that you can simply do:

choco install cake.portable


Sample

*	[https://github.com/cake-build/example/blob/master/build.cake](https://github.com/cake-build/example/blob/master/build.cake)


https://github.com/cake-build/bootstrapper/blob/master/res/scripts/build.cake



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

