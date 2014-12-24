using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace Business.Models
{
    public partial class File
    {
        private static readonly string DirectoryPath = Path.GetFullPath(System.Configuration.ConfigurationManager.AppSettings["FilePath"]);

        /// <summary>
        /// Binary file content is stored in "FilePath"
        /// </summary>
        public byte[] SavedContent
        {
            get
            {
                // files MUST be saved to the database we store their content, this is the only way this abstraction works
                if (this.FileID == default(Guid))
                {
                    throw new InvalidOperationException("Files must be saved to the database before content can be stored");
                }

                if (this.IsInDirectory)
                {
                    var fullPath = this.GetPath();
                    return System.IO.File.ReadAllBytes(fullPath);
                }

                return this.Content;
            }

            set
            {
                // files MUST be saved to the database we store their content, this is the only way this abstraction works
                if (this.FileID == default(Guid))
                {
                    throw new InvalidOperationException("Files must be saved to the database before content can be stored");
                }

                this.IsInDirectory = true;
                var fullPath = this.GetPath();
                this.Content = new byte[0];
                System.IO.File.WriteAllBytes(fullPath, value);
            }
        }

        public string GetPath()
        {
            return Path.Combine(DirectoryPath, this.FileID.ToString());
        }

        public File()
        {
            this.Created = DateTime.Now;
        }

        public File(string name, string contentType)
            : this()
        {
            this.Name = name;
            this.ContentType = contentType;
        }

        public File(string name, string htmlcontent, string contentType)
            : this()
        {
            this.Name = name;
            this.HtmlContent = htmlcontent;
            this.ContentType = contentType;
        }
    }
}