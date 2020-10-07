using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageSchool.EF
{
    public partial class vw_ClientDetails
    {
        public List<Tag> Tags
        {
            get
            {
                using(var context = new LanguageContext())
                {
                    var client = context.Client.Find(Идентификатор);
                    return client.Tag.ToList();
                }
            }
        }
    }
}
