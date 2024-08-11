using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;

namespace GEORGE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //public class DataController : ControllerBase
    public class SqlTabeleNaSerwerzeController : Controller
    {

        [HttpGet]
        public IActionResult Get()
        {
            try

            {

                var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

                var connectionString = configuration.GetConnectionString("DefaultConnection");

                // Tworzenie połączenia do bazy danych
                using (var connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                    }
                    catch (Exception e)
                    {
                        var results = new List<TabeleNaSerwerzeViewModel>();
                        var myModel = new TabeleNaSerwerzeViewModel
                        {
                            // Tutaj odczytujemy wartości poszczególnych kolumn z wyników zapytania
                            // i przypisujemy je do właściwości obiektu MyModel
                            TableName = e.Message,
                            Datautowrzenia = DateTime.Now,
                        };
                        results.Add(myModel);
                        return new JsonResult(results);
                    }

                    // Tworzenie zapytania SQL
                    var query = "SELECT name FROM sys.tables where name not like '__EFMigrationsHistory' and name not like 'imieniny' order by name";
                    using (var command = new SqlCommand(query, connection))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            var results = new List<TabeleNaSerwerzeViewModel>();

                            while (reader.Read())
                            {
                                var myModel = new TabeleNaSerwerzeViewModel
                                {
                                    // Tutaj odczytujemy wartości poszczególnych kolumn z wyników zapytania
                                    // i przypisujemy je do właściwości obiektu MyModel
                                    TableName = reader.GetString(0)
                                    // ...
                                };
                                results.Add(myModel);
                            }

                            connection.Close();

                            // Zwracamy listę obiektów jako odpowiedź API w postaci JSON
                            return new JsonResult(results);

                        }
                    }
                }
            }
            catch (InvalidCastException e)
            {
                var results = new List<TabeleNaSerwerzeViewModel>();

                var myModel = new TabeleNaSerwerzeViewModel
                {

                    TableName = e.Message,
                    Datautowrzenia = DateTime.Now,
                };
                results.Add(myModel);

                return new JsonResult(results);
            }

        }
    }

}
