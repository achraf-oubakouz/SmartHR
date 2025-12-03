using System;

namespace SMART_HR.Models
{
    public class CompanyNewsItem
    {
        public DateTime Date { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }
}


