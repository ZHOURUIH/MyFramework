#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/ojpeg_chewey_subsamp21_multi_strip.tiff"
outfile="o-tiff2bw-ojpeg_chewey_subsamp21_multi_strip.tiff"
f_test_convert "$TIFF2BW" $infile $outfile
f_tiffinfo_validate $outfile
