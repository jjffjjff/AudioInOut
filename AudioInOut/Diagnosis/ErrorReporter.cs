using System;
using System.Diagnostics;

namespace AudioInOut.Diagnosis
{
    static class ErrorReporter
    {
        public static void LogWarning(Exception ex) => Trace.WriteLine($"## Warning ##: {ex}");
    }
}
