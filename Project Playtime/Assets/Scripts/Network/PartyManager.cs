using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PartyManager : NetworkBehaviour
{
    public string mainSceneName;
    public GameObject playerPrefab;

    private NetworkList<ushort> m_activePorts;

    public NetworkManager networkManager;
    public UnityTransport transport;

    public LobbyUIManager lobbyUIManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        networkManager.StartClient();

        if (m_activePorts == null)
        {
            m_activePorts = new NetworkList<ushort>();
        }

        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddHost()
    {
        if (IsServer) { return; }

        SceneManager.LoadScene(mainSceneName, LoadSceneMode.Single);

        ushort port = (ushort)(m_activePorts.Count <= 0 ? 0 : m_activePorts[m_activePorts.Count - 1] + 1);
        transport.ConnectionData.Port = port;
        AddPortRpc(port);

        StartCoroutine(AddHostRoutine());
    }

    [Rpc(SendTo.Server)]
    private void AddPortRpc(ushort port)
    {
        m_activePorts.Add(port);
        AddPortButtonRpc(port);
    }

    [Rpc(SendTo.Everyone)]
    private void AddPortButtonRpc(ushort port)
    {
        lobbyUIManager.AddButton(port);
    }

    private IEnumerator AddHostRoutine()
    {
        networkManager.Shutdown();

        yield return new WaitUntil(() => !networkManager.ShutdownInProgress);

        networkManager.StartHost();

        Destroy(gameObject);
    }

    public void JoinRandomPort()
    {
        JoinPort(m_activePorts[Random.Range(0, m_activePorts.Count - 1)]);
    }

    public void JoinPort(ushort port)
    {
        if (IsServer) { return; }
        if (!m_activePorts.Contains(port)) { return; }

        SceneManager.LoadScene(mainSceneName, LoadSceneMode.Single);

        transport.ConnectionData.Port = port;
        StartCoroutine(JoinRoutine());
    }

    private IEnumerator JoinRoutine()
    {
        networkManager.Shutdown();

        yield return new WaitUntil(() => networkManager.ShutdownInProgress);

        //networkManager.StartClient();

        Destroy(gameObject);
    }
}
