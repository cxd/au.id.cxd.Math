using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using au.id.cxd.Text.SpellingService.Models;

namespace au.id.cxd.Text.SpellingService
{
    [ServiceContract]
    public interface ISpellingService
    {
        [OperationContract]
        SpellingCorrectionResult CorrectSpelling(string[] sentence);
    }
}
