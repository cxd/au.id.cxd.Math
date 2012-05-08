build = /Applications/MonoDevelop.app/Contents/MacOS/mdtool -v build

output = au.id.cxd.Math.OSX.UI/bin/Debug/au.id.cxd.Math.OSX.UI.app

productdir = ../build

all:
	$(build)
	@cp -rf $(output) $(productdir) 
	
clean:
	rm -rf au.id.cxd.Math.OSX.UI/bin/Debug/*