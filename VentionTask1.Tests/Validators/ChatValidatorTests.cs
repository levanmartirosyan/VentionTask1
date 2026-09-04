using VentionTask1.Application.DTOs.Chat;
using VentionTask1.Application.Validators.Chat;

namespace VentionTask1.Tests.Validators
{
    public class ChatValidatorTests
    {
        [Fact]
        public void CreateChatSessionValidator_WhenReceiverIdIsValid_ShouldBeValid()
        {
            var validator = new CreateChatSessionDTOValidator();
            var dto = new CreateChatSessionDTO
            {
                ReceiverId = Guid.NewGuid()
            };

            var result = validator.Validate(dto);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void CreateChatSessionValidator_WhenReceiverIdIsEmpty_ShouldBeInvalid()
        {
            var validator = new CreateChatSessionDTOValidator();
            var dto = new CreateChatSessionDTO
            {
                ReceiverId = Guid.Empty
            };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void SendChatMessageValidator_WhenContentIsValid_ShouldBeValid()
        {
            var validator = new SendChatMessageDTOValidator();
            var dto = new SendChatMessageDTO
            {
                Content = "Hello"
            };

            var result = validator.Validate(dto);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void SendChatMessageValidator_WhenContentIsEmpty_ShouldBeInvalid()
        {
            var validator = new SendChatMessageDTOValidator();
            var dto = new SendChatMessageDTO
            {
                Content = string.Empty
            };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void SendChatMessageValidator_WhenContentIsTooLong_ShouldBeInvalid()
        {
            var validator = new SendChatMessageDTOValidator();
            var dto = new SendChatMessageDTO
            {
                Content = new string('a', 2001)
            };

            var result = validator.Validate(dto);

            Assert.False(result.IsValid);
        }
    }
}
