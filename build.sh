#!/bin/bash
SCRIPT_DIR=$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )
TOOLS_DIR=$SCRIPT_DIR/tools
NUGET_EXE=$TOOLS_DIR/nuget.exe
CAKE_EXE=$TOOLS_DIR/Cake/Cake.exe

# Define default arguments.
SCRIPT="build.cake"
TARGET="Default"
CONFIGURATION="Release"
VERBOSITY="verbose"
DRYRUN=false
SHOW_VERSION=false

DNXVERSION="1.0.0-rc1-update1"
DOWNLOAD_CAKE_BUILD_BOOTSTRAP="curl -Lsfo buildcake.sh http://cakebuild.net/bootstrapper/linux"

# Parse arguments.
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        -t|--target) TARGET="$2"; shift ;;
        -c|--configuration) CONFIGURATION="$2"; shift ;;
        -v|--verbosity) VERBOSITY="$2"; shift ;;
        -d|--dryrun) DRYRUN=true ;;
        -d|--dnxVersion) DNXVERSION="$2"; shift ;;
        --version) SHOW_VERSION=true ;;
    esac
    shift
done

if [ ! -f ~/.dnx/dnvm/dnvm.sh ] 
then
echo "Downloading dnvm"
curl -sSL https://raw.githubusercontent.com/aspnet/Home/dev/dnvminstall.sh | DNX_BRANCH=dev sh && source ~/.dnx/dnvm/dnvm.sh
else
echo "Using installed dnvm.sh script"
source ~/.dnx/dnvm/dnvm.sh
fi

dnvm install $DNXVERSION -a x64 -r coreclr
dnvm install $DNXVERSION -a x64 -r mono -alias default

dnvm use default

#echo "Downloading Cake bootstrap script"
#curl -Lsfo buildcake.sh http://cakebuild.net/bootstrapper/linux
#chmod u+x ./buildcake.sh

echo "Running cake"
#./buildcake.sh "build.cake" $@
mono "$CAKE_EXE" $SCRIPT -verbosity=$VERBOSITY -configuration=$CONFIGURATION -target=$TARGET -dnxVersion=$DNXVERSION