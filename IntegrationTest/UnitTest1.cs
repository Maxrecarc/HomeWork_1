namespace IntegrationTest
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var server = new Server();
            var client = new ServerConnection();

            var addr = "127.0.0.1";


            List<Addr> addresses = client.findServer(addr);

            Assert.Equal(1, addresses.Count);
        }
    }
}