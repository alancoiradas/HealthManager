namespace HealthManagerIntegrationTests.Helpers
{
    public static class AuxMethods
    {
        public static FormUrlEncodedContent ConvertClassObjectToFormUrlEncoded(object obj)
        {
            var dict = obj.GetType()
                          .GetProperties()
                          .ToDictionary(
                              prop => prop.Name,
                              prop => prop.GetValue(obj)?.ToString() ?? string.Empty
                          );

            return new FormUrlEncodedContent(dict);
        }
    }
}