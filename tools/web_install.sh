#!/bin/bash

HTML_DIR=$1

umask 002
echo Installing web.zip...
unzip -o -d $HTML_DIR web.zip
