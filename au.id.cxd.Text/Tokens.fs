namespace au.id.cxd.Text

open System
open System.Collections.Generic
open au.id.cxd.Text.WordTokeniser

/// <summary>
/// Module for tokens.
/// </summary>
module Tokens =
     
    type Gram = { Tokens: TokenList;
                    Count: float;
                    Weight: float; }

    type Token =
        | NoToken
        | NGram of Gram

    let makeGram tokens =
        { Tokens = tokens; Count = 0.0; Weight = 0.0 }

    let setCount c (gram:Gram) =
        { Tokens = gram.Tokens;
          Count = c;
          Weight = gram.Weight; }

    let setWeight w (gram:Gram) =
        { Tokens = gram.Tokens;
          Count = gram.Count;
          Weight = w; }

    
    /// <summary>
    /// lookup a data gram in the supplied table.
    /// </summary>
    let lookup (dataGram:Dictionary<string,Token>) w =
            if (dataGram.ContainsKey(w)) then dataGram.[w]
            else NoToken

    let update (dataGram:Dictionary<string, Token>) key t =
            dataGram.[key] <- t
