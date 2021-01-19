using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TableAttribute = Dapper.Contrib.Extensions.TableAttribute;

namespace SFA.DAS.EmployerIncentives.Api.AcceptanceTests.Extensions
{
    public static class DapperExtensions
    {
        public static Task<int> InsertWithEnumAsStringAsync<T>(this SqlConnection sqlConnection, T obj)
        {
            var propertyValuesMap = new Dictionary<string, object>();

            var columns = new StringBuilder();
            var values = new StringBuilder();
            var tableName = ((TableAttribute)obj.GetType().GetCustomAttribute(typeof(TableAttribute)))?.Name;
            var relevantProperties = obj.GetType().GetProperties().Where(x =>
                !Attribute.IsDefined(x, typeof(ComputedAttribute)) && HasNoWriteFalseAttribute(x)).ToList();

            for (var i = 0; i < relevantProperties.Count; i++)
            {
                object val;
                var propertyInfo = relevantProperties[i];

                if (propertyInfo.PropertyType.IsEnum)
                {
                    var isStringEnum = propertyInfo.GetCustomAttributes(typeof(ColumnAttribute), false).FirstOrDefault() is ColumnAttribute columnAttribute
                                       && columnAttribute.TypeName.Contains("varchar");

                    val = isStringEnum ? Enum.GetName(propertyInfo.PropertyType, propertyInfo.GetValue(obj) ?? "VALUE") : propertyInfo.GetValue(obj);
                }
                else
                {
                    val = propertyInfo.GetValue(obj);
                }

                propertyValuesMap.Add(propertyInfo.Name, val);
                var propName = i == relevantProperties.Count - 1 ? $"{propertyInfo.Name}" : $"{propertyInfo.Name},";
                columns.Append(propName);
                values.Append($"@{propName}");
            }

            return sqlConnection.ExecuteAsync($"Insert Into {tableName} ({columns}) values ({values})", propertyValuesMap);
        }

        private static bool HasNoWriteFalseAttribute(ICustomAttributeProvider propertyInfo)
        {
            var writeAttribute = propertyInfo.GetCustomAttributes(typeof(WriteAttribute), false).FirstOrDefault() as WriteAttribute;
            return writeAttribute == null || writeAttribute.Write;
        }
    }
}
