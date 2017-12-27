using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

using Provider.Error;
using Provider.Translate;
using Provider.Sql;
using Provider.Schema;
using Provider.Excution;

namespace Provider.Interface.Implemention
{
	internal class TableQuery<TTable> : ITableQuery<TTable> where TTable : new()
	{
		protected DbSet _dbSet;
		protected DbError _dbError;

		internal QueryTranslator _translateQuery;
		internal Query _excuteQuery;
		internal SqlBuilder _sqlBuilder;
		internal List<Parameter> _parameters;
		internal List<bool> _fieldsUsable;

		internal List<RecordCache<TTable>> _recordsCache = new List<RecordCache<TTable>>();

		public TableQuery(DbSet dbSet)
		{
			this._dbSet = dbSet;
			this._dbError = new DbError();

			this._translateQuery = new QueryTranslator();
			this._sqlBuilder = new SqlBuilder();
			this._parameters = new List<Parameter>();
			this._excuteQuery = new Query(dbSet.DbConnection);

			this._fieldsUsable = new List<bool>();
			TableSchema tableSchema = TableSchemaResolver.GetTableSchema(typeof(TTable));
			for (int i = 0, count = tableSchema.FieldsSchema.Count; i < count; i++)
			{
				this._fieldsUsable.Add(true);
			}
		}

		internal void OrderBy<T>(Expression<T> predicate, SORTTYPE type)
		{
			try
			{
				var result = _translateQuery.Translate(predicate, ClauseType.OrderBy);
				_sqlBuilder.OrderBy(TranslateToNativeColumn(result.CommandText), type);
			}
			catch(Exception e)
			{
				this._dbError.Code = ErrorCode.InvalidOperation;
				this._dbError.Text = e.Message;
			}
		}

		public ITableQuery<TTable> OrderBy<TKey>(Expression<Func<TTable, TKey>> predicate)
		{
			OrderBy(predicate, SORTTYPE.ASC);
			return this;
		}

		public ITableQuery<TTable> OrderByDescending<TKey>(Expression<Func<TTable, TKey>> predicate)
		{
			OrderBy(predicate, SORTTYPE.DESC);
			return this;
		}

		public List<TTable> Select(int count = -1)
		{
			TableSchema tableSchema = TableSchemaResolver.GetTableSchema(typeof(TTable));

			_sqlBuilder.From(tableSchema.PrimaryTable);
			_sqlBuilder.Top(count);

			for (int i = 0, fieldCount = tableSchema.FieldsSchema.Count; i < fieldCount; i++)
			{
				var field = tableSchema.FieldsSchema[i];
				if (this._fieldsUsable[i] == true)
				{
					string column = field.ToString();
					if (string.IsNullOrEmpty(field.AliasName) == false)
					{
						column = string.Format("{0} AS {1}", column, field.AliasName);
					}
					_sqlBuilder.Column(column);
				}
			}

			_recordsCache = _excuteQuery.Excute<TTable>(_sqlBuilder.Query(), _parameters, _fieldsUsable);

			if (_recordsCache == null)
			{
				this._dbError.Code = _excuteQuery.dbError.Code;
				this._dbError.Text = _excuteQuery.dbError.Text;

				return null;
			}

			return _recordsCache.Select(x => x.recordOut).ToList();
		}

		private void SetFieldUsable(bool usable)
		{
			TableSchema tableSchema = TableSchemaResolver.GetTableSchema(typeof(TTable));
			for (int i = 0, fieldCount = this._fieldsUsable.Count; i < fieldCount; i++)
			{
				var field = tableSchema.FieldsSchema[i];
				if (field.IsPrimaryKey || field.IsUpdCount)
				{
					this._fieldsUsable[i] = true;
				}
				else
				{
					this._fieldsUsable[i] = usable;
				}
			}
		}

		internal void SetFieldUsable<T>(Expression<T> predicate, ClauseType type)
		{
			try
			{
				var result = _translateQuery.Translate(predicate, type);
				string commandText = TranslateToNativeColumn(result.CommandText);

				TableSchema tableSchema = TableSchemaResolver.GetTableSchema(typeof(TTable));
				var fieldIndex = tableSchema.FieldsSchema.FindIndex(x => x.ToString() == commandText);
				if (fieldIndex != -1)
				{
					var field = tableSchema.FieldsSchema[fieldIndex];
					if (field.IsPrimaryKey || field.IsUpdCount)
					{
						this._fieldsUsable[fieldIndex] = true;
					}
					else
					{
						this._fieldsUsable[fieldIndex] = type == ClauseType.Take;
					}
				}
			}
			catch(Exception e)
			{
				this._dbError.Code = ErrorCode.InvalidOperation;
				this._dbError.Text = e.Message;
			}
		}

		public ITableQuery<TTable> Skip<TKey>(Expression<Func<TTable, TKey>> predicate)
		{
			SetFieldUsable(predicate, ClauseType.Skip);
			return this;
		}

		public ITableQuery<TTable> Skip()
		{
			SetFieldUsable(false);
			return this;
		}

		public ITableQuery<TTable> Take<TKey>(Expression<Func<TTable, TKey>> predicate)
		{
			SetFieldUsable(predicate, ClauseType.Take);
			return this;
		}

		public ITableQuery<TTable> Take()
		{
			SetFieldUsable(true);
			return this;
		}

		protected virtual string TranslateToNativeColumn(string commandText)
		{
			return commandText;
		}

		private void Where(TranslateResult result, Concatenate concatenate)
		{
			try
			{
				result.CommandText = TranslateToNativeColumn(result.CommandText);
				_parameters.AddRange(result.Parameters);
				_sqlBuilder.Where(result.CommandText, concatenate);
			}
			catch(Exception e)
			{
				this._dbError.Code = ErrorCode.InvalidOperation;
				this._dbError.Text = e.Message;
			}
		}

		internal void Where<T>(Expression<T> predicate, Concatenate concatenate)
		{
			try
			{
				var result = _translateQuery.Translate(predicate, ClauseType.Where);
				this.Where(result, concatenate);
			}
			catch (Exception e)
			{
				this._dbError.Code = ErrorCode.InvalidOperation;
				this._dbError.Text = e.Message;
			}
		}

		public ITableQuery<TTable> Where(Expression<Func<TTable, bool>> predicate)
		{
			_parameters.Clear();
			this.Where(predicate, Concatenate.None);
			return this;
		}

		public ITableQuery<TTable> WhereAnd(Expression<Func<TTable, bool>> predicate)
		{
			this.Where(predicate, Concatenate.And);
			return this;
		}

		public ITableQuery<TTable> WhereOr(Expression<Func<TTable, bool>> predicate)
		{
			this.Where(predicate, Concatenate.Or);
			return this;
		}

		internal void WhereIn<T>(Expression<T> predicate, Concatenate concatenate)
		{
			var result = _translateQuery.Translate(predicate, ClauseType.In);
			this.Where(result, concatenate);
		}

		public ITableQuery<TTable> WhereIn<TKey>(Expression<Func<TTable, Tuple<TKey, TKey[]>>> predicate)
		{
			_parameters.Clear();
			this.WhereIn(predicate, Concatenate.None);
			return this;
		}

		public ITableQuery<TTable> WhereInAnd<TKey>(Expression<Func<TTable, Tuple<TKey, TKey[]>>> predicate)
		{
			this.WhereIn(predicate, Concatenate.And);
			return this;
		}

		public ITableQuery<TTable> WhereInOr<TKey>(Expression<Func<TTable, Tuple<TKey, TKey[]>>> predicate)
		{
			this.WhereIn(predicate, Concatenate.Or);
			return this;
		}

	}
}
