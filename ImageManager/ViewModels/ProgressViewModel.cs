namespace ImageManager.ViewModels
{
    public class ProgressViewModel : Screen
    {
        private bool _isDoingCleanUp;
        private CancellationTokenSource _cancellationTokenSource = new();
        private bool _canClose = true;
        private Func<bool> _processFunc;
        private Action _donection;
        private Action? _cancelAction;

        /// <summary>
        /// 进度条，0-100
        /// </summary>
        public double Progress { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        public string Title =>
            string.Format("{0}中...({1:F2}%)", _isDoingCleanUp ? "取消" : "处理", Progress);

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        /// <summary>
        /// 进度条组件
        /// </summary>
        /// <remarks>如果处理失败或者用户取消，则运行取消函数，否则运行成功处理函数。</remarks>
        /// <param name="processFunc">处理</param>
        /// <param name="doneAction">成功处理</param>
        /// <param name="cancelFunc">取消</param>
        public ProgressViewModel(Func<bool> processFunc,
            Action doneAction, Action? cancelFunc)
        {
            _processFunc = processFunc;
            _donection = doneAction;
            _cancelAction = cancelFunc;
        }

        public async void Process()
        {
            _canClose = false;
            var result = await Task.Run(() => _processFunc());
            if (!result || _cancellationTokenSource.Token.IsCancellationRequested)
                await Task.Run(() => _cancelAction?.Invoke());
            else
                await Task.Run(() => _donection());
            _canClose = true;
            RequestClose();
        }

        public void CancelTask()
        {
            _cancellationTokenSource.Cancel();
        }

        public Task<bool> CanCloseAsync()
        {
            return Task.Run(() =>
            {
                if (_canClose == false && _cancellationTokenSource.Token.IsCancellationRequested == false)
                    CancelTask();
                return _canClose;
            });
        }


    }
}