using System;
using System.Net.WebSockets;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgileConfig.Server.Apisite.Websocket.Tests;

[TestClass]
public class WebsocketCollectionTests
{
    [TestMethod]
    public void AddClientTest()
    {
        WebsocketCollection.Instance.Clear();

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

    [TestMethod]
    public void RemoveClientTest()
    {
        WebsocketCollection.Instance.Clear();

        var client = new WebsocketClient();
        client.Id = Guid.NewGuid().ToString();
        WebsocketCollection.Instance.AddClient(client);
        var client1 = new WebsocketClient();
        client1.Id = Guid.NewGuid().ToString();
        WebsocketCollection.Instance.AddClient(client1);
        Assert.AreEqual(2, WebsocketCollection.Instance.Count);

        WebsocketCollection.Instance.RemoveClient(client1, WebSocketCloseStatus.Empty, "");
        WebsocketCollection.Instance.RemoveClient(client1, WebSocketCloseStatus.Empty, "");
        Assert.AreEqual(1, WebsocketCollection.Instance.Count);
        WebsocketCollection.Instance.RemoveClient(client, WebSocketCloseStatus.Empty, "");
        WebsocketCollection.Instance.RemoveClient(client, WebSocketCloseStatus.Empty, "");
        Assert.AreEqual(0, WebsocketCollection.Instance.Count);
    }
}