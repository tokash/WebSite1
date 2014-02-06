using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FBResultsFileReader
{
    public class Post
    {
        public string Feed {get;set;}
        public string PostMessage { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<Comment> Comments = new List<Comment>();
    }

    public class Comment
    {
        public string Commenter { get; set; }
        public string CommentMessage { get; set; }
        public string CommenterLink { get; set; }
        public DateTime CommentDate { get; set; }
        public string Taxonomy { get; set; }
        //public bool IsMeaningfull { get; set; }
    }
}
