using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq.Expressions;

namespace ORM_1_21_.Linq
{
    /// <summary>
    /// 
    /// </summary>
    internal abstract class ExpressionVisitor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        protected Expression Visit(Expression exp)
        {
            if (exp == null)
                return null;
            switch (exp.NodeType)
            {
                case ExpressionType.Negate:
                case ExpressionType.NegateChecked:
                case ExpressionType.Not:
                case ExpressionType.Convert:
                case ExpressionType.ConvertChecked:
                case ExpressionType.ArrayLength:
                case ExpressionType.Quote:
                case ExpressionType.TypeAs:
                    return VisitUnary((UnaryExpression)exp);
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Coalesce:
                case ExpressionType.ArrayIndex:
                case ExpressionType.RightShift:
                case ExpressionType.LeftShift:
                case ExpressionType.ExclusiveOr:
                    return VisitBinary((BinaryExpression)exp);
                case ExpressionType.TypeIs:
                    return VisitTypeIs((TypeBinaryExpression)exp);
                case ExpressionType.Conditional:
                    return VisitConditional((ConditionalExpression)exp);
                case ExpressionType.Constant:
                    return VisitConstant((ConstantExpression)exp);
                case ExpressionType.Parameter:
                    return VisitParameter((ParameterExpression)exp);
                case ExpressionType.MemberAccess:
                    return VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Call:
                    return VisitMethodCall((MethodCallExpression)exp);
                case ExpressionType.Lambda:
                    return VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return VisitNew((NewExpression)exp);
                case ExpressionType.NewArrayInit:
                case ExpressionType.NewArrayBounds:
                    return VisitNewArray((NewArrayExpression)exp);
                case ExpressionType.Invoke:
                    return VisitInvocation((InvocationExpression)exp);
                case ExpressionType.MemberInit:
                    return VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.ListInit:
                    return VisitListInit((ListInitExpression)exp);
                default:
                    throw new Exception(
                      string.Format(CultureInfo.CurrentCulture,"Unhandled expression type: '{0}'", exp.NodeType));
            }
        }

        private MemberBinding VisitBinding(MemberBinding binding)
        {
            switch (binding.BindingType)
            {
                case MemberBindingType.Assignment:
                    return VisitMemberAssignment((MemberAssignment)binding);
                case MemberBindingType.MemberBinding:
                    return VisitMemberMemberBinding((MemberMemberBinding)binding);
                case MemberBindingType.ListBinding:
                    return VisitMemberListBinding((MemberListBinding)binding);
                default:
                    throw new Exception(
                      string.Format(CultureInfo.CurrentCulture,"Unhandled binding type '{0}'", binding.BindingType));
            }
        }

        private ElementInit VisitElementInitializer(
          ElementInit initializer)
        {
            var arguments =
              VisitExpressionList(initializer.Arguments);
            return arguments != initializer.Arguments ? Expression.ElementInit(initializer.AddMethod, arguments) : initializer;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unaryExpression"></param>
        /// <returns></returns>
        protected virtual Expression VisitUnary(UnaryExpression unaryExpression)
        {
            var operand = Visit(unaryExpression.Operand);
            return operand != unaryExpression.Operand ? Expression.MakeUnary(unaryExpression.NodeType, operand, unaryExpression.Type, unaryExpression.Method) : unaryExpression;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="binaryExpression"></param>
        /// <returns></returns>
        protected virtual Expression VisitBinary(BinaryExpression binaryExpression)
        {
           var left = Visit(binaryExpression.Left);
           var right = Visit(binaryExpression.Right);
           var conversion = Visit(binaryExpression.Conversion);
            if (left != binaryExpression.Left || right != binaryExpression.Right || conversion != binaryExpression.Conversion)
            {
                if (binaryExpression.NodeType == ExpressionType.Coalesce)
                    return Expression.Coalesce(
                      left, right, conversion as LambdaExpression);
                return Expression.MakeBinary(
                    binaryExpression.NodeType, left, right, binaryExpression.IsLiftedToNull, binaryExpression.Method);
            }
            return binaryExpression;
        }

        private Expression VisitTypeIs(TypeBinaryExpression binaryExpression)
        {
            var expr = Visit(binaryExpression.Expression);
            return expr != binaryExpression.Expression ? Expression.TypeIs(expr, binaryExpression.TypeOperand) : binaryExpression;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="constantExpression"></param>
        /// <returns></returns>
        protected virtual Expression VisitConstant(ConstantExpression constantExpression)
        {
            return constantExpression;
        }

        private Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
           var test = Visit(conditionalExpression.Test);
           var ifTrue = Visit(conditionalExpression.IfTrue);
           var ifFalse = Visit(conditionalExpression.IfFalse);
            if (test != conditionalExpression.Test || ifTrue != conditionalExpression.IfTrue || ifFalse != conditionalExpression.IfFalse)
            {
                return Expression.Condition(test, ifTrue, ifFalse);
            }
            return conditionalExpression;
        }

        private static Expression VisitParameter(ParameterExpression parameterExpression)
        {
            return parameterExpression;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="memberExpression"></param>
        /// <returns></returns>
        protected virtual Expression VisitMemberAccess(MemberExpression memberExpression)
        {
            var exp = Visit(memberExpression.Expression);
            return exp != memberExpression.Expression ? Expression.MakeMemberAccess(exp, memberExpression.Member) : memberExpression;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodCallExpression"></param>
        /// <returns></returns>
        protected virtual Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            var obj = Visit(methodCallExpression.Object);
            IEnumerable<Expression> args = VisitExpressionList(methodCallExpression.Arguments);
            if (obj != methodCallExpression.Object || args != methodCallExpression.Arguments)
            {
                return Expression.Call(obj, methodCallExpression.Method, args);
            }
            return methodCallExpression;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        protected virtual ReadOnlyCollection<Expression> VisitExpressionList(ReadOnlyCollection<Expression> original)
         
        {
            List<Expression> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                
                var p = Visit(original[i]);
                if (list != null)
                {
                    list.Add(p);
                }
                else if (p != original[i])
                {
                    list = new List<Expression>(n);
                    for (var j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(p);
                }
            }
            return list != null ? list.AsReadOnly() : original;
        }

        private MemberAssignment
          VisitMemberAssignment(MemberAssignment assignment)
        {
            var e = Visit(assignment.Expression);
            return e != assignment.Expression ? Expression.Bind(assignment.Member, e) : assignment;
        }

        private MemberMemberBinding
          VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            var bindings =
              VisitBindingList(binding.Bindings);
            return bindings != binding.Bindings ? Expression.MemberBind(binding.Member, bindings) : binding;
        }

        private MemberListBinding
          VisitMemberListBinding(MemberListBinding binding)
        {
            var initializers =
              VisitElementInitializerList(binding.Initializers);
            return initializers != binding.Initializers ? Expression.ListBind(binding.Member, initializers) : binding;
        }

        private IEnumerable<MemberBinding>
          VisitBindingList(ReadOnlyCollection<MemberBinding> original)
        {
            List<MemberBinding> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                var b = VisitBinding(original[i]);
                if (list != null)
                {
                    list.Add(b);
                }
                else if (b != original[i])
                {
                    list = new List<MemberBinding>(n);
                    for (var j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(b);
                }
            }
            if (list != null)
                return list;
            return original;
        }

        private IEnumerable<ElementInit>
          VisitElementInitializerList(ReadOnlyCollection<ElementInit> original)
        {
            List<ElementInit> list = null;
            for (int i = 0, n = original.Count; i < n; i++)
            {
                var init = VisitElementInitializer(original[i]);
                if (list != null)
                {
                    list.Add(init);
                }
                else if (init != original[i])
                {
                    list = new List<ElementInit>(n);
                    for (var j = 0; j < i; j++)
                    {
                        list.Add(original[j]);
                    }
                    list.Add(init);
                }
            }
            if (list != null)
                return list;
            return original;
        }

        private Expression VisitLambda(LambdaExpression lambda)
        {
            var body = Visit(lambda.Body);
           
            return body != lambda.Body ? Expression.Lambda(lambda.Type, body, lambda.Parameters) : lambda;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nex"></param>
        /// <returns></returns>
        protected virtual NewExpression VisitNew(NewExpression nex)
        {
            IEnumerable<Expression> args = VisitExpressionList(nex.Arguments);
            if (args != nex.Arguments)
            {
                return Expression.New(nex.Constructor, args, nex.Members);
            }
            return nex;
        }

        private Expression VisitMemberInit(MemberInitExpression init)
        {
            var n = VisitNew(init.NewExpression);
            var bindings = VisitBindingList(init.Bindings);
            if (n != init.NewExpression || bindings != init.Bindings)
            {
                return Expression.MemberInit(n, bindings);
            }
            return init;
        }

        private Expression VisitListInit(ListInitExpression init)
        {
            var n = VisitNew(init.NewExpression);
            var initializers =
              VisitElementInitializerList(init.Initializers);
            if (n != init.NewExpression || initializers != init.Initializers)
            {
                return Expression.ListInit(n, initializers);
            }
            return init;
        }

        private Expression VisitNewArray(NewArrayExpression newArrayExpression)
        {
            IEnumerable<Expression> exprs = VisitExpressionList(newArrayExpression.Expressions);
            if (exprs != newArrayExpression.Expressions)
            {
                if (newArrayExpression.NodeType == ExpressionType.NewArrayInit)
                {
                    return Expression.NewArrayInit(newArrayExpression.Type.GetElementType(), exprs);
                }
                return Expression.NewArrayBounds(newArrayExpression.Type.GetElementType(), exprs);
            }
            return newArrayExpression;
        }

        private Expression VisitInvocation(InvocationExpression invocationExpression)
        {
            IEnumerable<Expression> args = VisitExpressionList(invocationExpression.Arguments);
            var expr = Visit(invocationExpression.Expression);
            if (args != invocationExpression.Arguments || expr != invocationExpression.Expression)
            {
                return Expression.Invoke(expr, args);
            }
            return invocationExpression;
        }
    }
}
