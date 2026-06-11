#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/ojpeg_zackthecat_subsamp22_single_strip.tiff"
outfile="o-tiffcrop-extract-ojpeg_zackthecat_subsamp22_single_strip.tiff"
f_test_convert "$TIFFCROP -U px -E top -X 60 -Y 60" $infile $outfile
f_tiffinfo_validate $outfile
