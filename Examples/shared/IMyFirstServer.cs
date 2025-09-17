namespace shared
{
    public interface IMyFirstServer
    {
        void SayHello(string msg);

        T SayHelloT<T>(T msg);
    }
}
