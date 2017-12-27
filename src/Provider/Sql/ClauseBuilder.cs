using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Sql
{

	internal class Clause
	{
		internal string SqlText { get; set; }
	}
	internal class Clause<T> : Clause
	{
		internal T Value { get; set; }
	}

	internal class ClauseBuilder
	{
		private List<Clause> _listClause = new List<Clause>();
		
		public void AppendClause(string text)
		{
			_listClause.Add(new Clause { SqlText = text });
		}

		public void AppendClause<T>(string text, T value)
		{
			_listClause.Add(new Clause<T> { SqlText = text, Value = value });
		}

		public string GetExpressionText(String separator = "")
		{
			List<string> clauses = new List<string>();

			this._listClause.ForEach(clause => clauses.Add(clause.SqlText));

			return string.Join(separator, clauses);
		}

		public List<Parameter> GetExpressionParameters()
		{
			List<Parameter> parameters = new List<Parameter>();

			_listClause.ForEach(clause =>
			{
				var c = clause as Clause<object>;
				if (c != null)
				{
					parameters.Add(new Parameter
					{
						Name = c.SqlText,
						Value = c.Value
					});
				}
			});

			return parameters;
		}

	}
}
