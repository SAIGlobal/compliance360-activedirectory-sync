using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;

namespace Compliance360.EmployeeSync.Testing
{
    public static class SearchResultFactory
    {
        private const BindingFlags NonPublicInstance = BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

        public static SearchResult Construct<T>(T anonInstance)
        {
            var searchResult = GetUninitializedObject<SearchResult>();
            SetPropertiesFieled(searchResult);
            var dictionary = (IDictionary) searchResult.Properties;
            var type = typeof(T);
            var propertyInfos = type.GetProperties(PublicInstance);
            foreach (var propertyInfo in propertyInfos)
            {
                var value = propertyInfo.GetValue(anonInstance, null);
                var propertyCollection = GetUninitializedObject<ResultPropertyValueCollection>();
                var innerList = GetInnerList(propertyCollection);
                if (propertyInfo.PropertyType.IsArray)
                {
                    var stringArray = (string[]) value;
                    foreach (var subValue in stringArray)
                        innerList.Add(subValue);
                }
                else
                {
                    innerList.Add(value);
                }
                var lowerKey = propertyInfo.Name.ToLower(CultureInfo.InvariantCulture);
                dictionary.Add(lowerKey, propertyCollection);
            }
            return searchResult;
        }

        private static ArrayList GetInnerList(object resultPropertyCollection)
        {
            var propertyInfo = typeof(ResultPropertyValueCollection).GetProperty("InnerList", NonPublicInstance);
            return (ArrayList) propertyInfo?.GetValue(resultPropertyCollection, null);
        }

        private static void SetPropertiesFieled(SearchResult searchResult)
        {
            var propertiesFiled = typeof(SearchResult).GetField("properties", NonPublicInstance);
            propertiesFiled?.SetValue(searchResult, GetUninitializedObject<ResultPropertyCollection>());
        }

        private static T GetUninitializedObject<T>()
        {
            return (T) FormatterServices.GetUninitializedObject(typeof(T));
        }
    }
}