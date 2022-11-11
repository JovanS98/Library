namespace BibliotekaWeb.Core
{
    public class Constants
    {
        // Konstante koje se koriste kod autorizacije, kako bismo na jednom mestu lako menjali
        // Izbacujemo magicne stringove gde mozemo
        public static class Roles
        {
            public const string Administrator = "Administrator";
            public const string User = "User";
        }

        public static class Policies
        {
            public const string RequireAdmin = "RequireAdmin";
            public const string RequireUser = "RequireUser";
        }

        public static class Limits
        {
            public const int ReservationLimit = 5;
            public const int Deadline = 30;
        }
    }
}
