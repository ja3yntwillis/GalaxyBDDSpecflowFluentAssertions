using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataGeneration.Galaxy.Models;
using FluentAssertions;

namespace DataGeneration.Galaxy.DBOperation
{
    public class GetPartnerId
    {
        public static void fetchPartnerIds(String PartnerName, string PartnerId)
        {
            int partner_Id;
            using (var dbContext = new DevEHGalaxyContext())
            {
                partner_Id = dbContext.Partners.Where(x => x.LEGALENTITYNAME.Equals(PartnerName)).First().PARTNERID;

            }
            partner_Id.Should().Be(Int32.Parse(PartnerId));
        }
    }
}
