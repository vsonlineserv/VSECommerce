using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSOnline.VSECommerce.Persistence.Data;

namespace VSOnline.VSECommerce.Domain
{
    public class ManufacturerRepository
    {
        private readonly DataContext _context;
        public ManufacturerRepository(DataContext context)
        {
            _context = context;
        }
        public List<KeyValuePair<int, string>> GetBrands(int BranchId)
        {
            Dictionary<int, string> dictionary = _context.Manufacturer.Where(b=>b.BranchId == BranchId && (b.Deleted == false || b.Deleted == null)).Select(b => new { b.ManufacturerId, b.Name }).ToDictionary(o => o.ManufacturerId, o => o.Name);
            return dictionary.ToList<KeyValuePair<int, string>>();
        }
        public List<KeyValuePair<int, string>> GetBrandsForVbuy()
        {
            Dictionary<int, string> dictionary =_context.Manufacturer.Select(b => new { b.ManufacturerId, b.Name }).ToDictionary(o => o.ManufacturerId, o => o.Name);
            return dictionary.ToList<KeyValuePair<int, string>>();
        }
    }
}
