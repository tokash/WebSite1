using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBResultsFileReader
{
    class FBResultsFileReader
    {
        #region Members
        List<string> _RawFileData = new List<string>();
        bool _IsNewPost = true;
        List<Post> _Posts = new List<Post>();
        Post _CurrPost = new Post();
        #endregion

        private void ParseFile(string iFilepath)
        {
            if (File.Exists(iFilepath))
            {
                try 
	            {
                    ReadFile(iFilepath);

                    int i = 0;
                    string feed = string.Empty;
                    string postMessage = string.Empty;
                    DateTime createdAt = DateTime.Now;

                    while( i < _RawFileData.Count)
                    {
                        if (_RawFileData[i].Contains("Feed:"))
                        {
                            feed = _RawFileData[i].Substring(5);
                        }
                        else if (_RawFileData[i].Contains("Post message:"))
                        {
                            postMessage = _RawFileData[i].Substring(13);
                        }
                        else if (_RawFileData[i].Contains("Created at: "))
                        {
                            createdAt = DateTime.Parse(_RawFileData[i].Substring(12));
                        }
                        else if (_RawFileData[i].Contains("Commenters:"))
                        {
                            List<Comment> comments = ParsePostComments(ref i);
                            _Posts.Add(new Post()
                                                {
                                                    Feed = feed,
                                                    PostMessage = postMessage,
                                                    CreatedAt = createdAt,
                                                    Comments = comments.Count > 0 ? comments : null
                                                });

                            feed = string.Empty;
                            postMessage = string.Empty;
                            createdAt = DateTime.Now;
                        }

                        i++;
                    }
	            }
	            catch (Exception)
	            {
		
		            throw;
	            }
            }
            else
            {
                throw new Exception(string.Format("File: {0} doesn't exist", iFilepath));
            }
        }

        private List<Comment> ParsePostComments(ref int iLineIndex)
        {
            List<Comment> comments = new List<Comment>();
            string commenter = string.Empty;
            string comment = string.Empty;
            string commenterLink = string.Empty;
            DateTime commentDate = DateTime.Now;
            bool isMeaningfull = false;

            while (!_RawFileData[iLineIndex].Contains("---------------------------------------"))
            {
                int startIdx = 0;
                if (_RawFileData[iLineIndex].Contains("Commenter: "))
                {
                    startIdx = 11;
                    if (_RawFileData[iLineIndex].StartsWith("##"))
                    {
                        isMeaningfull = true;
                        startIdx = 13;
                    }

                    commenter = _RawFileData[iLineIndex].Substring(startIdx, _RawFileData[iLineIndex].IndexOf(" Wrote:") - startIdx);
                    string[] split = _RawFileData[iLineIndex].Split(new string[] { "Wrote:" }, StringSplitOptions.None);
                    comment = split[1];

                    
                }
                else if (_RawFileData[iLineIndex].Contains("Commenter link:"))
                {
                    commenterLink = _RawFileData[iLineIndex].Substring(15);
                }
                else if (_RawFileData[iLineIndex].Contains("Comment date: "))
                {
                    commentDate = DateTime.Parse(_RawFileData[iLineIndex].Substring(14));
                }
                else if (_RawFileData[iLineIndex] == string.Empty)
                {
                    if (commenter != string.Empty && commenterLink != string.Empty)
                    {
                        comments.Add(new Comment() { CommentDate = commentDate,
                                                     Commenter = commenter,
                                                     CommentMessage = comment,
                                                     CommenterLink = commenterLink,
                                                     IsMeaningfull = isMeaningfull
                        });
                        commenter = string.Empty;
                        commenterLink = string.Empty;
                        commentDate = DateTime.Now;
                        isMeaningfull = false;
                    }
                }

                iLineIndex++;
            }

            return comments;
        }

        private void ReadFile(string iFilepath)
        {
            try
            {
                _RawFileData.AddRange(File.ReadAllLines(iFilepath).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ConvertToCSV(string iFilePath)
        {
            try
            {
                ParseFile(iFilePath);
                WriteCSVFile(iFilePath);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void WriteCSVFile(string iFilePath)
        {
            string filename = Path.GetFileNameWithoutExtension(iFilePath);            

            try
            {
                foreach (Post post in _Posts)
                {
                    string csvfilename = string.Format("{0}.{1}.{2}.csv", filename, post.Feed, post.CreatedAt.ToString("dd.MM.yyyy.hh.mm.ss"));
                    using (StreamWriter writer = new StreamWriter(csvfilename, true))
                    {
                    
                        string line = string.Empty;

                        line = string.Format("{0},{1},{2}", post.Feed, post.PostMessage.Replace(",", " "), post.CreatedAt);
                        writer.WriteLine(line);

                        if (post.Comments != null)
                        {
                            foreach (Comment comment in post.Comments)
                            {
                                line = string.Format("{0},{1},{2},{3},{4}", comment.Commenter, comment.CommentMessage.Replace(",", " "), comment.CommenterLink, comment.CommentDate, comment.IsMeaningfull);
                                writer.WriteLine(line);
                            }
                        }                        
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            
        }


    }
}
