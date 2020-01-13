# Khmer text corpus parser

A simple script to parse a text corpus in the Khmer language and convert it to a form that can be imported by FieldWorks.

## Instructions

* Install .Net Core SDK 3.0 or later
* Run `dotnet fsi src/Khmer/doit.fsx file1.txt file2.txt ... fileNNN.txt > all-output.txt` to create output file
* Run `dotnet fsi src/Khmer/split.fsx all-output.txt` to create `split-output-NNN.sfm` files
