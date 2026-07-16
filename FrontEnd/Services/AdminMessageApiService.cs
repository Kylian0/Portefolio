using System.Net.Http.Json;
using FrontEnd.Models;

namespace FrontEnd.Services;

public sealed class AdminMessageApiService(HttpClient httpClient) : IAdminMessageService
{
    public async Task<IReadOnlyList<AdminMessage>> GetAllAsync()=>await httpClient.GetFromJsonAsync<AdminMessage[]>("api/contact-messages")??[];
    public async Task SendAsync(AdminMessage message){using var response=await httpClient.PostAsJsonAsync("api/contact-messages",message);response.EnsureSuccessStatusCode();}
    public async Task SetReadStatusAsync(Guid id,bool isRead){using var response=await httpClient.PutAsJsonAsync($"api/contact-messages/{id}/read",isRead);response.EnsureSuccessStatusCode();}
    public async Task DeleteAsync(Guid id){using var response=await httpClient.DeleteAsync($"api/contact-messages/{id}");response.EnsureSuccessStatusCode();}
}
