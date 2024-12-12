using ISO_Server.Models;

namespace ISO_Server.Database
{
    public static class Seed
    {
        public static List<Account> Accounts { get; set; } = new List<Account> 
        { 
            new Account
            {
                FullName = "Micheal Oje",
                AccountBalance = 2000000000,
                PAN = "8999388994"
            },
            new Account
            {
                FullName = "Huje Oue",
                AccountBalance = 20440000,
                PAN = "0299388994"
            },
            new Account
            {
                FullName = "George Ken",
                AccountBalance = 770000000,
                PAN = "3497888994"
            },
            new Account
            {
                FullName = "Jenneth Lens",
                AccountBalance = 600,
                PAN = "5699388994"
            },
            new Account
            {
                FullName = "Henry Jen",
                AccountBalance = 8504780,
                PAN = "2439388994"
            },
            new Account
            {
                FullName = "Paul Ventry",
                AccountBalance = 83452440,
                PAN = "0299388994"
            },
            new Account
            {
                FullName = "Ben Moses",
                AccountBalance = 840000,
                PAN = "2899388994"
            },
            new Account
            {
                FullName = "Lance Paul",
                AccountBalance = 7600778000,
                PAN = "2039388994"
            }
        };
    }
}
