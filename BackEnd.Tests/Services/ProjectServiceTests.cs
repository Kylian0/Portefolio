using BackEnd.Models;
using BackEnd.Dtos;
using BackEnd.Repositories;
using BackEnd.Services;
using Xunit;

namespace BackEnd.Tests.Services;

public sealed class ProjectServiceTests
{
    [Fact]
    public async Task GetAllAsync_WhenRepositoryContainsProjects_ReturnsMappedProjects()
    {
        var source = new[]
        {
            CreateProject(1, "Premier projet", "premier-projet"),
            CreateProject(2, "Second projet", "second-projet")
        };
        var service = new ProjectService(new StubProjectRepository(source));

        var result = await service.GetAllAsync(TestContext.Current.CancellationToken);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal((uint)1, first.Id);
                Assert.Equal("Premier projet", first.Title);
                Assert.Equal("premier-projet", first.Slug);
            },
            second =>
            {
                Assert.Equal((uint)2, second.Id);
                Assert.Equal("Second projet", second.Title);
                Assert.Equal("second-projet", second.Slug);
            });
    }

    [Fact]
    public async Task GetAllAsync_WhenRepositoryIsEmpty_ReturnsEmptyList()
    {
        var service = new ProjectService(new StubProjectRepository([]));

        var result = await service.GetAllAsync(TestContext.Current.CancellationToken);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProjectExists_ReturnsMappedProject()
    {
        var service = new ProjectService(new StubProjectRepository(
            [CreateProject(7, "Projet recherché", "projet-recherche")]
        ));

        var result = await service.GetByIdAsync(7, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal((uint)7, result.Id);
        Assert.Equal("Projet recherché", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProjectDoesNotExist_ReturnsNull()
    {
        var service = new ProjectService(new StubProjectRepository([]));

        var result = await service.GetByIdAsync(42, TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_MapsInputAndReturnsCreatedProject()
    {
        var repository = new StubProjectRepository([]);
        var service = new ProjectService(repository);
        var request = new ProjectCreateDto
        {
            Title = "  Nouveau projet  ",
            Slug = "nouveau-projet",
            ShortDescription = "  Une description de projet.  ",
            Status = "draft",
            DisplayOrder = 3
        };

        var result = await service.CreateAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal((uint)1, result.Id);
        Assert.Equal("Nouveau projet", result.Title);
        Assert.Equal("Une description de projet.", result.ShortDescription);
        Assert.Equal("nouveau-projet", result.Slug);
        Assert.Equal(3, result.DisplayOrder);
    }

    [Fact]
    public async Task UpdateAsync_WhenProjectExists_MapsInputAndReturnsUpdatedProject()
    {
        var repository = new StubProjectRepository([CreateProject(4, "Ancien titre", "ancien-titre")]);
        var service = new ProjectService(repository);
        var request = new ProjectUpdateDto
        {
            Title = "  Nouveau titre  ",
            Slug = "nouveau-titre",
            ShortDescription = "  Nouvelle description.  ",
            Status = "published",
            IsFeatured = true,
            DisplayOrder = 2
        };

        var result = await service.UpdateAsync(4, request, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal((uint)4, result.Id);
        Assert.Equal("Nouveau titre", result.Title);
        Assert.Equal("nouveau-titre", result.Slug);
        Assert.Equal("Nouvelle description.", result.ShortDescription);
        Assert.True(result.IsFeatured);
    }

    [Fact]
    public async Task UpdateAsync_WhenProjectDoesNotExist_ReturnsNull()
    {
        var service = new ProjectService(new StubProjectRepository([]));
        var request = new ProjectUpdateDto
        {
            Title = "Projet absent",
            Slug = "projet-absent",
            ShortDescription = "Description"
        };

        var result = await service.UpdateAsync(99, request, TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_WhenProjectExists_ReturnsTrueAndRemovesProject()
    {
        var repository = new StubProjectRepository([CreateProject(5, "Projet à supprimer", "projet-a-supprimer")]);
        var service = new ProjectService(repository);

        var deleted = await service.DeleteAsync(5, TestContext.Current.CancellationToken);
        var project = await service.GetByIdAsync(5, TestContext.Current.CancellationToken);

        Assert.True(deleted);
        Assert.Null(project);
    }

    [Fact]
    public async Task DeleteAsync_WhenProjectDoesNotExist_ReturnsFalse()
    {
        var service = new ProjectService(new StubProjectRepository([]));

        var deleted = await service.DeleteAsync(99, TestContext.Current.CancellationToken);

        Assert.False(deleted);
    }

    private static Project CreateProject(uint id, string title, string slug) => new()
    {
        Id = id,
        Title = title,
        Slug = slug,
        ShortDescription = "Description",
        Status = "published",
        CreatedAt = new DateTime(2026, 1, 1),
        UpdatedAt = new DateTime(2026, 1, 2)
    };

    private sealed class StubProjectRepository(IReadOnlyList<Project> projects) : IProjectRepository
    {
        private readonly List<Project> items = [.. projects];

        public Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<Project>>(items);

        public Task<Project?> GetByIdAsync(uint id, CancellationToken cancellationToken = default) =>
            Task.FromResult(items.FirstOrDefault(project => project.Id == id));

        public Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
        {
            var createdProject = new Project
            {
                Id = (uint)(items.Count + 1),
                Title = project.Title,
                Slug = project.Slug,
                ShortDescription = project.ShortDescription,
                ThumbnailUrl = project.ThumbnailUrl,
                RepositoryUrl = project.RepositoryUrl,
                DemoUrl = project.DemoUrl,
                Status = project.Status,
                IsFeatured = project.IsFeatured,
                DisplayOrder = project.DisplayOrder,
                StartedAt = project.StartedAt,
                CompletedAt = project.CompletedAt,
                CreatedAt = new DateTime(2026, 7, 13),
                UpdatedAt = new DateTime(2026, 7, 13),
                PublishedAt = project.PublishedAt
            };
            items.Add(createdProject);
            return Task.FromResult(createdProject);
        }

        public Task<Project?> UpdateAsync(uint id, Project project, CancellationToken cancellationToken = default)
        {
            var index = items.FindIndex(item => item.Id == id);
            if (index < 0)
            {
                return Task.FromResult<Project?>(null);
            }

            var updatedProject = new Project
            {
                Id = id,
                Title = project.Title,
                Slug = project.Slug,
                ShortDescription = project.ShortDescription,
                ThumbnailUrl = project.ThumbnailUrl,
                RepositoryUrl = project.RepositoryUrl,
                DemoUrl = project.DemoUrl,
                Status = project.Status,
                IsFeatured = project.IsFeatured,
                DisplayOrder = project.DisplayOrder,
                StartedAt = project.StartedAt,
                CompletedAt = project.CompletedAt,
                CreatedAt = items[index].CreatedAt,
                UpdatedAt = new DateTime(2026, 7, 13),
                PublishedAt = project.PublishedAt
            };
            items[index] = updatedProject;
            return Task.FromResult<Project?>(updatedProject);
        }

        public Task<bool> DeleteAsync(uint id, CancellationToken cancellationToken = default)
        {
            var removedCount = items.RemoveAll(project => project.Id == id);
            return Task.FromResult(removedCount > 0);
        }
    }
}
