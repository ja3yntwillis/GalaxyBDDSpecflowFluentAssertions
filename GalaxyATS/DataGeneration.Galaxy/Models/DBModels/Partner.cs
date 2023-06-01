using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBOperations.Galaxy.DBModels
{
    public class Partner
    {
        public int PARTNERID { get; set; }
        public string PARTNERNAME { get; set; }
        public string? EMPLOYERIDENTIFICATIONNUMBER { get; set; }
        public int COMPANYTYPEID { get; set; }
        public string? LOGOIMAGEFILENAME { get; set; }
        public string? PRIMARYWEBSITEURL { get; set; }
        public string? EMAILADDRESS { get; set; }
        public Guid PARTNERGUID { get; set; }
        public DateTime LASTMODIFIEDDATE { get; set; }
        public string? PARTNERNAMEFORMAILING { get; set; }
        public int? CONTACTPHONENUMBER { get; set; }
        public bool INCLUDEINDUPLICATECHECKVIEW { get; set; }
        public string? SFDCACCOUNTID { get; set; }
    }
}

