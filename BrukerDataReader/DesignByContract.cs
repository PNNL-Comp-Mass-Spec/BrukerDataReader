using System;
using System.Diagnostics;

// ReSharper disable UnusedMember.Global
namespace BrukerDataReader
{
    [AttributeUsage(AttributeTargets.All)]
    public class CoverageExcludeAttribute : Attribute { }

    [CoverageExclude]
    public static class Check
    {
        /// <summary>
        /// Precondition check - should run regardless of preprocessor directives.
        /// </summary>
        public static void Require(bool assertion, string message)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new PreconditionException(message);
            }
            else
            {
                Trace.Assert(assertion, "Precondition: " + message);
            }
        }

        /// <summary>
        /// Precondition check - should run regardless of preprocessor directives.
        /// </summary>
        public static void Require(bool assertion, string message, Exception inner)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new PreconditionException(message, inner);
            }
            else
            {
                Trace.Assert(assertion, "Precondition: " + message);
            }
        }

        /// <summary>
        /// Precondition check - should run regardless of preprocessor directives.
        /// </summary>
        public static void Require(bool assertion)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new PreconditionException("Precondition failed.");
            }
            else
            {
                Trace.Assert(assertion, "Precondition failed.");
            }
        }

        /// <summary>
        /// Postcondition check.
        /// </summary>
        public static void Ensure(bool assertion, string message)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new PostconditionException(message);
            }
            else
            {
                Trace.Assert(assertion, "Postcondition: " + message);
            }
        }

        /// <summary>
        /// Postcondition check.
        /// </summary>
        public static void Ensure(bool assertion, string message, Exception inner)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new PostconditionException(message, inner);
            }
            else
            {
                Trace.Assert(assertion, "Postcondition: " + message);
            }
        }

        /// <summary>
        /// Postcondition check.
        /// </summary>
        public static void Ensure(bool assertion)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new PostconditionException("Postcondition failed.");
            }
            else
            {
                Trace.Assert(assertion, "Postcondition failed.");
            }
        }

        /// <summary>
        /// Invariant check.
        /// </summary>
        public static void Invariant(bool assertion, string message)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new InvariantException(message);
            }
            else
            {
                Trace.Assert(assertion, "Invariant: " + message);
            }
        }

        /// <summary>
        /// Invariant check.
        /// </summary>
        public static void Invariant(bool assertion, string message, Exception inner)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new InvariantException(message, inner);
            }
            else
            {
                Trace.Assert(assertion, "Invariant: " + message);
            }
        }

        /// <summary>
        /// Invariant check.
        /// </summary>
        public static void Invariant(bool assertion)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new InvariantException("Invariant failed.");
            }
            else
            {
                Trace.Assert(assertion, "Invariant failed.");
            }
        }

        /// <summary>
        /// Assertion check.
        /// </summary>
        public static void Assert(bool assertion, string message)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new AssertionException(message);
            }
            else
            {
                Trace.Assert(assertion, "Assertion: " + message);
            }
        }

        /// <summary>
        /// Assertion check.
        /// </summary>
        public static void Assert(bool assertion, string message, Exception inner)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new AssertionException(message, inner);
            }
            else
            {
                Trace.Assert(assertion, "Assertion: " + message);
            }
        }

        /// <summary>
        /// Assertion check.
        /// </summary>
        public static void Assert(bool assertion)
        {
            if (UseExceptions)
            {
                if (!assertion) throw new AssertionException("Assertion failed.");
            }
            else
            {
                Trace.Assert(assertion, "Assertion failed.");
            }
        }

        /// <summary>
        /// Set this if you wish to use Trace Assert statements
        /// instead of exception handling.
        /// (The Check class uses exception handling by default.)
        /// </summary>
        public static bool UseAssertions { get; set; } = false;

        /// <summary>
        /// Is exception handling being used?
        /// </summary>
        private static bool UseExceptions => !UseAssertions;
    }

    /// <summary>
    /// Exception raised when a contract is broken.
    /// Catch this exception type if you wish to differentiate between
    /// any DesignByContract exception and other runtime exceptions.
    ///
    /// </summary>
    [CoverageExclude]
    [Serializable]
    public class DesignByContractException : ApplicationException
    {
        protected DesignByContractException() { }
        protected DesignByContractException(string message) : base(message) { }
        protected DesignByContractException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Exception raised when a precondition fails.
    /// </summary>
    [CoverageExclude]
    [Serializable]
    public class PreconditionException : DesignByContractException
    {
        /// <summary>
        /// Precondition Exception.
        /// </summary>
        public PreconditionException() { }

        /// <summary>
        /// Precondition Exception.
        /// </summary>
        public PreconditionException(string message) : base(message) { }

        /// <summary>
        /// Precondition Exception.
        /// </summary>
        public PreconditionException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Exception raised when a postcondition fails.
    /// </summary>
    [CoverageExclude]
    [Serializable]
    public class PostconditionException : DesignByContractException
    {
        /// <summary>
        /// Postcondition Exception.
        /// </summary>
        public PostconditionException() { }

        /// <summary>
        /// Postcondition Exception.
        /// </summary>
        public PostconditionException(string message) : base(message) { }

        /// <summary>
        /// Postcondition Exception.
        /// </summary>
        public PostconditionException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Exception raised when an invariant fails.
    /// </summary>
    [CoverageExclude]
    [Serializable]
    public class InvariantException : DesignByContractException
    {
        /// <summary>
        /// Invariant Exception.
        /// </summary>
        public InvariantException() { }
        /// <summary>
        /// Invariant Exception.
        /// </summary>
        public InvariantException(string message) : base(message) { }
        /// <summary>
        /// Invariant Exception.
        /// </summary>
        public InvariantException(string message, Exception inner) : base(message, inner) { }
    }

    /// <summary>
    /// Exception raised when an assertion fails.
    /// </summary>
    [CoverageExclude]
    [Serializable]
    public class AssertionException : DesignByContractException
    {
        /// <summary>
        /// Assertion Exception.
        /// </summary>
        public AssertionException() { }

        /// <summary>
        /// Assertion Exception.
        /// </summary>
        public AssertionException(string message) : base(message) { }

        /// <summary>
        /// Assertion Exception.
        /// </summary>
        public AssertionException(string message, Exception inner) : base(message, inner) { }
    }
}
