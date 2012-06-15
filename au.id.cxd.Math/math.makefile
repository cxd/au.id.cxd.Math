# specify list of sourcefiles here
source = TextIO.fs \
RawData.fs \
TrainingData.fs \
DataDescription.fs \
DataHistogram.fs \
DataProbabilities.fs \
DataSummary.fs \
MathProject.fs\
Regression.fs \
CartAlgorithm.fs \
DTreeLearn3.fs \
DecisionTreeXmlSerializer.fs \
Graph.fs \
Matrix.fs \
MatrixDeterminant.fs \
MatrixGaussElimination.fs \
ClassifierMetrics.fs \
Neuron.fs \
MLPTrainer.fs \
Scratch.fs \
DecisionTreeGraphMLSerializer.fs \
MaybeBuilder.fs


	 
# specify target file here.
output = au.id.cxd.Math.dll

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
-r:../lib/MathProvider.dll

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
	