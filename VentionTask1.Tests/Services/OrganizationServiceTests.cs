using FluentValidation;
using FluentValidation.Results;
using Moq;
using VentionTask1.Application.DTOs;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Tests.Services
{
    public class OrganizationServiceTests
    {
        private readonly Mock<IOrganizationRepository> _organizationRepositoryMock;
        private readonly Mock<IValidator<CreateOrganizationDTO>> _createValidatorMock;
        private readonly Mock<IValidator<UpdateOrganizationDTO>> _updateValidatorMock;
        private readonly OrganizationService _service;

        public OrganizationServiceTests()
        {
            _organizationRepositoryMock = new Mock<IOrganizationRepository>();
            _createValidatorMock = new Mock<IValidator<CreateOrganizationDTO>>();
            _updateValidatorMock = new Mock<IValidator<UpdateOrganizationDTO>>();

            _createValidatorMock
                .Setup(validator => validator.ValidateAsync(It.IsAny<CreateOrganizationDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _updateValidatorMock
                .Setup(validator => validator.ValidateAsync(It.IsAny<UpdateOrganizationDTO>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            _service = new OrganizationService(
                _organizationRepositoryMock.Object,
                _createValidatorMock.Object,
                _updateValidatorMock.Object);
        }

        [Fact]
        public async Task GetOrganizationsPaginatedAsync_WhenRepositoryReturnsMoreThanPageSize_ShouldReturnPageAndNextCursor()
        {
            var organizations = new List<Organization>
            {
                new() { Id = Guid.NewGuid(), Name = "Org 1" },
                new() { Id = Guid.NewGuid(), Name = "Org 2" },
                new() { Id = Guid.NewGuid(), Name = "Org 3" }
            };

            _organizationRepositoryMock
                .Setup(repository => repository.GetOrganizationsPaginatedAsync(null, 2, CancellationToken.None))
                .ReturnsAsync(organizations);

            var result = await _service.GetOrganizationsPaginatedAsync(null, 2, CancellationToken.None);

            Assert.Equal(2, result.Items.Count);
            Assert.True(result.HasNextPage);
            Assert.Equal(organizations[1].Id, result.NextCursor);
        }

        [Fact]
        public async Task GetOrganizationByIdAsync_WhenOrganizationDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            var organizationId = Guid.NewGuid();

            _organizationRepositoryMock
                .Setup(repository => repository.GetOrganizationByIdAsync(organizationId, CancellationToken.None))
                .ReturnsAsync((Organization?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _service.GetOrganizationByIdAsync(organizationId, CancellationToken.None));
        }

        [Fact]
        public async Task CreateOrganizationAsync_WhenNameAlreadyExists_ShouldThrowInvalidOperationException()
        {
            var dto = new CreateOrganizationDTO { Name = "Vention" };

            _organizationRepositoryMock
                .Setup(repository => repository.GetOrganizationByNameAsync(dto.Name, CancellationToken.None))
                .ReturnsAsync(new Organization { Id = Guid.NewGuid(), Name = dto.Name });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateOrganizationAsync(dto, CancellationToken.None));
        }

        [Fact]
        public async Task CreateOrganizationAsync_WhenDataIsValid_ShouldCreateOrganization()
        {
            var dto = new CreateOrganizationDTO { Name = "New Org" };
            var createdOrganization = new Organization { Id = Guid.NewGuid(), Name = dto.Name };

            _organizationRepositoryMock
                .Setup(repository => repository.GetOrganizationByNameAsync(dto.Name, CancellationToken.None))
                .ReturnsAsync((Organization?)null);

            _organizationRepositoryMock
                .Setup(repository => repository.CreateOrganizationAsync(It.IsAny<Organization>(), CancellationToken.None))
                .ReturnsAsync(createdOrganization);

            _organizationRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            var result = await _service.CreateOrganizationAsync(dto, CancellationToken.None);

            Assert.Equal(createdOrganization.Id, result.Id);
            Assert.Equal(createdOrganization.Name, result.Name);
        }

        [Fact]
        public async Task UpdateOrganizationAsync_WhenOrganizationExists_ShouldUpdateName()
        {
            var organizationId = Guid.NewGuid();
            var organization = new Organization { Id = organizationId, Name = "Old Name" };
            var dto = new UpdateOrganizationDTO { Name = "New Name" };

            _organizationRepositoryMock
                .Setup(repository => repository.GetOrganizationByIdAsync(organizationId, CancellationToken.None))
                .ReturnsAsync(organization);

            _organizationRepositoryMock
                .Setup(repository => repository.GetOrganizationByNameAsync(dto.Name, CancellationToken.None))
                .ReturnsAsync((Organization?)null);

            _organizationRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            var result = await _service.UpdateOrganizationAsync(organizationId, dto, CancellationToken.None);

            Assert.Equal(dto.Name, result.Name);
            _organizationRepositoryMock.Verify(repository => repository.UpdateOrganizationAsync(organization), Times.Once);
        }

        [Fact]
        public async Task DeleteOrganizationAsync_WhenOrganizationExists_ShouldDeleteOrganization()
        {
            var organizationId = Guid.NewGuid();
            var organization = new Organization { Id = organizationId, Name = "Org" };

            _organizationRepositoryMock
                .Setup(repository => repository.GetOrganizationByIdAsync(organizationId, CancellationToken.None))
                .ReturnsAsync(organization);

            _organizationRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            await _service.DeleteOrganizationAsync(organizationId, CancellationToken.None);

            _organizationRepositoryMock.Verify(repository => repository.DeleteOrganizationAsync(organization), Times.Once);
        }
    }
}
