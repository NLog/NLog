#!/bin/bash
BASEDIR=/home/groups/n/nl/nlog
rm -rf $BASEDIR/htdocs/help
mkdir $BASEDIR/htdocs/help
cd $BASEDIR/htdocs/help && unzip -o $BASEDIR/help.zip
find $BASEDIR/htdocs/help -type d | xargs -izzz chmod 2775 zzz
find $BASEDIR/htdocs/help -type f | xargs chmod 0664 
