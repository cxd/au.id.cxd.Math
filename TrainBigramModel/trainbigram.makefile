# specify list of sourcefiles here
source = Program.fs

# specify target file here.
output = TrainBigramModel.exe

# search paths

# targets can be
# one of
# exe, winexe, library or module
#
# 
target = --target:exe

opts = --tailcalls+

#embed the fsharp runtime.
static = --standalone

# example references
references = -r:System.Data.dll \
-r:System.Xml.dll \
-r:System.Core.dll \
-r:System.Numerics.dll \
-r:../lib/FSharp.PowerPack/gac/FSharp.PowerPack.dll \
-r:../lib/FSharp.PowerPack/gac/FSharp.PowerPack.Compatibility.dll \
-r:../lib/org.graphdrawing.graphml.dll \
-r:../lib/MathProvider.dll \
-r:../build/au.id.cxd.Math.dll \
-r:../build/au.id.cxd.Text.dll


all:
	mkdir -p ../build
	fsc $(references) $(target) --out:$(output) $(opts) $(source)
	mv $(output) ../build
	cp -f ../lib/org.graphdrawing.graphml.dll ../build
	cp -f ../lib/MathProvider.dll ../build
	cp -f ../lib/osx/osx.config ../build/MathProvider.dll.config
	cp -f ../lib/FSharp.PowerPack/gac/FSharp.PowerPack.dll ../build
	cp -f ../lib/FSharp.PowerPack/gac/FSharp.PowerPack.Compatibility.dll ../build


clean:
	
	rm ../$(output)

run:
	mono $(output)
	