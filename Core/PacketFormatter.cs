using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core
{
    // Klasa przekształcająca pakiet na tablice bajtów i odwrotnie
    public class PacketFormatter : IPacketFormatter
    {
        // Metoda przekształcająca pakiet na tablicę bajtów
        public byte[] Serialize(Packet packet)
        {
            var stringBuilder = new StringBuilder();
            var setProperties = packet.GetSetProperties();
            foreach (var property in setProperties)
            {
                stringBuilder.Append(MakeHeaderPiece(property.Key, property.ObjectValue.ToString()));
            }

            return Encoding.UTF8.GetBytes(stringBuilder.ToString());
        }

        private static string MakeHeaderPiece(string key, string value)
        {
            return new string($"{key}-){value}(|");
        }

        // Metoda przekształcająca tablicę bajtów (odczytaną asynchronicznie ze strumienia) na pakiet danych
        public async Task<Packet> DeserializeAsync(Stream stream)
        {
            var packet = new Packet();


            var buffer = new byte[Packet.MaximumPacketSize];
            var dataRead = await stream.ReadAsync(buffer, 0, buffer.Length);
            
            // Strumień został zamknięty
            if (dataRead == 0)
                return null;
                
            // Usunięcie pustych komórek bufora
            var formattedBytes = new byte[dataRead];
            Array.Copy(buffer, formattedBytes, dataRead);

            var serializedMessage = Encoding.UTF8.GetString(formattedBytes);

            // Część przypisująca odpowiednie pola pakietu w zależności od odczytanych danych
            var regex = new Regex(@"(?<key>\w+)-\)(?<value>[-\w\s]+)\(\|");
            if (!regex.IsMatch(serializedMessage))
                throw new InvalidDataException("Received message doesn't match the pattern of header");
            foreach (Match match in regex.Matches(serializedMessage))
            {
                switch (match.Groups["key"].Value)
                {
                    case "sid":
                        packet.SetSessionId(Guid.Parse(match.Groups["value"].Value));
                        break;
                    case "Operacja":
                        Enum.TryParse(match.Groups["value"].Value, out Operation operation);
                        packet.SetOperation(operation);
                        break;
                    case "Status":
                        Enum.TryParse(match.Groups["value"].Value, out Status status);
                        packet.SetStatus(status);
                        break;
                    case "timestamp":
                        var unixTimestamp = int.Parse(match.Groups["value"].Value);
                        packet.SetTimestamp(new Timestamp(unixTimestamp));
                        break;
                    case "source":
                        packet.SetSourceId(int.Parse(match.Groups["value"].Value));
                        break;
                    case "destination":
                        packet.SetDestinationId(int.Parse(match.Groups["value"].Value));
                        break;
                    case "Identyfikator":
                        packet.SetClientId(int.Parse(match.Groups["value"].Value));
                        break;
                    case "message":
                        break;
                    default:
                        throw new InvalidDataException("Received message doesn't match the pattern of header");
                }
            }

            // Przypisanie wartości wiadomości zostało oddzielone ze względu na to,
            // że w wiadomości może zostać przesłanych więcej rodzajów znaków
            var messageRegex = new Regex(@"^(\w+-\)[-\w\s]+\(\|){4,}message-\)(?<message>.+)\(\|$");

            if (messageRegex.IsMatch(serializedMessage))
                packet.SetMessage(messageRegex.Match(serializedMessage).Groups["message"].Value);

            return packet;
        }
    }
}