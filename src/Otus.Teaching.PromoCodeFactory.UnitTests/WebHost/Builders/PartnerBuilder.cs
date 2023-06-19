using Otus.Teaching.PromoCodeFactory.Core.Domain.PromoCodeManagement;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Otus.Teaching.PromoCodeFactory.UnitTests.WebHost.Builders
{
    public static class PartnerBuilder
    {
        public static Partner Build()
        {
            return new Partner()
            {
                Id = Guid.Parse("7d994823-8226-4273-b063-1a95f3cc1df8"),
                Name = "Суперигрушки",
                IsActive = true,
                PartnerLimits = new List<PartnerPromoCodeLimit>()
                {
                    new PartnerPromoCodeLimit()
                    {
                        Id = Guid.Parse("e00633a5-978a-420e-a7d6-3e1dab116393"),
                        CreateDate = new DateTime(2020,07,9),
                        EndDate = new DateTime(2020,10,9),
                        Limit = 100
                    }
                }
            };
        }

        public static Partner WithLimit(this Partner partner)
        {
           
            partner.PartnerLimits = new List<PartnerPromoCodeLimit>();
            partner.PartnerLimits.Add(new PartnerPromoCodeLimit()
            {
                Id = Guid.NewGuid(),
                CreateDate = new DateTime(2023, 01, 01),
                EndDate = new DateTime(2023,09,01),
                Limit = 5
            }) ;
            return partner;
        }
        public static Partner ResetPromoCount(this Partner partner)
        {
            partner.NumberIssuedPromoCodes = 0;
            return partner;
        }
    }
}
