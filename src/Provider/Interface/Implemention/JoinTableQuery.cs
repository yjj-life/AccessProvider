using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Provider.Error;
using Provider.Sql;
using Provider.Schema;

namespace Provider.Interface.Implemention
{
	internal class JoinTableQuery<TTable> : TableQuery<TTable>, IQueryJoinable<TTable> where TTable : new()
	{
		public JoinTableQuery(DbSet dbSet) : base(dbSet)
		{
		}

		//private void Join<TPrimary, TSecondary>(Expression<Func<TPrimary, TSecondary, bool>> predicate, JOINTYPE type)
		//{
		//	try
		//	{
		//		var result = _translateQuery.Translate(predicate, ClauseType.Join);
		//		TableSchema tableSchema = TableSchemaResolver.GetTableSchema(typeof(TSecondary));
		//		_sqlBuilder.Join(tableSchema.PrimaryTable, result.CommandText, type);
		//	}
		//	catch(Exception e)
		//	{
		//		this._dbError.Code = ErrorCode.InvalidOperation;
		//		this._dbError.Text = e.Message;
		//	}
		//}

		//public IQueryJoinable<TTable> Join<TPrimary, TSecondary>(Expression<Func<TPrimary, TSecondary, bool>> predicate)
		//{
		//	this.Join(predicate, JOINTYPE.INNER);
		//	return this;
		//}

		//public IQueryJoinable<TTable> LeftJoin<TPrimary, TSecondary>(Expression<Func<TPrimary, TSecondary, bool>> predicate)
		//{
		//	this.Join(predicate, JOINTYPE.LEFT);
		//	return this;
		//}

		//public IQueryJoinable<TTable> RightJoin<TPrimary, TSecondary>(Expression<Func<TPrimary, TSecondary, bool>> predicate)
		//{
		//	this.Join(predicate, JOINTYPE.RIGHT);
		//	return this;
		//}

		protected override string TranslateToNativeColumn(string commandText)
		{
			TableSchema tableSchema = TableSchemaResolver.GetTableSchema(typeof(TTable));
			for (int i = 0, count = tableSchema.FieldsSchema.Count; i < count; i++)
			{
				var field = tableSchema.FieldsSchema[i];
				string column = string.Format("{0}.{1}", field.PropertyInfo.DeclaringType.Name, field.PropertyInfo.Name);
				if (commandText.IndexOf(column) != -1)
				{
					commandText = commandText.Replace(column, field.ToString());
				}
			}

			return commandText;
		}

		protected override void Join(TableSchema tableSchema)
		{
			if (tableSchema.JoinInfos == null || tableSchema.JoinInfos.Count == 0)
			{
				return;
			}
			var keys = tableSchema.JoinInfos.Keys.ToList();
			keys.ForEach(key =>
			{
				var info = tableSchema.JoinInfos[key];
				var items = info.JoinItems.Select(item => item.ToString());

				_sqlBuilder.Join(key, string.Join(" And ", items), info.JoinType);
			});
		}

		public IQueryJoinable<TTable> Where<TJoiner>(Expression<Func<TJoiner, bool>> predicate)
		{
			_parameters.Clear();

			base.Where(predicate, Concatenate.None);
			return this;
		}
		public IQueryJoinable<TTable> WhereAnd<TJoiner>(Expression<Func<TJoiner, bool>> predicate)
		{
			base.Where(predicate, Concatenate.And);
			return this;
		}
		public IQueryJoinable<TTable> WhereOr<TJoiner>(Expression<Func<TJoiner, bool>> predicate)
		{
			base.Where(predicate, Concatenate.Or);
			return this;
		}

		public IQueryJoinable<TTable> Where<TJoiner1, TJoiner2>(Expression<Func<TJoiner1, TJoiner2, bool>> predicate)
		{
			_parameters.Clear();

			base.Where(predicate, Concatenate.None);
			return this;
		}
		public IQueryJoinable<TTable> WhereAnd<TJoiner1, TJoiner2>(Expression<Func<TJoiner1, TJoiner2, bool>> predicate)
		{
			base.Where(predicate, Concatenate.And);
			return this;
		}
		public IQueryJoinable<TTable> WhereOr<TJoiner1, TJoiner2>(Expression<Func<TJoiner1, TJoiner2, bool>> predicate)
		{
			base.Where(predicate, Concatenate.Or);
			return this;
		}

		public IQueryJoinable<TTable> Where<TJoiner1, TJoiner2, TJoiner3>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, bool>> predicate)
		{
			_parameters.Clear();

			base.Where(predicate, Concatenate.None);
			return this;
		}
		public IQueryJoinable<TTable> WhereAnd<TJoiner1, TJoiner2, TJoiner3>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, bool>> predicate)
		{
			base.Where(predicate, Concatenate.And);
			return this;
		}
		public IQueryJoinable<TTable> WhereOr<TJoiner1, TJoiner2, TJoiner3>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, bool>> predicate)
		{
			base.Where(predicate, Concatenate.Or);
			return this;
		}

		public IQueryJoinable<TTable> Where<TJoiner1, TJoiner2, TJoiner3, TJoiner4>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, TJoiner4, bool>> predicate)
		{
			_parameters.Clear();

			base.Where(predicate, Concatenate.None);
			return this;
		}
		public IQueryJoinable<TTable> WhereAnd<TJoiner1, TJoiner2, TJoiner3, TJoiner4>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, TJoiner4, bool>> predicate)
		{
			base.Where(predicate, Concatenate.And);
			return this;
		}
		public IQueryJoinable<TTable> WhereOr<TJoiner1, TJoiner2, TJoiner3, TJoiner4>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, TJoiner4, bool>> predicate)
		{
			base.Where(predicate, Concatenate.Or);
			return this;
		}

		public IQueryJoinable<TTable> Where<TJoiner1, TJoiner2, TJoiner3, TJoiner4, TJoiner5>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, TJoiner4, TJoiner5, bool>> predicate)
		{
			_parameters.Clear();

			base.Where(predicate, Concatenate.None);
			return this;
		}
		public IQueryJoinable<TTable> WhereAnd<TJoiner1, TJoiner2, TJoiner3, TJoiner4, TJoiner5>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, TJoiner4, TJoiner5, bool>> predicate)
		{
			base.Where(predicate, Concatenate.And);
			return this;
		}
		public IQueryJoinable<TTable> WhereOr<TJoiner1, TJoiner2, TJoiner3, TJoiner4, TJoiner5>(Expression<Func<TJoiner1, TJoiner2, TJoiner3, TJoiner4, TJoiner5, bool>> predicate)
		{
			base.Where(predicate, Concatenate.Or);
			return this;
		}

		public IQueryJoinable<TTable> OrderBy<TJoiner, TKey>(Expression<Func<TJoiner, TKey>> predicate)
		{
			base.OrderBy(predicate, SORTTYPE.ASC);
			return this;
		}

		public IQueryJoinable<TTable> OrderByDescending<TJoiner, TKey>(Expression<Func<TJoiner, TKey>> predicate)
		{
			base.OrderBy(predicate, SORTTYPE.DESC);
			return this;
		}

		public IQueryJoinable<TTable> Take<TJoiner, TKey>(Expression<Func<TJoiner, TKey>> predicate)
		{
			base.SetFieldUsable(predicate, ClauseType.Take);
			return this;
		}

		public IQueryJoinable<TTable> Skip<TJoiner, TKey>(Expression<Func<TJoiner, TKey>> predicate)
		{
			base.SetFieldUsable(predicate, ClauseType.Skip);
			return this;
		}

		public IQueryJoinable<TTable> WhereIn<TJoiner, TKey>(Expression<Func<TJoiner, Tuple<TKey, TKey[]>>> predicate)
		{
			_parameters.Clear();

			base.WhereIn(predicate, Concatenate.None);
			return this;
		}

		public IQueryJoinable<TTable> WhereInAnd<TJoiner, TKey>(Expression<Func<TJoiner, Tuple<TKey, TKey[]>>> predicate)
		{
			base.WhereIn(predicate, Concatenate.And);
			return this;
		}

		public IQueryJoinable<TTable> WhereInOr<TJoiner, TKey>(Expression<Func<TJoiner, Tuple<TKey, TKey[]>>> predicate)
		{
			base.WhereIn(predicate, Concatenate.Or);
			return this;
		}
	}
}
