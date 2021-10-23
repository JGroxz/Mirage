using System;
using System.Collections;
using Mirage.Examples.InterestManagement;
using Mirage.InterestManagement;
using Mirage.Sockets.Udp;
using NUnit.Framework;
using Unity.PerformanceTesting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using static UnityEngine.Object;

namespace Mirage.Tests.Performance.Runtime
{
    [Ignore("NotImplemented")]
    public class SpatialHashInterestManagementPerformanceTest : InterestManagementPerformanceBase
    {
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            throw new NotImplementedException();
            //server.gameObject.AddComponent<SpatialHashInterestManager>();

            // wait frame for setup
            yield return null;
        }
    }
    [Ignore("NotImplemented")]
    public class GridAndDistanceInterestManagementPerformanceTest : InterestManagementPerformanceBase
    {
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            throw new NotImplementedException();
            //server.gameObject.AddComponent<SpatialHashInterestManager>();

            // wait frame for setup
            yield return null;
        }
    }

    [Ignore("NotImplemented")]
    public class QuadTreeInterestManagementPerformanceTest : InterestManagementPerformanceBase
    {
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            throw new NotImplementedException();
            //server.gameObject.AddComponent<QuadTreeInterestManager>();

            // wait frame for setup
            yield return null;
        }
    }

    public class GlobalInterestManagementPerformanceTest : InterestManagementPerformanceBase
    {
        protected override IEnumerator SetupInterestManagement(NetworkServer server)
        {
            Setup();
            // wait frame for setup
            yield return null;
        }
    }

    [Category("Performance"), Category("InterestManagement")]
    public abstract class InterestManagementPerformanceBase
    {
        const string testScene = "Assets/Examples/InterestManagement/Scenes/Scene.unity";
        const string NpcSpawnerName = "Mobs";
        const string LootSpawnerName = "Ground";
        const int clientCount = 100;
        const int stationaryCount = 3500;
        const int movingCount = 500;


        private NetworkServer server;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(testScene, new LoadSceneParameters(LoadSceneMode.Single));

            // wait 1 frame for start to be called
            yield return null;
            GameObject.Find(LootSpawnerName).GetComponent<Spawner>().count = stationaryCount;
            GameObject.Find(NpcSpawnerName).GetComponent<Spawner>().count = movingCount;


            server = FindObjectOfType<NetworkServer>();
            server.SocketFactory = server.gameObject.AddComponent<UdpSocketFactory>();

            bool started = false;
            server.MaxConnections = clientCount;

            removeExistingIM();
            // wait frame for destroy
            yield return null;

            yield return SetupInterestManagement(server);

            server.Started.AddListener(() => started = true);
            server.StartServer();

            // wait for start
            while (!started) { yield return null; }

            for (int i = 0; i < clientCount; i++)
            {
                try
                {
                    var clientGo = new GameObject { name = $"Client {i}" };
                    NetworkClient client = clientGo.AddComponent<NetworkClient>();
                    client.SocketFactory = client.gameObject.AddComponent<UdpSocketFactory>();
                    client.Connect("localhost");
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
        }

        private void removeExistingIM()
        {
            INetworkVisibility[] existing = server.GetComponents<INetworkVisibility>();

            for (int i = 0; i < existing.Length; i++)
            {
                //Destroy(existing[i]);
            }
        }

        /// <summary>
        /// Called before server starts
        /// </summary>
        /// <param name="server"></param>
        /// <returns></returns>
        protected abstract IEnumerator SetupInterestManagement(NetworkServer server);


        [UnityTearDown]
        public IEnumerator TearDown()
        {
            server.Stop();

            // open new scene so that old one is destroyed
            SceneManager.CreateScene("empty", new CreateSceneParameters(LocalPhysicsMode.None));
            yield return SceneManager.UnloadSceneAsync(testScene);
        }

        [UnityTest]
        public IEnumerator RunsWithoutErrors()
        {
            yield return new WaitForSeconds(5);
        }

        [UnityTest, Performance]
        public IEnumerator FramePerformance()
        {
            SampleGroup[] sampleGroups =
            {
                new SampleGroup("Observers", SampleUnit.Microsecond),
                new SampleGroup("OnAuthenticated", SampleUnit.Microsecond),
                new SampleGroup("OnSpawned", SampleUnit.Microsecond),
            };

            yield return Measure.Frames()
                .ProfilerMarkers(sampleGroups)
                .WarmupCount(5)
                .MeasurementCount(300)
                .Run();
        }
    }
}