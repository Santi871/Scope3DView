using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// [MANDATORY] The following GUID is used as a unique identifier of the plugin
[assembly: Guid("35db5c57-a1a5-4504-b65f-c1bef3407eaa")]

// [MANDATORY] The assembly versioning
//Should be incremented for each new release build of a plugin
[assembly: AssemblyVersion("1.1.3.0")]
[assembly: AssemblyFileVersion("1.1.3.0")]

// [MANDATORY] The name of your plugin
[assembly: AssemblyTitle("Scope 3D View")]
// [MANDATORY] A short description of your plugin
[assembly: AssemblyDescription("Dockable real-time 3D telescope view based on GSPoint3D")]


// The following attributes are not required for the plugin per se, but are required by the official manifest meta data

// Your name
[assembly: AssemblyCompany("Santiago Vegega")]
// The product name that this plugin is part of
[assembly: AssemblyProduct("Scope3DView")]
[assembly: AssemblyCopyright("Copyright ©  2021")]

// The minimum Version of N.I.N.A. that this plugin is compatible with
[assembly: AssemblyMetadata("MinimumApplicationVersion", "1.11.0.1168")]

// The license your plugin code is using
[assembly: AssemblyMetadata("License", "GPL-3.0")]
// The url to the license
[assembly: AssemblyMetadata("LicenseURL", "https://www.gnu.org/licenses/gpl-3.0.txt")]
// The repository where your pluggin is hosted
[assembly: AssemblyMetadata("Repository", "https://github.com/Santi871/Scope3DView")]


// The following attributes are optional for the official manifest meta data

//[Optional] Your plugin homepage URL - omit if not applicaple
[assembly: AssemblyMetadata("Homepage", "")]

//[Optional] Common tags that quickly describe your plugin
[assembly: AssemblyMetadata("Tags", "Template,Sequencer")]

//[Optional] A link that will show a log of all changes in between your plugin's versions
[assembly: AssemblyMetadata("ChangelogURL", "https://github.com/Santi871/Scope3DView/releases/")]

//[Optional] The url to a featured logo that will be displayed in the plugin list next to the name
[assembly: AssemblyMetadata("FeaturedImageURL", "https://github.com/Santi871/Scope3DView/blob/master/Images/Screenshot1.png?raw=true")]
//[Optional] A url to an example screenshot of your plugin in action
[assembly: AssemblyMetadata("ScreenshotURL", "https://github.com/Santi871/Scope3DView/blob/master/Images/Screenshot2.png?raw=true")]
//[Optional] An additional url to an example example screenshot of your plugin in action
[assembly: AssemblyMetadata("AltScreenshotURL", "https://github.com/Santi871/Scope3DView/blob/master/Images/Screenshot3.png?raw=true")]
//[Optional] An in-depth description of your plugin
[assembly: AssemblyMetadata("LongDescription", @"This plugin adds a dockable 3D view of the connected telescope to the imaging tab. Ported from [GSPoint3D](https://github.com/rmorgan001/GS.Point3d).

The dockable can be enabled in the imaging tab by clicking the '3D' icon in the info bar.")]


// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
// [Unused]
[assembly: AssemblyConfiguration("")]
// [Unused]
[assembly: AssemblyTrademark("")]
// [Unused]
[assembly: AssemblyCulture("")]