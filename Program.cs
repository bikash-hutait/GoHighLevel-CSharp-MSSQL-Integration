using System;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


class Program
{
    static async Task Main()
    {
        string user = "databaseuser";
        string password = "pass1234";
        string server = "localhost"; // Change this to your SQL Server hostname or IP
        string database = "contactMaster";
        bool trustedConnection = false;
        int port = 1433;

        // Set your GoHighLevel API key and endpoint
        string apiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI8IkpXVCJ9.eyJsb2NhdGlvbl9pZCI6InZqbnBYckVkS0huOW85VnRQZkowIiwiY29tcGFueV9pZCI6InRPeHR2SEdjcmNEb2xhU2ozVGx3IiwidmVyc2lvbiI6MSwiaWF0IjoxNjkxNzc2MDkxOTI1LCJzdWIiOiJadGRDN0hLTDlQYkRlSml5eFFrTCJ9.SzCcTIrh78jti_7yhG86l4gBGn53ClyODM392whqkus";
        string apiEndpoint = "https://rest.gohighlevel.com/v1/contacts/"; // Adjust this based on GoHighLevel API documentation

        // Build the connection string
        string connectionString = $"Data Source={server},{port};Initial Catalog={database};User Id={user};Password={password};";
        if (!trustedConnection)
        {
            connectionString += "Integrated Security=False;";
        }

        // Create a SqlConnection object with the connection string
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            try
            {
                // Open the database connection
                connection.Open();

                // For example, you can execute a query
using (SqlCommand command = new SqlCommand("SELECT TOP 1 * FROM [contactMaster].[dbo].[tblClixloPrimary]", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Process the data retrieved from the database
                            Console.WriteLine(reader["FirstName"].ToString());

                            // Call the GoHighLevel API to add a contact
                            await AddContactToGoHighLevel(apiKey, apiEndpoint, reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    static async Task AddContactToGoHighLevel(string apiKey, string apiEndpoint, SqlDataReader reader)
    {
      // Prepare the custom fields for the GoHighLevel API request
    var customFields = new[]
    {
        new { key = "lead_id", field_value = reader["LeadID"] },
        new { key = "lead_source", field_value = reader["LeadSource"] },
        new { key = "lead_created", field_value = reader["LeadCreated"] },
        new { key = "source", field_value = reader["ContactSource"] },
        new { key = "company_name", field_value = reader["BusineeeName"] },
        new { key = "client_id", field_value = reader["ClientID"] },
        new { key = "rep_first_name", field_value = reader["RepFirstName"] },
        new { key = "rep_last_name", field_value = reader["RepLastName"] },
        new { key = "rep_phone", field_value = reader["RepPhone"] },
        new { key = "law_firm", field_value = reader["LawFirm"] },
        new { key = "debt_amount", field_value = reader["DebtAmount"] },
        new { key = "noofdebts", field_value = reader["NoOfDebts"] },
        new { key = "date_hired", field_value = reader["DateHired"] },
        new { key = "initial_draft_amount", field_value = reader["InitialDraftAmount"] },
        new { key = "regular_draft_amount", field_value = reader["RegularDraftAmount"] },
        new { key = "fixed_fee", field_value = reader["FixedFee"] },
        new { key = "total_fixed_fee", field_value = reader["TotalFixedFee"] },
        new { key = "first_deposit_date", field_value = reader["FirstDepositDate"] },
        new { key = "deposit_day", field_value = reader["DepositDay"] },
        new { key = "lead_status", field_value = reader["LeadStatus"] },
        new { key = "lead_status_reason", field_value = reader["Reason"] },
        new { key = "status_other", field_value = reader["OtherDetail"] },
        new { key = "product_code", field_value = reader["ProductID"] }
    };

    // Prepare the data for the GoHighLevel API request
    var data = new
    {
        firstName = reader["FirstName"],
        lastName = reader["LastName"],
        email = reader["Email"],
        phone = reader["PhoneNumber"],
        address1 = reader["StreetAddress"],
        city = reader["City"],
        state = reader["State"],
        postalCode = reader["PostalCode"],
        customField = customFields
    };

        // Convert the data to JSON
        string jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);
         Console.WriteLine("JSON DATA: " + jsonData);

        // Make a POST request to the GoHighLevel API
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(apiEndpoint, content);

            // Check the response status
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Contact added to GoHighLevel successfully.");
            }
            else
            {
                Console.WriteLine($"Error adding contact to GoHighLevel. Status code: {response.StatusCode}");
                // Print out the response content for further analysis
                string responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response content: {responseContent}");
            }
        }
    }
}
