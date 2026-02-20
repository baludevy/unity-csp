using TMPro;
using UnityEngine;

public class NetworkDebug : MonoBehaviour {
    public static NetworkDebug Instance;
    private float bReceived;
    private float bSent;
    private TMP_Text byteDownText;
    private TMP_Text byteUpText;
    private TMP_Text gameSpeedText;

    private Transform holder;

    private TMP_Text localLagCompPlayerPositions;
    private TMP_Text packetDownText;
    private TMP_Text packetUpText;

    private TMP_Text pingText;
    private float pReceived;

    private float pSent;
    private TMP_Text serverLagCompPlayerPositions;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        InitUI();
    }

    #region Bandwidth UI

    public void ToggleUI() {
        holder.gameObject.SetActive(!holder.gameObject.activeSelf);
    }

    private void InitUI() {
        holder = transform.GetChild(0);

        pingText = holder.GetChild(1).gameObject.GetComponent<TMP_Text>();
        byteUpText = holder.GetChild(2).gameObject.GetComponent<TMP_Text>();
        byteDownText = holder.GetChild(3).gameObject.GetComponent<TMP_Text>();
        packetUpText = holder.GetChild(4).gameObject.GetComponent<TMP_Text>();
        packetDownText = holder.GetChild(5).gameObject.GetComponent<TMP_Text>();
        gameSpeedText = holder.GetChild(6).gameObject.GetComponent<TMP_Text>();

        InvokeRepeating(nameof(Bandwidth), 0f, 1f);
    }

    public void SetPing(float a) {
        pingText.text = $"ping: {a}ms";
    }

    private void SetByteUp(float a) {
        byteUpText.text = $"bytes up/s: {a}";
    }

    private void SetByteDown(float a) {
        byteDownText.text = $"bytes down/s: {a}";
    }

    private void SetPacketUp(float a) {
        packetUpText.text = $"packets up/s: {a}";
    }

    private void SetPacketDown(float a) {
        packetDownText.text = $"packets down/s: {a}";
    }

    public void SetGameSpeed(float a) {
        gameSpeedText.text = $"game speed: {a:F2}";
    }

    private void Bandwidth() {
        pSent = ClientSend.packetsSent;
        pReceived = ClientHandle.packetsReceived;
        bSent = ClientSend.bytesSent;
        bReceived = ClientHandle.bytesReceived;

        SetByteUp(bSent);
        SetByteDown(bReceived);
        SetPacketUp(pSent);
        SetPacketDown(pReceived);

        ClientSend.packetsSent = 0;
        ClientHandle.packetsReceived = 0;
        ClientSend.bytesSent = 0;
        ClientHandle.bytesReceived = 0;
    }

    #endregion
}