using AutoFixture.AutoMoq;
using AutoFixture;
using Moq;
using Otus.Teaching.PromoCodeFactory.Core.Abstractions.Repositories;
using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using Otus.Teaching.PromoCodeFactory.WebHost.Controllers;
using Xunit;
using Otus.Teaching.PromoCodeFactory.UnitTests.WebHost.Builders;
using System;
using Otus.Teaching.PromoCodeFactory.WebHost.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Namotion.Reflection;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.WebHost.Controllers.Partners
{
    public class SetPartnerPromoCodeLimitAsyncTests
    {
        private readonly Mock<IRepository<Partner>> _partnersRepositoryMock;
        private readonly PartnersController _partnersController;

        public SetPartnerPromoCodeLimitAsyncTests()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _partnersRepositoryMock = fixture.Freeze<Mock<IRepository<Partner>>>();
            _partnersController = fixture.Build<PartnersController>().OmitAutoProperties().Create();
        }

        private Partner CreateBasePartner()
        {
            var partner = PartnerBuilder.Build();
            return partner;
        }


        //Партнер не найден.
        [Fact]
        public async void SetPartnerPromoCodeLimit_PartnerNotFound_ReturnsNotFound()
        {
            //arrange
            Guid partnerId = Guid.NewGuid();
            var request = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
            Partner partner = null;
            _partnersRepositoryMock.Setup(x => x.GetByIdAsync(partnerId)).ReturnsAsync(partner);

            //Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            //Assert
            result.Should().BeAssignableTo<NotFoundResult>();
        }

        //Партнер заблокирован
        [Fact]
        public async void SetPartnerPromoCodeLimit_PartnerBlock_ReturnsBadRequest()
        {
            //arrange
            Guid partnerId = Guid.Parse("7d994823-8226-4273-b063-1a95f3cc1df8");
            var request = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
            var partner = CreateBasePartner();
            partner.IsActive = false;
            _partnersRepositoryMock.Setup(x => x.GetByIdAsync(partnerId)).ReturnsAsync(partner);


            //Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            //Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
        }

        //Обнуление количества выданных промокодов при выставлении лимита
        [Fact]
        public async void SetPartnerPromoCodeLimit_AddLimit_ShouldResetPromoCounter()
        {
            //Arrange
            var partner = CreateBasePartner().WithLimit();
            var request = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
            _partnersRepositoryMock.Setup(x => x.GetByIdAsync(partner.Id)).ReturnsAsync(partner);
            var expectedPartner = CreateBasePartner().WithLimit().ResetPromoCount();

            //Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            //Assert
            partner.NumberIssuedPromoCodes.Should().Be(expectedPartner.NumberIssuedPromoCodes);
        }

        //Отключение предыдущего лимита при установке нового
        [Fact]
        public async void SetPartnerPromoCodeLimit_AddLimit_ShouldResetPreviousLimit()
        {
            //Arrange
            var partner = CreateBasePartner().WithLimit();
            var firstElement = partner.PartnerLimits.First();
            var request = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
            var dateTimeToday = DateTime.Today;
            var expectedLimit = new PartnerPromoCodeLimit()
            {
                Id = Guid.Parse("0e94624b-1ff9-430e-ba8d-ef1e3b77f2d8"),
                CreateDate = new DateTime(2023, 01, 01),
                CancelDate = dateTimeToday,
                EndDate = new DateTime(2023, 09, 01),
                Limit = 5
            };
            _partnersRepositoryMock.Setup(x => x.GetByIdAsync(partner.Id)).ReturnsAsync(partner);
            

            //Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            //Assert
            firstElement.Should().BeEquivalentTo(expectedLimit);
        }

        //Лимит должен быть не отрицательным
        [Fact]
        public async void SetPartnerPromoCodeLimit_AddLimit_ShouldBeNotNegative()
        {
            // Arrange
            var partnerId = Guid.NewGuid();
            var request = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
            request.Limit = -5;

            var partner = CreateBasePartner();

            _partnersRepositoryMock.Setup(x => x.GetByIdAsync(partnerId)).ReturnsAsync(partner);

            // Act
            var result = await _partnersController.SetPartnerPromoCodeLimitAsync(partnerId, request);

            // Assert
            result.Should().BeAssignableTo<BadRequestObjectResult>();
        }

        //Новый лимит должен успешно сохраниться
        [Fact]
        public async void SetPartnerPromoCodeLimit_AddLimit_ShouldSaveSuccesful()
        {
            //Arrange
            var partner = CreateBasePartner();
            var request = new Fixture().Create<SetPartnerPromoCodeLimitRequest>();
            var mock = new Mock<IRepository<Partner>>();
            mock.Setup(x => x.GetByIdAsync(partner.Id)).ReturnsAsync(partner);
            var controller = new PartnersController(mock.Object);

            //Act
            await controller.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            //Assert
            mock.Verify(x => x.UpdateAsync(partner));
        }
    }
}