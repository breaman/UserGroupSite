using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserGroupSite.Data.Models;

public class FingerPrintEntityBase : EntityBase
{
    public DateTime? CreatedOn { get; set; }
    public int CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }
    public int ModifiedBy { get; set; }
}
