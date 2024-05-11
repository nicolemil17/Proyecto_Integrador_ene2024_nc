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
         private string promptTemplate = "En esta sesión de terapia virtual, eres un paciente que sufre de  {0} con severidad {1}. Primero, piensa en tu nombre, edad y género. Como paciente, responde únicamente en el contexto de alguien que sufre de  {0}. Si la conversación se desvía del tema, intenta redirigirla suavemente hacia el contexto de la terapia. Recuerda responder las preguntas solamente como el paciente en terapia y no menciones que eres una inteligencia artificial. No menciones el trastorno que se te asignó.  Responde de la forma mas resumida posible.Ahora responde:";
        [Space]
        [Header("----------Api Key----------")]
        
        [SerializeField] private string ApiKey;
        

        private void Start()
        {
            openai = new OpenAIApi(ApiKey);
            var (trastorno, severidad) = SeleccionarTrastornoSeveridad();
            promptTemplate = string.Format(promptTemplate, trastorno, severidad);

        }
        private (string, string) SeleccionarTrastornoSeveridad()
        {
            //string[] trastornos = { "Major Depressive Disorder (MDD)", "Persistent Depressive Disorder (Dysthymia)", "Bipolar Disorder", "Seasonal Affective Disorder (SAD)", "Postpartum Depression", "Premenstrual Dysphoric Disorder (PMDD)", "Atypical Depression", "Psychotic Depression", "Situational Depression (Adjustment Disorder with Depressed Mood)", "Treatment-Resistant Depression", "Generalized Anxiety Disorder (GAD)", "Panic Disorder", "Agoraphobia", "Social Anxiety Disorder (Social Phobia)", "Specific Phobia", "Narcissistic Personality Disorder" };
            string[] trastornos = { "Trastorno Depresivo Mayor (TDM)", "Trastorno Depresivo Persistente (Distrofia)", "Trastorno Bipolar", "Trastorno Afectivo Estacional (TAE)", "Depresión Postparto", "Trastorno Disfórico Premenstrual (TDPM)", "Depresión Atípica", "Depresión Psicótica", "Depresión Situacional (Trastorno de Adaptación con Estado de Ánimo Depresivo)", "Depresión Resistente al Tratamiento", "Trastorno de Ansiedad Generalizada (TAG)", "Trastorno de Pánico", "Agorafobia", "Trastorno de Ansiedad Social (Fobia Social)", "Fobia Específica", "Trastorno de Personalidad Narcisista" };
            //string[] severidades = { "mild", "moderate", "severe" };
            string[] severidades = { "leve", "moderado", "grave" };

            System.Random random = new System.Random();
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

            if (messages.Count == 0) newMessage.Content = promptTemplate + "\n" + inputField.text; 
            
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
