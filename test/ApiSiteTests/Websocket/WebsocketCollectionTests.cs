using Microsoft.VisualStudio.TestTools.UnitTesting;
using AgileConfig.Server.Apisite.Websocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgileConfig.Server.Apisite.Websocket.Tests
{
    [TestClass()]
    public class WebsocketCollectionTests
    {
        [TestMethod()]
        public void AddClientTest()
        {
            var client = new WebsocketClient();
            client.Id = Guid.NewGuid().ToString();
            WebsocketCollection.Instance.AddClient(client);
            WebsocketCollection.Instance.AddClient(client);
            Assert.AreEqual(1, WebsocketCollection.Instance.Count);

            var client1 = new WebsocketClient();
            client1.Id = Guid.NewGuid().ToString();
            WebsocketCollection.Instance.AddClient(client1);
            WebsocketCollection.Instance.AddClient(client1);
            Assert.AreEqual(2, WebsocketCollection.Instance.Count);
        }

        [TestMethod()]
        public void RemoveClientTest()
        {
            var client = new WebsocketClient();
            client.Id = Guid.NewGuid().ToString();
            WebsocketCollection.Instance.AddClient(client);
            var client1 = new WebsocketClient();
            client1.Id = Guid.NewGuid().ToString();
            WebsocketCollection.Instance.AddClient(client1);
            Assert.AreEqual(2, WebsocketCollection.Instance.Count);

            WebsocketCollection.Instance.RemoveClient(client1, System.Net.WebSockets.WebSocketCloseStatus.Empty, "");
            WebsocketCollection.Instance.RemoveClient(client1, System.Net.WebSockets.WebSocketCloseStatus.Empty, "");
            Assert.AreEqual(1, WebsocketCollection.Instance.Count);
            WebsocketCollection.Instance.RemoveClient(client, System.Net.WebSockets.WebSocketCloseStatus.Empty, "");
            WebsocketCollection.Instance.RemoveClient(client, System.Net.WebSockets.WebSocketCloseStatus.Empty, "");
            Assert.AreEqual(0, WebsocketCollection.Instance.Count);
        }
    }
}