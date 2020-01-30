#addin nuget:?package=Cake.Android.SdkManager

string JAVA_HOME = EnvironmentVariable ("JAVA_HOME") ?? Argument ("java_home", "");
string ANDROID_HOME = EnvironmentVariable ("ANDROID_HOME") ?? Argument ("android_home", "");
string ANDROID_SDK_ROOT = EnvironmentVariable ("ANDROID_SDK_ROOT") ?? Argument ("android_sdk_root", "");

/*
ANDROID_HOME    C:\Android\android-sdk
*/

// Log some variables
Information ($"JAVA_HOME            : {JAVA_HOME}");
Information ($"ANDROID_HOME         : {ANDROID_HOME}");
Information ($"ANDROID_SDK_ROOT     : {ANDROID_SDK_ROOT}");

// Resolve Xamarin.Android installation
var XAMARIN_ANDROID_PATH = EnvironmentVariable ("XAMARIN_ANDROID_PATH");
var ANDROID_SDK_BASE_VERSION = "v1.0";
var ANDROID_SDK_VERSION = "v9.0";
if (string.IsNullOrEmpty(XAMARIN_ANDROID_PATH)) {
	if (IsRunningOnWindows()) {
		var vsInstallPath = VSWhereLatest(new VSWhereLatestSettings { Requires = "Component.Xamarin", IncludePrerelease = true });
		XAMARIN_ANDROID_PATH = vsInstallPath.Combine("Common7/IDE/ReferenceAssemblies/Microsoft/Framework/MonoAndroid").FullPath;
	} else {
		if (DirectoryExists("/Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/xamarin.android/xbuild-frameworks/MonoAndroid"))
			XAMARIN_ANDROID_PATH = "/Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/xamarin.android/xbuild-frameworks/MonoAndroid";
		else
			XAMARIN_ANDROID_PATH = "/Library/Frameworks/Xamarin.Android.framework/Versions/Current/lib/xbuild-frameworks/MonoAndroid";
	}
}
if (!DirectoryExists($"{XAMARIN_ANDROID_PATH}/{ANDROID_SDK_VERSION}"))
	throw new Exception($"Unable to find Xamarin.Android {ANDROID_SDK_VERSION} at {XAMARIN_ANDROID_PATH}.");


Information($"XAMARIN_ANDROID_PATH = {XAMARIN_ANDROID_PATH}");
Information($"ANDROID_HOME = {ANDROID_HOME}");


// https://github.com/Redth/ZXing.Net.Mobile/blob/master/build.cake
// https://github.com/jamesmontemagno/GeolocatorPlugin/blob/master/build.cake

var androidSdkSettings = new AndroidSdkManagerToolSettings
                                        {
                                            SdkRoot = ANDROID_HOME,
                                            SkipVersionCheck = false
                                        };

AndroidSdkManagerList  list = AndroidSdkManagerList(androidSdkSettings);

AndroidSdkManagerInstall 
(
    new [] 
    { 
        "platforms;android-15",
        "platforms;android-23",
        "platforms;android-25",
        "platforms;android-26"
    }, 
    androidSdkSettings
);