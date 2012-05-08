using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace au.id.cxd.Text.SpellingService.Models
{

    public class DemoCorrection
    {
        public int Index
        {
            get;
            set;
        }

        public string Word
        {
            get;
            set;
        }
    }

    public class CorrectedSentence
    {
        public CorrectedSentence()
        {
            NewSentence = new List<string>();
            Corrections = new List<DemoCorrection>(); 
        }

        public List<string> NewSentence
        {
            get;
            set;
        }

        public List<DemoCorrection> Corrections
        {
            get;
            set;
        }

        public DemoCorrection CorrectionFor(int idx)
        {
            if (Corrections.Exists(item => item.Index == idx))
            {
                return Corrections.Find(item => item.Index == idx);
            }
            return null;
        }
    }

    public class DemoSpellingModel
    {

        public DemoSpellingModel()
        {
            Corrections = new List<CorrectedSentence>();
        }

        public bool IsCorrected
        {
            get;
            set;
        }

        public string InputText
        {
            get;
            set;
        }



        public List<CorrectedSentence> Corrections
        {
            get;
            set;
        }

        /// <summary>
        /// Tokenise the input.
        /// </summary>
        /// <returns></returns>
        public List<List<string>> Tokenise()
        {
            if (string.IsNullOrEmpty(InputText))
            {
                return new List<List<string>>();
            }
            List<char> split = new List<char>(Environment.NewLine.ToCharArray());
            split.Add('.');
            string[] lines = InputText.Split(split.ToArray());
            List<List<string>> sentences = new List<List<string>>();
            foreach (var line in lines)
            {
                string[] tokens = WordTokeniser.defaultSplitter(line);
                for (int i = 0; i < tokens.Length; i++)
                {
                    tokens[i] = WordTokeniser.emptyFilter(tokens[i]);
                }
                sentences.Add(new List<string>(tokens));
            }
            return sentences;
        }

        public void CorrectSpelling()
        {
            Corrections = new List<CorrectedSentence>();
            foreach (List<string> sentence in Tokenise())
            {
                var client = new SpellingServiceClient.SpellingServiceClient();
                var results = client.CorrectSpelling(sentence.ToArray());
                var correction = new CorrectedSentence();
                correction.NewSentence = new List<string>(results.CorrectedSentence);
                foreach (var result in results.Corrections)
                {
                    var correct = new DemoCorrection();
                    correct.Word = result.CorrectSpellingOfWord;
                    correct.Index = result.WordIndex;
                    correction.Corrections.Add(correct);
                }
                Corrections.Add(correction);
            }
            IsCorrected = true;
        }
    }
}