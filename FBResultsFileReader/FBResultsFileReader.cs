using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FBDataEngine
{
    public class FBResultsFileReader
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
                            if (_RawFileData[i].Length >= 30)
                            {
                                createdAt = DateTime.Parse(_RawFileData[i].Substring(12));
                            }
                            else
                            {
                                createdAt = DateTime.MinValue;
                            }
                        }
                        else if (_RawFileData[i].Contains("Commenters:"))
                        {
                            List<Comment> comments = ParsePostComments(ref i);
                            i--;
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

        public List<Post> GetPostsFromFile(string iFilepath)
        {
            List<Post> filePosts = new List<Post>();

            if (File.Exists(iFilepath))
            {
                try 
	            {
                    _RawFileData.Clear();
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
                            if (_RawFileData[i].Length >= 30)
                            {
                                createdAt = DateTime.Parse(_RawFileData[i].Substring(12));
                            }
                            else
                            {
                                createdAt = DateTime.MinValue;
                            }
                        }
                        else if (_RawFileData[i].Contains("Commenters:"))
                        {
                            List<Comment> comments = ParsePostComments(ref i);
                            i--;
                            filePosts.Add(new Post()
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

            return filePosts;
        }

        private List<Comment> ParsePostComments(ref int iLineIndex)
        {
            List<Comment> comments = new List<Comment>();
            string commenter = string.Empty;
            string comment = string.Empty;
            string commenterLink = string.Empty;
            DateTime commentDate = DateTime.MinValue;
            string data = string.Empty;
            bool isMeaningfull = false;

            while (iLineIndex < _RawFileData.Count && !_RawFileData[iLineIndex].StartsWith("Feed"))
            {
                int startIdx = 0;
                if (_RawFileData[iLineIndex].Contains("Commenter: "))
                {
                    startIdx = 11;
                    if (_RawFileData[iLineIndex].StartsWith("#"))
                    {
                        data = ExtractDataBetweenCharacters(_RawFileData[iLineIndex], "#");
                        //isMeaningfull = true;
                        //startIdx = 13;
                        startIdx = 13 + data.Length;
                    }

                    try
                    {
                        commenter = _RawFileData[iLineIndex].Substring(startIdx, _RawFileData[iLineIndex].IndexOf(" Wrote :") - startIdx);
                        string[] split = _RawFileData[iLineIndex].Split(new string[] { " Wrote :" }, StringSplitOptions.None);
                        comment = split[1];
                    }
                    catch (Exception)
                    {
                        commenter = _RawFileData[iLineIndex].Substring(startIdx, _RawFileData[iLineIndex].IndexOf(" Wrote:") - startIdx);
                        string[] split = _RawFileData[iLineIndex].Split(new string[] { " Wrote:" }, StringSplitOptions.None);
                        comment = split[1];
                    }
                    

                    
                }
                else if (_RawFileData[iLineIndex].Contains("Commenter link:"))
                {
                    commenterLink = _RawFileData[iLineIndex].Substring(15);
                }
                else if (_RawFileData[iLineIndex].Contains("Comment date: "))
                {
                    startIdx = 14;
                    if (_RawFileData[iLineIndex].StartsWith("#"))
                    {
                        isMeaningfull = true;
                        //startIdx = 16;
                        data = ExtractDataBetweenCharacters(_RawFileData[iLineIndex], "#");
                        startIdx = 16 + data.Length;
                    }

                    try
                    {
                        commentDate = DateTime.Parse(_RawFileData[iLineIndex].Substring(startIdx));
                    }
                    catch (Exception)
                    {
                        
                    }
                }
                else if (/*_RawFileData[iLineIndex] == string.Empty &&*/ comment != string.Empty && commentDate != DateTime.MinValue)
                {
                    if (commenter != string.Empty && commenterLink != string.Empty)
                    {
                        if (isMeaningfull == true && data == string.Empty)
                        {
                            data = "9999";
                        }
                        else if(isMeaningfull == false)
                        {
                            data = "0";
                        }
                        

                        comments.Add(new Comment() { CommentDate = commentDate,
                                                     Commenter = commenter,
                                                     CommentMessage = comment,
                                                     CommenterLink = commenterLink,
                                                     Taxonomy = data
                                                     //IsMeaningfull = isMeaningfull
                        });
                        commenter = string.Empty;
                        commenterLink = string.Empty;
                        commentDate = DateTime.MinValue;
                        comment = string.Empty;
                        isMeaningfull = false;
                        data = string.Empty;
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
                //_RawFileData.AddRange(File.ReadAllLines(iFilepath).ToList());
                string temp = new DocxToText(iFilepath).ExtractText();
                string[] lines = temp.Split('\t', '\n');
                List<string> listLines = new List<string>();

                foreach (var line in lines)
                {
                    if (line != string.Empty && line != "\r")
                    {
                        if (listLines.Count > 0)
                        {
                            if (listLines[listLines.Count - 1].StartsWith("Commenter link"))
                            {
                                listLines.Add("\n");
                                listLines.Add(line.Replace("\r", string.Empty));
                            }
                            else
                            {
                                listLines.Add(line.Replace("\r", string.Empty));
                            }
                        }
                        else
                        {
                            listLines.Add(line.Replace("\r", string.Empty));
                        }
                    }
                }
                listLines.Add("\n");
                _RawFileData.AddRange(listLines);
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
                _RawFileData.Clear();
                _Posts.Clear();
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
                    using (StreamWriter writer = new StreamWriter(csvfilename.Replace("\r", string.Empty), true))
                    {
                    
                        string line = string.Empty;

                        line = string.Format("{0},{1},{2}", post.Feed, post.PostMessage.Replace(",", " "), post.CreatedAt);
                        writer.WriteLine(line);

                        if (post.Comments != null)
                        {
                            foreach (Comment comment in post.Comments)
                            {
                                line = string.Format("{0},{1},{2},{3},{4}", comment.Commenter, comment.CommentMessage.Replace(",", " "), comment.CommenterLink, comment.CommentDate, comment.Taxonomy);
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

        private string ExtractDataBetweenCharacters(string iOriginalString, string iMarker)
        {
            string data = string.Empty;

            //locate openning marker location
            //get substring from that location to end of original string
            //locate closing marker location
            //get substring from position 0 to closing marker location

            int openningMarker = iOriginalString.IndexOf(iMarker);
            string intermediate = iOriginalString.Substring(openningMarker + 1);
            int closingMarker = intermediate.IndexOf(iMarker) + openningMarker;
            data = intermediate.Substring(openningMarker, closingMarker - openningMarker);

            return data;
        }
    }
}
