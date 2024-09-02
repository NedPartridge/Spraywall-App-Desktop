using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpraywallAppDesktop.Models;

// For the climb management page, data representing the climbs under a wall
class ClimbDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool Flagged { get; set; }
}