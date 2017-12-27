using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Translate
{
	internal static class Evaluator
	{
		public static Expression PartialEval(Expression expression, Func<Expression, bool> fnCanBeEvaluated)
		{
			return new SubtreeEvaluator(new Nominator(fnCanBeEvaluated).Nominate(expression)).Eval(expression);
		}

		public static Expression PartialEval(Expression expression)
		{
			return PartialEval(expression, Evaluator.CanBeEvaluatedLocally);
		}

		private static bool CanBeEvaluatedLocally(Expression expression)
		{
			return expression.NodeType != ExpressionType.Parameter;
		}

		class SubtreeEvaluator : ExpressionVisitor
		{
			HashSet<Expression> candidates;

			internal SubtreeEvaluator(HashSet<Expression> candidates)
			{
				this.candidates = candidates;
			}

			internal Expression Eval(Expression exp)
			{
				return this.Visit(exp);
			}

			public override Expression Visit(Expression node)
			{
				if (node == null)
				{
					return null;
				}

				if (this.candidates.Contains(node))
				{
					return this.Evaluate(node);
				}

				return base.Visit(node);
			}

			private Expression Evaluate(Expression node)
			{
				if (node.NodeType == ExpressionType.Constant)
				{
					return node;
				}

				LambdaExpression lambda = Expression.Lambda(node);
				Delegate fn = lambda.Compile();

				object obj = fn.DynamicInvoke(null);

				return Expression.Constant(fn.DynamicInvoke(null), node.Type);
			}
		}

		class Nominator : ExpressionVisitor
		{
			Func<Expression, bool> fnCanBeEvaluated;
			HashSet<Expression> candidates;

			bool cannotBeEvaluated;

			internal Nominator(Func<Expression, bool> fnCanBeEvaluated)
			{
				this.fnCanBeEvaluated = fnCanBeEvaluated;
			}

			internal HashSet<Expression> Nominate(Expression expression)
			{
				this.candidates = new HashSet<Expression>();
				this.Visit(expression);
				return this.candidates;
			}

			public override Expression Visit(Expression node)
			{
				if (node != null)
				{
					bool saveCannotBeEvaluated = this.cannotBeEvaluated;
					this.cannotBeEvaluated = false;
					base.Visit(node);

					if (!this.cannotBeEvaluated)
					{
						if (this.fnCanBeEvaluated(node))
						{
							this.candidates.Add(node);
						}
						else
						{
							this.cannotBeEvaluated = true;
						}
					}

					this.cannotBeEvaluated |= saveCannotBeEvaluated;
				}

				return node;
			}
		}
	}
}
