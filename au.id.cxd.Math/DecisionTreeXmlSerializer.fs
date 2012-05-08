namespace au.id.cxd.Math.Cart

module DecisionTreeXmlSerializer =

    open System
    open System.IO
    open System.Xml
    open au.id.cxd.Math.Cart.CartAlgorithm
    open au.id.cxd.Math.TrainingData

    
    /// <summary>
    /// Write the decision tree to an xml file for inspection
    /// </summary>
    let writeTreeToXml (file:string) tree = 
      let rec write (writer:XmlWriter) subtree =
            match subtree with
                | Rule (body, dtree) -> 
                    writer.WriteStartElement("rule")
                    writer.WriteAttributeString("entropy", (ruleEntropy body).ToString())
                    writer.WriteStartElement("operation")
                    writer.WriteString(operationText (ruleOp body))
                    writer.WriteEndElement()
                    writer.WriteStartElement("datum")
                    let datumVal = if (ruleAttr body).AttributeType.Equals(Continuous) then (ruleDatum body).FloatVal.ToString()
                                   else if (ruleAttr body).AttributeType.Equals(NumericOrdinal) then (ruleDatum body).FloatVal.ToString()
                                   else if (ruleAttr body).AttributeType.Equals(Bool) then (ruleDatum body).BoolVal.ToString()
                                   else (ruleDatum body).StringVal
                    writer.WriteString(datumVal)
                    writer.WriteEndElement()
                    write writer dtree
                    writer.WriteEndElement()
                | Head (at, gain, dtreeList) -> 
                    writer.WriteStartElement("head")
                    writer.WriteStartElement("attribute")
                    writer.WriteAttributeString("column", at.Column.ToString())
                    writer.WriteAttributeString("type", attributeTypeText at.AttributeType)
                    writer.WriteAttributeString("informationGain", gain.ToString())
                    writer.WriteString(at.AttributeLabel)
                    writer.WriteEndElement()
                    List.iter(fun dtree -> write writer dtree) dtreeList
                    writer.WriteEndElement()
                | Terminal classAttr -> 
                    writer.WriteStartElement("classification")
                    writer.WriteAttributeString("classColumn", classAttr.Column.ToString())
                    writer.WriteString(classAttr.Label)
                    writer.WriteEndElement()
                | Empty -> ()
      let xmlFmt = new XmlWriterSettings()
      xmlFmt.Indent <- true
      xmlFmt.NewLineHandling <- NewLineHandling.Entitize
      let xmlWriter = XmlWriter.Create(file, xmlFmt)
      xmlWriter.WriteStartDocument()
      write xmlWriter tree
      xmlWriter.Flush()
      xmlWriter.Close()

