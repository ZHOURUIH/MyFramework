#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/minisblack-2c-8b-alpha.tiff"
outfile="o-tiff2bw-minisblack-2c-8b-alpha.tiff"
f_test_convert "$TIFF2BW" $infile $outfile
f_tiffinfo_validate $outfile
