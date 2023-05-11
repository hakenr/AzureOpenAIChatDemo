using System.Text;
using Azure;
using Azure.AI.OpenAI;

// úvodní setup
Console.OutputEncoding = Encoding.UTF8;
Console.WriteLine("DEBUG: Zdejte klíč k Azure OpenAI:");
var azureOpenAIApiKey = Console.ReadLine();
Console.Clear();

// intro
Console.WriteLine("Ahoj, jsem Mensík, umělá inteligence. Rád si s tebou promluvím o {TODO}. Kdykoliv můžeš skončit napsáním \"konec\".");

var chatMessages = new List<ChatMessage>();

// system message = instrukce pro OpenAI
chatMessages.Add(new ChatMessage(ChatRole.System, GetSystemMessage()));

// hlavní smyčka konverzace
while (true)
{
	Console.Write("Ty: ");
	var input = Console.ReadLine();
	if (input == "konec")
	{
		break;
	}

	chatMessages.Add(new ChatMessage(ChatRole.User, input));
	GetOpenAIResponse();

	Console.WriteLine($"Mensík: {chatMessages.Last().Content}");
}

void GetOpenAIResponse()
{
	OpenAIClient client = new OpenAIClient(new Uri("https://mensagymnaziumsemprgoai.openai.azure.com/"), new AzureKeyCredential(azureOpenAIApiKey));

	ChatCompletionsOptions chatCompletionsOptions = new ChatCompletionsOptions()
	{
		Temperature = (float)0.7,
		MaxTokens = 800,
		NucleusSamplingFactor = (float)0.95,
		FrequencyPenalty = 0,
		PresencePenalty = 0,
	};

	chatCompletionsOptions.Messages.Add(new ChatMessage(ChatRole.System, GetSystemMessage()));
	foreach (var chatMessage in chatMessages.Skip(1).TakeLast(10)) // máme k dispozici jen 4K tokenů, proto omezíme historii konverzace
	{		
		chatCompletionsOptions.Messages.Add(chatMessage);
	}

	Response<ChatCompletions> response = client.GetChatCompletions("MensaGymnaziumSemPrgGpt35TurboDeployment", chatCompletionsOptions);

	chatMessages.Add(response.Value.Choices.First().Message);
}

string GetSystemMessage()
{
	return @"
Jseš Mensík, chatbot Mensa gymnázia.
Odpovídej stručně, spíše neformálně a nespisovně.
Odpovídej pravdivě na základě známých informací o Mensa gymnáziu a zohledni níže poskytnutý kontext.
Neodpovídej na otázky, které nesouvisí se Mensa gymnnáziem a reaguj ""Mohu pomoci pouze s otázkami souvisejícími se Mensa gymnáziem"".
Pokud neznáš odpověď, směřuj tazatele na e-mail info@mensagymnazium.cz
";
}