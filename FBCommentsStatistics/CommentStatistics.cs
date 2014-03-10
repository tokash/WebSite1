using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FBDataEngine;

namespace FBCommentsStatistics
{
    public class CommentStatistics
    {
        public Comment _Comment { get; set; }
        public string _Type { get; set; }
        public List<PropertyStatistic> _Properties = new List<PropertyStatistic>();           
    }
}
