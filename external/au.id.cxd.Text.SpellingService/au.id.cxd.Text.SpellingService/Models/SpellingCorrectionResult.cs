using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace au.id.cxd.Text.SpellingService.Models
{
    [DataContract]
    public class SpellingCorrectionResult
    {
        /// <summary>
        /// default.
        /// </summary>
        public SpellingCorrectionResult()
        {
            OriginalSentence = new List<string>();
            CorrectedSentence = new List<string>();
            Corrections = new List<SpellingCorrection>();
        }

        /// <summary>
        /// Initialise from a list of new collections
        /// </summary>
        /// <param name="corrections"></param>
        public SpellingCorrectionResult(string[] inputSentence, List<SpellingCorrection> corrections)
        {
            OriginalSentence = new List<string>(inputSentence);
            CorrectedSentence = new List<string>(inputSentence);
            Corrections = new List<SpellingCorrection>();
            if (corrections != null)
            {
                corrections.ForEach(ApplyCorrection);
            }
        }

        /// <summary>
        /// the original sentence.
        /// </summary>
        [DataMember]
        public List<string> OriginalSentence
        {
            get;
            set;
        }

        /// <summary>
        /// the sentence with all corrections applied.
        /// </summary>
        [DataMember]
        public List<string> CorrectedSentence
        {
            get;
            set;
        }

        /// <summary>
        /// the set of corrections
        /// </summary>
        [DataMember]
        public List<SpellingCorrection> Corrections
        {
            get;
            set;
        }

        /// <summary>
        /// apply the correction to the original sentence.
        /// </summary>
        /// <param name="correction"></param>
        public void ApplyCorrection(SpellingCorrection correction)
        {
            if (CorrectedSentence == null || CorrectedSentence.Count == 0)
            {
                CorrectedSentence = new List<string>(OriginalSentence);
            }
            if (correction.WordIndex >= 0 && correction.WordIndex < CorrectedSentence.Count)
            {
                CorrectedSentence[correction.WordIndex] = correction.CorrectSpellingOfWord;
                Corrections.Add(correction);
            }
        }
    }
}