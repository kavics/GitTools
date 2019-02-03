# GitTools
GitTools is an experimental tool for discover or manipulation of multiple GitHub repositories simultaneously. It can be useful if the code owner has more than one repositories that need to be modified parallel.

GitTools is a command line application that runs in your machine and uses the git.exe.

# INSTALLATION

There is no installation kit yet, but it is relatively easy to obtain and configure. Follow these instructions:

STEP 1: **Download** the latest binaries from [github](https://github.com/kavics/GitTools/releases "GitTools releases")

STEP 2: **Extract** to the final place. Memorize the full path of the extraction directory. <br/>
For example: `C:\Tools\GitTools`.

STEP 3: **Set environment** of the command shell with extension of the **PATH** environment variable. Run a *Powershell* instance and extend the `Path` environment variable with your extract directory. <br/>
For example: `$env:Path += ";C:\Tools\GitTools"`
> Warning: it is important that you only run once this command.

STEP 4: Change the **current directory** to the container of your local github repositories. <br/>
For example: `CD C:\Projects\github\MyRepos`

STEP 5: **Run GitTools** first time. The tool uses the preinstalled git.exe that's location is configurable. Default locations: <br/>
```
C:\Program Files\Git\bin\git.exe
C:\Program Files (x86)\Git\bin\git.exe
```
If one of these exists, the tool can run, otherwise displays these lines:
```
GitT cannot run because git.exe was not found.
Please configure the full path of the git.exe with the following command:
GitT Configure GitExe <fullpath>
```
In this case, the git.exe need to be found and configured with the `Configure` command (see below).

# USAGE

GetTools executes subcommands. Command name is always the second word int the command line:<br/>
For example: ```> GitT Status```<br/>

These commands have their own arguments described below. 

## Help

These arguments mean the help request:
```
/? | -? | /h | -h | /help | -help | --help
```

Help argument can be used globally or after a command. Global help writes these lines:
```
Git Tools <version>
===================

Usage:
GitT </? | -? | /h | -h | /help | -help | --help>
GitT <command> [command-arguments]
GitT <command> </? | -? | /h | -h | /help | -help | --help>

Available commands:
  Components
  Configure
  Status
  ... <more commands if there are>
```

# COMMANDS

## Configure

Customizes the GitTools. The settings are user sensitive and stored in the local machine.

### Usage:
```
GitT Configure [-G|-Git|-GitExe] [-I|-Internal|-InternalNuget] [-P|-Private|-PrivateNuget] [-?|...]
```

Without any argument the command queries the current setting. For example:
```
PS C:\Projects\github\MyRepos> GitT Configure
GitExe              D:\Program Files\Git\cmd
InternalNuget       (not configured yet)
PrivateNuget        (not configured yet)
```
### Arguments:
- **GitExe**: Customizes the full path of the 'git.exe' on the local filesystem.
- **InternalNuget**: Customizes the full path of the internal corporate wide nuget container.
- **PrivateNuget**: Customizes the full path of the private nuget container on the local filesystem.
- **Help**: Displays this command's help text. All alternatives: ?, -?, /?, -h, -H, /h, /H, -help, --help


## Status

Enumerates the currend directory's first level subdirectories as github repository clones and writes their own status information. Every repository is a line with four colums:
- **Repository**: The name of the repository. Normally the directory name and github repository name are equal.
- **Current branch**: The name of the checked out branch. It is a color column.  The 'develop' is normal, the 'master' is light blue, any other branch is written by yellow color.
- **Status**: Status of the current branch. If it is empty, there is no any changes. The available characters in this column:
  - '+': number of file additions.
  - '~': numer of modified files.
  - '-': numer of file deletions.
  - '?': any othe file changes.
  - LOCAL: this indicates these changes are in a local branch which is not pushed and remote-tracked yet.
- **Modified/Last Fetch**: This is the freshness indicator. Information in this line is based on the local repository that is synchronized at the time that was written to this column.

Here is an example:
```
Repository  Current branch  Status             Modified/Last Fetch
=========== =============== ================== ===================
API         develop                            2018-08-15 18:10
DllViewer   master                             2018-08-15 18:13
GitTools    doc             +0 ~1 -0           3 minutes ago
KLOC        testability     +0 ~5 -0 ?1 LOCAL  2018-08-18 05:09
```

### Usage:
```
GitT Status [-F|-Fetch] [-?|...]
```
### Arguments:
- **Fetch**: Fetches every repository immediately.
- **Help**: Displays this command's help text. All alternatives: ?, -?, /?, -h, -H, /h, /H, -help, --help


## Components

Discovers the nuget packages in every local repositories. If the `-R` (references) switch is on, the referenced, otherwise the emitted packages are listed. See examples:

View components:
```
PS C:\Projects\github\MyRepos> GitT Components
COMPONENTS
Component.Id                   Version        
============================== ========
MyComponent                    1.0.1          
...
```
The tool discovers the emitted packages in the *.nuspec files and *.csproj files (in case of .net standard or .net core project) in every repository any depth. The list contains the package versions from the local disk. The -n (or -nuget) argument adds a column to the list with the latest published version. If this cell of a component is empty, the package has not been released yet.
> Warning. The -n (-nuget) option causes that the tool can run radically slower.

View released components:
```
PS C:\Projects\github\MyRepos> GitT Components -n
COMPONENTS
Component.Id                   Version  nuget.org
============================== ======== ==========
MyLoaclComponent               1.0.1 
MyComponent                    1.0.1    1.0.0
...
```

Not only the emitted but the referenced components can be displayed. The list is grouped by projects. The first lie of a group shows the project name and the relative path of it's container. After that the lines contain the referenced nuget packages names and versions. This list can be long so it can be filtered by a simple prefix.

View references filtered by a prefix:
```
PS C:\Projects\github\MyRepos> GitT Components -r -p "SenseNet.Tools"
REFERENCES
GetApi - API\src\GetApi
  SenseNet.Tools                                     2.1.1
GitT - GitTools\src\GitT
  NuGet.Core                                         2.14.0
  SenseNet.Tools                                     3.1.0
  ...
```

### Usage:
```
GitT Components [-D|-Diff|-Differences] [-N|-Nuget] [-P|-Prefix <prefix>] [-R|-Ref|-Refs|-References] [-?|...]
```

### Arguments:

- **Differences**: Lists only the package that is different from the emitted package. Can be used only with reference
- **Nuget**: Fetches every emitted component version from nuget.org.
- **Prefix** (followed by any text in quotation marks): Lists only the package whose identifier begins with this prefix.
- **References**: Fetches every component version from nuget.org.
- **Help**: Displays this command's help text. All alternatives: ?, -?, /?, -h, -H, /h, /H, -help, --help

