using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Provider.Schema;
using Provider.Sql;
using Provider.ExpFunc;

namespace Provider.Translate
{
	internal class InstanceTranslator
	{
		ClauseBuilder clauseBuilder;
		ParameterBuilder parameterBuilder;

		public TranslateResult Translator(object instance, Type type, List<bool> fieldsUsable, ClauseType clauseType, List<Parameter> parameters = null)
		{
			clauseBuilder = new ClauseBuilder();
			parameterBuilder = new ParameterBuilder();

			switch (clauseType)
			{
				case ClauseType.SaveWhere:
					return TranslateWhere(instance, type, parameters);
				case ClauseType.Values:
					return TranslateValues(instance, type, parameters);
				case ClauseType.ColumnValue:
					return TranslateColumnValue(instance, type, fieldsUsable, parameters);
			}

			return null;
		}

		private TranslateResult TranslateWhere(object instance, Type type, List<Parameter> parameters)
		{
			bool isFirst = true;

			TableSchema tableSchema = TableSchemaResolver.GetTableSchema(type);
			int paramIndex = 0;
			for (int i = 0, count = tableSchema.FieldsSchema.Count; i < count; i++)
			{
				var field = tableSchema.FieldsSchema[i];
				if (field.IsPrimaryKey || field.IsUpdCount)
				{
					object value = ExpressionFunc.GetPropertyValue(instance, type, field.PropertyInfo);
					if (value == null)
					{
						throw new NotSupportedException(string.Format("The field '{0}' is not supported", field.FieldName));
					}
					if (!isFirst)
					{
						clauseBuilder.AppendClause(" And ");
					}
					clauseBuilder.AppendClause(field.FieldName);
					clauseBuilder.AppendClause("=");
					if (parameters == null)
					{
						clauseBuilder.AppendClause(parameterBuilder.GenerateParameterName(), value);
					}
					else
					{
						clauseBuilder.AppendClause(parameters[paramIndex++].Name, value);
					}
					isFirst = false;
				}
			}
			return new TranslateResult
			{
				CommandText = clauseBuilder.GetExpressionText(),    // UPDATE ...Where key_column=some_value And UpdCount=some_value
																	// DELETE ...Where key_column=some_value And UpdCount=some_value
				Parameters = clauseBuilder.GetExpressionParameters(),
			};
		}

		private TranslateResult TranslateValues(object instance, Type type, List<Parameter> parameters)
		{
			TableSchema tableSchema = TableSchemaResolver.GetTableSchema(type);
			for (int i = 0, count = tableSchema.FieldsSchema.Count; i < count; i++)
			{
				var field = tableSchema.FieldsSchema[i];
				object value = ExpressionFunc.GetPropertyValue(instance, type, field.PropertyInfo);
				if (value == null && field.PropertyInfo.PropertyType.Name == "String")
				{
					value = "";
				}

				if (parameters == null)
				{
					clauseBuilder.AppendClause(parameterBuilder.GenerateParameterName(), value);
				}
				else
				{
					clauseBuilder.AppendClause(parameters[i].Name, value);
				}
			}

			return new TranslateResult
			{
				CommandText = clauseBuilder.GetExpressionText(","), // INSERT ... VALUES(@value1,@value2,@value3,...)
				Parameters = clauseBuilder.GetExpressionParameters(),
			};
		}

		private TranslateResult TranslateColumnValue(object instance, Type type, List<bool> fieldsUsable, List<Parameter> parameters)
		{
			bool isFirst = true;

			TableSchema tableSchema = TableSchemaResolver.GetTableSchema(type);
			for (int i = 0, count = tableSchema.FieldsSchema.Count; i < count; i++)
			{
				var field = tableSchema.FieldsSchema[i];
				if (fieldsUsable[i] && field.IsPrimaryKey == false)
				{
					object value = ExpressionFunc.GetPropertyValue(instance, type, field.PropertyInfo);
					if (value == null && field.PropertyInfo.PropertyType.Name == "String")
					{
						value = "";
					}
					if (value == null)
					{
						throw new NotSupportedException(string.Format("The field '{0}' is not supported", field.FieldName));
					}
					if (!isFirst)
					{
						clauseBuilder.AppendClause(",");
					}
					clauseBuilder.AppendClause(field.FieldName);
					clauseBuilder.AppendClause("=");
					if (field.IsUpdCount)
					{
						int iValue = Convert.ToInt32(value);
						if (iValue == short.MaxValue)
						{
							iValue = 1;
						}
						else
						{
							iValue++;
						}
						value = Convert.ChangeType(iValue, field.PropertyInfo.PropertyType);
						ExpressionFunc.SetPropertyValue(instance, type, field.PropertyInfo, value);
					}
					clauseBuilder.AppendClause(parameterBuilder.GenerateParameterName(), value);
					isFirst = false;
				}
			}

			return new TranslateResult
			{
				CommandText = clauseBuilder.GetExpressionText(),    // UPDATE ... column1=@value1,column2=@value2,...
				Parameters = clauseBuilder.GetExpressionParameters(),
			};
		}
	}
}
