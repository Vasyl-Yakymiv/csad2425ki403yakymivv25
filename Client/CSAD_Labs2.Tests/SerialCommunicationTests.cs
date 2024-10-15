using CSAD_Labs2;
using Moq;
using Xunit;
using FluentAssertions;

namespace CSAD_Labs2.Tests
{
    public class SerialCommunicationTests
    {
        private readonly Mock<ISerialPort> _mockSerialPort;
        private readonly SerialCommunication _serialCommunication;

        public SerialCommunicationTests()
        {
            _mockSerialPort = new Mock<ISerialPort>();
            _serialCommunication = new SerialCommunication(_mockSerialPort.Object);
        }

        [Fact]
        public void Start_SendsAndReceivesMessageFromArduino()
        {
            // Arrange
            string inputMessage = "test message";
            string responseMessage = "response from Arduino";
            _mockSerialPort.Setup(sp => sp.ReadLine()).Returns(responseMessage);

            // Замокайте введення з консолі
            var consoleInput = new StringReader(inputMessage + Environment.NewLine + "exit" + Environment.NewLine);
            Console.SetIn(consoleInput);

            // Замокайте виведення з консолі
            var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            // Act
            _serialCommunication.Start();

            // Assert
            _mockSerialPort.Verify(sp => sp.WriteLine(It.Is<string>(s => s == inputMessage)), Times.Once);
            _mockSerialPort.Verify(sp => sp.ReadLine(), Times.Once);

            // Перевірка виводу на консоль
            var output = consoleOutput.ToString();
            Assert.Contains("Connected to Arduino", output);
            Assert.Contains("Received from Arduino: response from Arduino", output);
        }

        [Fact]
        public void Start_ClosesConnectionAfterExit()
        {
            // Arrange
            // Симулюємо введення команди "exit" без відправлення повідомлень
            var consoleInput = new StringReader("exit" + Environment.NewLine);
            Console.SetIn(consoleInput);

            // Замокайте виведення з консолі
            var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            // Act
            _serialCommunication.Start();

            // Assert
            _mockSerialPort.Verify(sp => sp.Open(), Times.Once);  // Перевірка, що з'єднання відкривається
            _mockSerialPort.Verify(sp => sp.Close(), Times.Once); // Перевірка, що з'єднання закривається
            _mockSerialPort.Verify(sp => sp.WriteLine(It.IsAny<string>()), Times.Never); // Перевірка, що нічого не надсилається

            // Перевірка, чи було виведено повідомлення про закриття з'єднання
            var output = consoleOutput.ToString();
            Assert.Contains("The connection is closed.", output);
        }
        [Fact]
        public void Start_SendsMultipleMessagesAndReceivesResponses()
        {
            // Arrange
            string firstMessage = "message 1";
            string secondMessage = "message 2";
            string firstResponse = "response 1 from Arduino";
            string secondResponse = "response 2 from Arduino";

            // Налаштування моків для отримання відповідей на кожне повідомлення
            _mockSerialPort.SetupSequence(sp => sp.ReadLine())
                           .Returns(firstResponse)
                           .Returns(secondResponse);

            // Створення симуляції користувацького вводу (два повідомлення і команда exit)
            var consoleInput = new StringReader(firstMessage + Environment.NewLine
                                                + secondMessage + Environment.NewLine
                                                + "exit" + Environment.NewLine);
            Console.SetIn(consoleInput);

            // Замокане виведення консолі
            var consoleOutput = new StringWriter();
            Console.SetOut(consoleOutput);

            // Act
            _serialCommunication.Start();

            // Assert
            // Перевірка, що перше повідомлення відправлено правильно
            _mockSerialPort.Verify(sp => sp.WriteLine(It.Is<string>(s => s == firstMessage)), Times.Once);

            // Перевірка, що друге повідомлення відправлено правильно
            _mockSerialPort.Verify(sp => sp.WriteLine(It.Is<string>(s => s == secondMessage)), Times.Once);

            // Перевірка отримання відповідей
            _mockSerialPort.Verify(sp => sp.ReadLine(), Times.Exactly(2));

            // Перевірка консолі для правильного виводу відповідей від Arduino
            var output = consoleOutput.ToString();
            Assert.Contains("Received from Arduino: response 1 from Arduino", output);
            Assert.Contains("Received from Arduino: response 2 from Arduino", output);

            // Перевірка закриття порту
            _mockSerialPort.Verify(sp => sp.Close(), Times.Once);
        }

    }
}