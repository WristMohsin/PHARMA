using System.Collections.Generic;
using PHARMA.Models;

namespace PHARMA.DataAccess.Repositories
{
    public class CompanyRepository : BaseRepository
    {
        public List<Company> GetAll()
        {
            return Query<Company>("SELECT TOP 200 * FROM Company ORDER BY cmpnm");
        }

        public Company Get(string cmpcd)
        {
            return QuerySingleOrDefault<Company>("SELECT * FROM Company WHERE cmpcd = ?", cmpcd);
        }
    }
}
