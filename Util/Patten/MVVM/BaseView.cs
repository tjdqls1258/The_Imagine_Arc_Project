using UnityEngine;

namespace MVVM
{
    public abstract class BaseView<TModel> : MonoBehaviour where TModel : BaseViewModel, new()
    {
        protected TModel model;

        protected virtual void Awake()
        {
            model = new();
            model.Init();
            BindUI();
        }

        protected abstract void BindUI();
    }
}