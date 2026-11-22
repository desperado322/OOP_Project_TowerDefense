using OOP_Project.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OOP_Project.Strategies
{
    public interface IStrategy
    {
        string Name { get; }
        T FindTarget<T>(Tower tower, List<Enemy> enemies);
    }
}
