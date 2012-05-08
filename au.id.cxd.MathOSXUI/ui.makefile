# specify list of sourcefiles here
source = OSXUIBuilder.fs\
DualChartTypes.fs\
UIState.fs\
FileBrowser.fs\
FileMenu.fs\
EditMenu.fs\
ProjectMenu.fs\
MathMenu.fs\
TabbedDetailsView.fs\
ProjectTree.fs\
DataSourceTableView.fs\
Window.fs

	 
# specify target file here.
output = au.id.cxd.MathOSXUI.dll

appResource = ../build/ui

# search paths

# targets can be
# one of
# exe, winexe, library or module
#
# 
target = --target:library

opts = --tailcalls+

#embed the fsharp runtime.
static = --standalone

# example references
references = -r:System.Data.dll \
-r:System.Xml.dll \
-r:System.Core.dll \
-r:System.Numerics.dll \
-r:/Applications/MonoDevelop.app/Contents/MacOS/lib/monodevelop/AddIns/MonoDevelop.MonoMac/MonoMac.dll \
-r:../lib/FSharp.PowerPack/gac/FSharp.PowerPack.dll \
-r:../lib/FSharp.PowerPack/gac/FSharp.PowerPack.Compatibility.dll \
-r:../lib/org.graphdrawing.graphml.dll \
-r:../lib/MathProvider.dll \
-r:../build/au.id.cxd.Math.dll

all:
	mkdir -p ../build
	fsc $(references) $(target) --out:$(output) $(opts) $(source)
	mv $(output) $(appResource)
	cp -f ../lib/org.graphdrawing.graphml.dll $(appResource)
	cp -f ../lib/MathProvider.dll $(appResource)
	cp -f ../lib/osx/osx.config $(appResource)MathProvider.dll.config
	cp -f ../lib/FSharp.PowerPack/gac/FSharp.PowerPack.dll $(appResource)
	cp -f ../lib/FSharp.PowerPack/gac/FSharp.PowerPack.Compatibility.dll $(appResource)
	cp -f /Applications/MonoDevelop.app/Contents/MacOS/lib/monodevelop/AddIns/MonoDevelop.MonoMac/MonoMac.dll $(appResource)


clean:
	
	rm ../$(output)

run:
	mono $(output)
	