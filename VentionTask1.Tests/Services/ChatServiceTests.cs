using FluentValidation;
using Moq;
using VentionTask1.Application.DTOs.Chat;
using VentionTask1.Application.Exceptions;
using VentionTask1.Application.Repositories.Interfaces;
using VentionTask1.Application.Services.Implementation;
using VentionTask1.Application.Services.Interfaces;
using VentionTask1.Application.Validators.Chat;
using VentionTask1.Domain.Constants;
using VentionTask1.Domain.Entities;

namespace VentionTask1.Tests.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IChatRepository> _chatRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IOrganizationMemberRepository> _memberRepositoryMock;
        private readonly Mock<IOrganizationPermissionService> _permissionServiceMock;
        private readonly ChatService _service;

        public ChatServiceTests()
        {
            _chatRepositoryMock = new Mock<IChatRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _memberRepositoryMock = new Mock<IOrganizationMemberRepository>();
            _permissionServiceMock = new Mock<IOrganizationPermissionService>();

            _service = new ChatService(
                _chatRepositoryMock.Object,
                _userRepositoryMock.Object,
                _memberRepositoryMock.Object,
                _permissionServiceMock.Object,
                new CreateChatSessionDTOValidator(),
                new SendChatMessageDTOValidator());
        }

        [Fact]
        public async Task GetChatsAsync_WhenMoreItemsThanPageSize_ShouldReturnPageAndNextCursor()
        {
            var organizationId = Guid.NewGuid();
            var currentUser = CreateUser("Current User", "current@example.com");
            var firstReceiver = CreateUser("First Receiver", "first@example.com");
            var secondReceiver = CreateUser("Second Receiver", "second@example.com");
            var thirdReceiver = CreateUser("Third Receiver", "third@example.com");
            var firstChat = CreateChat(organizationId, currentUser, firstReceiver);
            var secondChat = CreateChat(organizationId, currentUser, secondReceiver);
            var extraChat = CreateChat(organizationId, currentUser, thirdReceiver);

            _permissionServiceMock
                .Setup(service => service.EnsureCanAccessOrganizationAsync(currentUser.Id, RoleType.MEMBER, organizationId, CancellationToken.None))
                .Returns(Task.CompletedTask);

            _chatRepositoryMock
                .Setup(repository => repository.GetUserChatsAsync(currentUser.Id, organizationId, null, 2, CancellationToken.None))
                .ReturnsAsync([firstChat, secondChat, extraChat]);

            var result = await _service.GetChatsAsync(currentUser.Id, organizationId, null, 2, CancellationToken.None);

            Assert.True(result.HasNextPage);
            Assert.Equal(secondChat.Id, result.NextCursor);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(firstReceiver.Id, result.Items[0].Participant.Id);
            Assert.Equal(secondReceiver.Id, result.Items[1].Participant.Id);
        }

        [Fact]
        public async Task CreateChatAsync_WhenChatAlreadyExists_ShouldReturnExistingChat()
        {
            var organizationId = Guid.NewGuid();
            var currentUser = CreateUser("Current User", "current@example.com");
            var receiver = CreateUser("Receiver", "receiver@example.com");
            var dto = new CreateChatSessionDTO
            {
                ReceiverId = receiver.Id
            };
            var participantIds = SortParticipants(currentUser, receiver);
            var existingChat = CreateChat(organizationId, participantIds.ParticipantOne, participantIds.ParticipantTwo);

            SetupOrganizationMember(organizationId, currentUser.Id);
            SetupOrganizationMember(organizationId, receiver.Id);

            _userRepositoryMock
                .Setup(repository => repository.GetUserByIdAsync(receiver.Id, CancellationToken.None))
                .ReturnsAsync(receiver);

            _chatRepositoryMock
                .Setup(repository => repository.GetByUsersAsync(
                    organizationId,
                    participantIds.ParticipantOne.Id,
                    participantIds.ParticipantTwo.Id,
                    CancellationToken.None))
                .ReturnsAsync(existingChat);

            var result = await _service.CreateChatAsync(currentUser.Id, organizationId, dto, CancellationToken.None);

            Assert.Equal(existingChat.Id, result.Id);
            Assert.Equal(receiver.Id, result.Participant.Id);

            _chatRepositoryMock.Verify(
                repository => repository.AddChatAsync(It.IsAny<ChatSession>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateChatAsync_WhenUsersAreOrganizationMembers_ShouldCreateChat()
        {
            var organizationId = Guid.NewGuid();
            var currentUser = CreateUser("Current User", "current@example.com");
            var receiver = CreateUser("Receiver", "receiver@example.com");
            var dto = new CreateChatSessionDTO
            {
                ReceiverId = receiver.Id
            };
            var participantIds = SortParticipants(currentUser, receiver);
            ChatSession? createdChat = null;

            SetupOrganizationMember(organizationId, currentUser.Id);
            SetupOrganizationMember(organizationId, receiver.Id);

            _userRepositoryMock
                .Setup(repository => repository.GetUserByIdAsync(receiver.Id, CancellationToken.None))
                .ReturnsAsync(receiver);

            _chatRepositoryMock
                .Setup(repository => repository.GetByUsersAsync(
                    organizationId,
                    participantIds.ParticipantOne.Id,
                    participantIds.ParticipantTwo.Id,
                    CancellationToken.None))
                .ReturnsAsync((ChatSession?)null);

            _chatRepositoryMock
                .Setup(repository => repository.AddChatAsync(It.IsAny<ChatSession>(), CancellationToken.None))
                .Callback<ChatSession, CancellationToken>((chat, _) =>
                {
                    chat.Id = Guid.NewGuid();
                    chat.ParticipantOne = participantIds.ParticipantOne;
                    chat.ParticipantTwo = participantIds.ParticipantTwo;
                    createdChat = chat;
                })
                .Returns(Task.CompletedTask);

            _chatRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            _chatRepositoryMock
                .Setup(repository => repository.GetByIdAsync(It.IsAny<Guid>(), CancellationToken.None))
                .ReturnsAsync(() => createdChat);

            var result = await _service.CreateChatAsync(currentUser.Id, organizationId, dto, CancellationToken.None);

            Assert.NotNull(createdChat);
            Assert.Equal(organizationId, createdChat.OrganizationId);
            Assert.Equal(participantIds.ParticipantOne.Id, createdChat.ParticipantOneId);
            Assert.Equal(participantIds.ParticipantTwo.Id, createdChat.ParticipantTwoId);
            Assert.Equal(receiver.Id, result.Participant.Id);
        }

        [Fact]
        public async Task CreateChatAsync_WhenCurrentUserIsNotOrganizationMember_ShouldThrowForbiddenAccessException()
        {
            var organizationId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            var receiverId = Guid.NewGuid();
            var dto = new CreateChatSessionDTO
            {
                ReceiverId = receiverId
            };

            _memberRepositoryMock
                .Setup(repository => repository.GetByOrganizationAndUserAsync(organizationId, currentUserId, CancellationToken.None))
                .ReturnsAsync((OrganizationMember?)null);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                _service.CreateChatAsync(currentUserId, organizationId, dto, CancellationToken.None));

            _chatRepositoryMock.Verify(
                repository => repository.AddChatAsync(It.IsAny<ChatSession>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task CreateChatAsync_WhenReceiverIsCurrentUser_ShouldThrowInvalidOperationException()
        {
            var organizationId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid();
            var dto = new CreateChatSessionDTO
            {
                ReceiverId = currentUserId
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateChatAsync(currentUserId, organizationId, dto, CancellationToken.None));

            _memberRepositoryMock.Verify(
                repository => repository.GetByOrganizationAndUserAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetMessagesAsync_WhenMoreItemsThanPageSize_ShouldReturnMessagesAndNextCursor()
        {
            var organizationId = Guid.NewGuid();
            var currentUser = CreateUser("Current User", "current@example.com");
            var receiver = CreateUser("Receiver", "receiver@example.com");
            var chat = CreateChat(organizationId, currentUser, receiver);
            var newestMessage = CreateMessage(chat.Id, receiver, "Newest", DateTime.UtcNow);
            var olderMessage = CreateMessage(chat.Id, currentUser, "Older", DateTime.UtcNow.AddMinutes(-1));
            var extraMessage = CreateMessage(chat.Id, receiver, "Extra", DateTime.UtcNow.AddMinutes(-2));

            _chatRepositoryMock
                .Setup(repository => repository.GetByIdAsync(chat.Id, CancellationToken.None))
                .ReturnsAsync(chat);

            SetupOrganizationMember(organizationId, currentUser.Id);

            _chatRepositoryMock
                .Setup(repository => repository.GetMessagesAsync(chat.Id, null, 2, CancellationToken.None))
                .ReturnsAsync([newestMessage, olderMessage, extraMessage]);

            var result = await _service.GetMessagesAsync(currentUser.Id, chat.Id, null, 2, CancellationToken.None);

            Assert.True(result.HasNextPage);
            Assert.Equal(olderMessage.Id, result.NextCursor);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(olderMessage.Id, result.Items[0].Id);
            Assert.True(result.Items[0].IsOwn);
            Assert.Equal(newestMessage.Id, result.Items[1].Id);
            Assert.False(result.Items[1].IsOwn);
        }

        [Fact]
        public async Task GetMessagesAsync_WhenUserIsNotChatParticipant_ShouldThrowForbiddenAccessException()
        {
            var organizationId = Guid.NewGuid();
            var firstUser = CreateUser("First User", "first@example.com");
            var secondUser = CreateUser("Second User", "second@example.com");
            var otherUserId = Guid.NewGuid();
            var chat = CreateChat(organizationId, firstUser, secondUser);

            _chatRepositoryMock
                .Setup(repository => repository.GetByIdAsync(chat.Id, CancellationToken.None))
                .ReturnsAsync(chat);

            await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
                _service.GetMessagesAsync(otherUserId, chat.Id, null, 10, CancellationToken.None));

            _chatRepositoryMock.Verify(
                repository => repository.GetMessagesAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task SendMessageAsync_WhenUserCanAccessChat_ShouldCreateMessageAndUpdateLastMessage()
        {
            var organizationId = Guid.NewGuid();
            var currentUser = CreateUser("Current User", "current@example.com");
            var receiver = CreateUser("Receiver", "receiver@example.com");
            var chat = CreateChat(organizationId, currentUser, receiver);
            var dto = new SendChatMessageDTO
            {
                Content = "  Hello chat  "
            };
            ChatMessage? createdMessage = null;

            _chatRepositoryMock
                .Setup(repository => repository.GetByIdAsync(chat.Id, CancellationToken.None))
                .ReturnsAsync(chat);

            SetupOrganizationMember(organizationId, currentUser.Id);

            _chatRepositoryMock
                .Setup(repository => repository.AddMessageAsync(It.IsAny<ChatMessage>(), CancellationToken.None))
                .Callback<ChatMessage, CancellationToken>((message, _) =>
                {
                    message.Id = Guid.NewGuid();
                    message.CreatedAt = DateTime.UtcNow;
                    createdMessage = message;
                })
                .Returns(Task.CompletedTask);

            _chatRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            var result = await _service.SendMessageAsync(currentUser.Id, chat.Id, dto, CancellationToken.None);

            Assert.NotNull(createdMessage);
            Assert.Equal(chat.Id, createdMessage.ChatSessionId);
            Assert.Equal(currentUser.Id, createdMessage.SenderId);
            Assert.Equal("Hello chat", createdMessage.Content);
            Assert.Equal("Hello chat", chat.LastMessage);
            Assert.NotNull(chat.LastMessageAt);
            Assert.Equal(currentUser.Id, result.SenderId);
            Assert.Equal(currentUser.Name, result.SenderName);
            Assert.True(result.IsOwn);
        }

        [Fact]
        public async Task SendMessageAsync_WhenContentIsLongerThanLastMessageLimit_ShouldTrimLastMessageToFiveHundredCharacters()
        {
            var organizationId = Guid.NewGuid();
            var currentUser = CreateUser("Current User", "current@example.com");
            var receiver = CreateUser("Receiver", "receiver@example.com");
            var chat = CreateChat(organizationId, currentUser, receiver);
            var dto = new SendChatMessageDTO
            {
                Content = new string('a', 600)
            };

            _chatRepositoryMock
                .Setup(repository => repository.GetByIdAsync(chat.Id, CancellationToken.None))
                .ReturnsAsync(chat);

            SetupOrganizationMember(organizationId, currentUser.Id);

            _chatRepositoryMock
                .Setup(repository => repository.AddMessageAsync(It.IsAny<ChatMessage>(), CancellationToken.None))
                .Returns(Task.CompletedTask);

            _chatRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(true);

            await _service.SendMessageAsync(currentUser.Id, chat.Id, dto, CancellationToken.None);

            Assert.Equal(500, chat.LastMessage.Length);
        }

        [Fact]
        public async Task SendMessageAsync_WhenMessageCannotBeSaved_ShouldThrowInvalidOperationException()
        {
            var organizationId = Guid.NewGuid();
            var currentUser = CreateUser("Current User", "current@example.com");
            var receiver = CreateUser("Receiver", "receiver@example.com");
            var chat = CreateChat(organizationId, currentUser, receiver);
            var dto = new SendChatMessageDTO
            {
                Content = "Hello"
            };

            _chatRepositoryMock
                .Setup(repository => repository.GetByIdAsync(chat.Id, CancellationToken.None))
                .ReturnsAsync(chat);

            SetupOrganizationMember(organizationId, currentUser.Id);

            _chatRepositoryMock
                .Setup(repository => repository.AddMessageAsync(It.IsAny<ChatMessage>(), CancellationToken.None))
                .Returns(Task.CompletedTask);

            _chatRepositoryMock
                .Setup(repository => repository.SaveChangesAsync(CancellationToken.None))
                .ReturnsAsync(false);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.SendMessageAsync(currentUser.Id, chat.Id, dto, CancellationToken.None));
        }

        private void SetupOrganizationMember(Guid organizationId, Guid userId)
        {
            _memberRepositoryMock
                .Setup(repository => repository.GetByOrganizationAndUserAsync(organizationId, userId, CancellationToken.None))
                .ReturnsAsync(new OrganizationMember
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    UserId = userId,
                    Role = RoleType.MEMBER
                });
        }

        private static User CreateUser(string name, string email)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                PasswordHash = "hash",
                Role = RoleType.MEMBER
            };
        }

        private static ChatSession CreateChat(Guid organizationId, User participantOne, User participantTwo)
        {
            return new ChatSession
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ParticipantOneId = participantOne.Id,
                ParticipantOne = participantOne,
                ParticipantTwoId = participantTwo.Id,
                ParticipantTwo = participantTwo,
                LastMessage = "Last message",
                LastMessageAt = DateTime.UtcNow
            };
        }

        private static ChatMessage CreateMessage(Guid chatId, User sender, string content, DateTime createdAt)
        {
            return new ChatMessage
            {
                Id = Guid.NewGuid(),
                ChatSessionId = chatId,
                SenderId = sender.Id,
                Sender = sender,
                Content = content,
                CreatedAt = createdAt
            };
        }

        private static (User ParticipantOne, User ParticipantTwo) SortParticipants(User firstUser, User secondUser)
        {
            return firstUser.Id.CompareTo(secondUser.Id) < 0
                ? (firstUser, secondUser)
                : (secondUser, firstUser);
        }
    }
}
