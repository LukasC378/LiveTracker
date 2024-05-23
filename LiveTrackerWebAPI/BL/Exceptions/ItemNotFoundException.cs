namespace BL.Exceptions
{
    /// <summary>
    /// Exception for 404 not found
    /// </summary>
    public class ItemNotFoundException : Exception
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="message"></param>
        public ItemNotFoundException(string message) : base(message) { }
    }
}
