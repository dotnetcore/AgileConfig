using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Client.Tests
{
    [TestClass()]
    public class RandomServersTests
    {
        [TestMethod()]
        public void NextTest()
        {
            var urls = "";
            Assert.ThrowsException<ArgumentNullException>(() => {
                var rs = new RandomServers(urls);
            });

            Console.WriteLine("1");
            urls = "server0";
            RandomServers rs = null;
            for (int i = 0; i < 100; i++)
            {
                rs = new RandomServers(urls);
            }
            Assert.IsFalse(rs.IsComplete);
            var url = rs.Next();
            Console.WriteLine(url);
            Assert.AreEqual("server0", url);
            Assert.IsTrue(rs.IsComplete);

            Console.WriteLine("2");
            urls = "server0,server1";
            for (int i = 0; i < 100; i++)
            {
                rs = new RandomServers(urls);
            }
            Assert.IsFalse(rs.IsComplete);
            var list = new List<string>();
            while (!rs.IsComplete)
            {
                url = rs.Next();
                list.Add(url);
                Console.WriteLine(url);
            }
            Assert.IsTrue(rs.IsComplete);
            Assert.AreEqual(2, list.Count);

            Console.WriteLine("3");
            urls = "server0,server1,server2";
            for (int i = 0; i < 100; i++)
            {
                rs = new RandomServers(urls);
            }
            Assert.IsFalse(rs.IsComplete);
            list = new List<string>();
            while (!rs.IsComplete)
            {
                url = rs.Next();
                list.Add(url);
                Console.WriteLine(url);
            }
            Assert.IsTrue(rs.IsComplete);
            Assert.AreEqual(3, list.Count);

            Console.WriteLine("4");
            urls = "server0,server1,server2,server3";
            for (int i = 0; i < 100; i++)
            {
                rs = new RandomServers(urls);
            }
            Assert.IsFalse(rs.IsComplete);
            list = new List<string>();
            while (!rs.IsComplete)
            {
                url = rs.Next();
                list.Add(url);
                Console.WriteLine(url);
            }
            Assert.IsTrue(rs.IsComplete);
            Assert.AreEqual(4, list.Count);
        }
    }
}