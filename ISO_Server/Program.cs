// See https://aka.ms/new-console-template for more information

using BIM_ISO8583.NET;
using ISO_Server.Database;
using ISO_Server.Models;
using ISO_Server.Services;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Action = ISO_Server.Models.Action;

Console.WriteLine("Booting up the ISO Server...");
Console.WriteLine("\n\n");
InitiateServer();


///////////////////////////
async void InitiateServer()
{
    var hostName = Dns.GetHostName(); // Get the current machine's host name
    var host = Dns.GetHostEntry("localhost");
    var ipAddress = host.AddressList[1];
    var remoteHost = Dns.GetHostEntry("localhost");
    var remoteIpAddress = host.AddressList[1];
    Console.WriteLine("IP Address: {0}", ipAddress);
    var endpoint = new IPEndPoint(ipAddress, 9000);
    try
    {
        var listener = new Socket(remoteIpAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(endpoint);
        listener.Listen(10);
        while (true)
        {
            Console.WriteLine("Listening for incoming requests");
            var connection = listener.Accept();
            while (connection.Connected)
            {
                string incomingRequestData = null;
                byte[] bytes = null;

                while (true)
                {
                    bytes = new byte[1024];
                    int bytesRec = connection.Receive(bytes);
                    incomingRequestData += Encoding.ASCII.GetString(bytes, 0, bytesRec);

                    if (incomingRequestData.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }
                //var incomingRequestData = Console.ReadLine();
                Console.WriteLine("Text received : {0}", incomingRequestData);

                string response = await RequestHandler(incomingRequestData);

                byte[] msg = Encoding.ASCII.GetBytes(response);
                try
                {
                    connection.Send(msg);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                connection.Close();
            }
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
}

async Task<string> RequestHandler(string incomingRequestData)
{
    var isoRequest = Unpack(incomingRequestData);

    string[] dataElement = new string[130];

    var messageType = incomingRequestData.Substring(0, 4);
    var response = await ProcessRequest(messageType, isoRequest, dataElement);
    return response;
}

async Task<string> ProcessRequest(string messageType, string[] isoRequest, string[] dataElement)
{
    var iso8583 = new ISO8583();
    var responseCode = string.Empty;
    var isoResponse = string.Empty;
    switch (messageType)
    {
        case Action.AUTHORIZATION:
            Console.WriteLine("Executing a authorization request - Balance Enquiry");

            var customerAccountNumber = isoRequest[2];
            var amount = Convert.ToDecimal(isoRequest[4]);

            var customerBalanceResponse = CustomerService.GetAccountBalance(customerAccountNumber);
            if (!customerBalanceResponse.success)
            {
                Console.WriteLine("Request unsuccessful");
                responseCode = Responses.FAILED;
                break;
            }
            messageType = "0110";
            dataElement[4] = customerBalanceResponse.amount.ToString();
            dataElement[39] = responseCode;

            break;
        case Action.FINANCIALTRANSACTION:
            Console.WriteLine("Executing a financial request");
            responseCode = ExecuteFinancialRequest(messageType, isoRequest, dataElement);
            messageType = "0210";
            dataElement[39] = responseCode;

            break;
        case Action.REVERSED_FUNDS:
            Console.WriteLine("Not supported yet");
            messageType = "0310";
            dataElement[39] = Responses.FAILED;

            break;
        default:
            Console.WriteLine("Not supported yet");

            dataElement[39] = Responses.FAILED;

            break;
    }
    ProcessResponseDataElements(dataElement, isoRequest);
    isoResponse = iso8583.Build(dataElement, messageType);
    return isoResponse;
}

void ProcessResponseDataElements(string[] dataElement, string[] isoRequest)
{
    dataElement[2] = isoRequest[2];
    dataElement[3] = isoRequest[3];
    dataElement[4] = isoRequest[4];
    dataElement[12] = isoRequest[12];
    dataElement[18] = isoRequest[18];
    dataElement[22] = isoRequest[22];
    dataElement[25] = isoRequest[25];
    dataElement[32] = isoRequest[32];
    dataElement[37] = isoRequest[37];
    dataElement[49] = isoRequest[49];
    dataElement[128] = isoRequest[128];
}

string ExecuteFinancialRequest(string messageType, string[] isoRequest, string[] dataElement)
{
    var responseCode = string.Empty;
    Account account;
    var processingCode = isoRequest[3];
    var PAN = isoRequest[2];
    var isValidAmount = decimal.TryParse(isoRequest[4], out decimal amount);
    if (!isValidAmount)
    {
        return Responses.FAILED;
    }
    switch (processingCode.Substring(0,2))
    {
        case "00":
            account = Seed.Accounts.FirstOrDefault(a=>a.PAN==PAN);
            if (account is null)
            {
                responseCode = Responses.FAILED;
                break;
            }
            if (account.AccountBalance < amount)
            {
                responseCode = Responses.FAILED;
                break;
            }
            account.AccountBalance -= amount;
            responseCode = Responses.SUCCESSFUL;
            break;
        case "21":
            account = Seed.Accounts.FirstOrDefault(a => a.PAN == PAN);
            if (account is null)
            {
                responseCode = Responses.FAILED;
                break;
            }
            if (account.AccountBalance < amount)
            {
                responseCode = Responses.FAILED;
                break;
            }
            account.AccountBalance -= amount;
            responseCode = Responses.SUCCESSFUL;
            break;
    }
    return responseCode;
}

static string[] Unpack(string resData)
{
    ISO8583 iso8583 = new ISO8583();
    string[] DE;

    DE = iso8583.Parse(resData);

    return DE;
}