using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGeneration.Galaxy.Models.FundingCacheClientDBModels
{
    public class Partner
    {
        public int PARTNERID { get; set; }
        public string? LEGALENTITYNAME { get; set; }
        public string? ORGANIZATIONNAME { get; set; }
        public Guid PARTNERGUID { get; set; }
        public DateTime ROWCREATEDATTIMEUTC { get; set; }
        public DateTime ROWEXPIREDATTIMEUTC { get; set; }
    }
}

