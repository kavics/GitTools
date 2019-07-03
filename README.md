# GitTools
GitTools is an experimental tool for discovering and manipulating multiple GitHub repositories simultaneously. It can be useful if the code owner has more than one repositories that need to be modified in parallel.

GitTools is a command line application that runs on your machine and uses git.exe.

# INSTALLATION

There is no installation kit yet, but it is relatively easy to obtain and configure the tool. Follow these instructions:

STEP 1: **Download** the latest binaries from [github](https://github.com/kavics/GitTools/releases "GitTools releases").

STEP 2: **Extract** to a local directory and memorize the full path of the folder. <br/>
For example: `C:\Tools\GitTools`.

STEP 3: **Set the environment** of the command shell: run a *Powershell* instance and extend the `Path` environment variable with the directory where you extracted the tool. <br/>
For example: `$env:Path += ";C:\Tools\GitTools"`
> Warning: it is important that you run this command only once.

STEP 4: Change the **current directory** to the **container** of your local github repositories. <br/>
For example: `CD C:\Projects\github\MyRepos`

STEP 5: **Run GitTools** the first time. The tool uses the preinstalled git.exe from a configurable location. Default locations: <br/>
```
C:\Program Files\Git\bin\git.exe
C:\Program Files (x86)\Git\bin\git.exe
```
If one of these exists, the tool can run, otherwise it displays this message:
```
GitT cannot run because git.exe was not found.
Please configure the full path of git.exe with the following command:
GitT Configure GitExe <fullpath>
```
In this case git.exe need to be configured using the `Configure` command (see below).

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
  ... <more commands are coming in the future>
```

# COMMANDS

## Configure

Customizes the GitTools environment. The settings are related to the current user and stored on the local machine.

### Usage:
```
GitT Configure [-G|-Git|-GitExe] [-I|-Internal|-InternalNuget] [-P|-Private|-PrivateNuget] [-?|...]
```

Without arguments the command queries the current setting. For example:
```
PS C:\Projects\github\MyRepos> GitT Configure
GitExe              D:\Program Files\Git\cmd
InternalNuget       (not configured yet)
PrivateNuget        (not configured yet)
```
### Arguments:
- **GitExe**: Customizes the full path of 'git.exe' on the local filesystem.
- **InternalNuget**: Customizes the full path of the internal corporate wide nuget container.
- **PrivateNuget**: Customizes the full path of the private nuget container on the local filesystem.
- **Help**: Displays this command's help text. All alternatives: ?, -?, /?, -h, -H, /h, /H, -help, --help


## Status

Enumerates the currend directory's first level subdirectories as github repository clones and writes their own status information. Every repository is a line with four colums:
- **Repository**: The name of the repository. Normally the directory name and github repository name are equal.
- **Current branch**: The name of the checked out branch. The 'develop' branch is indicated by the default color, the 'master' is light blue, any other branch is written in yellow.
- **Status**: Status of the current branch. If it is empty, there are no changes. The available characters in this column:
  - '+': number of file additions.
  - '~': numer of modified files.
  - '-': numer of file deletions.
  - '?': other file changes.
  - LOCAL: this indicates these changes are in a local branch which is not pushed and remote-tracked yet.
- **Modified/Last Fetch**: This is a freshness indicator. Information in this line is based on the time the local repository was synchronized.

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

Discovers nuget packages in all local repositories. If the `-R` (references) switch is on, the referenced, otherwise the emitted packages are listed. See examples:

View components:
```
PS C:\Projects\github\MyRepos> GitT Components
COMPONENTS
Component.Id                   Version        
============================== ========
MyComponent                    1.0.1          
...
```
The tool discovers the emitted packages in *.nuspec files and *.csproj files (in case of .net standard or .net core projects) in all repositories in the container in any depth. The list contains the package versions from the local disk. The -n (or -nuget) argument adds a column to the list with the latest published version. If this cell of a component is empty, the package has not been released yet.
> Warning. The -n (-nuget) option may make the tool run significantly slower.

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

Not only the emitted but the referenced components can be displayed too. The list is grouped by projects. The first line of a group shows the project name and the relative path of it's container. Subsequent lines contain the referenced nuget packages' names and versions. This list can be long so it can be filtered by a simple prefix.

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

- **Differences**: Lists packages that are different from the emitted package. Can be used only with reference.
- **Nuget**: Fetches every emitted component version from nuget.org.
- **Prefix** (followed by a text between quotation marks): Lists only packages whose identifier begins with this prefix.
- **References**: Fetches every component version from nuget.org.
- **Help**: Displays this command's help text. All alternatives: ?, -?, /?, -h, -H, /h, /H, -help, --help
