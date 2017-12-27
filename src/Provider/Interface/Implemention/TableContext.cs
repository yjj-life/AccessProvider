using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Provider.Error;
using Provider.Schema;
using Provider.Excution;
using Provider.Translate;
using Provider.Sql;
using Provider.ExpFunc;

namespace Provider.Interface.Implemention
{
	internal class TableContext<TTable> : TableQuery<TTable>, IUpdatable<TTable> where TTable : class , new()
	{
		internal InstanceTranslator _translate;

		internal Update _excuteUpdate;

		public TableContext(DbSet dbSet) : base(dbSet)
		{
			_translate = new InstanceTranslator();
			_excuteUpdate = new Update(dbSet.DbConnection);
		}

		public bool AddNew(TTable record)
		{
			return AddNew(new List<TTable>() { record });
		}

		public bool AddNew(Func<TTable> recordFunc)
		{
			return AddNew(recordFunc());
		}

		private TranslateResult PreAddNew(TTable record, List<Parameter> parameters)
		{
			var result = _translate.Translator(record, typeof(TTable), _fieldsUsable, ClauseType.Values, parameters);

			if (parameters == null)
			{
				_sqlBuilder.ColumnClear();
				_sqlBuilder.ValuesClear();

				_sqlBuilder.Values(result.CommandText);
			}

			return result;
		}

		public bool AddNew(List<TTable> records)
		{
			if (records == null || records.Count == 0)
			{
				return false;
			}

			List<List<Parameter>> listParameters = new List<List<Parameter>>();

			TranslateResult resultFirst = null;
			TranslateResult resultNext = null;
			for (int i = 0, count = records.Count; i < count; i++)
			{
				var record = records[i];
				if (i == 0)
				{
					resultFirst = PreAddNew(record, null);
					listParameters.Add(resultFirst.Parameters);
				}
				else
				{
					resultNext = PreAddNew(record, resultFirst.Parameters);
					listParameters.Add(resultNext.Parameters);
				}
			}

			TableSchema tableSchema = TableSchemaResolver.GetTableSchema(typeof(TTable));
			_sqlBuilder.From(tableSchema.PrimaryTable);

			for (int i = 0, fieldCount = tableSchema.FieldsSchema.Count; i < fieldCount; i++)
			{
				_sqlBuilder.Column(tableSchema.FieldsSchema[i].FieldName);
			}

			_dbSet.BeginTransaction();

			if (listParameters.Count == 1)
			{
				this._dbError = _excuteUpdate.Excute<TTable>(_sqlBuilder.Insert(), listParameters[0], _dbSet.Transaction);
			}
			else
			{
				this._dbError = _excuteUpdate.ExcuteBatch<TTable>(_sqlBuilder.Insert(), listParameters, _dbSet.Transaction);
			}

			if (this._dbError.Code == ErrorCode.Success)
			{
				records.ForEach(rec => InsertRecordToCache(rec));
			}
			else
			{
				_dbSet.Cancel();
			}

			return this._dbError.Code == ErrorCode.Success;
		}

		public bool Delete()
		{
			TableSchema tableSchema = TableSchemaResolver.GetTableSchema(typeof(TTable));
			_sqlBuilder.From(tableSchema.PrimaryTable);

			_dbSet.BeginTransaction();

			this._dbError = _excuteUpdate.Excute<TTable>(_sqlBuilder.DeleteAll(), null, _dbSet.Transaction);

			if (this._dbError.Code != ErrorCode.Success)
			{
				_dbSet.Cancel();
			}

			return this._dbError.Code == ErrorCode.Success;
		}

		public bool Delete(TTable record)
		{
			if (IsValidRecord(record) == false)
			{
				return false;
			}

			return Delete(new List<TTable>() { record });
		}

		public bool Delete(List<TTable> records)
		{
			if (records == null || records.Count == 0)
			{
				return false;
			}

			Action<TTable, TTable> copyAction = ExpressionFunc.PrimaryKeyCopy<TTable>();

			List<List<Parameter>> listParameters = new List<List<Parameter>>();

			TranslateResult resultFirst = null;
			TranslateResult resultNext = null;
			for (int i = 0, count = records.Count; i < count; i++)
			{
				var record = records[i];
				if (IsValidRecord(record) == false)
				{
					return false;
				}

				copyAction(record, GetCacheRecord(record));

				if (i == 0)
				{
					resultFirst = _translate.Translator(record, typeof(TTable), _fieldsUsable, ClauseType.SaveWhere);
					_sqlBuilder.SaveWhere(resultFirst.CommandText);
					listParameters.Add(resultFirst.Parameters);
				}
				else
				{
					resultNext = _translate.Translator(record, typeof(TTable), _fieldsUsable, ClauseType.SaveWhere, resultFirst.Parameters);
					listParameters.Add(resultNext.Parameters);
				}
			}

			TableSchema tableSchema = TableSchemaResolver.GetTableSchema(typeof(TTable));
			_sqlBuilder.From(tableSchema.PrimaryTable);

			_dbSet.BeginTransaction();

			if (listParameters.Count == 1)
			{
				this._dbError = _excuteUpdate.Excute<TTable>(_sqlBuilder.Delete(), listParameters[0], _dbSet.Transaction);
			}
			else
			{
				this._dbError = _excuteUpdate.ExcuteBatch<TTable>(_sqlBuilder.Delete(), listParameters, _dbSet.Transaction);
			}

			if (this._dbError.Code == ErrorCode.Success)
			{
				records.ForEach(rec => DeleteRecordFromCache(rec));
			}
			else
			{
				_dbSet.Cancel();
			}

			return this._dbError.Code == ErrorCode.Success;
		}

		public bool Update(TTable record)
		{
			if (IsValidRecord(record) == false)
			{
				return false;
			}

			return Update(new List<TTable>() { record });
		}

		public bool Update(Func<TTable> recordFunc)
		{
			return Update(recordFunc());
		}

		private Tuple<TranslateResult, TranslateResult> PreUpdate(TTable record, List<Parameter> parametersWhere, List<Parameter> parametersColumn)
		{
			var resultWhere = _translate.Translator(record, typeof(TTable), _fieldsUsable, ClauseType.SaveWhere, parametersWhere);
			var resultColumn = _translate.Translator(record, typeof(TTable), _fieldsUsable, ClauseType.ColumnValue, parametersColumn);

			if (parametersWhere == null && parametersColumn == null)
			{
				_sqlBuilder.SaveWhere(resultWhere.CommandText);
				_sqlBuilder.ColumnValue(resultColumn.CommandText);
			}

			return new Tuple<TranslateResult, TranslateResult>(resultWhere, resultColumn);
		}

		public bool Update(List<TTable> records)
		{
			if (records == null || records.Count == 0)
			{
				return false;
			}

			Action<TTable, TTable> copyAction = ExpressionFunc.PrimaryKeyCopy<TTable>();

			List<List<Parameter>> listParameters = new List<List<Parameter>>();

			Tuple<TranslateResult, TranslateResult> resultFirst = null;
			Tuple<TranslateResult, TranslateResult> resultNext = null;
			for (int i = 0, count = records.Count; i < count; i++)
			{
				var record = records[i];
				if (IsValidRecord(record) == false)
				{
					return false;
				}

				copyAction(record, GetCacheRecord(record));

				List<Parameter>  parameters = new List<Parameter>();
				if (i == 0)
				{
					resultFirst = PreUpdate(record, null, null);

					parameters.AddRange(resultFirst.Item1.Parameters);
					parameters.AddRange(resultFirst.Item2.Parameters);
				}
				else
				{
					resultNext = PreUpdate(record, resultFirst.Item1.Parameters, resultFirst.Item2.Parameters);

					parameters.AddRange(resultNext.Item1.Parameters);
					parameters.AddRange(resultNext.Item2.Parameters);
				}

				listParameters.Add(parameters);
			}

			_dbSet.BeginTransaction();

			if (listParameters.Count == 1)
			{
				this._dbError = _excuteUpdate.Excute<TTable>(_sqlBuilder.Update(), listParameters[0], _dbSet.Transaction);
			}
			else
			{
				this._dbError = _excuteUpdate.ExcuteBatch<TTable>(_sqlBuilder.Update(), listParameters, _dbSet.Transaction);
			}

			if (this._dbError.Code == ErrorCode.Success)
			{
				records.ForEach(rec => copyAction(GetCacheRecord(rec), rec));
			}
			else
			{
				_dbSet.Cancel();
			}

			return this._dbError.Code == ErrorCode.Success;
		}

		private bool IsValidRecord(TTable record)
		{
			if (this._recordsCache == null || this._recordsCache.Count == 0)
			{
				return false;
			}
			return this._recordsCache.Any(cache => Object.ReferenceEquals(cache.recordOut, record));
		}

		private void InsertRecordToCache(TTable record)
		{
			Func<TTable, TTable> cloneFunc = ExpressionFunc.PrimaryKeyClone<TTable>();

			this._recordsCache.Add(new RecordCache<TTable>
			{
				recordOut = record,
				recordCache = cloneFunc(record)
			});
		}

		private void DeleteRecordFromCache(TTable record)
		{
			var index = this._recordsCache.FindIndex(cache => object.ReferenceEquals(cache.recordOut, record));
			if (index != -1)
			{
				this._recordsCache.RemoveAt(index);
			}
		}

		private TTable GetCacheRecord(TTable record)
		{
			var index = this._recordsCache.FindIndex(cache => object.ReferenceEquals(cache.recordOut, record));
			if (index != -1)
			{
				return this._recordsCache[index].recordCache;
			}

			return null;
		}
	}
}
