using System.Collections.Generic;

namespace Framework.Galaxy.Dtos
{
    /// <summary>
    /// Dto for the Debit card Pool information
    /// </summary>
    public class DebitCardPoolConfigDto
    {
        public List<DebitCardPool> DebitCardPool { get; set;}
    }

    public class DebitCardPool
    {
       public string Environment { get; set; }
        public string PlanCombination { get; set; }
        public string DebitCardNo { get; set; }
        public string DebitCardMemberId { get; set; }
        public string DebitCardCustomerId { get; set; }
        public string VendorDebitCardStatus { get; set; }
        public List<DebitCardPurseId> DebitCardPurseId { get; set; }
        public string DebitCardGroupId { get; set; }
        public string DebitCardPlanId { get; set; }
        public string EffectiveDate { get; set; }
    }
    public class DebitCardPurseId
    {
        public string ActpCd { get; set; }
        public string PurseId { get; set; }
    }
}
