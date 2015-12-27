#!/bin/bash

DNXVERSION="1.0.0-rc1-update1"
DOWNLOAD_CAKE_BUILD_BOOTSTRAP="curl -Lsfo buildcake.sh http://cakebuild.net/bootstrapper/linux"

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

echo "Downloading Cake bootstrap script"
curl -Lsfo buildcake.sh http://cakebuild.net/bootstrapper/linux
chmod u+x ./buildcake.sh

echo "Running cake"
./buildcake.sh "build.cake" $@