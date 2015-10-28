using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var nameIsAnXMLElement = elementNames.Contains("name");

            var nameIsAnAttribute = attributeNames.Contains("name");

            var nameValue = String.Empty;
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

            var fieldName = string.Empty;

            if (elementNames.Contains("value") )
                fieldName = "value";
            else if (elementNames.Contains("Value"))
                fieldName = "Value";

            var valueString = string.Empty;

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
            // Bruker's acquisition software will write out data like this to the apexAcquisition.method file
            // if the user enters a sample description of "<2mg/mL  100mM  AA  SID35"
            //
            //   <sampledescription>&lt;<2mg/mL&#x20; 100mM&#x20; AA&#x20; SID35</sampledescription>
            //
            // The extra "<" after &lt; should not be there
            // Its presence causes the XDocument.Load() event to fail
            // Thus, we must pre-scrub the file prior to passing it to .Load()

            var paramList = new List<brukerNameValuePair>();

            var tmpFilePath = PrescanApexAcqFile(fiSettingsFile.FullName);

            using (var fileReader = new StreamReader(new FileStream(tmpFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                var xdoc = XDocument.Load(fileReader);

                var methodElement = xdoc.Element("method");

                if (methodElement != null)
                {
                    var paramListElement = methodElement.Element("paramlist");

                    if (paramListElement != null)
                    {
                        var paramNodes = (from node in paramListElement.Elements() select node);

                        foreach (var node in paramNodes)
                        {
                            var nameValuePair = new brukerNameValuePair
                            {
                                Name = GetNameFromNode(node),
                                Value = GetValueFromNode(node)
                            };

                            paramList.Add(nameValuePair);
                        }
                    }
                }
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

            try
            {
                // Delete the temp file
                File.Delete(tmpFilePath);
            }
            catch (Exception)
            {
                // Ignore exceptions
            }

            return parameters;

        }

        private string PrescanApexAcqFile(string apexAcqFilePath)
        {
            // Look for 
            //   &lt;<  
            // but exclude matches to 
            //   &lt;</

            var reLessThanMatcher = new Regex(@"&lt;<(?!/)", RegexOptions.Compiled);

            var fixedFilePath = Path.GetTempFileName();

            using (var reader = new StreamReader(new FileStream(apexAcqFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            using (var writer = new StreamWriter(new FileStream(fixedFilePath, FileMode.Create, FileAccess.Write, FileShare.Read)))
            {                
                while (!reader.EndOfStream)
                {
                    var dataLine = reader.ReadLine();
                    if (string.IsNullOrEmpty(dataLine))
                    {
                        writer.WriteLine();
                        continue;
                    }

                    var reMatch = reLessThanMatcher.Match(dataLine);

                    if (reMatch.Success)
                    {
                        writer.WriteLine(reLessThanMatcher.Replace(dataLine, "&lt;"));
                    }
                    else
                    {
                        writer.WriteLine(dataLine);
                    }

                }
                
            }
            
            return fixedFilePath;
        }

        public GlobalParameters LoadApexAcqusParameters(FileInfo fiSettingsFile)
        {
            
            var dataLookupTable = new Dictionary<string, double>();

            using (var sr = new StreamReader(fiSettingsFile.FullName))
            {
                while (!sr.EndOfStream)
                {
                    var currentLine = sr.ReadLine();
                    if (string.IsNullOrWhiteSpace(currentLine))
                        continue;

                    var match = Regex.Match(currentLine, @"^##\$(?<name>.*)=\s(?<value>[0-9-\.]+)");

                    if (!match.Success)
                    {
                        continue;
                    }

                    var variableName = match.Groups["name"].Value;

                    double parsedResult;
                    var canParseValue = double.TryParse(match.Groups["value"].Value, out parsedResult);
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
