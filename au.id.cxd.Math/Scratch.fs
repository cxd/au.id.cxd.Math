#light

module Scratch

/// an example of scalar logsig    
let logSigScalar h T A c =
    A / (1.0 + exp(-1.0*T*h))

/// an example of scalar derivative of log sig.
let derivativeLogSigScalar o T A =
    T*o * (A - o)
