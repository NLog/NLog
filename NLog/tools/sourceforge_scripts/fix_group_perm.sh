#!/bin/bash
BASEDIR=/home/groups/n/nl/nlog
find $BASEDIR/htdocs -type d | grep -v "htdocs$" | xargs -izzz chmod 2775 zzz
find $BASEDIR/htdocs -type f | xargs chmod 0664 
