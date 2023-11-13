using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Galaxy.Models.FundingCacheClientDBModels
{
    
    public class CampaignSegment
    {
        public int campaignSegmentId { get; set; }
        public int campaignId { get; set; }
        public string campaignSegmentName { get; set; }
    }
}
