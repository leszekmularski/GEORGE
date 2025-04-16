using Microsoft.AspNetCore.Mvc;
using ReservationBookingSystem.Services;

[ApiController]
[Route("api/[controller]")]
public class MailController : ControllerBase
{
    private readonly IMailService _mailService;

    public MailController(IMailService mailService)
    {
        _mailService = mailService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ToEmail) || string.IsNullOrWhiteSpace(request.Subject))
        {
            return BadRequest("Nieprawidłowe dane wejściowe.");
        }

        try
        {
            await _mailService.SendEmailAsync(request.ToEmail, request.Subject, request.HtmlBody ?? "---", request.Password ?? "---");
            return Ok("Wiadomość wysłana pomyślnie.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd serwera: {ex.Message}");
        }
    }

    [HttpPost("sendwithattachment")]
    public async Task<IActionResult> SendWithAttachment()
    {
        var form = Request.Form;

        var toEmail = form["ToEmail"].ToString();
        var subject = form["Subject"].ToString();
        var htmlBody = form["HtmlBody"].ToString();
        var password = form["Password"].ToString();
        var file = form.Files.FirstOrDefault();

        if (string.IsNullOrWhiteSpace(toEmail) || string.IsNullOrWhiteSpace(subject))
        {
            return BadRequest("Nieprawidłowe dane.");
        }

        try
        {
            using var stream = new MemoryStream();
            if (file != null)
            {
                await file.CopyToAsync(stream);
            }

            // Cofnij się na początek strumienia
            stream.Position = 0;

            await _mailService.SendEmailWithAttachmentAsync(toEmail, subject, htmlBody, password, stream, file?.FileName);

            return Ok("Wiadomość z załącznikiem wysłana.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Błąd serwera: {ex.Message}");
        }
    }

}

public class EmailRequest
{
    public string? ToEmail { get; set; }
    public string? Subject { get; set; }
    public string? Password { get; set; }
    public string? HtmlBody { get; set; }
}
