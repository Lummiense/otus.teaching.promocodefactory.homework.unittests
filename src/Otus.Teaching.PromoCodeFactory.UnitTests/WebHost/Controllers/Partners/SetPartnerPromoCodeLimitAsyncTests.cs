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
            var expectedPartner = CreateBasePartner().WithLimit();

            //Act
            await _partnersController.SetPartnerPromoCodeLimitAsync(partner.Id, request);

            //Assert
            partner.NumberIssuedPromoCodes.Should().Be(expectedPartner.NumberIssuedPromoCodes);
        }

        //Отключение предыдущего лимита при установке нового
        public async void SetPartnerPromoCodeLimit_AddLimit_SholdResetPreviousLimit()
        {

        }

    }
}