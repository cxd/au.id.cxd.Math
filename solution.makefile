

all:
	@cd au.id.cxd.Math;\
	make -f math.makefile all;
	@cd au.id.cxd.Text;\
	make -f text.makefile all;
	@cd TrainBigramModel;\
	make -f trainbigram.makefile all;
	@cd au.id.cxd.MathOSXUI;\
	make -f ui.makefile;
	@cd au.id.cxd.Math.OSX.UI;\
	make -f mono.makefile;
	cp -f lib/FSharp.PowerPack/osx/FSharp.PowerPack.dll build\
	cp -f lib/FSharp.PowerPack/osx/FSharp.PowerPack.Compatibility.dll build
ui:	
	@cd au.id.cxd.MathOSXUI;\
	make -f ui.makefile;
	@cd au.id.cxd.Math.OSX.UI;\
	make -f mono.makefile;

testui:
	open -n build/au.id.cxd.Math.OSX.UI.app
	
clean:
	rm build/*.dll;
	rm -rf build/*.app;