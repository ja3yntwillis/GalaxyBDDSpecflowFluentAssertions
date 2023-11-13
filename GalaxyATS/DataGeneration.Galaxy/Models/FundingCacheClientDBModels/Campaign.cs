using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Galaxy.Models.FundingCacheClientDBModels
{
    
    public class Campaign
    {
        
        public int campaignId { get; set; }
        public string campaignName { get; set; }
        public int partnerId { get; set; }

    }
}
