#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/logluv-3c-16b.tiff"
outfile="o-tiff2bw-logluv-3c-16b.tiff"
f_test_convert "$TIFF2BW" $infile $outfile
f_tiffinfo_validate $outfile
