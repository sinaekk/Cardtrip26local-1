using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AIGuideCanvas : MonoBehaviour
{
    // public Gemini_VoiceToText geminiVoiceToText;
    //public Gemini_Chatbot geminiChatbotBatch;
    public Gemini_TextToVoice geminiTextToVoiceLive;

    [Space(10)]
    public Toggle ActiveToggle;
    public TextMeshProUGUI TitleText;

    [Space(10)]
    public Image MicLevelImage;

    public void Start()
    {
        ActiveToggle.onValueChanged.AddListener((value) =>
        {
            // geminiVoiceToText.enabled = value;
            TitleText.alpha = value ? 1f : 0.5f;
        });

        // ���� -> ê�� + ���̽�
//         geminiVoiceToText.OnReceiveVoiceToText.AddListener(() =>
//         {
//             if (!string.IsNullOrEmpty(geminiVoiceToText.LatestTranscript))
//             {
//                 string prompt = $@"
// ����� ģ���ϰ� �������� ������ ���α׷� ���̵� AI�Դϴ�. ������� �ൿ�� ��Ȳ�� �����ϰ�, ������ ��������� Ȱ���Ͽ� ������ �����ϰ� �亯�� �ּ���.

// [��Ȳ]:
// * ����ڴ� ���������� �ְ��ϰ�, �ѱ������������� ���õ� ǳ�� ���� ���� ���α׷��� ���� ���Դϴ�.

// [�������]:
// * �÷��̸� ���ؼ��� ��� �ִ� ��ư�� ������ �մϴ�.

// * ǳ�¹����� �ٶ��� ���� �̿��Ͽ� ���⸦ �����ϴ� ��� ���� �������Դϴ�.
// * ǳ�¹������ �ٶ��� � �������� ����� �������� ��ȯ�� ��, �����⸦ ���� ���� �������� ��ȯ�մϴ�.
// * **��������**�� ǳ�¹����� ������ �پ��� ����������� ���� ģȯ�� ������ ���޿� �⿩�ϰ� �ֽ��ϴ�.
// * ǳ�¹����Ⱑ �ٶ��� ���� ���� ��ġ�� �� ���� ȿ�������� �������� ������ �� �ֽ��ϴ�.
// * ǳ�¹������� ������ ����� ������ ����� ������ ȸ���մϴ�. �ٶ��� ���� ���Ʒ��� �帣�鼭 �з� ���̰� �߻��� ������ �о�� ���(Lift)�� ����ϴ�.
// * ǳ�¹������ �ٶ��� �ʹ� ���ϰų� ���� ���� �ڵ����� ���߰ų� ���� ������ �����Ͽ� �����ϰ� ��ȣ�˴ϴ�.

// [����� �Է�]:
// �����: {geminiVoiceToText.LatestTranscript}
// ";

//                 // ���� -> �ؽ�Ʈ ��ȯ�� �Ϸ�Ǹ� ê���� ����
//                 geminiTextToVoiceLive.SendText(prompt);
//             }
//         });
    }

    // public void Update()
    // {
    //     // ����ũ ������ ���� �̹��� ũ�� ����
    //     float averageSound = geminiVoiceToText.LastMicAverage;
    //     float scale = 0f;
    //     if (geminiVoiceToText.enabled & geminiVoiceToText.IsSpeaking)
    //     {
    //         scale = Mathf.Clamp(averageSound * 10f, 0.1f, 1f);
    //     }
    //     MicLevelImage.transform.localScale = new Vector3(scale, scale, 1f);
    // }
}
