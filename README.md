au.id.cxd.Math
==============

A collection of experiments focusing on processing tabular data. 

The core library focuses on providing supporting methods for summarising data sets in the following ways.

1. It defines standard data types for raw data.
* String - labels and categories expressed as strings.
* NumericOrdinal - numeric ordinal ranges
* Continuous - continuous real values
* Bool - boolean values

A data set is described as a set of attributes which consist of a column and data type pair and a set of data columns 
each containing samples for the types represented by that column.


2. It provides some simple methods for summarisation.
* Basic statistics for each columnn
- average, variance, standard deviation, min, max etc.
* histograms
* CDF curves
* gaussian distributions

3. It provides some simple methods for approximating values based on the current sample.
* Regression for single values.

4. It provides some tools to display the summary information.
* au.id.cxd.Math.UI provides a WPF application with partial functionality for graphing summaries of the data.
* Current tinkering is to change this structure to a web application which will provide equivalent functionality.

Among other things, there are also a number of experiments with the following types of problems.
* Supervised learning - implementations of a simple backpropogration network, and a decision tree.
* Text analysis - bigram and unigram models using a bayesian model.
* Optimisation problems - experiments with an implementation of the simplex algorithm and 
integer programming branch and bound methods. 

The main useful tool has been the math ui which has proven useful at work when taking data from load tests 
and using it to provide some insight into the distribution, and similarity between counters used
for profiling different aspects of the systems under test.




