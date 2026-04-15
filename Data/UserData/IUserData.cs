using Cysharp.Threading.Tasks;

public interface IUserDataBase
{
}

public interface IUserData : IUserDataBase
{
    public void InitData();
    public bool LoadData();
    public bool SaveData();
}

public interface IAsyncUserData : IUserDataBase
{
    public UniTask InitData();
    public UniTask LoadData();
    public UniTask SaveData();
}