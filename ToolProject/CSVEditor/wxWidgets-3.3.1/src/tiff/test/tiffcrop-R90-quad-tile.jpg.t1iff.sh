#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/quad-tile.jpg.t1iff"
outfile="o-tiffcrop-R90-quad-tile.jpg.t1iff.tiff"
f_test_convert "$TIFFCROP -R90" $infile $outfile
f_tiffinfo_validate $outfile
