using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using NClap.Metadata;

namespace NClap.Types
{
    /// <summary>
    /// Method that parses an integer string using the provided number style
    /// flags.
    /// </summary>
    /// <typeparam name="T">Argument type.</typeparam>
    /// <param name="stringToParse">String to parse.</param>
    /// <param name="numberStyles">Number style flags to use while parsing.</param>
    /// <returns>The parsed object.</returns>
    internal delegate T IntegerArgumentTypeParseHandler<out T>(string stringToParse, NumberStyles numberStyles);

    /// <summary>
    /// Method that performs a binary operation on two values.
    /// </summary>
    /// <typeparam name="T">Type of the values.</typeparam>
    /// <param name="operand0">The left-hand operand.</param>
    /// <param name="operand1">The right-hand operand.</param>
    /// <returns>The result of the operation.</returns>
    internal delegate T BinaryOp<T>(T operand0, T operand1);

    /// <summary>
    /// Basic implementation of IArgumentType, useful for describing built-in
    /// integral .NET types with simple semantics.
    /// </summary>
    internal class IntegerArgumentType : ArgumentTypeBase
    {
        private readonly IntegerArgumentTypeParseHandler<object> _parseHandler;

        /// <summary>
        /// Constructs a new object to describe the provided integer type.
        /// </summary>
        /// <param name="type">Type to describe.</param>
        /// <param name="parseHandler">Delegate used to parse strings with the
        /// given type.</param>
        /// <param name="signed">True if this type is signed; false if it's
        /// unsigned.</param>
        public IntegerArgumentType(Type type, IntegerArgumentTypeParseHandler<object> parseHandler, bool signed)
            : base(type)
        {
            if (!type.IsPrimitive)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            _parseHandler = parseHandler;
            IsSigned = signed;
        }

        /// <summary>
        /// True if the type is signed; false if it's unsigned.
        /// </summary>
        public bool IsSigned { get; }

        /// <summary>
        /// Convenience method, primarily useful to allow for inference of the
        /// first argument to the constructor.
        /// </summary>
        /// <typeparam name="T">Type to describe.</typeparam>
        /// <param name="parseHandler">Delegate used to parse strings with the
        /// given type.</param>
        /// <param name="signed">True if this type is signed; false if it's
        /// unsigned.</param>
        /// <returns>The constructed object.</returns>
        public static IntegerArgumentType Create<T>(IntegerArgumentTypeParseHandler<T> parseHandler, bool signed) =>
            new IntegerArgumentType(typeof(T), (s, styles) => parseHandler(s, styles), signed);

        /// <summary>
        /// Adds two values of this type without checking for overflow.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public object Add(object operand0, object operand1) =>
            Add(operand0, operand1, checkForOverflow: false);

        /// <summary>
        /// Adds two values of this type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <param name="checkForOverflow">True to check for and throw an
        /// exception on overflow; false to ignore overflow.</param>
        /// <returns>The result of the operation.</returns>
        public object Add(object operand0, object operand1, bool checkForOverflow)
        {
            var opcode = checkForOverflow ? (IsSigned ? OpCodes.Add_Ovf : OpCodes.Add_Ovf_Un) : OpCodes.Add;
            return GenerateOpMethod(opcode)(operand0, operand1);
        }

        /// <summary>
        /// Subtracts two values of this type without checking for overflow.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public object Subtract(object operand0, object operand1) =>
            Subtract(operand0, operand1, checkForOverflow: false);

        /// <summary>
        /// Subtracts two values of this type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <param name="checkForOverflow">True to check for and throw an
        /// exception on overflow; false to ignore overflow.</param>
        /// <returns>The result of the operation.</returns>
        public object Subtract(object operand0, object operand1, bool checkForOverflow)
        {
            var opcode = checkForOverflow ? (IsSigned ? OpCodes.Sub_Ovf : OpCodes.Sub_Ovf_Un) : OpCodes.Sub;
            return GenerateOpMethod(opcode)(operand0, operand1);
        }

        /// <summary>
        /// Multiplies two values of this type without checking for overflow.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public object Multiply(object operand0, object operand1) =>
            Multiply(operand0, operand1, checkForOverflow: false);

        /// <summary>
        /// Multiplies two values of this type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <param name="checkForOverflow">True to check for and throw an
        /// exception on overflow; false to ignore overflow.</param>
        /// <returns>The result of the operation.</returns>
        public object Multiply(object operand0, object operand1, bool checkForOverflow)
        {
            var opcode = checkForOverflow ? (IsSigned ? OpCodes.Mul_Ovf : OpCodes.Mul_Ovf_Un) : OpCodes.Mul;
            return GenerateOpMethod(opcode)(operand0, operand1);
        }

        /// <summary>
        /// Divides two values of this type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public object Divide(object operand0, object operand1)
        {
            var opcode = IsSigned ? OpCodes.Div : OpCodes.Div_Un;
            return GenerateOpMethod(opcode)(operand0, operand1);
        }

        /// <summary>
        /// Computes the bitwise AND of two values of this type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public object And(object operand0, object operand1) =>
            GenerateOpMethod(OpCodes.And)(operand0, operand1);

        /// <summary>
        /// Computes the bitwise OR of two values of this type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public object Or(object operand0, object operand1) =>
            GenerateOpMethod(OpCodes.Or)(operand0, operand1);

        /// <summary>
        /// Computes the bitwise XOR of two values of this type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public object Xor(object operand0, object operand1) =>
            GenerateOpMethod(OpCodes.Xor)(operand0, operand1);

        /// <summary>
        /// Checks if one value of this type is less than another value of this
        /// type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public bool IsLessThan(object operand0, object operand1)
        {
            var opcode = IsSigned ? OpCodes.Clt : OpCodes.Clt_Un;
            return GenerateBinaryPredicate(opcode)(operand0, operand1);
        }

        /// <summary>
        /// Checks if one value of this type is less than or equal to another
        /// value of this type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public bool IsLessThanOrEqualTo(object operand0, object operand1) =>
            !IsGreaterThan(operand0, operand1);

        /// <summary>
        /// Checks if one value of this type is greater than another value of
        /// this type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public bool IsGreaterThan(object operand0, object operand1)
        {
            var opcode = IsSigned ? OpCodes.Cgt : OpCodes.Cgt_Un;
            return GenerateBinaryPredicate(opcode)(operand0, operand1);
        }

        /// <summary>
        /// Checks if one value of this type is greater than or equal to another
        /// value of this type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public bool IsGreaterThanOrEqualTo(object operand0, object operand1) =>
            !IsLessThan(operand0, operand1);

        /// <summary>
        /// Checks if one value of this type is equal to another value of this
        /// type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public bool IsEqualTo(object operand0, object operand1) =>
            GenerateBinaryPredicate(OpCodes.Ceq)(operand0, operand1);

        /// <summary>
        /// Checks if one value of this type is not equal to another value of
        /// this type.
        /// </summary>
        /// <param name="operand0">Left-hand operand.</param>
        /// <param name="operand1">Right-hand operand.</param>
        /// <returns>The result of the operation.</returns>
        public bool IsNotEqualTo(object operand0, object operand1) =>
            !IsEqualTo(operand0, operand1);

        /// <summary>
        /// Parses the provided string.  Throws an exception if the string
        /// cannot be parsed.
        /// </summary>
        /// <param name="context">Context for parsing.</param>
        /// <param name="stringToParse">String to parse.</param>
        /// <returns>The parsed object.</returns>
        protected override object Parse(ArgumentParseContext context, string stringToParse)
        {
            Debug.Assert(stringToParse != null);

            var numberStyle = NumberStyles.Integer;
            ulong? multiplier = null;

            // Inspect the number string's prefix, likely indicating the number's
            // style.
            if (stringToParse.StartsWith("0x", StringComparison.Ordinal))
            {
                stringToParse = stringToParse.Remove(0, 2);
                numberStyle = NumberStyles.HexNumber;
            }
            else if (stringToParse.StartsWith("0n", StringComparison.Ordinal))
            {
                stringToParse = stringToParse.Remove(0, 2);
                numberStyle = NumberStyles.Integer;
            }

            // Inspect the number string's suffix, that is if we've been allowed
            // to do so.
            if (context.NumberOptions.HasFlag(NumberOptions.AllowBinaryMetricUnitSuffix))
            {
                stringToParse = RemoveUnitSuffix(stringToParse, true, out multiplier);
            }
            else if (context.NumberOptions.HasFlag(NumberOptions.AllowMetricUnitSuffix))
            {
                stringToParse = RemoveUnitSuffix(stringToParse, false, out multiplier);
            }

            // If the number is not being scaled, then directly parse and return
            // it.
            if (!multiplier.HasValue)
            {
                return _parseHandler(stringToParse, numberStyle);
            }
            else
            {
                var decimalValue = decimal.Parse(
                    stringToParse,
                    numberStyle | NumberStyles.AllowDecimalPoint,
                    CultureInfo.CurrentCulture);

                var scaledDecimalValue = decimalValue * multiplier.Value;
                var scaledLongValue = Convert.ToInt64(scaledDecimalValue);

                if (scaledLongValue != scaledDecimalValue)
                {
                    throw new OverflowException();
                }

                var scaledStringValue = scaledLongValue.ToString(CultureInfo.InvariantCulture);
                return _parseHandler(scaledStringValue, NumberStyles.Integer);
            }
        }

        private static ulong GetUnitMultiplier(string suffix, bool unitsAreBinary)
        {
            var @base = unitsAreBinary ? 1024U : 1000U;
            uint? exponent = null;

            // Remove IEC suffix from the suffix, if present and if applicable.
            if (unitsAreBinary)
            {
                if (suffix.EndsWith("iB", StringComparison.Ordinal))
                {
                    suffix = suffix.Substring(0, suffix.Length - 2);
                }
                else if (suffix.EndsWith("B", StringComparison.Ordinal))
                {
                    suffix = suffix.Substring(0, suffix.Length - 1);
                }
            }

            switch (suffix)
            {
            case "k": // kilo
            case "K":
                exponent = 1;
                break;
            case "M": // mega
                exponent = 2;
                break;
            case "G": // giga
                exponent = 3;
                break;
            case "T": // tera
                exponent = 4;
                break;
            case "P": // peta
                exponent = 5;
                break;
            case "E": // exa
                exponent = 6;
                break;
            case "Z": // zetta
                exponent = 7;
                break;
            case "Y": // yotta
                exponent = 8;
                break;
            }

            // Check for non-binary-only units.
            if (!exponent.HasValue && !unitsAreBinary)
            {
                switch (suffix)
                {
                case "h":
                    @base = 10;
                    exponent = 2;
                    break;

                case "da":
                    @base = 10;
                    exponent = 1;
                    break;
                }
            }

            if (!exponent.HasValue)
            {
                throw new ArgumentOutOfRangeException(nameof(suffix));
            }

            return Power(@base, exponent.Value);
        }

        private static ulong Power(uint @base, uint exponent)
        {
            ulong value = 1;

            for (var index = 0; index < exponent; ++index)
            {
                var newValue = value * @base;
                if (newValue < value)
                {
                    throw new OverflowException();
                }

                value = newValue;
            }

            return value;
        }

        private static string RemoveUnitSuffix(string value, bool unitsAreBinary, out ulong? multiplier)
        {
            int index;
            for (index = value.Length - 1; index >= 0; --index)
            {
                if (char.IsDigit(value[index]))
                {
                    break;
                }
            }

            var suffixIndex = index + 1;
            if (suffixIndex >= value.Length)
            {
                multiplier = null;
                return value;
            }

            var suffix = value.Substring(suffixIndex);

            multiplier = GetUnitMultiplier(suffix, unitsAreBinary);
            return value.Substring(0, suffixIndex);
        }

        private Func<object, object, bool> GenerateBinaryPredicate(OpCode op) =>
            (o0, o1) => (int)GenerateOpMethod(op)(o0, o1) != 0;

        private Func<object, object, object> GenerateOpMethod(OpCode op) => GenerateOpMethod(op, Type);

        private Func<object, object, object> GenerateOpMethod(OpCode op, Type returnType)
        {
            var method = new DynamicMethod(Guid.NewGuid().ToString(), returnType, new[] { Type, Type });

            var gen = method.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Ldarg_1);
            gen.Emit(op);
            gen.Emit(OpCodes.Ret);

            return (o0, o1) =>
            {
                try
                {
                    return method.Invoke(null, new[] { o0, o1 });
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            };
        }
    }
}
