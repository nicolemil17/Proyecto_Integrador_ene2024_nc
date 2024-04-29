using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;
        
        //[SerializeField] private RectTransform sent;
        //[SerializeField] private RectTransform received;
        [SerializeField] private Text myQuestion;
        [SerializeField] private Text ServerAnswer;

        private float height;
        private string chatgptAnswer;

        private OpenAIApi openai;

        private List<ChatMessage> messages = new List<ChatMessage>();
        [SerializeField]
        private string promptTemplate ="En esta sesión de terapia virtual, eres un paciente que sufre de {0} con severidad {1}. Primero, preséntate proporcionando tu nombre, edad y género. Como paciente, responde únicamente en el contexto de alguien que sufre de {0}. Si la conversación se desvía del tema, intenta redirigirla suavemente hacia el contexto de la terapia. Recuerda responder las preguntas solo como el paciente en terapia";
       // private string prompt = "Actua como una persona con problemas emocionales, perdiste a tus pades en un accidente de auto, nada te alegra ultimamente, no rompas personaje, responde de la forma mas resumida posible y no menciones que eres una inteligencia artificial. Ahora responde:";
        //private string prompt = "Actua como una psicologa que trata de solucionar mis problemas emocionales, no rompas personaje, responde de la forma mas resumida posible y no menciones que eres una inteligencia artificial. Ahora responde:";

        [Space]
        [Header("----------Api Key----------")]
        //[SerializeField] private string ApiKey;
        [SerializeField] private string ApiKey = "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";

        private void Start()
        {
            //Para pruebas
            //button.onClick.AddListener(SendReply);

            openai = new OpenAIApi(ApiKey);
             // Setup initial prompt with random disorder and severity
            var (disorder, severity) = SeleccionarTrastornoSeveridad();
            prompt = string.Format(promptTemplate, disorder, severity);
        }
         private (string, string) SeleccionarTrastornoSeveridad()
        {
            string[] trastornos = { "Trastorno Depresivo Mayor (TDM)", "Trastorno Depresivo Persistente (Distrofia)", "Trastorno Bipolar", "Trastorno Afectivo Estacional (TAE)", "Depresión Postparto", "Trastorno Disfórico Premenstrual (TDPM)", "Depresión Atípica", "Depresión Psicótica", "Depresión Situacional (Trastorno de Adaptación con Estado de Ánimo Depresivo)", "Depresión Resistente al Tratamiento", "Trastorno de Ansiedad Generalizada (TAG)", "Trastorno de Pánico", "Agorafobia", "Trastorno de Ansiedad Social (Fobia Social)", "Fobia Específica", "Trastorno de Personalidad Narcisista" };
            string[] severidades = { "leve", "moderado", "grave" };

            Random random = new Random();
            string trastorno = trastornos[random.Next(trastornos.Length)];
            string severidad = severidades[random.Next(severidades.Length)];

            return (trastorno, severidad);
        }
        
        private void AppendMessage(ChatMessage message)
        {
            if (message.Role == "user")
            {
                // Si el mensaje es del usuario, actualiza myQuestion.text
                myQuestion.text += message.Content;
            }
            else
            {
                // Si el mensaje es del servidor, actualiza ServerAnswer.text
                ServerAnswer.text += message.Content;
                chatgptAnswer = message.Content;

            }

            /*
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
            */
        }

        public string GetServerAnswer()
        {
            return chatgptAnswer;
        }

        public async void SendReply(string myText)
        {
            chatgptAnswer = null;

            var newMessage = new ChatMessage()
            {
                Role = "user",
                //Content = inputField.text
                Content = myText
            };
            
            AppendMessage(newMessage);

            if (messages.Count == 0) newMessage.Content = prompt + "\n" + inputField.text; 
            
            messages.Add(newMessage);
            
            button.enabled = false;
            inputField.text = "";
            inputField.enabled = false;
            
            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0613",
                Messages = messages
            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();
                
                messages.Add(message);
                AppendMessage(message);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }

            button.enabled = true;
            inputField.enabled = true;
        }
    }
}
