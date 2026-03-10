using Cysharp.Threading.Tasks;

public interface IUserDataBase
{
}

public interface IUserData : IUserDataBase
{
    //单捞磐 包府
    public void InitData();
    public bool LoadData();
    public bool SaveData();
}

public interface IAsyncUserData : IUserDataBase
{
    //单捞磐 包府
    public UniTask InitData();
    public UniTask LoadData();
    public UniTask SaveData();
}