namespace SimEngineInterfaceHelpers
{
    /// <summary>
    /// A keyvalue pair that holds a request argument.
    /// </summary>
    public class RequestArgument
    {
        /// <summary>
        /// Unique case-insensitive key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The value for the key
        /// </summary>
        public string Value { get; set; }

        public RequestArgument(string key, string value)
        {
            Key = key.Trim().ToLower(); // Case insensitive
            Value = value.Trim();
        }

        /// <summary>
        /// Argument-free method needed for deserialize
        /// </summary>
        public RequestArgument()
        {

        }
    }

}
