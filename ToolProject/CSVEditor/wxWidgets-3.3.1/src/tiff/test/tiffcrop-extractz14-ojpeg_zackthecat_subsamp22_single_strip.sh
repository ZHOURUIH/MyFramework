#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/ojpeg_zackthecat_subsamp22_single_strip.tiff"
outfile="o-tiffcrop-extractz14-ojpeg_zackthecat_subsamp22_single_strip.tiff"
f_test_convert "$TIFFCROP -E left -Z1:4,2:4" $infile $outfile
f_tiffinfo_validate $outfile
