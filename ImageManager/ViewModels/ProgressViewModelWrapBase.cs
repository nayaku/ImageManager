using StyletIoC;

namespace ImageManager.ViewModels
{
    public abstract class ProgressViewModelWrapBase : Screen, IInjectionAware
    {
        public ProgressViewModel ProgressViewModel { get; set; }

        protected CancellationToken _cancellationToken;

        public ProgressViewModelWrapBase()
        {
            ProgressViewModel = new(Process, Done, Cancel);
            _cancellationToken = ProgressViewModel.CancellationToken;
        }
        /// <summary>
        /// 设置消息
        /// </summary>
        public string Message
        {
            get => ProgressViewModel.Message;
            set => ProgressViewModel.Message = value;
        }

        /// <summary>
        /// 进度条，0-100
        /// </summary>
        public double Progress
        {
            get => ProgressViewModel.Progress;
            set => ProgressViewModel.Progress = value;
        }
        /// <summary>
        /// 处理
        /// </summary>
        /// <returns>true为成功，false为失败</returns>
        protected abstract bool Process();
        /// <summary>
        /// 处理结束后操作
        /// </summary>
        protected abstract void Done();
        /// <summary>
        /// 取消操作
        /// </summary>
        protected abstract void Cancel();

        public void ParametersInjected() { }
    }
}
