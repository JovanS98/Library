namespace BibliotekaWeb.Core
{
    public class Enums
    {
        public enum Status
        {
            reserved,
            returned,
            added
        }

        public enum ActiveStatus
        {
            reserved,
            pendingForReserve,
            pendingForReturn,
            aboveTheLimit,
            noAvailableBooks,
            nothing
        }
    }
}
