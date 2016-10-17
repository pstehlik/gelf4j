
function Delete-Directory($directoryName){
	Remove-Item -Force -Recurse $directoryName -ErrorAction SilentlyContinue
}

function Create-Directory($directoryName){
	New-Item $directoryName -ItemType Directory | Out-Null
}

function Get-RegistryValues($key) {
  (Get-Item $key -ErrorAction SilentlyContinue).GetValueNames()
}

function Get-RegistryValue($key, $value) {
    (Get-ItemProperty $key $value -ErrorAction SilentlyContinue).$value
}

function AddType{
	Add-Type -TypeDefinition "
	using System;
	using System.Runtime.InteropServices;
	public static class Win32Api
	{
	    [DllImport(""Kernel32.dll"", EntryPoint = ""IsWow64Process"")]
	    [return: MarshalAs(UnmanagedType.Bool)]
	    public static extern bool IsWow64Process(
	        [In] IntPtr hProcess,
	        [Out, MarshalAs(UnmanagedType.Bool)] out bool wow64Process
	    );
	}
	"
}

function Is64BitOS{
    return (Test-64BitProcess) -or (Test-Wow64)
}

function Is64BitProcess{
    return [IntPtr]::Size -eq 8
}

function IsWow64{
    if ([Environment]::OSVersion.Version.Major -eq 5 -and
        [Environment]::OSVersion.Version.Major -ge 1 -or
        [Environment]::OSVersion.Version.Major -ge 6)
    {
		AddType
        $process = [System.Diagnostics.Process]::GetCurrentProcess()

        $wow64Process = $false

        if ([Win32Api]::IsWow64Process($process.Handle, [ref]$wow64Process) -eq $true)
        {
            return $true
        }
		else
		{
			return $false
		}
    }
    else
    {
        return $false
    }
}

 $ilMergeExec = ".\tools\IlMerge\ilmerge.exe"
function Ilmerge($key, $directory, $name, $assemblies, $attributeAssembly, $extension, $ilmergeTargetframework, $logFileName, $excludeFilePath){

    new-item -path $directory -name "temp_merge" -type directory -ErrorAction SilentlyContinue

	if($attributeAssembly -ne ""){
    	&$ilMergeExec /keyfile:$key /out:"$directory\temp_merge\$name.$extension" /log:$logFileName /internalize:$excludeFilePath /attr:$attributeAssembly $ilmergeTargetframework $assemblies
	}
	else{
		&$ilMergeExec /keyfile:$key /out:"$directory\temp_merge\$name.$extension" /log:$logFileName /internalize:$excludeFilePath $ilmergeTargetframework $assemblies
	}
    Get-ChildItem "$directory\temp_merge\**" -Include *.$extension, *.pdb, *.xml | Copy-Item -Destination $directory
    Remove-Item "$directory\temp_merge" -Recurse -ErrorAction SilentlyContinue
}

function Generate-Assembly-Info{

param(
	[string]$assemblyTitle,
	[string]$assemblyDescription,
	[string]$clsCompliant = "true",
	[string]$internalsVisibleTo = "",
	[string]$configuration,
	[string]$company,
	[string]$product,
	[string]$copyright,
	[string]$version,
	[string]$fileVersion,
	[string]$infoVersion,
	[string]$file = $(throw "file is a required parameter.")
)
	if($infoVersion -eq ""){
		$infoVersion = $fileVersion
	}

	$asmInfo = "using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle(""$assemblyTitle"")]
[assembly: AssemblyDescription(""$assemblyDescription"")]
[assembly: AssemblyVersion(""$version"")]
[assembly: AssemblyFileVersion(""$fileVersion"")]
[assembly: AssemblyCopyright(""$copyright"")]
[assembly: AssemblyProduct(""$product"")]
[assembly: AssemblyCompany(""$company"")]
[assembly: AssemblyConfiguration(""$configuration"")]
[assembly: AssemblyInformationalVersion(""$infoVersion"")]
[assembly: ComVisible(false)]
"

	if($clsCompliant.ToLower() -eq "true"){
		 $asmInfo += "[assembly: CLSCompliantAttribute($clsCompliant)]
"
	}

	if($internalsVisibleTo -ne ""){
		$asmInfo += "[assembly: InternalsVisibleTo(""$internalsVisibleTo"")]
"
	}



	$dir = [System.IO.Path]::GetDirectoryName($file)

	if ([System.IO.Directory]::Exists($dir) -eq $false)
	{
		Write-Host "Creating directory $dir"
		[System.IO.Directory]::CreateDirectory($dir)
	}
	Write-Host "Generating assembly info file: $file"
	Write-Output $asmInfo > $file
}

function Package-Legacy($UploadPackage){
    import-module $toolsDir\NuGet\packit.psm1
	Write-Output "Loading the module for packing.............."
	$packit.push_to_nuget = $UploadPackage
	$packit.nugetKey  = $NugetKey

	$packit.framework_Isolated_Binaries_Loc = "$baseDir\release"
	$packit.PackagingArtifactsRoot = "$baseDir\release\PackagingArtifacts"
	$packit.packageOutPutDir = "$baseDir\release\packages"
	$packit.targeted_Frameworks = "net45";

	#region Packing
	$packit.package_description = "GELF log4net UdpAppender, AmqpAppender, HttpAppender - graylog2. Built for log4net"
	$script:packit.package_owners = "micahlmartin, jjchiw, contributors"
	$script:packit.package_authors = "micahlmartin, jjchiw, contributors"
	$script:packit.release_notes = ""
	$script:packit.package_licenseUrl = "https://opensource.org/licenses/MIT"
	$script:packit.package_projectUrl = "https://github.com/jjchiw/gelf4net"
	$script:packit.package_tags = "tools utilities gelf graylog log4net"
	$script:packit.package_iconUrl = "http://nuget.org/Content/Images/packageDefaultIcon.png"
	$script:packit.versionAssemblyName = $script:packit.binaries_Location + "\Gelf4Net.dll"
    invoke-packit "Gelf4Net" $PackageVersion @{"log4net"="2.0.0"} "binaries\Gelf4Net.dll" @{}
	#endregion

	remove-module packit
}

function Package-Amqp-Appender($UploadPackage){
    import-module $toolsDir\NuGet\packit.psm1
	Write-Output "Loading the module for packing.............."
	$packit.push_to_nuget = $UploadPackage
	$packit.nugetKey  = $NugetKey

	$packit.framework_Isolated_Binaries_Loc = "$baseDir\release"
	$packit.PackagingArtifactsRoot = "$baseDir\release\PackagingArtifacts"
	$packit.packageOutPutDir = "$baseDir\release\packages"
	$packit.targeted_Frameworks = "net45";

	#region Packing
	$packit.package_description = "GELF log4net AmqpAppender - graylog2. Built for log4net"
	$script:packit.package_owners = "micahlmartin, jjchiw, contributors"
	$script:packit.package_authors = "micahlmartin, jjchiw, contributors"
	$script:packit.release_notes = ""
	$script:packit.package_licenseUrl = "https://opensource.org/licenses/MIT"
	$script:packit.package_projectUrl = "https://github.com/jjchiw/gelf4net"
	$script:packit.package_tags = "tools utilities gelf graylog log4net"
	$script:packit.package_iconUrl = "http://nuget.org/Content/Images/packageDefaultIcon.png"
	$script:packit.versionAssemblyName = $script:packit.binaries_Location + "\Gelf4Net.AmqpAppender.dll"
    invoke-packit "Gelf4Net.AmqpAppender" $PackageVersion @{"log4net"="2.0.0"} "binaries\Gelf4Net.AmqpAppender.dll" @{}
	#endregion

	remove-module packit
}

function Package-Udp-Appender($UploadPackage){
    import-module $toolsDir\NuGet\packit.psm1
	Write-Output "Loading the module for packing.............."
	$packit.push_to_nuget = $UploadPackage
	$packit.nugetKey  = $NugetKey

	$packit.framework_Isolated_Binaries_Loc = "$baseDir\release"
	$packit.PackagingArtifactsRoot = "$baseDir\release\PackagingArtifacts"
	$packit.packageOutPutDir = "$baseDir\release\packages"
	$packit.targeted_Frameworks = "net45";

	#region Packing
	$packit.package_description = "GELF log4net UdpAppender - graylog2. Built for log4net"
	$script:packit.package_owners = "micahlmartin, jjchiw, contributors"
	$script:packit.package_authors = "micahlmartin, jjchiw, contributors"
	$script:packit.release_notes = ""
	$script:packit.package_licenseUrl = "https://opensource.org/licenses/MIT"
	$script:packit.package_projectUrl = "https://github.com/jjchiw/gelf4net"
	$script:packit.package_tags = "tools utilities gelf graylog log4net"
	$script:packit.package_iconUrl = "http://nuget.org/Content/Images/packageDefaultIcon.png"
	$script:packit.versionAssemblyName = $script:packit.binaries_Location + "\Gelf4Net.UdpAppender.dll"
    invoke-packit "Gelf4Net.UdpAppender" $PackageVersion @{"log4net"="2.0.0"} "binaries\Gelf4Net.UdpAppender.dll" @{}
	#endregion

	remove-module packit
}

function Package-Http-Appender($UploadPackage){
    import-module $toolsDir\NuGet\packit.psm1
	Write-Output "Loading the module for packing.............."
	$packit.push_to_nuget = $UploadPackage
	$packit.nugetKey  = $NugetKey

	$packit.framework_Isolated_Binaries_Loc = "$baseDir\release"
	$packit.PackagingArtifactsRoot = "$baseDir\release\PackagingArtifacts"
	$packit.packageOutPutDir = "$baseDir\release\packages"
	$packit.targeted_Frameworks = "net45";

	#region Packing
	$packit.package_description = "GELF log4net HttpAppender - graylog2. Built for log4net"
	$script:packit.package_owners = "micahlmartin, jjchiw, contributors"
	$script:packit.package_authors = "micahlmartin, jjchiw, contributors"
	$script:packit.release_notes = ""
	$script:packit.package_licenseUrl = "https://opensource.org/licenses/MIT"
	$script:packit.package_projectUrl = "https://github.com/jjchiw/gelf4net"
	$script:packit.package_tags = "tools utilities gelf graylog log4net"
	$script:packit.package_iconUrl = "http://nuget.org/Content/Images/packageDefaultIcon.png"
	$script:packit.versionAssemblyName = $script:packit.binaries_Location + "\Gelf4Net.HttpAppender.dll"
    invoke-packit "Gelf4Net.HttpAppender" $PackageVersion @{"log4net"="2.0.0"; "Microsoft.Net.Http"="2.2.29"} "binaries\Gelf4Net.HttpAppender.dll" @{}
	#endregion

	remove-module packit
}