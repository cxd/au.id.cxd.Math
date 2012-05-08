using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace au.id.cxd.Text.SpellingService.Models
{

    /// <summary>
    /// A class to encapsulate the results of a spelling correction
    /// task for consumption via a web service
    /// </summary>
    [DataContract]
    public class SpellingCorrection
    {
        /// <summary>
        /// default
        /// </summary>
        public SpellingCorrection()
        {
        }

        /// <summary>
        /// Initialise from the output of the model.
        /// </summary>
        /// <param name="modelCorrection"></param>
        public SpellingCorrection(ModelSpellCheck.SpellingCorrection modelCorrection)
        {
            WordIndex = modelCorrection.Item1;
            ModelSpellCheck.Spelling replacement = modelCorrection.Item2;
            Tuple<double, string[]> newSentence = modelCorrection.Item3;
            Rank = System.Math.Log(replacement.Item2) + newSentence.Item1;
            NewSentence = newSentence.Item2;
            CorrectSpellingOfWord = replacement.Item1;
        }

        /// <summary>
        /// this is the index of the word that requires the correction
        /// </summary>
        [DataMember]
        public int WordIndex
        {
            get;
            set;
        }

        /// <summary>
        /// this is the rank of the correction assigned by the edit model
        /// </summary>
        [DataMember]
        public double Rank
        {
            get;
            set;
        }

        /// <summary>
        /// this is the new sentence that results from the single correction
        /// </summary>
        [DataMember]
        public string[] NewSentence
        {
            get;
            set;
        }

        /// <summary>
        /// this is the correct spelling of the word according to the model.
        /// </summary>
        [DataMember]
        public string CorrectSpellingOfWord
        {
            get;
            set;
        }
    }

}