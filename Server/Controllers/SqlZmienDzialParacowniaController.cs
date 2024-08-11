using Microsoft.AspNetCore.Mvc;
using GEORGE.Shared.Models;
using GEORGE.Shared.ViewModels;
using System.Data.SqlClient;

namespace PSSE.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SqlZmienDzialParacowniaController : Controller
    {

        [HttpGet("{RowIdRekorduPracownika}/{RowIdDzialu}")]
        public IActionResult Get(string RowIdRekorduPracownika, string RowIdDzialu)
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
                        Console.WriteLine(e.Message);
                        var results = new List<Logowania>();
                        var myModel = new Logowania
                        {
                            // Tutaj odczytujemy wartości poszczególnych kolumn z wyników zapytania
                        };
                        results.Add(myModel);
                        return new JsonResult(results);
                    }

                    // Tworzenie zapytania SQL

                    var query = "UPDATE [dbo].[Pracownicy] SET [RowIdDzialu] = '" + RowIdDzialu
                    + "' WHERE [RowId] = '" + RowIdRekorduPracownika + "';";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();

                        connection.Close();

                        // Zwracamy listę obiektów jako odpowiedź API w postaci JSON
                        var results = new List<Logowania>();

                        var myModel = new Logowania
                        {

                            Uzytkownik = "OK",
                        };
                        results.Add(myModel);

                        return new JsonResult(results);

                    }

                }
            }
            catch (InvalidCastException e)
            {
                Console.WriteLine(e.Message);

                var results = new List<Logowania>();

                var myModel = new Logowania
                {

                    Uzytkownik = e.Message,
                    //Lp = -1,
                    //Opisrejestru = "",
                    //Dotyczy = "",
                    //Datautowrzenia = DateTime.Now,
                    // ...
                };
                results.Add(myModel);

                return new JsonResult(results);
            }

        }
    }
}

