using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Provider.Sql;

namespace Provider.Translate
{
	internal class QueryTranslator : ExpressionVisitor
	{
		ClauseBuilder clauseBuilder;
		ParameterBuilder parameterBuilder;
		ClauseType clauseType;

		internal TranslateResult Translate<T>(Expression<T> expression, ClauseType clauseType)
		{
			this.clauseBuilder = new ClauseBuilder();
			this.parameterBuilder = new ParameterBuilder();

			this.clauseType = clauseType;

			this.Visit(expression);

			return new TranslateResult
			{
				CommandText = clauseBuilder.GetExpressionText(),
				Parameters = clauseBuilder.GetExpressionParameters(),
			};
		}

		private void Visit<T>(Expression<T> node)
		{
			if (node.NodeType != ExpressionType.Lambda)
			{
				throw new NotSupportedException(string.Format("The clause '{0}' is not supported", Enum.GetName(typeof(ClauseType), clauseType)));
			}

			switch (this.clauseType)
			{
				case ClauseType.OrderBy:
				case ClauseType.ThenBy:
				case ClauseType.Take:
				case ClauseType.Skip:
					if (node.Body.NodeType != ExpressionType.MemberAccess)
					{
						throw new NotSupportedException(string.Format("The clause '{0}' is not supported", Enum.GetName(typeof(ClauseType), clauseType)));
					}
					break;
			}
			this.Visit(Evaluator.PartialEval(node));
		}

		protected override Expression VisitUnary(UnaryExpression node)
		{
			switch (node.NodeType)
			{
				case ExpressionType.Not:
					clauseBuilder.AppendClause(" NOT ");
					this.Visit(node.Operand);
					break;
				case ExpressionType.Convert:
					this.Visit(node.Operand);
					break;
				default:
					throw new NotSupportedException(string.Format("The unary operator '{0}' is not suported", node.NodeType));
			}

			return node;
		}

		protected override Expression VisitBinary(BinaryExpression node)
		{
			clauseBuilder.AppendClause("(");
			this.Visit(node.Left);
			switch (node.NodeType)
			{
				case ExpressionType.AndAlso:
					clauseBuilder.AppendClause(" AND ");
					break;
				case ExpressionType.OrElse:
					clauseBuilder.AppendClause(" OR ");
					break;
				case ExpressionType.Equal:
					clauseBuilder.AppendClause(" = ");
					break;
				case ExpressionType.NotEqual:
					clauseBuilder.AppendClause(" <> ");
					break;
				case ExpressionType.LessThan:
					clauseBuilder.AppendClause(" < ");
					break;
				case ExpressionType.LessThanOrEqual:
					clauseBuilder.AppendClause(" <= ");
					break;
				case ExpressionType.GreaterThan:
					clauseBuilder.AppendClause(" > ");
					break;
				case ExpressionType.GreaterThanOrEqual:
					clauseBuilder.AppendClause(" >= ");
					break;
				default:
					throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", node.NodeType.ToString()));
			}
			this.Visit(node.Right);
			clauseBuilder.AppendClause(")");

			return node;
		}

		protected override Expression VisitConstant(ConstantExpression node)
		{
			if (this.clauseType == ClauseType.Join)
			{
				throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", node.Value));
			}

			switch (Type.GetTypeCode(node.Value.GetType()))
			{
				case TypeCode.Boolean:
					clauseBuilder.AppendClause(parameterBuilder.GenerateParameterName(), ((bool)node.Value) ? 1 : 0);
					break;
				case TypeCode.Int16:
					clauseBuilder.AppendClause(parameterBuilder.GenerateParameterName(), node.Value);
					break;
				case TypeCode.Int32:
					clauseBuilder.AppendClause(parameterBuilder.GenerateParameterName(), node.Value);
					break;
				case TypeCode.String:
					clauseBuilder.AppendClause(parameterBuilder.GenerateParameterName(), node.Value);
					break;
				case TypeCode.Object:
					if (this.clauseType == ClauseType.In)
					{
						this.VisitInConstant(node);
						break;
					}
					throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", node.Value));
				default:
					clauseBuilder.AppendClause(parameterBuilder.GenerateParameterName(), node.Value);
					break;
			}
			return node;
		}

		private void VisitInConstant(ConstantExpression node)
		{
			object[] values = node.Value as object[];
			if (values == null || values.Length == 0)
			{
				throw new NotSupportedException(string.Format("The constant for '{0}' is not supported", node.Value));
			}

			clauseBuilder.AppendClause(" IN ");
			clauseBuilder.AppendClause("(");
			bool isFirst = true;
			foreach (var obj in values)
			{
				if (isFirst == false)
				{
					clauseBuilder.AppendClause(",");
				}
				clauseBuilder.AppendClause(parameterBuilder.GenerateParameterName(), obj);
				isFirst = false;
			}
			clauseBuilder.AppendClause(")");
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			if (node.Expression != null && node.Expression.NodeType == ExpressionType.Parameter)
			{
				clauseBuilder.AppendClause(string.Format("{0}.{1}", node.Member.DeclaringType.Name, node.Member.Name));
				return node;
			}

			throw new NotSupportedException(string.Format("The member '{0}' is not supported", node.Member.Name));
		}

		protected override CatchBlock VisitCatchBlock(CatchBlock node)
		{
			return base.VisitCatchBlock(node);
		}

		protected override ElementInit VisitElementInit(ElementInit node)
		{
			return base.VisitElementInit(node);
		}

		protected override LabelTarget VisitLabelTarget(LabelTarget node)
		{
			return base.VisitLabelTarget(node);
		}

		protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
		{
			return base.VisitMemberAssignment(node);
		}

		protected override MemberBinding VisitMemberBinding(MemberBinding node)
		{
			return base.VisitMemberBinding(node);
		}

		protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
		{
			return base.VisitMemberListBinding(node);
		}

		protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
		{
			return base.VisitMemberMemberBinding(node);
		}

		protected override SwitchCase VisitSwitchCase(SwitchCase node)
		{
			return base.VisitSwitchCase(node);
		}

		protected override Expression VisitBlock(BlockExpression node)
		{
			return base.VisitBlock(node);
		}

		protected override Expression VisitConditional(ConditionalExpression node)
		{
			return base.VisitConditional(node);
		}

		protected override Expression VisitDefault(DefaultExpression node)
		{
			return base.VisitDefault(node);
		}

		protected override Expression VisitExtension(Expression node)
		{
			return base.VisitExtension(node);
		}

		protected override Expression VisitGoto(GotoExpression node)
		{
			return base.VisitGoto(node);
		}

		protected override Expression VisitInvocation(InvocationExpression node)
		{
			return base.VisitInvocation(node);
		}

		protected override Expression VisitLabel(LabelExpression node)
		{
			return base.VisitLabel(node);
		}

		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			return base.VisitLambda(node);
		}

		protected override Expression VisitLoop(LoopExpression node)
		{
			return base.VisitLoop(node);
		}

		protected override Expression VisitIndex(IndexExpression node)
		{
			return base.VisitIndex(node);
		}

		protected override Expression VisitNewArray(NewArrayExpression node)
		{
			return base.VisitNewArray(node);
		}

		protected override Expression VisitNew(NewExpression node)
		{
			return base.VisitNew(node);
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			return base.VisitParameter(node);
		}

		protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
		{
			return base.VisitRuntimeVariables(node);
		}

		protected override Expression VisitSwitch(SwitchExpression node)
		{
			return base.VisitSwitch(node);
		}

		protected override Expression VisitTry(TryExpression node)
		{
			return base.VisitTry(node);
		}

		protected override Expression VisitTypeBinary(TypeBinaryExpression node)
		{
			return base.VisitTypeBinary(node);
		}

		protected override Expression VisitMemberInit(MemberInitExpression node)
		{
			return base.VisitMemberInit(node);
		}

		protected override Expression VisitListInit(ListInitExpression node)
		{
			return base.VisitListInit(node);
		}
	}
}
