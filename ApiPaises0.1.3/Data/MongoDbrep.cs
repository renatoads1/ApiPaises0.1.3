namespace ApiPaises013.Data
{

    public class MongoDbrep : IMongoDbrep
        {
        public string Colecao { get; set; }
        public string ConnectionString { get; set; }
        public string NomeBanco { get; set; }
    }

    public interface IMongoDbrep
        {
        string Colecao { get; set; }
        string ConnectionString { get; set; }
        string NomeBanco { get; set; }
    }

}
