using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Couchbase.Authentication;
using Couchbase.Configuration.Client;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Couchbase.Example
{
    public class BloggingContext : DbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var clientConfiguration = new ClientConfiguration
            {
                Servers = new List<Uri> {new Uri("http://localhost:8091")}
            };
            var authenticator = new PasswordAuthenticator("Administrator", "password");
            var bucketName = "default";
            optionsBuilder.UseCouchbase(clientConfiguration, authenticator, bucketName);
        }
    }

    public class Blog
    {
        [Key]
        public Guid BlogId { get; set; }
        public string Url { get; set; }
        public int Rating { get; set; }
        public List<Post> Posts { get; set; }
    }

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new BloggingContext())
            {
                var blog = new Blog { Url = "http://sample.com" };
                db.Blogs.Add(blog);
                db.SaveChanges();
            }
        }
    }
}
