#!/usr/bin/env bash

##########################################################################
# Custom bootstrapper for Cake on Linux with .NET Core 2.0
##########################################################################

# Define directories.
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools
TOOLS_PROJ=$TOOLS_DIR/tools.csproj
DEFAULT_CAKE_VERSION=0.21.1
NETCOREFRAMEWORKSDK1=netcoreapp1.1
NETCOREFRAMEWORKSDK2=netcoreapp2

if [ "$CAKE_VERSION" = "" ]; then
    CAKE_VERSION=$DEFAULT_CAKE_VERSION
fi

CAKE_DLL=$TOOLS_DIR/Cake.CoreCLR.$CAKE_VERSION/cake.coreclr/$CAKE_VERSION/Cake.dll

# Define md5sum or md5 depending on Linux/OSX
MD5_EXE=
if [[ "$(uname -s)" == "Darwin" ]]; then
    MD5_EXE="md5 -r"
else
    MD5_EXE="md5sum"
fi

# Define default arguments.
SCRIPT="build.cake"
TARGET="Default"
CONFIGURATION="Release"
VERBOSITY="verbose"
DRYRUN=
SHOW_VERSION=false
SCRIPT_ARGUMENTS=()

# Parse arguments.
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        -t|--target) TARGET="$2"; shift ;;
        -c|--configuration) CONFIGURATION="$2"; shift ;;
        -v|--verbosity) VERBOSITY="$2"; shift ;;
        -d|--dryrun) DRYRUN="-dryrun" ;;
        --version) SHOW_VERSION=true ;;
        --) shift; SCRIPT_ARGUMENTS+=("$@"); break ;;
        *) SCRIPT_ARGUMENTS+=("$1") ;;
    esac
    shift
done

# Output cake version warning
echo "Running with Cake.CoreCLR '$CAKE_VERSION'. The version needs to be manually updated in the build script or set as environment variable CAKE_VERSION."

# Make sure the tools folder exist.
if [ ! -d "$TOOLS_DIR" ]; then
  mkdir "$TOOLS_DIR"
fi

# Make sure .NET Core is installed
if ! [ -x "$(command -v dotnet)" ]; then
    echo ".NET Core SDK needs to be installed"
fi
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
dotnet --info

if [[ $(dotnet --version) = 1.1.0 ]]; then
    NETCOREFRAMEWORK=$NETCOREFRAMEWORKSDK1
else
    NETCOREFRAMEWORK=$NETCOREFRAMEWORKSDK2
fi

# Restore Cake.CoreCLR
if [ ! -f "$CAKE_DLL" ]; then
    if [ ! -f "$TOOLS_PROJ" ]; then
        echo "Creating dummy dotnet project"
        dotnet new classlib -f $NETCOREFRAMEWORK -o $TOOLS_DIR
    fi
    dotnet add "$TOOLS_DIR" package Cake.CoreCLR -v "$CAKE_VERSION" --package-directory $TOOLS_DIR/Cake.CoreCLR.$CAKE_VERSION
fi

# Make sure that Cake has been installed.
if [ ! -f "$CAKE_DLL" ]; then
    echo "Could not find Cake DLL at '$CAKE_DLL'."
    exit 1
fi

# Start Cake
if $SHOW_VERSION; then
    exec dotnet "$CAKE_DLL" -version
else
    exec dotnet "$CAKE_DLL" $SCRIPT -verbosity=$VERBOSITY -configuration=$CONFIGURATION -target=$TARGET $DRYRUN "${SCRIPT_ARGUMENTS[@]}"
fi