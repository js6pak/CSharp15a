// This file is part of CSharp15a.
// 
// CSharp15a is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// CSharp15a is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with CSharp15a. If not, see <https://www.gnu.org/licenses/>.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

// A collection of helper routines that are normally internal to System.Runtime 


namespace System.Runtime.CompilerServices
{
    // Calls to methods or references to fields marked with this attribute may be replaced at
    // some call sites with jit intrinsic expansions.
    // Types marked with this attribute may be specially treated by the runtime/compiler.
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Field, Inherited = false)]
    internal sealed class IntrinsicAttribute : Attribute
    {
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class SR
    {
        public static string Arg_NullArgumentNullRef => "Arg_NullArgumentNullRef";
        public static string Arg_ArgumentOutOfRangeException => "Arg_ArgumentOutOfRangeException";
        public static string Arg_ElementsInSourceIsGreaterThanDestination => "Arg_ElementsInSourceIsGreaterThanDestination";

        public static string Format(string str, object? arg0)
            => string.Format(System.Globalization.CultureInfo.InvariantCulture, str, arg0);

        public static string Format(string str, object? arg0, object? arg1)
            => string.Format(System.Globalization.CultureInfo.InvariantCulture, str, arg0, arg1);

        public static string Format(string str, object? arg0, object? arg1, object? arg2)
            => string.Format(System.Globalization.CultureInfo.InvariantCulture, str, arg0, arg1, arg2);
    }
}

namespace System.Numerics
{
    internal class VectorS
    {
        [DoesNotReturn]
        internal static void ThrowInsufficientNumberOfElementsException(int requiredElementCount)
        {
            throw new IndexOutOfRangeException();
        }
    }
}

namespace System
{
    [StackTraceHidden]
    internal static class ThrowHelper
    {
        [DoesNotReturn]
        internal static void ThrowArgumentException_DestinationTooShort()
        {
            throw new ArgumentException("SR.Argument_DestinationTooShort", "destination");
        }
    }
}
