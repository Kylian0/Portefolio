using FrontEnd.Models;

namespace FrontEnd.Services;

public sealed class DemoAdminProjectService : IAdminProjectService
{
    private readonly List<AdminProject> projects =
    [
        new() { Slug = "gestionnaire-taches", Name = "Gestionnaire de tâches", Description = "Une application pour organiser ses priorités et suivre son avancement.", ImageUrl = "images/project-placeholder.svg", Type = "Web", Languages = ["C#"], Frameworks = ["Blazor", "ASP.NET Core"] },
        new() { Slug = "application-mobile", Name = "Application mobile", Description = "Un prototype mobile centré sur une navigation simple et rapide.", ImageUrl = "images/project-placeholder.svg", Type = "Mobile", Languages = ["C#"], Frameworks = [".NET MAUI"] },
        new() { Slug = "jeu-aventure", Name = "Jeu d'aventure", Description = "Un prototype de jeu explorant les interactions et la progression du joueur.", ImageUrl = "images/project-placeholder.svg", Type = "Jeu vidéo", Languages = ["C#"], Frameworks = ["Unity"] },
        new() { Slug = "moteur-3d", Name = "Expérience 3D", Description = "Une scène interactive conçue pour expérimenter le rendu en temps réel.", ImageUrl = "images/project-placeholder.svg", Type = "Jeu vidéo", Languages = ["C++"], Frameworks = ["Unreal Engine"] }
    ];

    public Task<IReadOnlyList<AdminProject>> GetAllAsync() => Task.FromResult<IReadOnlyList<AdminProject>>(projects);

    public Task UpdateAsync(AdminProject project)
    {
        var index = projects.FindIndex(item => item.Slug == project.Slug);
        if (index >= 0) projects[index] = project;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(string slug)
    {
        projects.RemoveAll(project => project.Slug == slug);
        return Task.CompletedTask;
    }
}
