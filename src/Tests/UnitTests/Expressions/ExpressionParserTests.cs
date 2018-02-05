using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NClap.Expressions;

namespace NClap.Tests.Expressions
{
    [TestClass]
    public class ExpressionParserTests
    {
        [TestMethod]
        public void TestThatEmptyStringIsNotValidExpression()
        {
            ExpressionParser.TryParse(string.Empty, out Expression expr)
                .Should().BeFalse();
            expr.Should().BeNull();
        }

        [TestMethod]
        public void TestThatWhiteSpaceStringIsNotValidExpression()
        {
            ExpressionParser.TryParse(" ", out Expression expr)
                .Should().BeFalse();
            expr.Should().BeNull();
        }

        [TestMethod]
        public void TestThatExpressionWithTrailingInvalidTextIsNotValid()
        {
            ExpressionParser.TryParse("$foo something trailing", out Expression expr)
                .Should().BeFalse();
            expr.Should().BeNull();
        }

        [TestMethod]
        public void TestThatValidVariableExpressionHasCorrectName()
        {
            ExpressionParser.TryParse("$foo", out Expression expr).Should().BeTrue();
            expr.Should().BeOfType<VariableExpression>()
                .Which.VariableName.Should().Be("foo");
        }

        [TestMethod]
        public void TestThatVariableNameCanContainNumbers()
        {
            ExpressionParser.TryParse("$f1o2o3", out Expression expr).Should().BeTrue();
            expr.Should().BeOfType<VariableExpression>()
                .Which.VariableName.Should().Be("f1o2o3");
        }

        [TestMethod]
        public void TestThatVariableNameCannotContainNonAlphaNumericCharacters()
        {
            ExpressionParser.TryParse("$f@f", out Expression expr).Should().BeFalse();
            ExpressionParser.TryParse("$$f", out expr).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatMissingVariableNameYieldsInvalidExpression()
        {
            ExpressionParser.TryParse("$", out Expression expr).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatLiteralExpressionHasCorrectContents()
        {
            ExpressionParser.TryParse("\"foo\"", out Expression expr).Should().BeTrue();
            expr.Should().BeOfType<StringLiteral>()
                .Which.Value.Should().Be("foo");
        }

        [TestMethod]
        public void TestThatLiteralExpressionCanBeEmptyString()
        {
            ExpressionParser.TryParse("\"\"", out Expression expr).Should().BeTrue();
            expr.Should().BeOfType<StringLiteral>()
                .Which.Value.Should().BeEmpty();
        }

        [TestMethod]
        public void TestThatLiteralExpressionCanContainQuote()
        {
            ExpressionParser.TryParse("\"\\\"foo\"", out Expression expr).Should().BeTrue();
            expr.Should().BeOfType<StringLiteral>()
                .Which.Value.Should().Be("\"foo");
        }

        [TestMethod]
        public void TestThatLiteralExpressionCanContainEscapedFormOfWhiteSpaceChars()
        {
            ExpressionParser.TryParse("\"foo\\nbar\"", out Expression expr).Should().BeTrue();
            expr.Should().BeOfType<StringLiteral>()
                .Which.Value.Should().Be("foo\nbar");

            ExpressionParser.TryParse("\"foo\\tbar\"", out expr).Should().BeTrue();
            expr.Should().BeOfType<StringLiteral>()
                .Which.Value.Should().Be("foo\tbar");
        }

        [TestMethod]
        public void TestThatLiteralExpressionWithUnknownEscapeSequenceIsInvalid()
        {
            ExpressionParser.TryParse("\"foo\\x\"", out Expression expr).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatLiteralExpressionWithUnterminatedEscapeSequenceIsInvalid()
        {
            ExpressionParser.TryParse("\"foo\\\"", out Expression expr).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatUnterminatedLiteralExpressionIsInvalid()
        {
            ExpressionParser.TryParse("\"foo", out Expression expr).Should().BeFalse();
            expr.Should().BeNull();
        }

        [TestMethod]
        public void TestThatBasicIfThenElseExpressionIsValid()
        {
            ExpressionParser.TryParse("if $x then $y else $z", out Expression expr).Should().BeTrue();
            var cond = expr.Should().BeOfType<ConditionalExpression>().Which;

            cond.Condition.Should().BeOfType<VariableExpression>().Which.VariableName.Should().Be("x");
            cond.ThenExpression.Should().BeOfType<VariableExpression>().Which.VariableName.Should().Be("y");
            cond.ElseExpression.Value.Should().BeOfType<VariableExpression>().Which.VariableName.Should().Be("z");
        }

        [TestMethod]
        public void TestThatWhiteSpaceIsRequiredBetweenPartsOfIfThenElseExpression()
        {
            ExpressionParser.TryParse("if$x then $y else $z", out Expression expr).Should().BeFalse();
            ExpressionParser.TryParse("if $x then$y else $z", out expr).Should().BeFalse();
            ExpressionParser.TryParse("if $x then $y else$z", out expr).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatIfThenElseExpressionCanContainExtraneousWhiteSpace()
        {
            ExpressionParser.TryParse("if  $x then $y else $z", out Expression expr).Should().BeTrue();
            ExpressionParser.TryParse("if $x  then $y else $z", out expr).Should().BeTrue();
            ExpressionParser.TryParse("if $x then  $y else $z", out expr).Should().BeTrue();
            ExpressionParser.TryParse("if $x then $y  else $z", out expr).Should().BeTrue();
            ExpressionParser.TryParse("if $x then $y else  $z", out expr).Should().BeTrue();
        }

        [TestMethod]
        public void TestThatIfThenElseKeywordsAreCaseSensitive()
        {
            ExpressionParser.TryParse("IF $x then $y else $z", out Expression _).Should().BeFalse();
            ExpressionParser.TryParse("if $x THEN $y else $z", out Expression _).Should().BeFalse();
            ExpressionParser.TryParse("if $x then $y ELSE $z", out Expression _).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatMalformedIfThenElseExpressionsAreInvalid()
        {
            ExpressionParser.TryParse("if", out Expression _).Should().BeFalse();
            ExpressionParser.TryParse("if $", out Expression _).Should().BeFalse();
            ExpressionParser.TryParse("if $x", out Expression _).Should().BeFalse();
            ExpressionParser.TryParse("if $x then", out Expression _).Should().BeFalse();
            ExpressionParser.TryParse("if $x then $", out Expression _).Should().BeFalse();
            ExpressionParser.TryParse("if $x then $y", out Expression _).Should().BeFalse();
            ExpressionParser.TryParse("if $x then $y else", out Expression _).Should().BeFalse();
            ExpressionParser.TryParse("if $x then $y else $", out Expression _).Should().BeFalse();
            ExpressionParser.TryParse("if $x else $y then $", out Expression _).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatBasicConcatenationIsValid()
        {
            ExpressionParser.TryParse("$x + $y", out Expression expr).Should().BeTrue();
            var concat = expr.Should().BeOfType<ConcatenationExpression>().Which;

            concat.Left.Should().BeOfType<VariableExpression>().Which.VariableName.Should().Be("x");
            concat.Right.Should().BeOfType<VariableExpression>().Which.VariableName.Should().Be("y");
        }

        [TestMethod]
        public void TestThatConcatenationRequiresTwoOperands()
        {
            ExpressionParser.TryParse("$x +", out Expression expr).Should().BeFalse();
            ExpressionParser.TryParse("+ $y", out expr).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatConcatenationRequiresSeparatingWhiteSpace()
        {
            ExpressionParser.TryParse("$x+$y", out Expression expr).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatUnaryOperationIsValid()
        {
            ExpressionParser.TryParse("lower($x)", out Expression expr).Should().BeTrue();
            var op = expr.Should().BeOfType<OperatorExpression>().Which;

            op.Operator.Should().Be(Operator.ConvertToLowerCase);
            op.Operand.Should().BeOfType<VariableExpression>().Which.VariableName.Should().Be("x");
        }

        [TestMethod]
        public void TestThatUnaryOperationAllowsWhiteSpaceBeforeParenthesis()
        {
            ExpressionParser.TryParse("lower ($x)", out Expression expr).Should().BeTrue();
        }

        [TestMethod]
        public void TestThatUnaryOperationAllowsWhiteSpaceBetweenParentheses()
        {
            ExpressionParser.TryParse("lower( $x)", out Expression expr).Should().BeTrue();
            ExpressionParser.TryParse("lower($x )", out expr).Should().BeTrue();
        }

        [TestMethod]
        public void TestThatAllUnaryOperatorsAreSupported()
        {
            ExpressionParser.TryParse("lower($x)", out Expression expr).Should().BeTrue();
            var op = expr.Should().BeOfType<OperatorExpression>().Which.Operator.Should().Be(Operator.ConvertToLowerCase);

            ExpressionParser.TryParse("upper($x)", out expr).Should().BeTrue();
            op = expr.Should().BeOfType<OperatorExpression>().Which.Operator.Should().Be(Operator.ConvertToUpperCase);
        }

        [TestMethod]
        public void TestThatAllUnaryOperatorsAreCaseSensitive()
        {
            ExpressionParser.TryParse("Lower($x)", out Expression expr).Should().BeFalse();
            ExpressionParser.TryParse("Upper($x)", out expr).Should().BeFalse();
        }

        [TestMethod]
        public void TestThatParentheticalExpressionIsValid()
        {
            ExpressionParser.TryParse("($x)", out Expression expr).Should().BeTrue();
            var paren = expr.Should().BeOfType<ParenthesisExpression>().Which;

            paren.InnerExpression.Should().BeOfType<VariableExpression>().Which.VariableName.Should().Be("x");
        }

        [TestMethod]
        public void TestThatParentheticalExpressionCanContainLeadingOrTrailingInnerWhiteSpace()
        {
            ExpressionParser.TryParse("( $x)", out Expression expr).Should().BeTrue();
            ExpressionParser.TryParse("($x )", out expr).Should().BeTrue();
        }
    }
}
