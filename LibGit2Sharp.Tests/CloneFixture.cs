﻿using System;
using System.IO;
using System.Linq;
using LibGit2Sharp.Tests.TestHelpers;
using Xunit;
using Xunit.Extensions;

namespace LibGit2Sharp.Tests
{
    public class CloneFixture : BaseFixture
    {
        [Theory]
        [InlineData("http://github.com/libgit2/TestGitRepository")]
        [InlineData("https://github.com/libgit2/TestGitRepository")]
        [InlineData("git://github.com/libgit2/TestGitRepository")]
        //[InlineData("git@github.com:libgit2/TestGitRepository")]
        public void CanClone(string url)
        {
            var scd = BuildSelfCleaningDirectory();

            string clonedRepoPath = Repository.Clone(url, scd.DirectoryPath);

            using (var repo = new Repository(clonedRepoPath))
            {
                string dir = repo.Info.Path;
                Assert.True(Path.IsPathRooted(dir));
                Assert.True(Directory.Exists(dir));

                Assert.NotNull(repo.Info.WorkingDirectory);
                Assert.Equal(Path.Combine(scd.RootedDirectoryPath, ".git" + Path.DirectorySeparatorChar), repo.Info.Path);
                Assert.False(repo.Info.IsBare);

                Assert.True(File.Exists(Path.Combine(scd.RootedDirectoryPath, "master.txt")));
                Assert.Equal(repo.Head.Name, "master");
                Assert.Equal(repo.Head.Tip.Id.ToString(), "49322bb17d3acc9146f98c97d078513228bbf3c0");
            }
        }

        private void AssertLocalClone(string path)
        {
            var scd = BuildSelfCleaningDirectory();

            string clonedRepoPath = Repository.Clone(path, scd.DirectoryPath);

            using (var clonedRepo = new Repository(clonedRepoPath))
            using (var originalRepo = new Repository(BareTestRepoPath))
            {
                Assert.NotEqual(originalRepo.Info.Path, clonedRepo.Info.Path);
                Assert.Equal(originalRepo.Head, clonedRepo.Head);

                Assert.Equal(originalRepo.Branches.Count(), clonedRepo.Branches.Count(b => b.IsRemote));
                Assert.Equal(1, clonedRepo.Branches.Count(b => !b.IsRemote));

                Assert.Equal(originalRepo.Tags.Count(), clonedRepo.Tags.Count());
                Assert.Equal(1, clonedRepo.Network.Remotes.Count());
            }
        }

        [Fact]
        public void CanCloneALocalRepositoryFromALocalUri()
        {
            var uri = new Uri(BareTestRepoPath);
            AssertLocalClone(uri.AbsoluteUri);
        }

        [Fact]
        public void CanCloneALocalRepositoryFromAStandardPath()
        {
            AssertLocalClone(BareTestRepoPath);
        }

        [Theory]
        [InlineData("http://github.com/libgit2/TestGitRepository")]
        [InlineData("https://github.com/libgit2/TestGitRepository")]
        [InlineData("git://github.com/libgit2/TestGitRepository")]
        //[InlineData("git@github.com:libgit2/TestGitRepository")]
        public void CanCloneBarely(string url)
        {
            var scd = BuildSelfCleaningDirectory();

            string clonedRepoPath = Repository.Clone(url, scd.DirectoryPath, bare: true);

            using (var repo = new Repository(clonedRepoPath))
            {
                string dir = repo.Info.Path;
                Assert.True(Path.IsPathRooted(dir));
                Assert.True(Directory.Exists(dir));

                Assert.Null(repo.Info.WorkingDirectory);
                Assert.Equal(scd.RootedDirectoryPath + Path.DirectorySeparatorChar, repo.Info.Path);
                Assert.True(repo.Info.IsBare);
            }
        }

        [Theory]
        [InlineData("git://github.com/libgit2/TestGitRepository")]
        public void WontCheckoutIfAskedNotTo(string url)
        {
            var scd = BuildSelfCleaningDirectory();

            string clonedRepoPath = Repository.Clone(url, scd.DirectoryPath, checkout: false);

            using (var repo = new Repository(clonedRepoPath))
            {
                Assert.False(File.Exists(Path.Combine(repo.Info.WorkingDirectory, "master.txt")));
            }
        }

        [Theory]
        [InlineData("git://github.com/libgit2/TestGitRepository")]
        public void CallsProgressCallbacks(string url)
        {
            bool transferWasCalled = false;
            bool checkoutWasCalled = false;

            var scd = BuildSelfCleaningDirectory();

            Repository.Clone(url, scd.DirectoryPath,
                onTransferProgress: _ => { transferWasCalled = true; return true; },
                onCheckoutProgress: (a, b, c) => checkoutWasCalled = true);

            Assert.True(transferWasCalled);
            Assert.True(checkoutWasCalled);
        }

        [SkippableFact]
        public void CanCloneWithCredentials()
        {
            InconclusiveIf(() => string.IsNullOrEmpty(Constants.PrivateRepoUrl),
                "Populate Constants.PrivateRepo* to run this test");

            var scd = BuildSelfCleaningDirectory();

            string clonedRepoPath = Repository.Clone(Constants.PrivateRepoUrl, scd.DirectoryPath,
                credentials: new BasicCredentials
                                 {
                                     Username = Constants.PrivateRepoUsername,
                                     Password = Constants.PrivateRepoPassword
                                 });


            using (var repo = new Repository(clonedRepoPath))
            {
                string dir = repo.Info.Path;
                Assert.True(Path.IsPathRooted(dir));
                Assert.True(Directory.Exists(dir));

                Assert.NotNull(repo.Info.WorkingDirectory);
                Assert.Equal(Path.Combine(scd.RootedDirectoryPath, ".git" + Path.DirectorySeparatorChar), repo.Info.Path);
                Assert.False(repo.Info.IsBare);
            }
        }
    }
}
