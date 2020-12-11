
using System.ComponentModel.DataAnnotations;

namespace ProductividadApiBack.Models
{
    public class ProponentModel
    {
        public int Id { get; set; }
        public string ContactName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string ProponentType { get; set; }
        public string BusinessName { get; set; }
        public string NIT { get; set; }
        public string WebSite { get; set; }
        public string TechnologyName { get; set; }
        public string TechnologyObject { get; set; }
        public string Functionality { get; set; }
        public string ValuePromise { get; set; }
        public string ApplicationSegment { get; set; }
        public string SpecificApplicationArea { get; set; }
        public string MaturityLevel { get; set; }
        public string ProposalState { get; set; }
        public string ProposalIdCode { get; set; }
        public string SIPROEID { get; set; }
        public string SIPROEOption { get; set; }
    }
}
