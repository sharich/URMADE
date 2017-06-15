using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using URMade.Models;

namespace URMade
{    
    public class TableViewSettings
    {
        public List<SortOrderColumn> SortBy { get; set; }
        public List<TableViewColumn> VisibleColumns { get; set; }

        public TableViewSettings()
        {
            SortBy = new List<SortOrderColumn>();
            VisibleColumns = new List<TableViewColumn>();
        }
    }

    // WARNING: Modifying this class may invalidate JSON stored in the database.
    public class SortOrderColumn
    {
        public string PropertyName { get; set; }
        public bool Descending { get; set; }

        public SortOrderColumn() { }

        public SortOrderColumn(string name)
        {
            PropertyName = name;
            Descending = true;
        }

        public SortOrderColumn(string name, bool descending)
        {
            PropertyName = name;
            Descending = descending;
        }
    }

    // WARNING: Modifying this class may invalidate JSON stored in the database.
    public class TableViewColumn
    {
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }
        public string TypeName { get; set; }
        public int? Width { get; set; }

        public TableViewColumn() { }
        
        public TableViewColumn(PropertyInfo property)
        {
            PropertyName = property.Name;
            DisplayName = property.DisplayName();
            TypeName = property.PropertyType.ToString();
        }
    }

    public class TableView<TEntity>
    {
        public List<TEntity> Rows { get; set; }
        public List<TableViewColumn> Columns { get; set; }
        public List<SortOrderColumn> SortOrder { get; set; }
        public Dictionary<string, PropertyInfo> CachedProperties { get; set; }
        public Dictionary<string, DisplayFormatAttribute> FormattedDates { get; set; }
        public Dictionary<string, string> CustomStringFormats { get; set; }

        private void CacheFormattedDates()
        {
            FormattedDates = new Dictionary<string, DisplayFormatAttribute>();
            if (CachedProperties != null && CachedProperties.Count() > 0)
            {
                foreach (var prop in CachedProperties)
                {
                    if (prop.Value.GetCustomAttribute<DisplayFormatAttribute>() != null 
                        && prop.Value.PropertyType.IsAssignableFrom(typeof(DateTime)))
                    {
                        FormattedDates.Add(prop.Key, prop.Value.GetCustomAttribute<DisplayFormatAttribute>());
                    }
                }
            }
        }

        public TableView()
        {
            CustomStringFormats = new Dictionary<string, string>();
            var properties = (typeof(TEntity)).OwnProperties().ForTableView().ToList();
            Columns = properties.Select(p => new TableViewColumn(p)).ToList();
            CachedProperties = new Dictionary<string, PropertyInfo>();
            properties.ForEach(p => CachedProperties.Add(p.Name, p));
            CacheFormattedDates();
        }

        public TableView(IEnumerable<TEntity> rows) : this()
        {
            Rows = rows.ToList();
        }
        
        public TableView(IEnumerable<TEntity> rows, IEnumerable<string> columns)
        {
            CustomStringFormats = new Dictionary<string, string>();
            Rows = rows.ToList();

            Columns = new List<TableViewColumn>();
            CachedProperties = new Dictionary<string, PropertyInfo>();

            (typeof(TEntity)).OwnProperties().ForTableView()
                .Where(p => columns.Contains(p.Name)).ToList()
                .ForEach(p =>
                {
                    CachedProperties.Add(p.Name, p);
                    Columns.Add(new TableViewColumn(p));
                });

            CacheFormattedDates();
        }

        public TableView(IEnumerable<TEntity> rows, TableViewSettings settings)
        {
            CustomStringFormats = new Dictionary<string, string>();
            CachedProperties = new Dictionary<string, PropertyInfo>();
            Columns = new List<TableViewColumn>();

            Rows = rows.ToList();

            if (settings.VisibleColumns == null || settings.VisibleColumns.Count() == 0)
            {
                typeof(TEntity).OwnProperties().ForTableView().ToList()
                    .ForEach(p =>
                    {
                        CachedProperties.Add(p.Name, p);
                        Columns.Add(new TableViewColumn(p));
                    });
            }
            else
            {
                List<string> columnNames = settings.VisibleColumns.Select(p => p.PropertyName).ToList();
                List<PropertyInfo> properties = (typeof(TEntity)).OwnProperties().ForTableView().ToList();

                settings.VisibleColumns.ForEach(column =>
                {
                    PropertyInfo prop = properties.FirstOrDefault(p => p.Name == column.PropertyName);
                    if (prop != null)
                    {
                        CachedProperties.Add(prop.Name, prop);
                        if (String.IsNullOrWhiteSpace(column.DisplayName)) column.DisplayName = prop.DisplayName();
                        Columns.Add(column);
                    }
                });
            }

            CacheFormattedDates();

            SortOrder = new List<SortOrderColumn>();

            if (settings.SortBy != null && settings.SortBy.Count() > 0)
            {
                SortOrder = settings.SortBy;

                IOrderedEnumerable<TEntity> sortedResult = null;

                var first = true;

                foreach (var column in SortOrder)
                {
                    if (first)
                    {
                        if (column.Descending)
                        {
                            sortedResult = Rows.OrderByDescending(p => GetColumnValue(p, column.PropertyName));
                        }
                        else
                        {
                            sortedResult = Rows.OrderBy(p => GetColumnValue(p, column.PropertyName));
                        }

                        first = false;
                    }
                    else
                    {
                        if (column.Descending)
                        {
                            sortedResult = sortedResult.ThenByDescending(p => GetColumnValue(p, column.PropertyName));
                        }
                        else
                        {
                            sortedResult = sortedResult.ThenBy(p => GetColumnValue(p, column.PropertyName));
                        }
                    }
                }

                Rows = sortedResult.ToList();
            }

        }

        public string GetColumnValue(TEntity row, string column)
        {
			if (column == null)
				return "";

            if (CachedProperties.ContainsKey(column) == false)
            { 
                CachedProperties[column] = typeof(TEntity).GetProperty(column);
            }
            PropertyInfo property = CachedProperties[column];
            if (property == null) return "";

            var value = property.GetValue(row);

            if (value != null)
            {
                if (CustomStringFormats.ContainsKey(column))
                {
                    if (value.ToString() == "") return "";
                    return String.Format(CustomStringFormats[column], value);
                }
                else if (FormattedDates.ContainsKey(column))
                {
                    DateTime date = (DateTime)value;
                    return String.Format(FormattedDates[column].DataFormatString, date);
                }
                else
                {
                    string result = value.ToString();
                    if (result == "True") return "Yes";
                    if (result == "False") return "";
                    return result;
                }
            }
            else
            {
                return "";
            }
        }

        public string SortOrderAsJson()
        {
            return JsonConvert.SerializeObject(SortOrder.Select(p => new { column = p.PropertyName, desc = p.Descending }));
        }
    }

    public class ExcludeFromTableViewAttribute : Attribute
    {
    }

    public static class PropertyExtensionMethods
    {
        public static IEnumerable<PropertyInfo> ForTableView (this IEnumerable<PropertyInfo> properties)
        {
            return properties.Where(p => p.GetCustomAttribute<ExcludeFromTableViewAttribute>() == null);
        }

        public static List<PropertyInfo> OwnProperties (this Type type)
        {
            return type.GetProperties().ToList();
               // .Where(p => p.DeclaringType == typeof(Type)).ToList();
        }

        public static string DisplayName(this PropertyInfo property)
        {
            string name = property.Name;

            var displayAttr = property.GetCustomAttributes(typeof(DisplayAttribute), false);
            if (displayAttr != null && displayAttr.Count() > 0)
            {
                name = displayAttr.Cast<DisplayAttribute>().Single().Name;
            }

            return name;
        }

        public static string StringValueFrom(this PropertyInfo property, object instance)
        {
            object prop = property.GetValue(instance);
            string fieldValue = "";

            if (prop != null)
            {
                fieldValue = prop.ToString();
            }

            return fieldValue;
        }

        public static List<string> PropertyNames(this Type type)
        {
            return type.OwnProperties().Select(p => p.Name).ToList();
        }
    }
}
