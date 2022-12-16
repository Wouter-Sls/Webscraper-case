using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webscraper_case
{
    internal class JobsiteVacancy
    {

        public string Title { get; set; }
        public string Company { get; set; }
        public string Location { get; set; }
        public string Keywords { get; set; }
        public string Url { get; set; }

        public JobsiteVacancy(string title, string company, string location, string keywords, string url)
        {
            this.Location = location;
            this.Keywords = keywords;
            this.Url = url;
            this.Title = title;
            this.Company = company;
        }  


    }
}
