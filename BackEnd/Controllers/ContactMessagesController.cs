using BackEnd.Dtos;
using BackEnd.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BackEnd.Controllers;

[ApiController, Route("api/contact-messages")]
public sealed class ContactMessagesController(ContactMessage model) : ControllerBase
{
    [Authorize, HttpGet] public async Task<ActionResult<IReadOnlyList<ContactMessageDto>>> GetAll(CancellationToken token)=>Ok((await model.GetAllAsync(token)).Select(Map));
    [AllowAnonymous, HttpPost, EnableRateLimiting("contact-form")]
    public async Task<ActionResult<ContactMessageDto>> Create(ContactMessageDto request, CancellationToken token)
    {
        var senderName = request.SenderName.Trim();
        var senderEmail = request.SenderEmail.Trim();
        var subject = request.Subject.Trim();
        var content = request.Content.Trim();

        ValidateTrimmedValue(nameof(request.SenderName), senderName, 2, 80);
        ValidateTrimmedValue(nameof(request.Subject), subject, 3, 120);
        ValidateTrimmedValue(nameof(request.Content), content, 10, 2000);
        if (senderEmail.Length > 254 || !new EmailAddressAttribute().IsValid(senderEmail)) ModelState.AddModelError(nameof(request.SenderEmail), "L'adresse e-mail n'est pas valide.");
        if (HasUnsafeControlCharacters(senderName) || HasUnsafeControlCharacters(senderEmail) || HasUnsafeControlCharacters(subject) || HasUnsafeControlCharacters(content)) ModelState.AddModelError(nameof(request.Content), "Le message contient des caractères de contrôle non autorisés.");
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var created = await model.CreateAsync(new ContactMessage(HttpContext.RequestServices.GetRequiredService<IConfiguration>())
        {
            SenderName = senderName,
            SenderEmail = senderEmail,
            Subject = subject,
            Content = content
        }, token);
        return StatusCode(StatusCodes.Status201Created, Map(created));
    }
    [Authorize, HttpPut("{id:guid}/read")] public async Task<IActionResult> SetRead(Guid id,[FromBody] bool isRead,CancellationToken token)=>await model.SetReadAsync(id,isRead,token)?NoContent():NotFound();
    [Authorize, HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id,CancellationToken token)=>await model.DeleteAsync(id,token)?NoContent():NotFound();
    private void ValidateTrimmedValue(string field, string value, int minimum, int maximum) { if (value.Length < minimum || value.Length > maximum) ModelState.AddModelError(field, $"Le champ doit contenir entre {minimum} et {maximum} caractères après nettoyage."); }
    private static bool HasUnsafeControlCharacters(string value) => value.Any(character => char.IsControl(character) && character is not '\r' and not '\n' and not '\t');
    private static ContactMessageDto Map(ContactMessage x)=>new(){Id=x.Id,SenderName=x.SenderName,SenderEmail=x.SenderEmail,Subject=x.Subject,Content=x.Content,ReceivedAt=x.ReceivedAt,IsRead=x.IsRead};
}
