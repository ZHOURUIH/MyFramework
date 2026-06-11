#!/bin/sh
# Generated file, master is Makefile.am
. ${srcdir:-.}/common.sh
infile="$srcdir/images/quad-tile.jpg.t1iff"
outfile="o-tiffcrop-doubleflip-quad-tile.jpg.t1iff.tiff"
f_test_convert "$TIFFCROP -F both" $infile $outfile
f_tiffinfo_validate $outfile
