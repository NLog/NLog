#!/bin/bash
BASEDIR=/home/groups/n/nl/nlog
cd $BASEDIR/htdocs && unzip -o $BASEDIR/website.zip
find $BASEDIR/htdocs -type d | grep -v "htdocs$" | xargs -izzz chmod 2775 zzz
find $BASEDIR/htdocs -type f | xargs chmod 0664 
