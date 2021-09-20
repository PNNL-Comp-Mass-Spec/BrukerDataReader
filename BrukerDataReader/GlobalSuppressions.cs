// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Design", "RCS1075:Avoid empty catch clause that catches System.Exception.", Justification = "Safe to ignore errors here", Scope = "member", Target = "~M:BrukerDataReader.BrukerSettingsFileReader.LoadApexAcqParameters(System.IO.FileInfo)~BrukerDataReader.GlobalParameters")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Leave parentheses as-is", Scope = "member", Target = "~M:BrukerDataReader.FourierTransform.RealFourierTransform(System.Double[]@)~System.Int32")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:BrukerDataReader.DataReader.GetIndexForMZ(System.Single,System.Int32)~System.Int32")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:BrukerDataReader.DataReader.GetMassSpectrum(System.Int32[],System.Single[]@,System.Single[]@)")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:BrukerDataReader.DataReader.GetMassSpectrumUsingSupposedlyFasterBinaryReader(System.Int32,System.Single[]@,System.Single[]@)")]
[assembly: SuppressMessage("Readability", "RCS1123:Add parentheses when necessary.", Justification = "Parentheses not needed", Scope = "member", Target = "~M:BrukerDataReader.FourierTransform.PerformFourierTransform(System.Int32,System.Double[]@,System.Int32)")]
