using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Provider.Schema.Attributes;

namespace Provider.Sql
{
	internal enum ClauseType
	{
		//Select,
		Top,
		Column,
		From,
		Where,
		In,
		OrderBy,
		ThenBy,
		Take,
		Skip,
		Join,
		Insert,
		Values,
		Update,
		ColumnValue,
		SaveWhere,
		Delete
	}

	internal enum SORTTYPE
	{
		ASC,
		DESC
	}
	//internal enum JOINTYPE
	//{
	//	INNER,
	//	LEFT,
	//	RIGHT
	//}

	internal enum Concatenate
	{
		None,
		And,
		Or
	}

	internal class SqlBuilder
	{
		Dictionary<ClauseType, List<string>> dicSqlText = new Dictionary<ClauseType, List<string>>();

		internal void Top(int count)
		{
			if (count != -1)
			{
				AppenSqlText(ClauseType.Top, count.ToString());
			}
		}

		internal void ColumnClear()
		{
			ClearSqlText(ClauseType.Column);
		}

		internal void Column(string commandText)
		{
			AppenSqlText(ClauseType.Column, commandText);
		}

		internal void ValuesClear()
		{
			ClearSqlText(ClauseType.Values);
		}
		internal void Values(string commandText)
		{
			AppenSqlText(ClauseType.Values, commandText);
		}

		internal void ColumnValue(string commandText)
		{
			ReplaceSqlText(ClauseType.ColumnValue, commandText);
		}

		internal void From(string tableName)
		{
			ReplaceSqlText(ClauseType.From, tableName);
		}

		internal void Where(string commandText, Concatenate concatenate = Concatenate.None)
		{
			if (concatenate == Concatenate.None)
			{
				ReplaceSqlText(ClauseType.Where, commandText);
			}
			else
			{
				if (this.dicSqlText.TryGetValue(ClauseType.Where, out List<string> listText))
				{
					AppenSqlText(ClauseType.Where, Enum.GetName(typeof(Concatenate), concatenate));
					AppenSqlText(ClauseType.Where, commandText);
				}
			}
		}

		internal void SaveWhere(string commandText)
		{
			ReplaceSqlText(ClauseType.SaveWhere, commandText);
		}

		internal void OrderBy(string commandText, SORTTYPE sort)
		{
			string text = string.Format("{0} {1}", commandText, Enum.GetName(typeof(SORTTYPE), sort));
			AppenSqlText(ClauseType.OrderBy, text);
		}

		internal void Join(string joinerTable, string commandText, JoinType type)
		{
			string text = string.Format("{0} JOIN {1} ON {2}", Enum.GetName(typeof(JoinType), type), joinerTable, commandText);
			AppenSqlText(ClauseType.Join, text);
		}

		private void AppenSqlText(ClauseType clauseType, string sqlText)
		{
			if (this.dicSqlText.TryGetValue(clauseType, out List<string> listText))
			{
				listText.Add(sqlText);
			}
			else
			{
				listText = new List<string>()
				{
					sqlText
				};
				this.dicSqlText.Add(clauseType, listText);
			}
		}

		private void ClearSqlText(ClauseType clauseType)
		{
			if (this.dicSqlText.TryGetValue(clauseType, out List<string> listText))
			{
				this.dicSqlText.Remove(clauseType);
			}
		}

		private void ReplaceSqlText(ClauseType clauseType, string sqlText)
		{
			ClearSqlText(clauseType);
			AppenSqlText(clauseType, sqlText);
		}

		public string Query()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("SELECT ");
			BuildTop(sb);
			sb.Append(" ");
			BuildColumn(sb);
			sb.Append(" ");
			BuildFrom(sb);
			sb.Append(" ");
			BuildJoin(sb);
			sb.Append(" ");
			BuildWhere(sb);
			sb.Append(" ");
			BuildOrderBy(sb);

			return sb.ToString();
		}

		public string DeleteAll()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("DELETE ");
			BuildFrom(sb);
			sb.Append(" ");
			BuildWhere(sb);

			return sb.ToString();
		}

		public string Delete()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("DELETE ");
			BuildFrom(sb);
			sb.Append(" ");
			BuildSaveWhere(sb);

			return sb.ToString();
		}

		public string Insert()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("INSERT INTO ");
			if (dicSqlText.TryGetValue(ClauseType.From, out List<string> listText))
			{
				sb.Append(listText[0]);
			}
			sb.Append("(");
			BuildColumn(sb);
			sb.Append(") ");
			BuildValues(sb);

			return sb.ToString();
		}

		public string Update()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append("UPDATE ");
			if (dicSqlText.TryGetValue(ClauseType.From, out List<string> listText))
			{
				sb.Append(listText[0]);
			}
			sb.Append(" SET ");
			BuildColumnValue(sb);
			sb.Append(" ");
			BuildSaveWhere(sb);

			return sb.ToString();
		}

		private bool BuildTop(StringBuilder sb)
		{
			if (dicSqlText.TryGetValue(ClauseType.Top, out List<string> listText))
			{
				sb.Append(string.Format("TOP({0})", listText[0]));
				return true;
			}

			return false;
		}

		//private bool BuildSelect(StringBuilder sb)
		//{
		//	string top = "";
		//	if (dicSqlText.TryGetValue(ClauseType.Top, out List<string> listText))
		//	{
		//		top = string.Format(" TOP({0}) ", listText[0]);
		//	}
		//	sb.Append(string.Format("SELECT{0} ", top));

		//	sb.Append(" ");

		//	return false;
		//}

		private bool BuildColumn(StringBuilder sb)
		{
			if (dicSqlText.TryGetValue(ClauseType.Column, out List<string> listText))
			{
				sb.Append(String.Join(",", listText));
				return true;
			}

			return false;
		}

		private bool BuildFrom(StringBuilder sb)
		{
			if (dicSqlText.TryGetValue(ClauseType.From, out List<string> listText))
			{
				sb.Append("FROM ");
				sb.Append(listText[0]);

				return true;
			}

			return false;
		}

		private bool BuildWhere(StringBuilder sb)
		{
			if (dicSqlText.TryGetValue(ClauseType.Where, out List<string> listText))
			{
				sb.Append("WHERE ");
				sb.Append(String.Join(" ", listText));

				return true;
			}

			return false;
		}

		private bool BuildSaveWhere(StringBuilder sb)
		{
			if (dicSqlText.TryGetValue(ClauseType.SaveWhere, out List<string> listText))
			{
				sb.Append("WHERE ");
				sb.Append(listText[0]);

				return true;
			}

			return false;
		}

		private bool BuildColumnValue(StringBuilder sb)
		{
			if (dicSqlText.TryGetValue(ClauseType.ColumnValue, out List<string> listText))
			{
				sb.Append(listText[0]);
				return true;
			}

			return false;
		}

		private bool BuildOrderBy(StringBuilder sb)
		{
			if (dicSqlText.TryGetValue(ClauseType.OrderBy, out List<string> listText))
			{
				sb.Append("ORDER BY ");
				sb.Append(String.Join(",", listText));

				return true;
			}

			return false;
		}

		private bool BuildJoin(StringBuilder sb)
		{
			if (dicSqlText.TryGetValue(ClauseType.Join, out List<string> listText))
			{
				sb.Append(String.Join(" ", listText));
				return true;
			}

			return false;
		}

		private bool BuildValues(StringBuilder sb)
		{
			if (dicSqlText.TryGetValue(ClauseType.Values, out List<string> listText))
			{
				sb.Append("VALUES(");
				sb.Append(String.Join(",", listText));
				sb.Append(")");
				return true;
			}

			return false;
		}
	}
}
