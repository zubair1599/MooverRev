using System.Linq;
using Business.Models;

namespace Moovers.WebModels
{
    public class SmartListModel
    {
        public IQueryable<SmartMove> SmartMovesQuotes { get; set; }


        public SmartListModel(IQueryable<SmartMove> moves)
        {
            this.SmartMovesQuotes = moves;           
        }
    }
}
