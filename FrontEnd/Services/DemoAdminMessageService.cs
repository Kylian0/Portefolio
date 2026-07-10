using FrontEnd.Models;

namespace FrontEnd.Services;

public sealed class DemoAdminMessageService : IAdminMessageService
{
    private readonly List<AdminMessage> messages =
    [
        new() { Id = Guid.NewGuid(), SenderName = "Camille Bernard", SenderEmail = "camille.bernard@example.com", Subject = "Opportunité de développeur junior", Content = "Bonjour Kylian, nous avons découvert votre portfolio et souhaiterions échanger avec vous au sujet d'une opportunité de développeur junior au sein de notre équipe. Seriez-vous disponible pour un premier entretien la semaine prochaine ?", ReceivedAt = DateTime.Today.AddHours(9).AddMinutes(35), IsRead = false },
        new() { Id = Guid.NewGuid(), SenderName = "Thomas Leroy", SenderEmail = "thomas.leroy@example.com", Subject = "Question sur votre projet Blazor", Content = "Bonjour, j'ai consulté la présentation de votre projet réalisé avec Blazor. Je travaille actuellement sur une application similaire et serais intéressé pour discuter de vos choix techniques.", ReceivedAt = DateTime.Today.AddDays(-1).AddHours(16), IsRead = false },
        new() { Id = Guid.NewGuid(), SenderName = "Léa Martin", SenderEmail = "lea.martin@example.com", Subject = "Échange autour du développement de jeux", Content = "Salut Kylian, je suis également passionnée par le développement de jeux vidéo. Ton parcours m'intéresse et je serais ravie d'échanger sur Unity et la création de prototypes.", ReceivedAt = DateTime.Today.AddDays(-3).AddHours(11), IsRead = true },
        new() { Id = Guid.NewGuid(), SenderName = "Nicolas Petit", SenderEmail = "nicolas.petit@example.com", Subject = "Retour sur votre portfolio", Content = "Bonjour, je souhaitais simplement vous féliciter pour votre portfolio. La navigation est claire et les projets sont agréables à découvrir. Bonne continuation dans votre parcours !", ReceivedAt = DateTime.Today.AddDays(-12).AddHours(14), IsRead = true }
    ];

    public Task<IReadOnlyList<AdminMessage>> GetAllAsync() => Task.FromResult<IReadOnlyList<AdminMessage>>(messages);

    public Task SetReadStatusAsync(Guid id, bool isRead)
    {
        var message = messages.FirstOrDefault(item => item.Id == id);
        if (message is not null) message.IsRead = isRead;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id)
    {
        messages.RemoveAll(message => message.Id == id);
        return Task.CompletedTask;
    }
}
