// This file is a script that can be executed with the F# Interactive.  
// It can be used to explore and test the library project.
// Note that script files will not be part of the project build.
#r @"..\lib\PorterStemmerAlgorithm.dll"
#load "WordTokeniser.fs"
#load "Edit1Counts.fs"

open System
open au.id.cxd.Text.Edit1Counts

let file = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\data\text-spelling\count_1edit.txt"
let dataFile = @"C:\Users\cd\Projects\au.id.cxd.ImageProcessor\au.id.cxd.ImageProcessor\au.id.cxd.Text\count1edit.bin"

let data = parseTextEditData file
storeEditData dataFile data

let data2 = readEditData dataFile

data2
