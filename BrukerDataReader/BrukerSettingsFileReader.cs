using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BrukerDataReader
{
    class BrukerSettingsFileReader
    {

        struct brukerNameValuePair
        {
            public string Name;
            public string Value;
        }

        private string GetNameFromNode(XElement node)
        {
            var elementNames = (from n in node.Elements() select n.Name.LocalName);
            var attributeNames = (from n in node.Attributes() select n.Name.LocalName);

            bool nameIsAnXMLElement = elementNames.Contains("name");

            bool nameIsAnAttribute = attributeNames.Contains("name");

            string nameValue = String.Empty;
            if (nameIsAnXMLElement)
            {
                var element = node.Element("name");

                if (element != null)
                {
                    nameValue = element.Value;
                }
            }
            else if (nameIsAnAttribute)
            {
                nameValue = node.Attribute("name").Value;
            }
          
            return nameValue;
        }

        private string GetValueFromNode(XElement node)
        {
            var elementNames = (from n in node.Elements() select n.Name.LocalName).ToList();

            string fieldName = string.Empty;

            if (elementNames.Contains("value") )
                fieldName = "value";
            else if (elementNames.Contains("Value"))
                fieldName = "Value";

            string valueString = string.Empty;

            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                var element = node.Element(fieldName);
                if (element != null)
                {
                    valueString = element.Value;
                }

            }
         
            return valueString;
        }

        private double GetDoubleFromParamList(IEnumerable<brukerNameValuePair> paramList, string paramName, double valueIfMissing)
        {
            foreach (var item in paramList)
            {
                if (item.Name == paramName)
                {
                    return Convert.ToDouble(item.Value);
                }
            }

            return valueIfMissing;
        }

        public GlobalParameters LoadApexAcqParameters(FileInfo fiSettingsFile)
        {
            XDocument xdoc = XDocument.Load(fiSettingsFile.FullName);
            var paramList = new List<brukerNameValuePair>();

           
            var paramNodes = (from node in xdoc.Element("method").Element("paramlist").Elements() select node);

            foreach (var node in paramNodes)
            {
                var nameValuePair = new brukerNameValuePair
                {
                    Name = GetNameFromNode(node),
                    Value = GetValueFromNode(node)
                };

                paramList.Add(nameValuePair);
            }

            var parameters = new GlobalParameters()
            {
                ML1 = GetDoubleFromParamList(paramList, "ML1", 0),                  // calA
                ML2 = GetDoubleFromParamList(paramList, "ML2", 0),                  // calB
                SampleRate = GetDoubleFromParamList(paramList, "SW_h", 0) * 2,       // sampleRate; SW_h is the digitizer rate and Bruker entered it as the nyquist frequency so it needs to be multiplied by 2.
                NumValuesInScan = Convert.ToInt32(GetDoubleFromParamList(paramList, "TD", 0)),   // numValuesInScan
                AcquiredMZMinimum = GetDoubleFromParamList(paramList, "EXC_low", 0),         // Minimum m/z value in each mass spectrum
                AcquiredMZMaximum = GetDoubleFromParamList(paramList, "EXC_hi", 0)           // Maximum m/z value in each mass spectrum
            };

            // Additional parameters that may be of interest

            // FR_Low = GetDoubleFromParamList(paramList, "FR_low", 0),
            // ByteOrder = Convert.ToInt32(GetDoubleFromParamList(paramList, "BYTORDP", 0))
            //this.CalibrationData.NF = Convert.ToInt32(getDoubleFromParamList(paramList, "NF", 0));

            return parameters;

        }

        public GlobalParameters LoadApexAcqusParameters(FileInfo fiSettingsFile)
        {
            
            var dataLookupTable = new Dictionary<string, double>();

            using (var sr = new StreamReader(fiSettingsFile.FullName))
            {
                while (!sr.EndOfStream)
                {
                    string currentLine = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(currentLine))
                        continue;

                    Match match = Regex.Match(currentLine, @"^##\$(?<name>.*)=\s(?<value>[0-9-\.]+)");

                    if (!match.Success)
                    {
                        continue;
                    }

                    string variableName = match.Groups["name"].Value;

                    double parsedResult = -1;
                    bool canParseValue = double.TryParse(match.Groups["value"].Value, out parsedResult);
                    if (!canParseValue)
                    {
                        parsedResult = -1;
                    }

                    dataLookupTable.Add(variableName, parsedResult);
                }

            }

            var parameters = new GlobalParameters
            {
                ML1 = dataLookupTable["ML1"],
                ML2 = dataLookupTable["ML2"],
                SampleRate = dataLookupTable["SW_h"] * 2, // From Gordon A.:  SW_h is the digitizer rate and Bruker entered it as the nyquist frequency so it needs to be multiplied by 2.
                NumValuesInScan = (int)dataLookupTable["TD"]
            };

            double dataValue;
            if (dataLookupTable.TryGetValue("EXC_low", out dataValue))
                parameters.AcquiredMZMinimum = dataValue;

            if (dataLookupTable.TryGetValue("EXC_hi", out dataValue))
                parameters.AcquiredMZMaximum = dataValue;
    
            return parameters;
        }

    }
}
