using ISO_Server.Database;

namespace ISO_Server.Services
{
    public static class CustomerService
    {
        public static (bool success, decimal amount) GetAccountBalance(string accountNumber)
        {
            var account = Seed.Accounts.Where(a=>a.PAN==accountNumber).FirstOrDefault();
            if(account==null)
            {
                return (false, 0);
            }
            return (true,account.AccountBalance);
        }
    }
}
