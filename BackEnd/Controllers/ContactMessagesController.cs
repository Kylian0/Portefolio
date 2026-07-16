using BackEnd.Dtos;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController, Route("api/contact-messages")]
public sealed class ContactMessagesController(ContactMessage model) : ControllerBase
{
    [Authorize, HttpGet] public async Task<ActionResult<IReadOnlyList<ContactMessageDto>>> GetAll(CancellationToken token)=>Ok((await model.GetAllAsync(token)).Select(Map));
    [AllowAnonymous, HttpPost] public async Task<ActionResult<ContactMessageDto>> Create(ContactMessageDto request,CancellationToken token){var created=await model.CreateAsync(new ContactMessage(HttpContext.RequestServices.GetRequiredService<IConfiguration>()){SenderName=request.SenderName.Trim(),SenderEmail=request.SenderEmail.Trim(),Subject=request.Subject.Trim(),Content=request.Content.Trim()},token);return StatusCode(201,Map(created));}
    [Authorize, HttpPut("{id:guid}/read")] public async Task<IActionResult> SetRead(Guid id,[FromBody] bool isRead,CancellationToken token)=>await model.SetReadAsync(id,isRead,token)?NoContent():NotFound();
    [Authorize, HttpDelete("{id:guid}")] public async Task<IActionResult> Delete(Guid id,CancellationToken token)=>await model.DeleteAsync(id,token)?NoContent():NotFound();
    private static ContactMessageDto Map(ContactMessage x)=>new(){Id=x.Id,SenderName=x.SenderName,SenderEmail=x.SenderEmail,Subject=x.Subject,Content=x.Content,ReceivedAt=x.ReceivedAt,IsRead=x.IsRead};
}
