ctreeExport
===========

This repository contains R-code to export a ctree to a textfile and C# code to import the text file and use the tree-structure in C# projects.

Ctrees are Conditional Inference Trees as described in the R package partykit (see http://cran.r-project.org/web/packages/partykit/partykit.pdf).

Party.R contains the R-functions
exportCtreeExample.r contains example code on how to create a ctree using partykit and export it to a txt-file.

The folder "ImportCtree" contains the C# code that can be used to import the ctree in C#-programs and an interface to look up values in the tree.
