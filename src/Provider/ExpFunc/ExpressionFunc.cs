using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Provider.Schema;

namespace Provider.ExpFunc
{
	static class ExpressionFunc
	{
		// Cache store for memorizing the delegate for later use
		//ConcurrentDictionary<Type, Delegate> ExpressionCache = new ConcurrentDictionary<Type, Delegate>();

		// Method for creating the dynamic funtion for setting entity properties
		public static Func<DbDataReader, TTable> GetReader<TTable>(List<bool> fieldsUsable)
		{
			//if (ExpressionCache.TryGetValue(typeof(TTable), out Delegate resDelegate))
			//{
			//	return (Func<DbDataReader, TTable>)resDelegate;
			//}

			List<FieldSchema> fieldsSchema = TableSchemaResolver.GetTableSchema(typeof(TTable)).FieldsSchema;
			if (fieldsUsable != null && fieldsUsable.Count != fieldsSchema.Count)
			{
				return null;
			}

			// Get the indexer property of DbDataReader 
			// e.g.: dbReader["name"]
			var indexerProperty = typeof(DbDataReader).GetProperty("Item", new[] { typeof(string) });

			// Instance type of target entity class 
			// e.g.: TTable record
			ParameterExpression instanceParam = Expression.Variable(typeof(TTable));

			// Create and assign new TTable to variable. Ex. var instance = new TTable(); 
			// e.g.: record = new TTable
			BinaryExpression createInstance = Expression.Assign(instanceParam, Expression.New(typeof(TTable)));

			// Parameter for the DbDataReader object
			ParameterExpression readerParam = Expression.Parameter(typeof(DbDataReader));

			// List of statements in our dynamic method 
			var statements = new List<Expression>
			{
				createInstance
			};

			for (int i = 0, fieldCount = fieldsSchema.Count; i < fieldCount; i++)
			//foreach (var field in fieldsSchema)
			{
				var field = fieldsSchema[i];
				if (fieldsUsable != null && fieldsUsable[i] == false)
				{
					continue;
				}

				var property = field.PropertyInfo;
				string columnName = field.FieldName;
				if (string.IsNullOrEmpty(field.AliasName) == false)
				{
					columnName = field.AliasName;
				}
				var subStatements = new List<Expression>();

				// instance.Property 
				// e.g.: TTable.Property property 
				MemberExpression getProperty = Expression.Property(instanceParam, property);

				// row[property] The assumption is, column names are the 
				// same as PropertyInfo names of TTable 
				IndexExpression readValue = Expression.MakeIndex(readerParam, indexerProperty, new[] { Expression.Constant(columnName) });

				// e.g.: property = value
				var assignProperty = Expression.Assign(getProperty, Expression.Convert(readValue, property.PropertyType));

				subStatements.Add(assignProperty);

				if (property.PropertyType.Name == "String")
				{
					var call = Expression.Call(null, typeof(ExpressionFunc).GetMethod("TrimEnd", new[] { typeof(string) }), getProperty);
					subStatements.Add(Expression.Assign(getProperty, call));
				}

				// if the column name dose not exist or data is DBNull
				TryExpression tryCatchExpr =
					Expression.TryCatch(
						Expression.Block(subStatements.ToArray()),
						Expression.Catch(typeof(Exception), Expression.Default(property.PropertyType))
					);

				statements.Add(tryCatchExpr);
			}

			// e.g.: return record
			var returnStatement = instanceParam;

			statements.Add(returnStatement);

			var body = Expression.Block(instanceParam.Type, new[] { instanceParam }, statements.ToArray());
			var lambda = Expression.Lambda<Func<DbDataReader, TTable>>(body, readerParam);

			//Func<DbDataReader, TTable> resDelegate = lambda.Compile();

			// Cache the dynamic method into ExpressionCache dictionary
			//ExpressionCache[typeof(TTable)] = resDelegate;

			return lambda.Compile();
		}

		public static string TrimEnd(string text)
		{
			if (text == null)
			{
				return text;
			}
			return text.TrimEnd();
		}

		// Cache store for memorizing the delegate for later use
		static ConcurrentDictionary<Type, Delegate> PrimaryKeyCloneExpressionCache = new ConcurrentDictionary<Type, Delegate>();

		public static Func<TTable, TTable> PrimaryKeyClone<TTable>()
		{
			if (PrimaryKeyCloneExpressionCache.TryGetValue(typeof(TTable), out Delegate resDelegate))
			{
				return (Func<TTable, TTable>)resDelegate;
			}

			// Instance type of target entity class 
			// e.g.: TTable record
			ParameterExpression instanceParamLeft = Expression.Variable(typeof(TTable));

			// Create and assign new TTable to variable. Ex. var instance = new TTable(); 
			// e.g.: record = new TTable
			BinaryExpression createInstanceLeft = Expression.Assign(instanceParamLeft, Expression.New(typeof(TTable)));

			// List of statements in our dynamic method 
			var statements = new List<Expression>
			{
				createInstanceLeft
			};

			// Parameter for the TTable object
			ParameterExpression instanceParamRight = Expression.Parameter(typeof(TTable));

			List<FieldSchema> fieldsSchema = TableSchemaResolver.GetTableSchema(typeof(TTable)).FieldsSchema;
			foreach (var field in fieldsSchema)
			{
				if (!field.IsPrimaryKey && !field.IsUpdCount)
				{
					continue;
				}

				// instance.Property 
				// e.g.: TTable.Property property 
				MemberExpression getPropertyLeft = Expression.Property(instanceParamLeft, field.PropertyInfo);

				MemberExpression getPropertyRight = Expression.Property(instanceParamRight, field.PropertyInfo);

				// e.g.: property = value
				var assignProperty = Expression.Assign(getPropertyLeft, getPropertyRight);

				statements.Add(assignProperty);
			}

			// e.g.: return record
			var returnStatement = instanceParamLeft;

			statements.Add(returnStatement);

			var body = Expression.Block(instanceParamLeft.Type, new[] { instanceParamLeft }, statements.ToArray());
			var lambda = Expression.Lambda<Func<TTable, TTable>>(body, instanceParamRight);

			resDelegate = lambda.Compile();

			// Cache the dynamic method into ExpressionCache dictionary
			PrimaryKeyCloneExpressionCache[typeof(TTable)] = resDelegate;

			return (Func<TTable, TTable>)resDelegate;
		}

		// Cache store for memorizing the delegate for later use
		static ConcurrentDictionary<Type, Delegate> PrimaryKeyCopyExpressionCache = new ConcurrentDictionary<Type, Delegate>();

		public static Action<TTable, TTable> PrimaryKeyCopy<TTable>()
		{
			if (PrimaryKeyCopyExpressionCache.TryGetValue(typeof(TTable), out Delegate resDelegate))
			{
				return (Action<TTable, TTable>)resDelegate;
			}

			// Instance type of target entity class 
			// e.g.: TTable record
			ParameterExpression instanceParamLeft = Expression.Parameter(typeof(TTable));

			// Parameter for the TTable object
			ParameterExpression instanceParamRight = Expression.Parameter(typeof(TTable));

			//// Create and assign new TTable to variable. Ex. var instance = new TTable(); 
			//// e.g.: record = new TTable
			//BinaryExpression createInstanceLeft = Expression.Assign(instanceParamLeft, Expression.New(typeof(TTable)));

			// List of statements in our dynamic method 
			var statements = new List<Expression>();


			List<FieldSchema> fieldsSchema = TableSchemaResolver.GetTableSchema(typeof(TTable)).FieldsSchema;
			foreach (var field in fieldsSchema)
			{
				if (!field.IsPrimaryKey && !field.IsUpdCount)
				{
					continue;
				}

				// instance.Property 
				// e.g.: TTable.Property property 
				MemberExpression getPropertyLeft = Expression.Property(instanceParamLeft, field.PropertyInfo);

				MemberExpression getPropertyRight = Expression.Property(instanceParamRight, field.PropertyInfo);

				// e.g.: property = value
				var assignProperty = Expression.Assign(getPropertyLeft, getPropertyRight);

				statements.Add(assignProperty);
			}

			//// e.g.: return record
			//var returnStatement = instanceParamLeft;

			//statements.Add(returnStatement);

			var body = Expression.Block(statements.ToArray());
			//var body = Expression.Block(instanceParamLeft.Type, new[] { instanceParamLeft }, statements.ToArray());
			var lambda = Expression.Lambda<Action<TTable, TTable>>(body, instanceParamLeft, instanceParamRight);

			resDelegate = lambda.Compile();

			// Cache the dynamic method into ExpressionCache dictionary
			PrimaryKeyCopyExpressionCache[typeof(TTable)] = resDelegate;

			return (Action<TTable, TTable>)resDelegate;
		}

		private static Dictionary<string, Func<object, object>> getValueDelegates = new Dictionary<string, Func<object, object>>();

		public static object GetPropertyValue(object instance, Type typeInstance, PropertyInfo memberInfo)
		{
			var key = string.Format("{0}.{1}", memberInfo.DeclaringType.Name, memberInfo.Name);
			if (getValueDelegates.TryGetValue(key, out Func<object, object> getValueDelegate) == false)
			{
				var target = Expression.Parameter(typeof(object), "target");
				var getter = Expression.Lambda(typeof(Func<object, object>),
					Expression.Convert(Expression.Property(Expression.Convert(target, typeInstance), memberInfo), typeof(object)),
					target
					);

				getValueDelegate = (Func<object, object>)getter.Compile();
				getValueDelegates.Add(key, getValueDelegate);
			}

			return getValueDelegate(instance);
		}

		private static Dictionary<string, Action<object, object>> setValueDelegates = new Dictionary<string, Action<object, object>>();

		internal static void SetPropertyValue(object instance, Type typeInstance, PropertyInfo memberInfo, object newValue)
		{
			var key = string.Format("{0}.{1}", memberInfo.DeclaringType.Name, memberInfo.Name);
			if (setValueDelegates.TryGetValue(key, out Action<object, object> setValueDelegate) == false)
			{
				var target = Expression.Parameter(typeof(object));
				var propertyValue = Expression.Parameter(typeof(object));
				var castTarget = Expression.Convert(target, typeInstance);
				var castPropertyValue = Expression.Convert(propertyValue, memberInfo.PropertyType);
				var setPropertyValue = Expression.Call(castTarget, memberInfo.GetSetMethod(), castPropertyValue);
				setValueDelegate = Expression.Lambda<Action<object, object>>(setPropertyValue, target, propertyValue).Compile();

				setValueDelegates.Add(key, setValueDelegate);
			}

			setValueDelegate(instance, newValue);
		}

	}
}
