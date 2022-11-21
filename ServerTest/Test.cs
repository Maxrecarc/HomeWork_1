using System.Text;

namespace ServerTest
{
    [TestClass]
    public class Test
    {

        //Проверяем, что добавление робота происходит корректно
        [TestMethod]
        public void addRobotIsCorrect()
        {
            //given
            String addCommand = "/add name function attack";
            int expectedId = 0;

            //when
            int idRobot = Int32.Parse(convertByteToString(Server.completeTask(addCommand)));


            //then
            Assert.AreEqual(expectedId, idRobot);
        }

        //Проверяем, что возвращает корректный ответ на запрос информации о роботе
        [TestMethod]
        public void getRobotIsCorrect()
        {
            //given
            String addCommand = "/add name function heal";
            String getCommand = "/get ";
            String expectedInfo = "name: function";

            String idRobot = convertByteToString(Server.completeTask(addCommand));

            //when
            String robotInfo = convertByteToString(Server.completeTask(getCommand + idRobot));

            //then
            Assert.AreEqual(expectedInfo, robotInfo);
        }


        //Проверяем, что робот удаляется корректно
        [TestMethod]
        public void removeRobotIsCorrect()
        {
            //given
            String addCommand = "/add name function heal";
            String removeCommand = "/delete ";
            String expectedAnswer = "Robot deleted";
            int expectedCount = 0;

            String idRobot = convertByteToString(Server.completeTask(addCommand));

            //when
            String result = convertByteToString(Server.completeTask(removeCommand + idRobot));

            //then
            Assert.AreEqual(expectedAnswer, result);
            Assert.AreEqual(expectedCount, Server.getRobotsCount());
        }

        [TestMethod]
        public void wrongCommandIsCorrect()
        {
            //given
            String command = "command";
            String expectedAnswer = "Wrong command";

            //when
            String result = convertByteToString(Server.completeTask(command));

            //then
            Assert.AreEqual(expectedAnswer, result);
        }

        [TestMethod]
        public void notEnoughDetailsIsCorrect()
        {
            String addCommandHeal = "/add name function heal";
            String addCommandDefault = "/add name function default";
            String addCommandAttack = "/add name function attack";
            String deleteCommand = "/delete ";

            String errorResult = "";
            for (int i = 0; i < 6; i++)
            {
                errorResult = convertByteToString(Server.completeTask(addCommandHeal));
            }
            Server.completeTask(deleteCommand + 1);

            Server.completeTask(addCommandDefault);
            String result = convertByteToString(Server.completeTask(addCommandAttack));

            Assert.AreEqual("Not enough details", errorResult);
            Assert.AreEqual("5", result);
        }

        private String convertByteToString(byte[] bytes)
        {
            return Encoding.Unicode.GetString(bytes, 0, bytes.Length);
        }
    }
}