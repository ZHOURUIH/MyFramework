#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/ojpeg_zackthecat_subsamp22_single_strip.tiff"
outfile="o-tiffcrop-R90-ojpeg_zackthecat_subsamp22_single_strip.tiff"
f_test_convert "$TIFFCROP -R90" $infile $outfile
f_tiffinfo_validate $outfile
