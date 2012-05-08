using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using au.id.cxd.Text.SpellingService.Models;
using System.Configuration;
using System.Web.Services.Protocols;

namespace au.id.cxd.Text.SpellingService
{
    /// <summary>
    /// the implementation of the spelling service.
    /// </summary>
    public class SpellingService : ISpellingService
    {
        const string EDITMODEL_KEY = "EditModel.File";
        const string LANGUAGEMODEL_KEY = "LanguageModel.File";
        /// <summary>
        /// internal static reference to the bigram spell checker.
        /// </summary>
        private static BigramLaplaceModelSpellCheck.BigramSpellChecker _spellChecker;
        /// <summary>
        /// initialise the underlying spelling corrector.
        /// </summary>
        static SpellingService()
        {
            string editModelFile = ConfigurationManager.AppSettings[EDITMODEL_KEY];
            string languageModelFile = ConfigurationManager.AppSettings[LANGUAGEMODEL_KEY];
            try
            {
                _spellChecker = new BigramLaplaceModelSpellCheck.BigramSpellChecker(editModelFile, languageModelFile);
            }
            catch (Exception ex)
            {
                _spellChecker = null;
            }

        }

        /// <summary>
        /// Perform spell checking using the existing spell checker.
        /// If the spell checker is not initialised correctly throw an exception.
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public SpellingCorrectionResult CorrectSpelling(string[] sentence)
        {
            if (_spellChecker == null)
            {
                throw new SoapException("Spell Checker Model is not Initialised", null);
            }
            var results = _spellChecker.CheckSpelling(sentence);
            List<SpellingCorrection> corrections = new List<SpellingCorrection>();
            foreach(var result in results) {
                corrections.Add(new SpellingCorrection(result));
            }
            return new SpellingCorrectionResult(sentence, corrections);
        }
    }
}
