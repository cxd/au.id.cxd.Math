# specify list of sourcefiles here
source = WordTokeniser.fs\
Edit1Counts.fs\
LevensteinDistance.fs\
DamerauLevensteinDistance.fs\
Tokens.fs\
LanguageModelUnigramLaplace.fs\
LanguageModelBigramLaplace.fs\
LanguageModelStupidBackoff.fs\
ModelSpellCheck.fs\
BigramLaplaceModelSpellCheck.fs\
NaiveBayesTextClassifier.fs

	 
# specify target file here.
output = au.id.cxd.Text.dll

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
-r:../lib/FSharp.PowerPack/gac/FSharp.PowerPack.dll \
-r:../lib/FSharp.PowerPack/gac/FSharp.PowerPack.Compatibility.dll \
-r:../lib/org.graphdrawing.graphml.dll \
-r:../lib/MathProvider.dll \
-r:../lib/PorterStemmerAlgorithm.dll \
-r:../build/au.id.cxd.Math.dll

all:
	mkdir -p ../build
	fsc $(references) $(target) --out:$(output) $(opts) $(source)
	mv $(output) ../build
	cp -f ../lib/org.graphdrawing.graphml.dll ../build
	cp -f ../lib/MathProvider.dll ../build
	cp -f ../lib/osx/osx.config ../build/MathProvider.dll.config
	cp -f ../lib/FSharp.PowerPack/gac/FSharp.PowerPack.dll ../build
	cp -f ../lib/FSharp.PowerPack/gac/FSharp.PowerPack.Compatibility.dll ../build
	cp -f ../lib/PorterStemmerAlgorithm.dll ../build


clean:
	
	rm ../$(output)

run:
	mono $(output)
	