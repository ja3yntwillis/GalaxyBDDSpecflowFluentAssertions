using System;
using Cedar.Configuration;
using static System.Net.Mime.MediaTypeNames;
using Tests.Galaxy.Utility;

namespace Tests.Galaxy.Utility
{
    
    /// <summary>
    /// Utility methods for common functionality
    /// </summary>
    public class CommonUtility
    {
       DateUtility dateUtility= new DateUtility();
        /// <summary>
        /// Converts string to First Letter in Upper Case Format.
        /// </summary>
        /// <param name="inputString">inputString</param>
        /// <returns>returns string having starting letter in capital</returns>
        public string UpperCaseFirst(string inputString)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(inputString))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return char.ToUpper(inputString[0]) + inputString.Substring(1);
        }

        /// <summary>
        /// Returns the base url
        /// </summary>
        /// <returns>Portal base url</returns>
        public string GetBaseUrl()
        {
            return TestConfiguration.BaseURL;
        }
        /// <summary>
        /// To convert a value into 2 decimal places taking from String.
        /// Note :- String value must be an Integer or Double.
        /// </summary>
        /// <param name="value">An Integer or decimal value in forms of String</param>
        /// <returns>value with 2 decimal places as String</returns>
        public string GenerateDecimalFromString(string value)
        {
            return double.Parse(value, System.Globalization.CultureInfo.InvariantCulture).ToString("0.00");
        }

        /// <summary>
        /// Set workgroup value according to run environment
        /// </summary>
        /// <returns>workgroup value for the environment</returns>
        public string GetOnlineImageUploadWorkgroup()
        {
            string workgroup = null;
            switch (TestConfiguration.BaseURL)
            {
                case "https://ent-dev.participantportal.com":
                case "https://ent-stg.participantportal.com":
                case "https://ent-uat.participantportal.com":
                case "https://pf-uat.participantportal.com":
                case "https://www.acclarisonline.com":
                    workgroup = "OLNL_IMG_UPLOAD_IEX";
                    break;
                case "https://eus-stg.viabenefitsaccounts.com":
                case "https://eus.viabenefitsaccounts.com":
                    workgroup = "OLNL_IMG_UPLOAD_IEX_ONSHORE";
                    break;
            }
            return workgroup;
        }
        /// <summary>
        /// Calculate Account Interest based on input rate and frequency for Validation with Actual Result.
        /// Logic (Refer: https://confluence.acclariscorp.com/pages/viewpage.action?pageId=5669279)
        /// For Interest Method Monthly.
        /// Current balance as on 31-Jan-2015 is 1000.
        /// Interest Method is Monthly.
        /// Interest Rate is 12% (Annually)
        /// So if calculate interest process runs, interest will be 1000*(1/100) = 10.
        /// For one month interest rate will be 1%.
        /// *******For Interest Method Annual.********
        /// Current balance as on 31-Dec-2015 is 1000.
        /// Interest Method is Monthly.
        /// Interest Rate is 12% (Annually)
        /// For one month interest rate will be 1%.
        /// So if calculate interest process runs, interest will be 1000*(1/100)*12(No. of months) = 120.*/
        /// </summary>
        /// <param name="rate">Rate as set during interest rate setup</param>
        /// <param name="frequency">Interest Accrual Frequency being used</param>
        /// <param name="balance">Account Balance</param>
        /// <returns>Returns Interest Amount</returns>
        public double CalculateInterest(double rate, string frequency, double balance)
        {
            double interestAmount = 0;
            rate = rate / 12;
            interestAmount = balance * (rate / 100);
            if (frequency == "Annual")
            {
                interestAmount = interestAmount * 12;
            }
            return interestAmount;
        }

        /// <summary>
        /// Delays the next action by specified seconds
        /// </summary>
        /// <param name="time">Number of seconds</param>
        public void DelayAction(double time = 2)
        {
            DateTime t = DateTime.Now;
            DateTime tf = DateTime.Now.AddSeconds(time);

            while (t < tf)
            {
                t = DateTime.Now;
            }
        }

        /// <summary>
        /// Method to get the number of contributions as per the contribution start date and end date
        /// </summary>
        /// <param name="startDate">start date for the contribution period</param>
        /// <param name="endDate">end date for the contribution period</param>
        /// <param name="contributionType">Type of contribution like Weekly, Monthly etc</param>
        /// <param name="secondContributionDate">In case of Twice per month the second Contribution occurrence date</param>
        /// <returns>Int value of the number of contribution occurrences</returns>
        public int CalculateTimeSpanFrequency(DateTime startDate, DateTime endDate, string contributionType, DateTime secondContributionDate = default(DateTime))
        {
            var contributionOccurrence = 0;
            var day = startDate.Day;
            DateTime startValue = startDate;
            switch (contributionType)
            {
                case "Monthly":
                    if (startValue.Month.Equals(endDate.Month))
                    {
                        contributionOccurrence = 1;
                    }
                    else
                    {
                        while (startValue <= endDate)
                        {
                            if (startValue.Day.Equals(day))
                            {
                                contributionOccurrence++;
                            }
                            startValue = startValue.AddDays(1);
                        }
                    }
                    break;
                case "Twice per month":
                    if (secondContributionDate != default(DateTime))
                    {
                        while (startValue <= endDate)
                        {
                            if (startValue.Day.Equals(day) || startValue.Day.Equals(secondContributionDate.Day))
                            {
                                contributionOccurrence++;
                            }
                            startValue = startValue.AddDays(1);
                        }
                    }
                    break;
                case "Weekly":
                    while (startValue <= endDate)
                    {
                        if (startValue.DayOfWeek.Equals(startDate.DayOfWeek))
                        {
                            contributionOccurrence++;
                        }
                        startValue = startValue.AddDays(1);
                    }
                    break;
                case "Every other week":
                    while (startValue <= endDate)
                    {
                        if (startValue.DayOfWeek.Equals(startDate.DayOfWeek))
                        {
                            contributionOccurrence++;
                        }
                        startValue = startValue.AddDays(1);
                    }
                    if (contributionOccurrence % 2 != 0)
                    {
                        contributionOccurrence++;
                    }
                    contributionOccurrence = contributionOccurrence / 2;
                    break;
            }
            return contributionOccurrence;
        }
        /// <summary>
        /// Get Last Business Day of any month for HSA Contribution.
        /// </summary>
        /// <returns>Last Business Day of any month for HSA Contribution</returns>
        /*public DateTime GetLastBusinessDayOfMonthForHSAContribution(DateTime date, string occurrenceType = null)
        {
            DateTime lastDayOfMonth;
            lastDayOfMonth = new DateTime(date.Year, date.Month, 28);
            while (dateUtility.IsHoliday(lastDayOfMonth))
            {
                lastDayOfMonth = lastDayOfMonth.AddDays(-1);
            }
            return lastDayOfMonth;
        }*/
    }
}