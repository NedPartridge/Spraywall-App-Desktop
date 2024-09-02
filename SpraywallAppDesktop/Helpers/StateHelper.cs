using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpraywallAppDesktop.Helpers;

// For maintaining wallid
// query params are preferred, but manage climbs broke for some reason, and this is the fix
static class StateHelper
{
    public static int wallId { get; set; }
}